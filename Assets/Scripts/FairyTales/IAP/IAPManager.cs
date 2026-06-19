using System;
using System.Text;
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

        /// <summary>
        /// True when the user has an active premium subscription.
        /// Persisted to PlayerPrefs so the flag survives app restarts
        /// even before IAP finishes initialising.
        /// </summary>
        public bool IsSubscribed
        {
            get => _isSubscribed;
            set
            {
                bool changed = _isSubscribed != value;
                _isSubscribed = value;
                PlayerPrefs.SetInt("ft_premium", value ? 1 : 0);
                PlayerPrefs.Save();
                Debug.Log($"[IAP-DBG] IsSubscribed persisted: {value}");
                if (changed) OnSubscriptionChanged?.Invoke(value);
            }
        }
        private bool _isSubscribed;

        public event Action<bool> OnSubscriptionChanged;
        public event Action<string> PurchaseFailed;

        private IStoreController _store;
        private IExtensionProvider _extensions;
        private Action<bool> _purchaseCallback;

        private void Awake()
        {
            Debug.Log("[IAP-DBG] ===== IAPManager.Awake() START =====");
            Debug.Log($"[IAP-DBG] Instance={Instance}, this={GetInstanceID()}, gameObject={gameObject.name}");

            if (Instance != null && Instance != this)
            {
                Debug.LogWarning($"[IAP-DBG] Duplicate IAPManager detected! Existing={Instance.GetInstanceID()}, destroying this={GetInstanceID()}");
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Restore cached premium state so features work immediately,
            // even before IAP store finishes initialising.
            _isSubscribed = PlayerPrefs.GetInt("ft_premium", 0) == 1;
            Debug.Log($"[IAP-DBG] IAPManager singleton set, cached IsSubscribed={_isSubscribed}");
            InitStore();
        }

        private void InitStore()
        {
            Debug.Log("[IAP-DBG] ===== InitStore() START =====");
            try
            {
                Debug.Log("[IAP-DBG] Creating StandardPurchasingModule...");
                var module = StandardPurchasingModule.Instance();
                Debug.Log($"[IAP-DBG] StandardPurchasingModule created, useFakeStoreUIMode={module.useFakeStoreUIMode}, useFakeStoreAlways={module.useFakeStoreAlways}");

                var builder = ConfigurationBuilder.Instance(module);
                Debug.Log("[IAP-DBG] ConfigurationBuilder created");

                builder.AddProduct(ProductMonthly, ProductType.Subscription);
                Debug.Log($"[IAP-DBG] Added product: {ProductMonthly} (Subscription)");

                builder.AddProduct(ProductYearly, ProductType.Subscription);
                Debug.Log($"[IAP-DBG] Added product: {ProductYearly} (Subscription)");

                Debug.Log("[IAP-DBG] Calling UnityPurchasing.Initialize()...");
                UnityPurchasing.Initialize(this, builder);
                Debug.Log("[IAP-DBG] UnityPurchasing.Initialize() called (async, waiting for callback)");
            }
            catch (Exception e)
            {
                Debug.LogError($"[IAP-DBG] EXCEPTION in InitStore(): {e.GetType().Name}: {e.Message}\n{e.StackTrace}");
            }
        }

        // ── IStoreListener ──────────────────────────────────

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            Debug.Log("[IAP-DBG] ===== OnInitialized() SUCCESS =====");
            _store = controller;
            _extensions = extensions;

            // Log all products info
            var sb = new StringBuilder();
            sb.AppendLine("[IAP-DBG] === Product catalog ===");
            foreach (var product in controller.products.all)
            {
                sb.AppendLine($"  ID: {product.definition.id}");
                sb.AppendLine($"    storeSpecificId: {product.definition.storeSpecificId}");
                sb.AppendLine($"    type: {product.definition.type}");
                sb.AppendLine($"    available: {product.availableToPurchase}");
                sb.AppendLine($"    hasReceipt: {product.hasReceipt}");
                if (product.metadata != null)
                {
                    sb.AppendLine($"    localizedTitle: {product.metadata.localizedTitle}");
                    sb.AppendLine($"    localizedPrice: {product.metadata.localizedPriceString}");
                    sb.AppendLine($"    isoCurrencyCode: {product.metadata.isoCurrencyCode}");
                    sb.AppendLine($"    localizedDesc: {product.metadata.localizedDescription}");
                }
                else
                {
                    sb.AppendLine("    metadata: NULL");
                }
                if (product.hasReceipt)
                {
                    sb.AppendLine($"    receipt(first 100): {product.receipt?.Substring(0, Math.Min(100, product.receipt?.Length ?? 0))}...");
                }
            }
            Debug.Log(sb.ToString());

            CheckSubscription();
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            Debug.LogError($"[IAP-DBG] ===== OnInitializeFailed() =====");
            Debug.LogError($"[IAP-DBG] Reason: {error}");
            Debug.LogError($"[IAP-DBG] Platform: {Application.platform}");
            Debug.LogError($"[IAP-DBG] ProductMonthly={ProductMonthly}, ProductYearly={ProductYearly}");
            Debug.LogError($"[IAP-DBG] Hint: Check that products exist in App Store Connect with EXACT IDs");
            Debug.LogError($"[IAP-DBG] Hint: Ensure 'In-App Purchase' capability is enabled in Xcode");
            Debug.LogError($"[IAP-DBG] Hint: Ensure 'Paid Applications' agreement is signed in App Store Connect");
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            Debug.LogError($"[IAP-DBG] ===== OnInitializeFailed(with message) =====");
            Debug.LogError($"[IAP-DBG] Reason: {error}");
            Debug.LogError($"[IAP-DBG] Message: {message}");
            Debug.LogError($"[IAP-DBG] Platform: {Application.platform}");
            Debug.LogError($"[IAP-DBG] ProductMonthly={ProductMonthly}, ProductYearly={ProductYearly}");
            Debug.LogError($"[IAP-DBG] Hint: Check that products exist in App Store Connect with EXACT IDs");
            Debug.LogError($"[IAP-DBG] Hint: Ensure 'In-App Purchase' capability is enabled in Xcode");
            Debug.LogError($"[IAP-DBG] Hint: Ensure 'Paid Applications' agreement is signed in App Store Connect");
            Debug.LogError($"[IAP-DBG] Hint: If 'NoProductsAvailable' — products may be in 'Missing Metadata' status");
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            var product = args.purchasedProduct;
            Debug.Log($"[IAP-DBG] ===== ProcessPurchase() =====");
            Debug.Log($"[IAP-DBG] Product: {product.definition.id}");
            Debug.Log($"[IAP-DBG] TransactionID: {product.transactionID}");
            Debug.Log($"[IAP-DBG] Receipt length: {product.receipt?.Length ?? 0}");
            Debug.Log($"[IAP-DBG] Receipt(first 200): {product.receipt?.Substring(0, Math.Min(200, product.receipt?.Length ?? 0))}");

            var receipt = product.receipt;
            Debug.Log("[IAP-DBG] Starting receipt validation...");
            ValidateReceipt(receipt, valid =>
            {
                Debug.Log($"[IAP-DBG] ValidateReceipt callback: valid={valid}");
                if (!valid)
                {
                    Debug.LogWarning("[IAP-DBG] Server validation failed — trusting local receipt");
                    valid = true;
                }
                IsSubscribed = valid;
                Debug.Log($"[IAP-DBG] IsSubscribed set to: {IsSubscribed}");
                Debug.Log($"[IAP-DBG] _purchaseCallback is null: {_purchaseCallback == null}");
                _purchaseCallback?.Invoke(valid);
                _purchaseCallback = null;
            });

            Debug.Log("[IAP-DBG] Returning PurchaseProcessingResult.Complete");
            return PurchaseProcessingResult.Complete;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason reason)
        {
            Debug.LogError($"[IAP-DBG] ===== OnPurchaseFailed(reason) =====");
            Debug.LogError($"[IAP-DBG] Product: {product.definition.id}");
            Debug.LogError($"[IAP-DBG] Reason: {reason}");
            Debug.LogError($"[IAP-DBG] Product available: {product.availableToPurchase}");
            Debug.LogError($"[IAP-DBG] Product hasReceipt: {product.hasReceipt}");
            PurchaseFailed?.Invoke(reason.ToString());
            _purchaseCallback?.Invoke(false);
            _purchaseCallback = null;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureDescription desc)
        {
            Debug.LogError($"[IAP-DBG] ===== OnPurchaseFailed(description) =====");
            Debug.LogError($"[IAP-DBG] Product: {product.definition.id}");
            Debug.LogError($"[IAP-DBG] Reason: {desc.reason}");
            Debug.LogError($"[IAP-DBG] Message: {desc.message}");
            Debug.LogError($"[IAP-DBG] Product available: {product.availableToPurchase}");
            Debug.LogError($"[IAP-DBG] Product hasReceipt: {product.hasReceipt}");
            PurchaseFailed?.Invoke(desc.message);
            _purchaseCallback?.Invoke(false);
            _purchaseCallback = null;
        }

        // ── Public API ──────────────────────────────────────

        public void Purchase(string productId, Action<bool> callback = null)
        {
            Debug.Log($"[IAP-DBG] ===== Purchase() called =====");
            Debug.Log($"[IAP-DBG] productId={productId}");
            Debug.Log($"[IAP-DBG] _store is null: {_store == null}");
            Debug.Log($"[IAP-DBG] IsInitialized: {IsInitialized}");

            if (_store == null)
            {
                Debug.LogError("[IAP-DBG] Store NOT initialized! Cannot purchase.");
                Debug.LogError("[IAP-DBG] Hint: Was OnInitializeFailed called earlier? Check logs above.");
                callback?.Invoke(false);
                return;
            }

            var product = _store.products.WithID(productId);
            Debug.Log($"[IAP-DBG] Product found: {product != null}");
            if (product != null)
            {
                Debug.Log($"[IAP-DBG] Product available: {product.availableToPurchase}");
                Debug.Log($"[IAP-DBG] Product price: {product.metadata?.localizedPriceString}");
                Debug.Log($"[IAP-DBG] Product hasReceipt: {product.hasReceipt}");
            }

            _purchaseCallback = callback;
            Debug.Log($"[IAP-DBG] Calling _store.InitiatePurchase({productId})...");
            _store.InitiatePurchase(productId);
        }

        public void RestorePurchases(Action<bool> callback = null)
        {
            Debug.Log("[IAP-DBG] ===== RestorePurchases() called =====");
            Debug.Log($"[IAP-DBG] Platform: {Application.platform}");
            Debug.Log($"[IAP-DBG] _extensions is null: {_extensions == null}");

#if UNITY_IOS
            Debug.Log("[IAP-DBG] iOS path — calling RestoreTransactions...");
            try
            {
                _extensions.GetExtension<IAppleExtensions>()
                    .RestoreTransactions((success, error) =>
                    {
                        Debug.Log($"[IAP-DBG] RestoreTransactions callback: success={success}, error={error}");
                        if (success) CheckSubscription();
                        callback?.Invoke(success);
                    });
            }
            catch (Exception e)
            {
                Debug.LogError($"[IAP-DBG] RestoreTransactions EXCEPTION: {e.GetType().Name}: {e.Message}\n{e.StackTrace}");
                callback?.Invoke(false);
            }
#elif UNITY_ANDROID
            Debug.Log("[IAP-DBG] Android path — Google Play restores on init");
            CheckSubscription();
            callback?.Invoke(true);
#else
            Debug.Log("[IAP-DBG] Unsupported platform for restore");
            callback?.Invoke(false);
#endif
        }

        public string GetLocalizedPrice(string productId)
        {
            Debug.Log($"[IAP-DBG] GetLocalizedPrice({productId}), _store null={_store == null}");
            if (_store == null)
            {
                Debug.LogWarning($"[IAP-DBG] GetLocalizedPrice: store is null, returning null");
                return null;
            }
            var product = _store.products.WithID(productId);
            var price = product?.metadata.localizedPriceString;
            Debug.Log($"[IAP-DBG] GetLocalizedPrice({productId}): product found={product != null}, price={price}");
            return price;
        }

        public decimal GetDecimalPrice(string productId)
        {
            if (_store == null) return 0m;
            var product = _store.products.WithID(productId);
            return product?.metadata.localizedPrice ?? 0m;
        }

        public string GetIsoCurrencyCode(string productId)
        {
            if (_store == null) return null;
            var product = _store.products.WithID(productId);
            return product?.metadata.isoCurrencyCode;
        }

        public bool HasTrialAvailable(string productId)
        {
            Debug.Log($"[IAP-DBG] HasTrialAvailable({productId})");
            if (_store == null)
            {
                Debug.LogWarning("[IAP-DBG] HasTrialAvailable: store is null");
                return false;
            }
            var product = _store.products.WithID(productId);
            if (product == null || !product.availableToPurchase)
            {
                Debug.LogWarning($"[IAP-DBG] HasTrialAvailable: product null={product == null}, available={product?.availableToPurchase}");
                return false;
            }

            try
            {
                Debug.Log($"[IAP-DBG] HasTrialAvailable: checking introductory price info...");
                Debug.Log($"[IAP-DBG] HasTrialAvailable: receipt={product.receipt?.Substring(0, Math.Min(100, product.receipt?.Length ?? 0))}");

                // On iOS, SubscriptionManager needs the Apple product's intro JSON.
                // Passing null can cause exceptions on real devices.
                var introJson = (string)null;
#if UNITY_IOS
                if (_extensions != null)
                {
                    var appleExt = _extensions.GetExtension<IAppleExtensions>();
                    var dict = appleExt.GetIntroductoryPriceDictionary();
                    if (dict != null && dict.ContainsKey(product.definition.storeSpecificId))
                    {
                        introJson = dict[product.definition.storeSpecificId];
                        Debug.Log($"[IAP-DBG] HasTrialAvailable: introJson={introJson}");
                    }
                    else
                    {
                        Debug.LogWarning($"[IAP-DBG] HasTrialAvailable: No intro price info for {product.definition.storeSpecificId}. Dict keys: {(dict != null ? string.Join(", ", dict.Keys) : "null")}");
                    }
                }
#endif

                if (string.IsNullOrEmpty(product.receipt))
                {
                    Debug.Log("[IAP-DBG] HasTrialAvailable: no receipt — trial available");
                    return true;
                }

                var sub = new SubscriptionManager(product, introJson);
                var info = sub.getSubscriptionInfo();
                var isTrial = info.isFreeTrial();
                Debug.Log($"[IAP-DBG] HasTrialAvailable: isFreeTrial={isTrial}, isSubscribed={info.isSubscribed()}, isExpired={info.isExpired()}, remainingTime={info.getRemainingTime()}");
                return isTrial == Result.True;
            }
            catch (Exception e)
            {
                Debug.LogError($"[IAP-DBG] HasTrialAvailable EXCEPTION: {e.GetType().Name}: {e.Message}\n{e.StackTrace}");
                return false;
            }
        }

        // ── Internal ────────────────────────────────────────

        private void CheckSubscription()
        {
            Debug.Log("[IAP-DBG] ===== CheckSubscription() =====");
            if (_store == null)
            {
                Debug.LogWarning("[IAP-DBG] CheckSubscription: store is null, aborting");
                return;
            }

            bool subscribed = false;
            foreach (var id in new[] { ProductMonthly, ProductYearly })
            {
                var product = _store.products.WithID(id);
                Debug.Log($"[IAP-DBG] CheckSub [{id}]: found={product != null}, hasReceipt={product?.hasReceipt}");
                if (product == null || !product.hasReceipt)
                {
                    Debug.Log($"[IAP-DBG] CheckSub [{id}]: skipping (no product or no receipt)");
                    continue;
                }

                try
                {
                    Debug.Log($"[IAP-DBG] CheckSub [{id}]: receipt length={product.receipt?.Length}");

                    // Get intro JSON for iOS to avoid null-pointer in SubscriptionManager
                    var introJson = (string)null;
#if UNITY_IOS
                    if (_extensions != null)
                    {
                        try
                        {
                            var appleExt = _extensions.GetExtension<IAppleExtensions>();
                            var dict = appleExt.GetIntroductoryPriceDictionary();
                            if (dict != null && dict.ContainsKey(product.definition.storeSpecificId))
                            {
                                introJson = dict[product.definition.storeSpecificId];
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.LogWarning($"[IAP-DBG] CheckSub: failed to get intro json: {ex.Message}");
                        }
                    }
#endif

                    var sub = new SubscriptionManager(product, introJson);
                    var info = sub.getSubscriptionInfo();
                    var isSubbed = info.isSubscribed();
                    Debug.Log($"[IAP-DBG] CheckSub [{id}]: isSubscribed={isSubbed}, isExpired={info.isExpired()}, isCancelled={info.isCancelled()}, remainingTime={info.getRemainingTime()}, purchaseDate={info.getPurchaseDate()}, expireDate={info.getExpireDate()}");

                    if (isSubbed == Result.True)
                    {
                        subscribed = true;
                        Debug.Log($"[IAP-DBG] CheckSub [{id}]: ACTIVE subscription found!");
                        break;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[IAP-DBG] CheckSub [{id}] EXCEPTION: {e.GetType().Name}: {e.Message}\n{e.StackTrace}");
                }
            }

            Debug.Log($"[IAP-DBG] CheckSubscription result: wasSubscribed={IsSubscribed}, nowSubscribed={subscribed}");
            if (IsSubscribed != subscribed)
            {
                IsSubscribed = subscribed;
                Debug.Log($"[IAP-DBG] Subscription state CHANGED to: {subscribed}");
            }
        }

        private void ValidateReceipt(string receipt, Action<bool> callback)
        {
            Debug.Log($"[IAP-DBG] ===== ValidateReceipt() =====");
            Debug.Log($"[IAP-DBG] Receipt length: {receipt?.Length ?? 0}");

            var service = FindAnyObjectByType<SubscriptionService>();
            Debug.Log($"[IAP-DBG] SubscriptionService found: {service != null}");

            if (service != null)
            {
                Debug.Log("[IAP-DBG] Delegating to SubscriptionService.ValidateReceipt()");
                service.ValidateReceipt(receipt, callback);
                return;
            }

            Debug.LogWarning("[IAP-DBG] No SubscriptionService found — skipping server validation, trusting local receipt");
            callback?.Invoke(true);
        }
    }
}
