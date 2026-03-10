using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FairyTales.Models;

namespace FairyTales.UI.Library
{
    public class TaleCard : MonoBehaviour
    {
        [SerializeField] private Image coverImage;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private GameObject lockIcon;
        [SerializeField] private Button button;

        private TaleSummary _tale;
        private Action<TaleSummary> _onClick;

        public void Init(TaleSummary tale, bool isLocked, Action<TaleSummary> onClick)
        {
            _tale = tale;
            _onClick = onClick;

            if (titleText) titleText.text = tale.title;

            var cover = CoverProvider.Get(tale.id);
            if (cover != null && coverImage)
                coverImage.sprite = cover;

            if (lockIcon) lockIcon.SetActive(isLocked);
            if (button) button.onClick.AddListener(OnClick);
        }

        private void OnClick() => _onClick?.Invoke(_tale);

        private void OnDestroy()
        {
            if (button) button.onClick.RemoveListener(OnClick);
        }
    }
}
