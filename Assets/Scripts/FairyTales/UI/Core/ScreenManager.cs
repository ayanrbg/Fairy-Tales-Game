using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace FairyTales.UI.Core
{
    public class ScreenManager : MonoBehaviour
    {
        [SerializeField] private BaseScreen initialScreen;
        [SerializeField] private BaseScreen onboardedScreen;
        [SerializeField] private Image sharedBackground;

        private AspectRatioFitter _bgArf;
        private readonly Dictionary<System.Type, BaseScreen> _screens = new();
        private BaseScreen _current;
        private bool _transitioning;

        private void Awake()
        {
            var all = GetComponentsInChildren<BaseScreen>(true);
            foreach (var screen in all)
            {
                _screens[screen.GetType()] = screen;
                screen.HideImmediate();
            }

            if (sharedBackground != null)
            {
                sharedBackground.gameObject.SetActive(true);
                _bgArf = sharedBackground.GetComponent<AspectRatioFitter>();
            }
        }

        private void Start()
        {
            var start = initialScreen;

            if (onboardedScreen != null &&
                !string.IsNullOrEmpty(PlayerPrefs.GetString("ft_childName", "")))
            {
                start = onboardedScreen;
            }

            if (start != null) start.ShowImmediate();
            _current = start;
        }

        public void Show<T>() where T : BaseScreen
        {
            if (_transitioning) return;

            if (!_screens.TryGetValue(typeof(T), out var next))
            {
                Debug.LogError($"[ScreenManager] Screen not found: {typeof(T).Name}");
                return;
            }

            if (next == _current) return;

            var prev = _current;
            _current = next;

            // Both slide-enabled → coordinated swipe-up (move as one unit)
            if (prev != null && prev.SlideEnabled && next.SlideEnabled)
            {
                CoordinatedSlide(prev, next);
                return;
            }

            // Fallback: independent animations
            if (prev != null) prev.Hide();
            next.Show();
        }

        private void CoordinatedSlide(BaseScreen prev, BaseScreen next)
        {
            _transitioning = true;
            var h = Screen.height;
            var duration = next.AnimDuration;

            // prev at 0, next directly below
            prev.Rect.anchoredPosition = Vector2.zero;
            next.PrepareBelow();

            var seq = DOTween.Sequence();
            seq.Append(prev.Rect.DOAnchorPosY(h, duration).SetEase(Ease.InOutCubic));
            seq.Join(next.Rect.DOAnchorPosY(0f, duration).SetEase(Ease.InOutCubic));
            seq.OnComplete(() =>
            {
                prev.FinalizeHidden();
                next.FinalizeShown();
                _transitioning = false;
            });
        }

        public void ShowImmediate<T>() where T : BaseScreen
        {
            if (!_screens.TryGetValue(typeof(T), out var next)) return;
            if (next == _current) return;

            _current?.HideImmediate();
            _current = next;
            next.ShowImmediate();
        }

        public T Get<T>() where T : BaseScreen
        {
            _screens.TryGetValue(typeof(T), out var screen);
            return screen as T;
        }

        public BaseScreen Current => _current;

        /// <summary>
        /// Sets shared background sprite with proper aspect ratio scaling.
        /// Pass null to clear (reverts to solid color).
        /// </summary>
        public void SetBackground(Sprite sprite)
        {
            if (sharedBackground == null) return;

            sharedBackground.sprite = sprite;
            sharedBackground.color = sprite != null ? Color.white : new Color(0.12f, 0.08f, 0.18f);

            if (_bgArf && sprite != null)
                _bgArf.aspectRatio = (float)sprite.texture.width / sprite.texture.height;
        }
    }
}
