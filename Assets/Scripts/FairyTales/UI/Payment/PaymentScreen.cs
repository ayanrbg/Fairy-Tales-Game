using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using FairyTales.IAP;
using FairyTales.UI.Core;

namespace FairyTales.UI.Payment
{
    public class PaymentScreen : BaseScreen
    {
        [SerializeField] private Image background;
        [SerializeField] private float bgFadeDuration = 0.5f;
        [SerializeField] private Button btnClose;
        [SerializeField] private Button btnMonthly;
        [SerializeField] private Button btnYearly;
        [SerializeField] private Button btnTrial;
        [SerializeField] private Button btnTerms;
        [SerializeField] private Button btnRestore;
        [SerializeField] private Button btnPrivacy;
        [SerializeField] private GameObject monthlySelect;
        [SerializeField] private GameObject yearlySelect;
        [SerializeField] private TextMeshProUGUI txtMonthlyPrice;
        [SerializeField] private TextMeshProUGUI txtYearlyPrice;
        [SerializeField] private TextMeshProUGUI txtTrialLabel;

        private ScreenManager _screens;
        private bool _yearlySelected = true;

        private void Awake()
        {
            _screens = GetComponentInParent<ScreenManager>();

            if (background) SetBgAlpha(0f);
            if (btnClose) btnClose.onClick.AddListener(OnClose);
            if (btnMonthly) btnMonthly.onClick.AddListener(() => SelectPlan(false));
            if (btnYearly) btnYearly.onClick.AddListener(() => SelectPlan(true));
            if (btnTrial) btnTrial.onClick.AddListener(OnPurchase);
            if (btnTerms) btnTerms.onClick.AddListener(OnTerms);
            if (btnRestore) btnRestore.onClick.AddListener(OnRestore);
            if (btnPrivacy) btnPrivacy.onClick.AddListener(OnPrivacy);
        }

        protected override void OnPrepare()
        {
            SelectPlan(true);
            UpdatePrices();
        }

        protected override void OnShown()
        {
            if (background) background.DOFade(1f, bgFadeDuration).SetEase(Ease.OutQuad);
        }

        protected override void OnHidden()
        {
            if (background) SetBgAlpha(0f);
        }

        private void SetBgAlpha(float a)
        {
            var c = background.color;
            c.a = a;
            background.color = c;
        }

        private void SelectPlan(bool yearly)
        {
            _yearlySelected = yearly;
            if (monthlySelect) monthlySelect.SetActive(!yearly);
            if (yearlySelect) yearlySelect.SetActive(yearly);
            UpdateTrialButton();
        }

        private void UpdatePrices()
        {
            var iap = IAPManager.Instance;
            if (iap == null || !iap.IsInitialized) return;

            var monthlyPrice = iap.GetLocalizedPrice(IAPManager.ProductMonthly);
            var yearlyPrice = iap.GetLocalizedPrice(IAPManager.ProductYearly);

            if (txtMonthlyPrice && monthlyPrice != null)
                txtMonthlyPrice.text = $"{monthlyPrice}/{Loc.Get("per_month")}";
            if (txtYearlyPrice && yearlyPrice != null)
                txtYearlyPrice.text = $"{yearlyPrice}/{Loc.Get("per_year")}";
        }

        private void UpdateTrialButton()
        {
            if (!txtTrialLabel) return;

            var iap = IAPManager.Instance;
            var productId = _yearlySelected
                ? IAPManager.ProductYearly
                : IAPManager.ProductMonthly;

            bool hasTrial = iap != null && iap.IsInitialized && iap.HasTrialAvailable(productId);
            txtTrialLabel.text = hasTrial
                ? Loc.Get("start_trial")
                : Loc.Get("subscribe");
        }

        private void OnPurchase()
        {
            var iap = IAPManager.Instance;
            if (iap == null || !iap.IsInitialized)
            {
                Toast.Show(Loc.Get("iap_not_ready"));
                return;
            }

            var productId = _yearlySelected
                ? IAPManager.ProductYearly
                : IAPManager.ProductMonthly;

            SetButtonsInteractable(false);

            iap.Purchase(productId, success =>
            {
                SetButtonsInteractable(true);

                if (success)
                {
                    Toast.Show(Loc.Get("purchase_success"));
                    OnClose();
                }
            });
        }

        private void OnClose()
        {
            if (background)
            {
                background.DOFade(0f, bgFadeDuration).SetEase(Ease.InQuad)
                    .OnComplete(() => _screens.Show<Library.LibraryScreen>());
            }
            else
            {
                _screens.Show<Library.LibraryScreen>();
            }
        }

        private void OnRestore()
        {
            var iap = IAPManager.Instance;
            if (iap == null || !iap.IsInitialized)
            {
                Toast.Show(Loc.Get("iap_not_ready"));
                return;
            }

            SetButtonsInteractable(false);

            iap.RestorePurchases(success =>
            {
                SetButtonsInteractable(true);

                if (success && iap.IsSubscribed)
                {
                    Toast.Show(Loc.Get("restore_success"));
                    OnClose();
                }
                else
                {
                    Toast.Show(Loc.Get("restore_none"));
                }
            });
        }

        private void OnTerms()
        {
            Application.OpenURL("https://example.com/terms");
        }

        private void OnPrivacy()
        {
            Application.OpenURL("https://example.com/privacy");
        }

        private void SetButtonsInteractable(bool interactable)
        {
            if (btnTrial) btnTrial.interactable = interactable;
            if (btnRestore) btnRestore.interactable = interactable;
            if (btnMonthly) btnMonthly.interactable = interactable;
            if (btnYearly) btnYearly.interactable = interactable;
        }
    }
}
