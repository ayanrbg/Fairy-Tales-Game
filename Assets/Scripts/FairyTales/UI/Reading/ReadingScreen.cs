using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using FairyTales.Api;
using FairyTales.Audio;
using FairyTales.Models;
using FairyTales.UI.Core;

namespace FairyTales.UI.Reading
{
    public enum NarrationMode { None, Default, AI }

    public class ReadingScreen : BaseScreen
    {
        [SerializeField] private PageNavigator pageNavigator;
        [SerializeField] private Button btnHome;
        [SerializeField] private Button btnToc;
        [SerializeField] private Button btnMusic;
        [SerializeField] private Button btnPrev;
        [SerializeField] private Button btnNext;
        [SerializeField] private RecordingOverlay recordingOverlay;
        [SerializeField] private GameObject selectionBtnMusicOff;
        [SerializeField] private GameObject selectionBtnMusicOn;

        private ScreenManager _screens;
        private BackgroundMusicManager _backgroundMusicManager;
        private NarrationPlayer _narrationPlayer;
        private NarrationService _narrationService;
        private DefaultNarrationProvider _defaultNarration;
        private TaleDetail _tale;
        private TaleSummary _taleSummary;
        private NarrationMode _mode = NarrationMode.Default;
        private bool _recordingMode;

        private void Awake()
        {
            _screens = GetComponentInParent<ScreenManager>();
            _narrationPlayer = FindAnyObjectByType<NarrationPlayer>();
            _backgroundMusicManager = FindAnyObjectByType<BackgroundMusicManager>();
            _defaultNarration = new DefaultNarrationProvider();
            var api = FindAnyObjectByType<ApiClient>();
            if (api) _narrationService = new NarrationService(api);

            if (btnHome) btnHome.onClick.AddListener(OnHome);
            if (btnToc) btnToc.onClick.AddListener(OnToc);
            if (btnMusic) btnMusic.onClick.AddListener(OnMusicToggle);
            if (btnPrev) btnPrev.onClick.AddListener(OnPrev);
            if (btnNext) btnNext.onClick.AddListener(OnNext);
            UpdateMusicButtons();
        }

        public void SetTale(TaleDetail tale, NarrationMode mode = NarrationMode.Default)
        {
            _tale = tale;
            _mode = mode;
            _recordingMode = false;
            _taleSummary = null;
        }

        public void SetRecordingMode(TaleDetail detail, TaleSummary summary)
        {
            _tale = detail;
            _taleSummary = summary;
            _mode = NarrationMode.None;
            _recordingMode = true;
        }

        protected override void OnPrepare()
        {
            if (_tale == null || _tale.pages == null) return;

            var startPage = _recordingMode ? 0 : ReadingState.LoadPage(_tale.id);
            pageNavigator.Init(_tale.id, _tale.pages, startPage);

            if (_recordingMode && recordingOverlay)
                recordingOverlay.Activate(_taleSummary, _screens);
            else if (recordingOverlay)
                recordingOverlay.Deactivate();

            SetRecordingUI(_recordingMode);
            UpdateNavButtons();
            UpdateMusicButtons();
        }

        protected override void OnShown()
        {
            if (_tale == null || _tale.pages == null) return;

            pageNavigator.OnPageChanged += OnPageChanged;

            var startPage = _recordingMode ? 0 : ReadingState.LoadPage(_tale.id);
            if (!_recordingMode) PlayNarration(startPage);
        }

        protected override void OnHidden()
        {
            if (pageNavigator != null)
                pageNavigator.OnPageChanged -= OnPageChanged;
            if (_narrationPlayer) _narrationPlayer.Stop();
            if (recordingOverlay && recordingOverlay.IsActive)
                recordingOverlay.Deactivate();
            SetRecordingUI(false);
            if (_tale != null && !_recordingMode)
                ReadingState.SavePage(_tale.id, pageNavigator.CurrentPage);
        }

        private void OnPageChanged(int page)
        {
            UpdateNavButtons();

            if (_recordingMode)
            {
                if (recordingOverlay) recordingOverlay.OnPageVisited(page);
            }
            else
            {
                PlayNarration(page);
                if (_tale != null) ReadingState.SavePage(_tale.id, page);
            }
        }

        private void PlayNarration(int page)
        {
            if (_narrationPlayer == null || _tale == null) return;
            _narrationPlayer.Stop();

            if (_mode == NarrationMode.AI)
                StartCoroutine(PlayAiNarration(page));
            else if (_mode == NarrationMode.Default)
                PlayDefaultNarration(page);
        }

        private void PlayDefaultNarration(int page)
        {
            var bytes = _defaultNarration.GetPageBytes(_tale.id, page);
            if (bytes != null)
            {
                _narrationPlayer.PlayFromBytes(bytes);
                return;
            }

            var clip = _defaultNarration.GetPage(_tale.id, page);
            if (clip != null) _narrationPlayer.PlayClip(clip);
        }

        private IEnumerator PlayAiNarration(int page)
        {
            if (_narrationService == null) yield break;

            yield return _narrationService.DownloadNarratedPage(_tale.id, page,
                onSuccess: bytes => _narrationPlayer.PlayFromBytes(bytes),
                onError: e =>
                {
                    Debug.LogWarning($"[Reading] AI narration failed p{page}: {e}");
                    PlayDefaultNarration(page);
                });
        }

        private void UpdateNavButtons()
        {
            if (btnPrev) btnPrev.interactable = pageNavigator.CurrentPage > 0;
            if (btnNext) btnNext.interactable =
                pageNavigator.CurrentPage < pageNavigator.TotalPages - 1;
        }

        private void SetRecordingUI(bool recording)
        {
            if (btnToc) btnToc.gameObject.SetActive(!recording);
            if (btnMusic) btnMusic.gameObject.SetActive(!recording);
        }

        private void OnPrev() => pageNavigator.PrevPage();
        private void OnNext() => pageNavigator.NextPage();

        private void OnHome()
        {
            if (_narrationPlayer) _narrationPlayer.Stop();
            _screens.Show<Library.LibraryScreen>();
        }

        private void OnToc()
        {
            var popup = GetComponentInChildren<TableOfContentsPopup>(true);
            if (popup) popup.Show(_tale, pageNavigator);
        }

        private void OnMusicToggle()
        {
            if (_backgroundMusicManager)
                _backgroundMusicManager.SetMuted(!_backgroundMusicManager.IsMuted);
            UpdateMusicButtons();
        }

        private void UpdateMusicButtons()
        {
            if (_backgroundMusicManager == null) return;
            if (selectionBtnMusicOff) selectionBtnMusicOff.SetActive(_backgroundMusicManager.IsMuted);
            if (selectionBtnMusicOn) selectionBtnMusicOn.SetActive(!_backgroundMusicManager.IsMuted);
        }
    }
}
