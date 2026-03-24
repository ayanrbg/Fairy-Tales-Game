using FairyTales.Audio;
using FairyTales.UI.Core;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization.Settings;

namespace FairyTales.UI.Onboarding
{
    public class LanguageSelectScreen : BaseScreen
    {
        [SerializeField] private BackgroundMusicManager backgroundMusicManager;
        [SerializeField] private Button btnRussian;
        [SerializeField] private Button btnKazakh;
        [SerializeField] private Button btnEnglish;
        [SerializeField] private Button btnContinue;
        [SerializeField] private Button btnMusic;

        [Header("Selection visuals")]
        [SerializeField] private GameObject selectedRu;
        [SerializeField] private GameObject selectedKz;
        [SerializeField] private GameObject selectedEn;
        [SerializeField] private GameObject selectionBtnMusicOff;
        [SerializeField] private GameObject selectionBtnMusicOn;

        private string _selectedLang = "ru";
        private ScreenManager _screens;

        private void Awake()
        {
            _screens = GetComponentInParent<ScreenManager>();

            btnRussian.onClick.AddListener(() => SelectLang("ru"));
            btnKazakh.onClick.AddListener(() => SelectLang("kz"));
            btnEnglish.onClick.AddListener(() => SelectLang("en"));
            btnContinue.onClick.AddListener(OnContinue);
            if (btnMusic) btnMusic.onClick.AddListener(OnMusicToggle);
            backgroundMusicManager = FindObjectOfType<BackgroundMusicManager>();
            selectionBtnMusicOff.SetActive(backgroundMusicManager.IsMuted);
            selectionBtnMusicOn.SetActive(!backgroundMusicManager.IsMuted);
        }

        protected override void OnPrepare()
        {
            // First launch — auto-detect device language
            if (!PlayerPrefs.HasKey("ft_lang"))
            {
                var detected = Loc.DetectSystemLang();
                SelectLang(detected);
            }
            else
            {
                SelectLang(Loc.Lang);
            }

            selectionBtnMusicOff.SetActive(backgroundMusicManager.IsMuted);
            selectionBtnMusicOn.SetActive(!backgroundMusicManager.IsMuted);
        }

        private void SelectLang(string lang)
        {
            _selectedLang = lang;
            if (selectedRu) selectedRu.SetActive(lang == "ru");
            if (selectedKz) selectedKz.SetActive(lang == "kz");
            if (selectedEn) selectedEn.SetActive(lang == "en");
            Loc.ApplyLocale(lang);
        }

        private void OnContinue()
        {
            Loc.Lang = _selectedLang;
            _screens.Show<PersonalizationScreen>();
        }

        private void OnMusicToggle()
        {
            if (backgroundMusicManager) backgroundMusicManager.SetMuted(!backgroundMusicManager.IsMuted);
            selectionBtnMusicOff.SetActive(backgroundMusicManager.IsMuted);
            selectionBtnMusicOn.SetActive(!backgroundMusicManager.IsMuted);
        }
    }
}
