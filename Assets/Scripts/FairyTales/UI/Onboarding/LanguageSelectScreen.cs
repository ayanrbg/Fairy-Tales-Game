using UnityEngine;
using UnityEngine.UI;
using FairyTales.UI.Core;

namespace FairyTales.UI.Onboarding
{
    public class LanguageSelectScreen : BaseScreen
    {
        [SerializeField] private Button btnRussian;
        [SerializeField] private Button btnKazakh;
        [SerializeField] private Button btnEnglish;
        [SerializeField] private Button btnContinue;

        [Header("Selection visuals")]
        [SerializeField] private GameObject selectedRu;
        [SerializeField] private GameObject selectedKz;
        [SerializeField] private GameObject selectedEn;

        private string _selectedLang = "ru";
        private ScreenManager _screens;

        private void Awake()
        {
            _screens = GetComponentInParent<ScreenManager>();

            btnRussian.onClick.AddListener(() => SelectLang("ru"));
            btnKazakh.onClick.AddListener(() => SelectLang("kz"));
            btnEnglish.onClick.AddListener(() => SelectLang("en"));
            btnContinue.onClick.AddListener(OnContinue);
        }

        protected override void OnShown()
        {
            var saved = PlayerPrefs.GetString("ft_lang", "ru");
            SelectLang(saved);
        }

        private void SelectLang(string lang)
        {
            _selectedLang = lang;
            if (selectedRu) selectedRu.SetActive(lang == "ru");
            if (selectedKz) selectedKz.SetActive(lang == "kz");
            if (selectedEn) selectedEn.SetActive(lang == "en");
        }

        private void OnContinue()
        {
            PlayerPrefs.SetString("ft_lang", _selectedLang);
            PlayerPrefs.Save();
            _screens.Show<PersonalizationScreen>();
        }
    }
}
