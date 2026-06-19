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
            Debug.Log("[IAP-DBG] SubscriptionService.Awake()");
            _api = FindAnyObjectByType<ApiClient>();
            Debug.Log($"[IAP-DBG] SubscriptionService: ApiClient found={_api != null}");
            if (_api == null)
            {
                Debug.LogError("[IAP-DBG] SubscriptionService: ApiClient NOT FOUND! Server validation will fail.");
            }
        }

        public void ValidateReceipt(string receipt, Action<bool> callback)
        {
            Debug.Log($"[IAP-DBG] SubscriptionService.ValidateReceipt() called, receipt length={receipt?.Length ?? 0}");
            if (_api == null)
            {
                Debug.LogError("[IAP-DBG] SubscriptionService.ValidateReceipt: _api is null! Returning false.");
                callback?.Invoke(false);
                return;
            }
            StartCoroutine(ValidateCoroutine(receipt, callback));
        }

        public void CheckStatus(Action<bool> callback)
        {
            Debug.Log("[IAP-DBG] SubscriptionService.CheckStatus() called");
            if (_api == null)
            {
                Debug.LogError("[IAP-DBG] SubscriptionService.CheckStatus: _api is null! Returning false.");
                callback?.Invoke(false);
                return;
            }
            StartCoroutine(CheckStatusCoroutine(callback));
        }

        private IEnumerator ValidateCoroutine(string receipt, Action<bool> callback)
        {
            var platform = "google";
#if UNITY_IOS
            platform = "apple";
#endif
            Debug.Log($"[IAP-DBG] ValidateCoroutine: platform={platform}");

            var body = JsonUtility.ToJson(new ReceiptPayload
            {
                receipt = receipt,
                platform = platform
            });
            Debug.Log($"[IAP-DBG] ValidateCoroutine: body length={body.Length}");
            Debug.Log($"[IAP-DBG] ValidateCoroutine: POSTing to /api/subscription/validate ...");

            bool? result = null;

            yield return _api.PostJson("/api/subscription/validate", body,
                onSuccess: response =>
                {
                    Debug.Log($"[IAP-DBG] ValidateCoroutine: SUCCESS response={response}");
                    result = true;
                },
                onError: e =>
                {
                    Debug.LogError($"[IAP-DBG] ValidateCoroutine: ERROR: {e}");
                    result = false;
                });

            Debug.Log($"[IAP-DBG] ValidateCoroutine: final result={result}");
            callback?.Invoke(result ?? false);
        }

        private IEnumerator CheckStatusCoroutine(Action<bool> callback)
        {
            Debug.Log("[IAP-DBG] CheckStatusCoroutine: GETting /api/subscription/status ...");
            bool? result = null;

            yield return _api.Get("/api/subscription/status",
                onSuccess: json =>
                {
                    Debug.Log($"[IAP-DBG] CheckStatusCoroutine: SUCCESS json={json}");
                    var status = JsonUtility.FromJson<StatusResponse>(json);
                    result = status.active;
                    Debug.Log($"[IAP-DBG] CheckStatusCoroutine: active={status.active}");
                },
                onError: e =>
                {
                    Debug.LogError($"[IAP-DBG] CheckStatusCoroutine: ERROR: {e}");
                    result = false;
                });

            Debug.Log($"[IAP-DBG] CheckStatusCoroutine: final result={result}");
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
