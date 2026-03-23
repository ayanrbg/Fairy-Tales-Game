using System;
using System.Collections;
using FairyTales.Api;
using FairyTales.Audio;
using FairyTales.Cache;
using FairyTales.Models;
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
        private AuthService _auth;
        private TalesService _tales;

        private void OnMusicToggle()
        {
            if (backgroundMusicManager) backgroundMusicManager.SetMuted(!backgroundMusicManager.IsMuted);
            selectionBtnMusicOff.SetActive(backgroundMusicManager.IsMuted);
            selectionBtnMusicOn.SetActive(!backgroundMusicManager.IsMuted);
        }

        private void Awake()
        {
            _screens = GetComponentInParent<ScreenManager>();
            var api = FindAnyObjectByType<ApiClient>();
            _auth = new AuthService(api);
            _tales = new TalesService(api);

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

        protected override void OnPrepare()
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

            btnContinue.interactable = false;
            StartCoroutine(RegisterAndRoute(childName));
        }

        private IEnumerator RegisterAndRoute(string childName)
        {
            var userId = GetOrCreateUserId();
            var lang = PlayerPrefs.GetString("ft_lang", "ru");

            // Register / login
            yield return _auth.Register(userId, childName, _gender, lang,
                onSuccess: _ => { },
                onError: e => Debug.LogWarning($"[Personalization] Register: {e}"));

            // Load tale list
            TaleSummary[] tales = null;
            yield return _tales.GetTales(lang,
                onSuccess: t => tales = t,
                onError: e => Debug.LogError($"[Personalization] Tales: {e}"));

            btnContinue.interactable = true;

            if (tales != null && HasMissingCovers(tales))
            {
                var download = _screens.Get<DownloadScreen>();
                if (download != null)
                {
                    download.SetTales(tales);
                    _screens.Show<DownloadScreen>();
                    yield break;
                }
            }

            _screens.Show<LibraryScreen>();
        }

        private bool HasMissingCovers(TaleSummary[] tales)
        {
            foreach (var t in tales)
                if (!AssetCache.IsCoverDownloaded(t.id)) return true;
            return false;
        }

        private string GetOrCreateUserId()
        {
            var id = PlayerPrefs.GetString("ft_userId", "");
            if (!string.IsNullOrEmpty(id)) return id;
            id = Guid.NewGuid().ToString();
            PlayerPrefs.SetString("ft_userId", id);
            return id;
        }
    }
}
