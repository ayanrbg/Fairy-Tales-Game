using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
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

        private ScreenManager _screens;
        private bool _yearlySelected = true;

        private void Awake()
        {
            _screens = GetComponentInParent<ScreenManager>();

            if (background) SetBgAlpha(0f);
            if (btnClose) btnClose.onClick.AddListener(OnClose);
            if (btnMonthly) btnMonthly.onClick.AddListener(() => SelectPlan(false));
            if (btnYearly) btnYearly.onClick.AddListener(() => SelectPlan(true));
            if (btnTrial) btnTrial.onClick.AddListener(OnTrial);
            if (btnTerms) btnTerms.onClick.AddListener(OnTerms);
            if (btnRestore) btnRestore.onClick.AddListener(OnRestore);
            if (btnPrivacy) btnPrivacy.onClick.AddListener(OnPrivacy);
        }

        protected override void OnShown()
        {
            SelectPlan(true);
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
        }

        private void OnTrial()
        {
            // TODO: IAP integration
            var plan = _yearlySelected
                ? Loc.Get("plan_yearly")
                : Loc.Get("plan_monthly");
            Toast.Show($"{plan} — {Loc.Get("coming_soon")}");
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

        private void OnTerms()
        {
            Application.OpenURL("https://example.com/terms");
        }

        private void OnRestore()
        {
            Toast.Show(Loc.Get("restore_coming_soon"));
        }

        private void OnPrivacy()
        {
            Application.OpenURL("https://example.com/privacy");
        }
    }
}
