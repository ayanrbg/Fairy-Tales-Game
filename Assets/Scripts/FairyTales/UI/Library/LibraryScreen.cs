using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using FairyTales.Api;
using FairyTales.Audio;
using FairyTales.Models;
using FairyTales.UI.Core;
using FairyTales.UI.Onboarding;
using FairyTales.UI.Payment;

namespace FairyTales.UI.Library
{
    public class LibraryScreen : BaseScreen
    {
        [SerializeField] private Transform cardContainer;
        [SerializeField] private GameObject cardPrefab;
        [SerializeField] private Button btnSettings;
        [SerializeField] private Button btnMusic;
        [SerializeField] private Button btnUnlockAll;
        [SerializeField] private GameObject selectionBtnMusicOff;
        [SerializeField] private GameObject selectionBtnMusicOn;
        [SerializeField] private BackgroundMusicManager backgroundMusicManager;
        
        private ScreenManager _screens;
        private AuthService _auth;
        private TalesService _tales;
        private TaleSummary[] _loadedTales;
        private bool _registered;

        private void Awake()
        {
            _screens = GetComponentInParent<ScreenManager>();
            var api = FindAnyObjectByType<ApiClient>();
            _auth = new AuthService(api);
            _tales = new TalesService(api);

            if (btnSettings) btnSettings.onClick.AddListener(OnSettings);
            if (btnMusic) btnMusic.onClick.AddListener(OnMusicToggle);
            if (btnUnlockAll) btnUnlockAll.onClick.AddListener(OnUnlockAll);
            
            selectionBtnMusicOff.SetActive(backgroundMusicManager.IsMuted);
            selectionBtnMusicOn.SetActive(!backgroundMusicManager.IsMuted);
        }

        protected override void OnShown()
        {
            StartCoroutine(EnsureRegisteredAndLoad());
            selectionBtnMusicOff.SetActive(backgroundMusicManager.IsMuted);
            selectionBtnMusicOn.SetActive(!backgroundMusicManager.IsMuted);
        }

        private IEnumerator EnsureRegisteredAndLoad()
        {
            if (!_registered)
            {
                var userId = GetOrCreateUserId();
                var childName = PlayerPrefs.GetString("ft_childName", "");
                var gender = PlayerPrefs.GetString("ft_gender", "male");
                var lang = PlayerPrefs.GetString("ft_lang", "ru");

                yield return _auth.Register(userId, childName, gender, lang,
                    onSuccess: _ => _registered = true,
                    onError: e =>
                    {
                        // Already registered is fine
                        Debug.LogWarning($"[Library] Register: {e}");
                        _registered = true;
                    });
            }

            // Always reload — name/gender may have changed via Settings
            _loadedTales = null;
            yield return LoadTales();
        }

        private IEnumerator LoadTales()
        {
            var lang = PlayerPrefs.GetString("ft_lang", "ru");

            yield return _tales.GetTales(lang,
                onSuccess: tales =>
                {
                    _loadedTales = tales;
                    BuildGrid();
                },
                onError: e => Debug.LogError($"[Library] {e}"));
        }

        private void BuildGrid()
        {
            foreach (Transform child in cardContainer)
                Destroy(child.gameObject);

            if (_loadedTales == null) return;

            for (int i = 0; i < _loadedTales.Length; i++)
            {
                var tale = _loadedTales[i];
                var go = Instantiate(cardPrefab, cardContainer);
                go.SetActive(true);
                var card = go.GetComponent<TaleCard>();
                card.Init(tale, false, OnCardClick);

                // Staggered scale-in animation
                var t = go.transform;
                t.localScale = Vector3.zero;
                t.DOScale(Vector3.one, 0.3f)
                    .SetDelay(i * 0.05f)
                    .SetEase(Ease.OutBack);
            }
        }

        private void OnCardClick(TaleSummary tale)
        {
            var detail = _screens.Get<TaleDetailScreen>();
            if (detail == null) return;

            detail.SetTale(tale);
            _screens.Show<TaleDetailScreen>();
        }

        private void OnSettings()
        {
            _screens.Show<PersonalizationScreen>();
        }

        private void OnUnlockAll()
        {
            _screens.Show<PaymentScreen>();
        }

        private void OnMusicToggle()
        {
            if (backgroundMusicManager) backgroundMusicManager.SetMuted(!backgroundMusicManager.IsMuted);
            selectionBtnMusicOff.SetActive(backgroundMusicManager.IsMuted);
            selectionBtnMusicOn.SetActive(!backgroundMusicManager.IsMuted);
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
