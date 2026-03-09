using System;
using DG.Tweening;
using UnityEngine;

namespace FairyTales.UI.Core
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class BaseScreen : MonoBehaviour
    {
        [SerializeField] private float fadeDuration = 0.3f;

        private CanvasGroup _canvasGroup;

        protected CanvasGroup CanvasGroup =>
            _canvasGroup ??= GetComponent<CanvasGroup>();

        public virtual void Show(Action onComplete = null)
        {
            gameObject.SetActive(true);
            CanvasGroup.alpha = 0f;
            CanvasGroup.interactable = false;

            CanvasGroup.DOFade(1f, fadeDuration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    CanvasGroup.interactable = true;
                    CanvasGroup.blocksRaycasts = true;
                    OnShown();
                    onComplete?.Invoke();
                });
        }

        public virtual void Hide(Action onComplete = null)
        {
            CanvasGroup.interactable = false;

            CanvasGroup.DOFade(0f, fadeDuration)
                .SetEase(Ease.InQuad)
                .OnComplete(() =>
                {
                    CanvasGroup.blocksRaycasts = false;
                    gameObject.SetActive(false);
                    OnHidden();
                    onComplete?.Invoke();
                });
        }

        public void ShowImmediate()
        {
            gameObject.SetActive(true);
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
            OnHidden();
        }

        protected virtual void OnShown() { }
        protected virtual void OnHidden() { }
    }
}
