using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FairyTales.Models;

namespace FairyTales.UI.Reading
{
    public class TableOfContentsPopup : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private Button btnClose;
        [SerializeField] private Transform thumbnailContainer;
        [SerializeField] private GameObject thumbnailPrefab;

        private static readonly Color SelectedBorder = new(0f, 0.8f, 0.9f);
        private static readonly Color NormalBorder = new(0.3f, 0.2f, 0.4f);

        private PageNavigator _navigator;
        private int _selectedPage;

        private void Awake()
        {
            if (btnClose) btnClose.onClick.AddListener(Hide);
            gameObject.SetActive(false);
        }

        public void Show(TaleDetail tale, PageNavigator navigator)
        {
            _navigator = navigator;
            _selectedPage = navigator.CurrentPage;

            if (titleText) titleText.text = tale.title;

            BuildThumbnails(tale);
            gameObject.SetActive(true);
            canvasGroup.alpha = 0f;
            canvasGroup.DOFade(1f, 0.2f);
        }

        public void Hide()
        {
            canvasGroup.DOFade(0f, 0.2f)
                .OnComplete(() => gameObject.SetActive(false));
        }

        private void BuildThumbnails(TaleDetail tale)
        {
            foreach (Transform child in thumbnailContainer)
                Destroy(child.gameObject);

            for (int i = 0; i < tale.totalPages; i++)
            {
                var go = Instantiate(thumbnailPrefab, thumbnailContainer);
                go.SetActive(true);
                SetupThumbnail(go, tale.id, i);
            }
        }

        private void SetupThumbnail(GameObject go, string taleId, int page)
        {
            var img = go.transform.Find("Cover")?.GetComponent<Image>();
            var label = go.transform.Find("Label")?.GetComponent<TMP_Text>();
            var border = go.GetComponent<Image>();
            var btn = go.GetComponent<Button>();

            var sprite = IllustrationProvider.GetThumbnail(taleId, page);
            if (img && sprite) img.sprite = sprite;
            if (label) label.text = (page + 1).ToString();
            if (border) border.color = page == _selectedPage
                ? SelectedBorder : NormalBorder;

            if (btn)
            {
                int p = page;
                btn.onClick.AddListener(() =>
                {
                    _navigator.GoToPage(p);
                    Hide();
                });
            }
        }
    }
}
