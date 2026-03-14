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
        [SerializeField] private float fadeDuration = 0.4f;

        private AspectRatioFitter _arf;
        private Image _bufferImage;
        private AspectRatioFitter _bufferArf;

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
            if (illustrationImage)
                _arf = illustrationImage.GetComponent<AspectRatioFitter>();
            EnsureBuffer();
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

            // Copy current illustration to buffer for crossfade
            if (illustrationImage && _bufferImage)
            {
                _bufferImage.sprite = illustrationImage.sprite;
                _bufferImage.color = Color.white;
                _bufferImage.enabled = true;
                if (_bufferArf && _arf)
                    _bufferArf.aspectRatio = _arf.aspectRatio;
            }

            // Apply new content
            ApplyPage(page);

            // Start new content transparent
            if (illustrationImage)
                illustrationImage.color = new Color(1, 1, 1, 0);
            if (pageText)
                pageText.alpha = 0f;

            // Crossfade: old (buffer) fades out, new fades in
            var seq = DOTween.Sequence();
            if (_bufferImage)
                seq.Join(_bufferImage.DOFade(0f, fadeDuration));
            if (illustrationImage)
                seq.Join(illustrationImage.DOFade(1f, fadeDuration));
            if (pageText)
                seq.Join(pageText.DOFade(1f, fadeDuration));

            seq.OnComplete(() =>
            {
                if (_bufferImage) _bufferImage.enabled = false;
                _transitioning = false;
            });
        }

        private void EnsureBuffer()
        {
            if (_bufferImage || !illustrationImage) return;

            var go = new GameObject("CrossfadeBuffer", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(illustrationImage.transform.parent, false);

            var src = illustrationImage.rectTransform;
            var dst = go.GetComponent<RectTransform>();
            dst.anchorMin = src.anchorMin;
            dst.anchorMax = src.anchorMax;
            dst.anchoredPosition = src.anchoredPosition;
            dst.sizeDelta = src.sizeDelta;
            dst.pivot = src.pivot;

            // Place behind main illustration
            go.transform.SetSiblingIndex(illustrationImage.transform.GetSiblingIndex());

            _bufferImage = go.GetComponent<Image>();
            _bufferImage.enabled = false;

            if (_arf)
            {
                _bufferArf = go.AddComponent<AspectRatioFitter>();
                _bufferArf.aspectMode = _arf.aspectMode;
            }
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
                if (sprite != null)
                {
                    illustrationImage.sprite = sprite;
                    if (_arf)
                        _arf.aspectRatio = (float)sprite.texture.width / sprite.texture.height;
                }
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
