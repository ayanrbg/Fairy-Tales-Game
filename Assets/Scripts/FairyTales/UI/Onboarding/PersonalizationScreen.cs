using FairyTales.Audio;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FairyTales.UI.Core;
using FairyTales.UI.Library;

namespace FairyTales.UI.Onboarding
{
    public class PersonalizationScreen : BaseScreen
    {
        [SerializeField] private BackgroundMusicManager backgroundMusicManager;
        [SerializeField] private TMP_InputField nameInput;
        [SerializeField] private Button btnBoy;
        [SerializeField] private Button btnGirl;
        [SerializeField] private Button btnContinue;
        [SerializeField] private Button btnChangeLang;
        [SerializeField] private Button btnMusic;
        
        [Header("Selection visuals")]
        [SerializeField] private GameObject selectedBoy;
        [SerializeField] private GameObject selectedGirl;
        [SerializeField] private GameObject selectionBtnMusicOff;
        [SerializeField] private GameObject selectionBtnMusicOn;
        private string _gender = "male";
        private ScreenManager _screens;
        private void OnMusicToggle()
        {
            if (backgroundMusicManager) backgroundMusicManager.SetMuted(!backgroundMusicManager.IsMuted);
            selectionBtnMusicOff.SetActive(backgroundMusicManager.IsMuted);
            selectionBtnMusicOn.SetActive(!backgroundMusicManager.IsMuted);
        }
        private void Awake()
        {
            _screens = GetComponentInParent<ScreenManager>();

            btnBoy.onClick.AddListener(() => SelectGender("male"));
            btnGirl.onClick.AddListener(() => SelectGender("female"));
            btnContinue.onClick.AddListener(OnContinue);
            if (btnMusic) btnMusic.onClick.AddListener(OnMusicToggle);
            if (btnChangeLang)
                btnChangeLang.onClick.AddListener(() =>
                    _screens.Show<LanguageSelectScreen>());
            backgroundMusicManager = FindObjectOfType<BackgroundMusicManager>();
            selectionBtnMusicOff.SetActive(backgroundMusicManager.IsMuted);
            selectionBtnMusicOn.SetActive(!backgroundMusicManager.IsMuted);
        }

        protected override void OnShown()
        {
            var savedGender = PlayerPrefs.GetString("ft_gender", "male");
            SelectGender(savedGender);
            nameInput.text = PlayerPrefs.GetString("ft_childName", "");
            selectionBtnMusicOff.SetActive(backgroundMusicManager.IsMuted);
            selectionBtnMusicOn.SetActive(!backgroundMusicManager.IsMuted);
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
