using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FairyTales.Api;
using FairyTales.Audio;
using FairyTales.Models;
using FairyTales.UI.Core;
using FairyTales.UI.Narration;
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
        }

        protected override void OnShown()
        {
            UpdateUI();
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
        }

        private void OnRead()
        {
            if (_detail == null) return;

            var reading = _screens.Get<ReadingScreen>();
            if (reading == null) return;

            reading.SetTale(_detail);
            _screens.Show<ReadingScreen>();
        }

        private void OnListen()
        {
            if (_detail == null) return;

            var reading = _screens.Get<ReadingScreen>();
            if (reading == null) return;

            if (_hasAiNarration)
            {
                reading.SetTale(_detail, NarrationMode.AI);
                _screens.Show<ReadingScreen>();
            }
            else if (_defaultNarration.HasAnyNarration(_tale?.id ?? ""))
            {
                reading.SetTale(_detail, NarrationMode.Default);
                _screens.Show<ReadingScreen>();
            }
            else
            {
                ShowNoNarrationToast();
            }
        }

        private void ShowNoNarrationToast()
        {
            Toast.Show(Loc.Get("no_narration"));
        }

        private void OnNarrate()
        {
            var setup = _screens.Get<NarrationSetupScreen>();
            if (setup == null) return;

            setup.SetTale(_tale);
            _screens.Show<NarrationSetupScreen>();
        }

        private void OnBack()
        {
            _screens.Show<LibraryScreen>();
        }
    }
}
