using System;
using DG.Tweening;
using UnityEngine;

namespace FairyTales.UI.Core
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class BaseScreen : MonoBehaviour
    {
        [SerializeField] private float animDuration = 0.4f;
        [SerializeField] protected bool slideFromBottom = true;

        private CanvasGroup _canvasGroup;
        private RectTransform _rect;

        protected CanvasGroup CanvasGroup =>
            _canvasGroup ??= GetComponent<CanvasGroup>();

        public RectTransform Rect =>
            _rect ??= GetComponent<RectTransform>();

        public bool SlideEnabled => slideFromBottom;
        public float AnimDuration => animDuration;

        /// <summary>Fade-in (+ slide up if enabled). Used for standalone transitions.</summary>
        public virtual void Show(Action onComplete = null)
        {
            gameObject.SetActive(true);
            CanvasGroup.alpha = 0f;
            CanvasGroup.interactable = false;

            if (slideFromBottom)
                Rect.anchoredPosition = new Vector2(0f, -Screen.height);

            var seq = DOTween.Sequence();
            seq.Append(CanvasGroup.DOFade(1f, animDuration).SetEase(Ease.OutQuad));

            if (slideFromBottom)
                seq.Join(Rect.DOAnchorPosY(0f, animDuration).SetEase(Ease.OutCubic));

            seq.OnComplete(() =>
            {
                CanvasGroup.interactable = true;
                CanvasGroup.blocksRaycasts = true;
                OnShown();
                onComplete?.Invoke();
            });
        }

        /// <summary>Fade-out (+ slide up if enabled). Used for standalone transitions.</summary>
        public virtual void Hide(Action onComplete = null)
        {
            CanvasGroup.interactable = false;

            var seq = DOTween.Sequence();
            seq.Append(CanvasGroup.DOFade(0f, animDuration).SetEase(Ease.InQuad));

            if (slideFromBottom)
                seq.Join(Rect.DOAnchorPosY(Screen.height, animDuration).SetEase(Ease.InCubic));

            seq.OnComplete(() =>
            {
                CanvasGroup.blocksRaycasts = false;
                gameObject.SetActive(false);
                Rect.anchoredPosition = Vector2.zero;
                OnHidden();
                onComplete?.Invoke();
            });
        }

        /// <summary>Prepare screen before coordinated slide (activate, set alpha, position).</summary>
        public void PrepareBelow()
        {
            gameObject.SetActive(true);
            CanvasGroup.alpha = 1f;
            CanvasGroup.interactable = false;
            CanvasGroup.blocksRaycasts = false;
            Rect.anchoredPosition = new Vector2(0f, -Screen.height);
        }

        /// <summary>Finalize after coordinated slide-in completed.</summary>
        public void FinalizeShown()
        {
            Rect.anchoredPosition = Vector2.zero;
            CanvasGroup.interactable = true;
            CanvasGroup.blocksRaycasts = true;
            OnShown();
        }

        /// <summary>Finalize after coordinated slide-out completed.</summary>
        public void FinalizeHidden()
        {
            CanvasGroup.alpha = 0f;
            CanvasGroup.interactable = false;
            CanvasGroup.blocksRaycasts = false;
            gameObject.SetActive(false);
            Rect.anchoredPosition = Vector2.zero;
            OnHidden();
        }

        public void ShowImmediate()
        {
            gameObject.SetActive(true);
            Rect.anchoredPosition = Vector2.zero;
            CanvasGroup.alpha = 1f;
            CanvasGroup.interactable = true;
            CanvasGroup.blocksRaycasts = true;
            OnShown();
        }

        public void HideImmediate()
        {
            CanvasGroup.alpha = 0f;
            CanvasGroup.interactable = false;
            CanvasGroup.blocksRaycasts = false;
            gameObject.SetActive(false);
            Rect.anchoredPosition = Vector2.zero;
            OnHidden();
        }

        protected virtual void OnShown() { }
        protected virtual void OnHidden() { }
    }
}
