using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FairyTales.Api;
using FairyTales.Audio;
using FairyTales.Cache;
using FairyTales.Models;
using FairyTales.UI.Core;
using FairyTales.UI.Narration;
using FairyTales.UI.Onboarding;
using FairyTales.UI.Reading;

namespace FairyTales.UI.Library
{
    public class TaleDetailScreen : BaseScreen
    {
        [SerializeField] private Image coverImage;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text pageCountText;
        [SerializeField] private Button btnRead;
        [SerializeField] private Button btnListen;
        [SerializeField] private Button btnNarrate;
        [SerializeField] private Button btnBack;

        private ScreenManager _screens;
        private TalesService _tales;
        private NarrationService _narration;
        private DefaultNarrationProvider _defaultNarration;
        private TaleSummary _tale;
        private TaleDetail _detail;
        private bool _hasAiNarration;

        private void Awake()
        {
            _screens = GetComponentInParent<ScreenManager>();
            var api = FindAnyObjectByType<ApiClient>();
            _tales = new TalesService(api);
            _narration = new NarrationService(api);
            _defaultNarration = new DefaultNarrationProvider();

            if (btnRead) btnRead.onClick.AddListener(OnRead);
            if (btnListen) btnListen.onClick.AddListener(OnListen);
            if (btnNarrate) btnNarrate.onClick.AddListener(OnNarrate);
            if (btnBack) btnBack.onClick.AddListener(OnBack);
        }

        public void SetTale(TaleSummary tale)
        {
            _tale = tale;
            _detail = null;
            _hasAiNarration = false;
            ClearUI();
        }

        private void ClearUI()
        {
            if (titleText) titleText.text = "";
            if (pageCountText) pageCountText.text = "";
            if (coverImage) coverImage.sprite = null;
            if (btnListen) btnListen.gameObject.SetActive(false);
        }

        protected override void OnPrepare()
        {
            UpdateUI();
        }

        protected override void OnShown()
        {
            StartCoroutine(LoadDetail());
        }

        private void UpdateUI()
        {
            if (titleText) titleText.text = _tale?.title ?? "";

            var cover = _tale != null ? CoverProvider.Get(_tale.id) : null;
            if (coverImage && cover != null) coverImage.sprite = cover;

            if (pageCountText && _detail != null)
                pageCountText.text = $"{_detail.totalPages} {Loc.Get("pages")}";
        }

        private IEnumerator LoadDetail()
        {
            if (_tale == null) yield break;

            yield return _tales.GetTale(_tale.id,
                onSuccess: d => { _detail = d; UpdateUI(); },
                onError: e => Debug.LogError($"[TaleDetail] {e}"));

            // Personalize pages (replace {childName} etc.)
            if (_detail?.pages != null)
            {
                var childName = PlayerPrefs.GetString("ft_childName", "");
                var gender = PlayerPrefs.GetString("ft_gender", "male");
                yield return _tales.Personalize(_detail.id, childName, gender,
                    onSuccess: pages => _detail.pages = pages,
                    onError: _ => { });
            }

            yield return _narration.GetNarrationStatus(_tale.id,
                onSuccess: s => _hasAiNarration = s.status == "done" || s.status == "ready",
                onError: _ => _hasAiNarration = false);

            UpdateListenButton();
        }

        private void UpdateListenButton()
        {
            if (!btnListen) return;
            bool hasDefault = _defaultNarration.HasAnyNarration(_tale?.id ?? "");
            btnListen.gameObject.SetActive(_hasAiNarration || hasDefault);
        }

        private void OnRead()
        {
            if (_detail == null) return;
            EnsureDownloadedThen(NarrationMode.None);
        }

        private void OnListen()
        {
            if (_detail == null) return;

            if (_hasAiNarration)
                EnsureDownloadedThen(NarrationMode.AI);
            else if (_defaultNarration.HasAnyNarration(_tale?.id ?? ""))
                EnsureDownloadedThen(NarrationMode.Default);
            else
                ShowNoNarrationToast();
        }

        private void EnsureDownloadedThen(NarrationMode mode)
        {
            if (AssetCache.IsTaleDownloaded(_tale.id))
            {
                Debug.Log($"[TaleDetail] Tale {_tale.id} already cached, opening");
                OpenReading(mode);
                return;
            }

            var download = _screens.Get<DownloadScreen>();
            if (download == null)
            {
                Debug.LogError("[TaleDetail] DownloadScreen not found in scene!");
                OpenReading(mode);
                return;
            }

            Debug.Log($"[TaleDetail] Starting download for {_tale.id}");
            download.SetSingleTale(_tale, () => OpenReading(mode));
            _screens.Show<DownloadScreen>();
        }

        private void OpenReading(NarrationMode mode)
        {
            var reading = _screens.Get<ReadingScreen>();
            if (reading == null) return;

            reading.SetTale(_detail, mode);
            _screens.Show<ReadingScreen>();
        }

        private void ShowNoNarrationToast()
        {
            Toast.Show(Loc.Get("no_narration"));
        }

        private void OnNarrate()
        {
            ChildGatePopup.Show(() =>
            {
                var setup = _screens.Get<NarrationSetupScreen>();
                if (setup == null) return;

                setup.SetTale(_tale, _detail);
                _screens.Show<NarrationSetupScreen>();
            });
        }

        private void OnBack()
        {
            _screens.Show<LibraryScreen>();
        }
    }
}
