using System;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

namespace FairyTales.IAP
{
    public class IAPManager : MonoBehaviour, IDetailedStoreListener
    {
        public const string ProductMonthly = "fairytales_monthly";
        public const string ProductYearly = "fairytales_yearly";

        public static IAPManager Instance { get; private set; }

        public bool IsInitialized => _store != null;
        public bool IsSubscribed { get; private set; }

        public event Action<bool> OnSubscriptionChanged;
        public event Action<string> PurchaseFailed;

        private IStoreController _store;
        private IExtensionProvider _extensions;
        private Action<bool> _purchaseCallback;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitStore();
        }

        private void InitStore()
        {
            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

            builder.AddProduct(ProductMonthly, ProductType.Subscription);
            builder.AddProduct(ProductYearly, ProductType.Subscription);

            UnityPurchasing.Initialize(this, builder);
        }

        // ── IStoreListener ──────────────────────────────────

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            _store = controller;
            _extensions = extensions;
            Debug.Log("[IAP] Initialized");
            CheckSubscription();
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            Debug.LogError($"[IAP] Init failed: {error}");
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            Debug.LogError($"[IAP] Init failed: {error} — {message}");
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            Debug.Log($"[IAP] Purchase success: {args.purchasedProduct.definition.id}");

            // Validate on server
            var receipt = args.purchasedProduct.receipt;
            ValidateReceipt(receipt, valid =>
            {
                IsSubscribed = valid;
                OnSubscriptionChanged?.Invoke(valid);
                _purchaseCallback?.Invoke(valid);
                _purchaseCallback = null;
            });

            return PurchaseProcessingResult.Complete;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason reason)
        {
            Debug.LogWarning($"[IAP] Purchase failed: {product.definition.id} — {reason}");
            PurchaseFailed?.Invoke(reason.ToString());
            _purchaseCallback?.Invoke(false);
            _purchaseCallback = null;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureDescription desc)
        {
            Debug.LogWarning($"[IAP] Purchase failed: {product.definition.id} — {desc.message}");
            PurchaseFailed?.Invoke(desc.message);
            _purchaseCallback?.Invoke(false);
            _purchaseCallback = null;
        }

        // ── Public API ──────────────────────────────────────

        public void Purchase(string productId, Action<bool> callback = null)
        {
            if (_store == null)
            {
                Debug.LogError("[IAP] Store not initialized");
                callback?.Invoke(false);
                return;
            }

            _purchaseCallback = callback;
            _store.InitiatePurchase(productId);
        }

        public void RestorePurchases(Action<bool> callback = null)
        {
#if UNITY_IOS
            _extensions.GetExtension<IAppleExtensions>()
                .RestoreTransactions((success, error) =>
                {
                    if (success) CheckSubscription();
                    callback?.Invoke(success);
                });
#elif UNITY_ANDROID
            // Google Play restores automatically on init
            CheckSubscription();
            callback?.Invoke(true);
#else
            callback?.Invoke(false);
#endif
        }

        public string GetLocalizedPrice(string productId)
        {
            if (_store == null) return null;
            var product = _store.products.WithID(productId);
            return product?.metadata.localizedPriceString;
        }

        public bool HasTrialAvailable(string productId)
        {
            if (_store == null) return false;
            var product = _store.products.WithID(productId);
            if (product == null || !product.availableToPurchase) return false;

            var sub = new SubscriptionManager(product, null);
            var info = sub.getSubscriptionInfo();
            return info.isFreeTrial() == Result.True;
        }

        // ── Internal ────────────────────────────────────────

        private void CheckSubscription()
        {
            if (_store == null) return;

            bool subscribed = false;
            foreach (var id in new[] { ProductMonthly, ProductYearly })
            {
                var product = _store.products.WithID(id);
                if (product == null || !product.hasReceipt) continue;

                try
                {
                    var sub = new SubscriptionManager(product, null);
                    var info = sub.getSubscriptionInfo();
                    if (info.isSubscribed() == Result.True)
                    {
                        subscribed = true;
                        break;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[IAP] Sub check error: {e.Message}");
                }
            }

            if (IsSubscribed != subscribed)
            {
                IsSubscribed = subscribed;
                OnSubscriptionChanged?.Invoke(subscribed);
            }
        }

        private void ValidateReceipt(string receipt, Action<bool> callback)
        {
            var service = FindAnyObjectByType<SubscriptionService>();
            if (service != null)
            {
                service.ValidateReceipt(receipt, callback);
                return;
            }

            // No server validation available — trust local receipt
            Debug.LogWarning("[IAP] No SubscriptionService found, skipping server validation");
            callback?.Invoke(true);
        }
    }
}
