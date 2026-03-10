using UnityEngine;
using UnityEngine.UI;
using FairyTales.UI.Core;

namespace FairyTales.UI.Payment
{
    public class PaymentScreen : BaseScreen
    {
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
            _screens.Show<Library.LibraryScreen>();
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
