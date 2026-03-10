using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FairyTales.UI.Core;
using FairyTales.UI.Library;

namespace FairyTales.UI.Onboarding
{
    public class PersonalizationScreen : BaseScreen
    {
        [SerializeField] private TMP_InputField nameInput;
        [SerializeField] private Button btnBoy;
        [SerializeField] private Button btnGirl;
        [SerializeField] private Button btnContinue;
        [SerializeField] private Button btnChangeLang;

        [Header("Selection visuals")]
        [SerializeField] private GameObject selectedBoy;
        [SerializeField] private GameObject selectedGirl;

        private string _gender = "male";
        private ScreenManager _screens;

        private void Awake()
        {
            _screens = GetComponentInParent<ScreenManager>();

            btnBoy.onClick.AddListener(() => SelectGender("male"));
            btnGirl.onClick.AddListener(() => SelectGender("female"));
            btnContinue.onClick.AddListener(OnContinue);

            if (btnChangeLang)
                btnChangeLang.onClick.AddListener(() =>
                    _screens.Show<LanguageSelectScreen>());
        }

        protected override void OnShown()
        {
            var savedGender = PlayerPrefs.GetString("ft_gender", "male");
            SelectGender(savedGender);
            nameInput.text = PlayerPrefs.GetString("ft_childName", "");
        }

        private void SelectGender(string gender)
        {
            _gender = gender;
            if (selectedBoy) selectedBoy.SetActive(gender == "male");
            if (selectedGirl) selectedGirl.SetActive(gender == "female");
        }

        private void OnContinue()
        {
            var childName = nameInput.text.Trim();
            if (string.IsNullOrEmpty(childName))
            {
                Debug.LogWarning("[Personalization] Name is empty");
                return;
            }

            PlayerPrefs.SetString("ft_childName", childName);
            PlayerPrefs.SetString("ft_gender", _gender);
            PlayerPrefs.Save();

            _screens.Show<LibraryScreen>();
        }
    }
}
