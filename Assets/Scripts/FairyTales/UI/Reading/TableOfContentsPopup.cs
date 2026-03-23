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
        [SerializeField] private int maxChapters = 12;

        private static readonly Color SelectedBorder = new(0f, 0.8f, 0.9f);
        private static readonly Color NormalBorder = new(0.3f, 0.2f, 0.4f);

        private PageNavigator _navigator;
        private TocThumbnail[] _pool;

        private void Awake()
        {
            if (btnClose) btnClose.onClick.AddListener(Hide);
        }

        private void EnsurePool()
        {
            if (_pool != null) return;
            _pool = new TocThumbnail[maxChapters];
            for (int i = 0; i < maxChapters; i++)
            {
                var go = Instantiate(thumbnailPrefab, thumbnailContainer);
                go.SetActive(false);
                _pool[i] = go.GetComponent<TocThumbnail>();
            }
        }

        public void Show(TaleDetail tale, PageNavigator navigator)
        {
            _navigator = navigator;
            if (titleText) titleText.text = tale.title;

            gameObject.SetActive(true);
            EnsurePool();
            Bind(tale, navigator.CurrentPage);

            canvasGroup.alpha = 0f;
            canvasGroup.DOFade(1f, 0.2f);
        }

        public void Hide()
        {
            canvasGroup.DOFade(0f, 0.2f)
                .OnComplete(() => gameObject.SetActive(false));
        }

        private void Bind(TaleDetail tale, int currentPage)
        {
            int total = tale.totalPages;
            int chapters = Mathf.Min(maxChapters, total);
            int pagesPerChapter = Mathf.CeilToInt((float)total / chapters);

            for (int i = 0; i < _pool.Length; i++)
            {
                var thumb = _pool[i];
                if (i >= chapters)
                {
                    thumb.gameObject.SetActive(false);
                    continue;
                }

                int startPage = i * pagesPerChapter;
                int endPage = Mathf.Min(startPage + pagesPerChapter - 1, total - 1);
                bool selected = currentPage >= startPage && currentPage <= endPage;

                var sprite = IllustrationProvider.GetThumbnail(tale.id, startPage);
                if (thumb.Cover) thumb.Cover.sprite = sprite;
                if (thumb.Label) thumb.Label.text = $"{startPage + 1}-{endPage + 1}";
                if (thumb.Border) thumb.Border.color =
                    selected ? SelectedBorder : NormalBorder;

                thumb.Button?.onClick.RemoveAllListeners();
                if (thumb.Button)
                {
                    int p = startPage;
                    thumb.Button.onClick.AddListener(() =>
                    {
                        _navigator.GoToPage(p);
                        Hide();
                    });
                }

                thumb.gameObject.SetActive(true);
            }
        }
    }
}
