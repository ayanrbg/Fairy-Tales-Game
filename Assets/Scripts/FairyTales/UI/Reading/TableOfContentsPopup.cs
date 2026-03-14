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

                var thumb = go.GetComponent<TocThumbnail>();
                if (!thumb) continue;

                var sprite = IllustrationProvider.GetThumbnail(tale.id, i);
                if (thumb.Cover && sprite) thumb.Cover.sprite = sprite;
                if (thumb.Label) thumb.Label.text = (i + 1).ToString();
                if (thumb.Border) thumb.Border.color = i == _selectedPage
                    ? SelectedBorder : NormalBorder;

                if (thumb.Button)
                {
                    int p = i;
                    thumb.Button.onClick.AddListener(() =>
                    {
                        _navigator.GoToPage(p);
                        Hide();
                    });
                }
            }
        }
    }
}
