using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FairyTales.Api;
using FairyTales.Audio;
using FairyTales.Models;
using FairyTales.UI.Core;
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
                pageCountText.text = $"{_detail.totalPages} стр.";
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
                onSuccess: s => _hasAiNarration = s.status == "ready",
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
            // AI narration available → server audio, otherwise → default client audio
            if (_hasAiNarration)
                Debug.Log($"[TaleDetail] Listen AI: {_tale?.id}");
            else if (_defaultNarration.HasAnyNarration(_tale?.id ?? ""))
                Debug.Log($"[TaleDetail] Listen Default: {_tale?.id}");
            else
                Debug.Log($"[TaleDetail] No narration: {_tale?.id}");
        }

        private void OnNarrate()
        {
            // Will navigate to NarrationSetupScreen (Phase 7)
            Debug.Log($"[TaleDetail] Narrate: {_tale?.id}");
        }

        private void OnBack()
        {
            _screens.Show<LibraryScreen>();
        }
    }
}
