using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FairyTales.UI.Core
{
    public class ChildGatePopup : MonoBehaviour
    {
        private static ChildGatePopup _instance;

        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TMP_Text problemText;
        [SerializeField] private TMP_Text inputText;
        [SerializeField] private TMP_Text placeholderText;
        [SerializeField] private Button btnClose;
        [SerializeField] private Button[] numButtons; // 0-9
        [SerializeField] private Button btnBackspace;

        private int _answer;
        private string _input = "";
        private Action _onSuccess;

        private void Awake()
        {
            _instance = this;

            if (btnClose) btnClose.onClick.AddListener(Close);
            if (btnBackspace) btnBackspace.onClick.AddListener(OnBackspace);

            for (int i = 0; i < numButtons.Length; i++)
            {
                int digit = i;
                if (numButtons[i]) numButtons[i].onClick.AddListener(() => OnDigit(digit));
            }

            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            gameObject.SetActive(false);
        }

        public static void Show(Action onSuccess)
        {
            if (_instance == null)
            {
                Debug.LogWarning("[ChildGate] No instance — bypassing");
                onSuccess?.Invoke();
                return;
            }
            _instance.Present(onSuccess);
        }

        private void Present(Action onSuccess)
        {
            _onSuccess = onSuccess;
            _input = "";
            GenerateProblem();
            UpdateDisplay();

            gameObject.SetActive(true);
            canvasGroup.alpha = 0f;
            canvasGroup.DOFade(1f, 0.25f).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            });
        }

        private void GenerateProblem()
        {
            int a = UnityEngine.Random.Range(10, 50);
            int b = UnityEngine.Random.Range(10, 50);
            _answer = a + b;
            if (problemText) problemText.text = $"{a}+{b}=?";
        }

        private void OnDigit(int digit)
        {
            if (_input.Length >= 4) return;
            _input += digit.ToString();
            UpdateDisplay();
            CheckAnswer();
        }

        private void OnBackspace()
        {
            if (_input.Length == 0) return;
            _input = _input.Substring(0, _input.Length - 1);
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            if (inputText) inputText.text = _input;
            if (placeholderText)
                placeholderText.gameObject.SetActive(_input.Length == 0);
        }

        private void CheckAnswer()
        {
            if (!int.TryParse(_input, out int val)) return;
            if (val != _answer) return;

            var cb = _onSuccess;
            _onSuccess = null;
            Close(() => cb?.Invoke());
        }

        private void Close(Action onComplete = null)
        {
            canvasGroup.interactable = false;
            canvasGroup.DOFade(0f, 0.2f).SetEase(Ease.InQuad).OnComplete(() =>
            {
                canvasGroup.blocksRaycasts = false;
                gameObject.SetActive(false);
                onComplete?.Invoke();
            });
        }

        private void Close() => Close(null);
    }
}
