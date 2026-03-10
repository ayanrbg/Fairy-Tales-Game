using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using FairyTales.Api;
using FairyTales.Audio;
using FairyTales.Models;
using FairyTales.UI.Core;

namespace FairyTales.UI.Library
{
    public class LibraryScreen : BaseScreen
    {
        [SerializeField] private Transform cardContainer;
        [SerializeField] private GameObject cardPrefab;
        [SerializeField] private Button btnSettings;
        [SerializeField] private Button btnMusic;
        [SerializeField] private Button btnUnlockAll;

        private ScreenManager _screens;
        private TalesService _tales;
        private TaleSummary[] _loadedTales;

        private void Awake()
        {
            _screens = GetComponentInParent<ScreenManager>();
            var api = FindAnyObjectByType<ApiClient>();
            _tales = new TalesService(api);

            if (btnMusic) btnMusic.onClick.AddListener(OnMusicToggle);
        }

        protected override void OnShown()
        {
            if (_loadedTales == null)
                StartCoroutine(LoadTales());
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

            foreach (var tale in _loadedTales)
            {
                var go = Instantiate(cardPrefab, cardContainer);
                go.SetActive(true);
                var card = go.GetComponent<TaleCard>();
                card.Init(tale, false, OnCardClick);
            }
        }

        private void OnCardClick(TaleSummary tale)
        {
            var detail = _screens.Get<TaleDetailScreen>();
            if (detail == null) return;

            detail.SetTale(tale);
            _screens.Show<TaleDetailScreen>();
        }

        private void OnMusicToggle()
        {
            var bgm = FindAnyObjectByType<BackgroundMusicManager>();
            if (bgm) bgm.SetMuted(!bgm.IsMuted);
        }

        public void Refresh()
        {
            _loadedTales = null;
            if (gameObject.activeInHierarchy)
                StartCoroutine(LoadTales());
        }
    }
}
