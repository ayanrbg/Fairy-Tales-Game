using System;
using System.Text.RegularExpressions;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace FairyTales.UI.Reading
{
    public class PageNavigator : MonoBehaviour
    {
        [SerializeField] private Image illustrationImage;
        [SerializeField] private TMP_Text pageText;
        [SerializeField] private CanvasGroup contentGroup;
        [SerializeField] private float fadeDuration = 0.25f;

        private static readonly Regex GenderTag =
            new(@"\{m:([^|]*)\|f:([^}]*)\}", RegexOptions.Compiled);

        private string _taleId;
        private string[] _pages;
        private string _gender;
        private int _currentPage;
        private bool _transitioning;

        public int CurrentPage => _currentPage;
        public int TotalPages => _pages?.Length ?? 0;
        public event Action<int> OnPageChanged;

        public void Init(string taleId, string[] pages, int startPage = 0)
        {
            _taleId = taleId;
            _pages = pages;
            _gender = PlayerPrefs.GetString("ft_gender", "male");
            _currentPage = Mathf.Clamp(startPage, 0, pages.Length - 1);
            ApplyPage(_currentPage);
        }

        public void NextPage()
        {
            if (_transitioning || _currentPage >= TotalPages - 1) return;
            GoToPage(_currentPage + 1);
        }

        public void PrevPage()
        {
            if (_transitioning || _currentPage <= 0) return;
            GoToPage(_currentPage - 1);
        }

        public void GoToPage(int page)
        {
            page = Mathf.Clamp(page, 0, TotalPages - 1);
            if (page == _currentPage || _transitioning) return;

            _transitioning = true;

            // Fade out illustration + text only
            var seq = DOTween.Sequence();
            if (illustrationImage)
                seq.Join(illustrationImage.DOFade(0f, fadeDuration));
            if (pageText)
                seq.Join(pageText.DOFade(0f, fadeDuration));

            seq.OnComplete(() =>
            {
                ApplyPage(page);

                var seqIn = DOTween.Sequence();
                if (illustrationImage)
                    seqIn.Join(illustrationImage.DOFade(1f, fadeDuration));
                if (pageText)
                    seqIn.Join(pageText.DOFade(1f, fadeDuration));
                seqIn.OnComplete(() => _transitioning = false);
            });
        }

        private void ApplyPage(int page)
        {
            _currentPage = page;

            if (pageText)
            {
                pageText.text = ResolveGender(_pages[page]);
                pageText.alpha = 1f;
            }

            var sprite = IllustrationProvider.GetPage(_taleId, page);
            if (illustrationImage)
            {
                if (sprite != null) illustrationImage.sprite = sprite;
                illustrationImage.color = Color.white;
            }

            OnPageChanged?.Invoke(page);
        }

        private string ResolveGender(string text)
        {
            bool isMale = _gender == "male";
            return GenderTag.Replace(text, m => isMale ? m.Groups[1].Value : m.Groups[2].Value);
        }
    }
}
