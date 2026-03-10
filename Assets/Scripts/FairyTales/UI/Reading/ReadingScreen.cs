using UnityEngine;
using UnityEngine.UI;
using FairyTales.Audio;
using FairyTales.Models;
using FairyTales.UI.Core;

namespace FairyTales.UI.Reading
{
    public class ReadingScreen : BaseScreen
    {
        [SerializeField] private PageNavigator pageNavigator;
        [SerializeField] private Button btnHome;
        [SerializeField] private Button btnToc;
        [SerializeField] private Button btnMusic;
        [SerializeField] private Button btnPrev;
        [SerializeField] private Button btnNext;

        private ScreenManager _screens;
        private NarrationPlayer _narrationPlayer;
        private DefaultNarrationProvider _defaultNarration;
        private TaleDetail _tale;
        private bool _autoNarrate = true;

        private void Awake()
        {
            _screens = GetComponentInParent<ScreenManager>();
            _narrationPlayer = FindAnyObjectByType<NarrationPlayer>();
            _defaultNarration = new DefaultNarrationProvider();

            if (btnHome) btnHome.onClick.AddListener(OnHome);
            if (btnToc) btnToc.onClick.AddListener(OnToc);
            if (btnMusic) btnMusic.onClick.AddListener(OnMusicToggle);
            if (btnPrev) btnPrev.onClick.AddListener(OnPrev);
            if (btnNext) btnNext.onClick.AddListener(OnNext);
        }

        public void SetTale(TaleDetail tale)
        {
            _tale = tale;
        }

        protected override void OnShown()
        {
            if (_tale == null || _tale.pages == null) return;

            pageNavigator.Init(_tale.id, _tale.pages);
            pageNavigator.OnPageChanged += OnPageChanged;
            PlayNarration(0);
            UpdateNavButtons();
        }

        protected override void OnHidden()
        {
            if (pageNavigator != null)
                pageNavigator.OnPageChanged -= OnPageChanged;
            if (_narrationPlayer) _narrationPlayer.Stop();
        }

        private void OnPageChanged(int page)
        {
            UpdateNavButtons();
            if (_autoNarrate) PlayNarration(page);
        }

        private void PlayNarration(int page)
        {
            if (_narrationPlayer == null || _tale == null) return;

            var clip = _defaultNarration.GetPage(_tale.id, page);
            if (clip != null)
                _narrationPlayer.PlayClip(clip);
        }

        private void UpdateNavButtons()
        {
            if (btnPrev) btnPrev.interactable = pageNavigator.CurrentPage > 0;
            if (btnNext) btnNext.interactable =
                pageNavigator.CurrentPage < pageNavigator.TotalPages - 1;
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
            var bgm = FindAnyObjectByType<BackgroundMusicManager>();
            if (bgm) bgm.SetMuted(!bgm.IsMuted);
        }
    }
}
