using System;
using System.Collections;
using UnityEngine;
using FairyTales.Api;

namespace FairyTales.IAP
{
    public class SubscriptionService : MonoBehaviour
    {
        private ApiClient _api;

        private void Awake()
        {
            _api = FindAnyObjectByType<ApiClient>();
        }

        public void ValidateReceipt(string receipt, Action<bool> callback)
        {
            StartCoroutine(ValidateCoroutine(receipt, callback));
        }

        public void CheckStatus(Action<bool> callback)
        {
            StartCoroutine(CheckStatusCoroutine(callback));
        }

        private IEnumerator ValidateCoroutine(string receipt, Action<bool> callback)
        {
            var platform = "google";
#if UNITY_IOS
            platform = "apple";
#endif

            var body = JsonUtility.ToJson(new ReceiptPayload
            {
                receipt = receipt,
                platform = platform
            });

            bool? result = null;

            yield return _api.PostJson("/api/subscription/validate", body,
                onSuccess: _ =>
                {
                    Debug.Log("[Subscription] Receipt validated");
                    result = true;
                },
                onError: e =>
                {
                    Debug.LogWarning($"[Subscription] Validation failed: {e}");
                    result = false;
                });

            callback?.Invoke(result ?? false);
        }

        private IEnumerator CheckStatusCoroutine(Action<bool> callback)
        {
            bool? result = null;

            yield return _api.Get("/api/subscription/status",
                onSuccess: json =>
                {
                    var status = JsonUtility.FromJson<StatusResponse>(json);
                    result = status.active;
                },
                onError: e =>
                {
                    Debug.LogWarning($"[Subscription] Status check failed: {e}");
                    result = false;
                });

            callback?.Invoke(result ?? false);
        }

        [Serializable]
        private class ReceiptPayload
        {
            public string receipt;
            public string platform;
        }

        [Serializable]
        private class StatusResponse
        {
            public bool active;
        }
    }
}
