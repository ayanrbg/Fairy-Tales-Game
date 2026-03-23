using System;
using System.Text;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace FairyTales.Api
{
    public class ApiClient : MonoBehaviour
    {
        [SerializeField] private string baseUrl = "http://localhost:3000";

        private string _token;

        public string BaseUrl => baseUrl;
        public bool HasToken => !string.IsNullOrEmpty(_token);

        public void SetToken(string token) => _token = token;

        // ── GET (JSON) ───────────────────────────────────
        public IEnumerator Get(string endpoint,
            Action<string> onSuccess, Action<string> onError = null)
        {
            using var request = UnityWebRequest.Get($"{baseUrl}{endpoint}");
            ApplyAuth(request);
            yield return request.SendWebRequest();
            HandleResponse(request, onSuccess, onError);
        }

        // ── GET (binary — audio, images, any raw bytes) ──
        public IEnumerator GetBytes(string endpoint,
            Action<byte[]> onSuccess, Action<string> onError = null)
        {
            using var request = UnityWebRequest.Get($"{baseUrl}{endpoint}");
            ApplyAuth(request);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
                onSuccess?.Invoke(request.downloadHandler.data);
            else
                onError?.Invoke(FormatError(request));
        }

        // ── POST (JSON) ──────────────────────────────────
        public IEnumerator PostJson(string endpoint, string jsonBody,
            Action<string> onSuccess, Action<string> onError = null)
        {
            using var request = new UnityWebRequest($"{baseUrl}{endpoint}", "POST");
            var bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            ApplyAuth(request);
            yield return request.SendWebRequest();
            HandleResponse(request, onSuccess, onError);
        }

        // ── POST (empty body — for narrate) ──────────────
        public IEnumerator Post(string endpoint,
            Action<string> onSuccess, Action<string> onError = null)
        {
            using var request = new UnityWebRequest($"{baseUrl}{endpoint}", "POST");
            request.downloadHandler = new DownloadHandlerBuffer();
            ApplyAuth(request);
            yield return request.SendWebRequest();
            HandleResponse(request, onSuccess, onError);
        }

        // ── POST (empty body → binary audio) ─────────────
        public IEnumerator PostForAudio(string endpoint,
            Action<byte[]> onSuccess, Action<string> onError = null)
        {
            using var request = new UnityWebRequest($"{baseUrl}{endpoint}", "POST");
            request.downloadHandler = new DownloadHandlerBuffer();
            ApplyAuth(request);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
                onSuccess?.Invoke(request.downloadHandler.data);
            else
                onError?.Invoke(FormatError(request));
        }

        // ── POST (multipart — voice upload) ──────────────
        public IEnumerator PostMultipart(string endpoint,
            byte[] fileData, string fileName, string fieldName, string mimeType,
            Action<string> onSuccess, Action<string> onError = null)
        {
            var form = new WWWForm();
            form.AddBinaryData(fieldName, fileData, fileName, mimeType);

            using var request = UnityWebRequest.Post($"{baseUrl}{endpoint}", form);
            ApplyAuth(request);
            yield return request.SendWebRequest();
            HandleResponse(request, onSuccess, onError);
        }

        // ── PUT (JSON) ───────────────────────────────────
        public IEnumerator PutJson(string endpoint, string jsonBody,
            Action<string> onSuccess, Action<string> onError = null)
        {
            using var request = new UnityWebRequest($"{baseUrl}{endpoint}", "PUT");
            var bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            ApplyAuth(request);
            yield return request.SendWebRequest();
            HandleResponse(request, onSuccess, onError);
        }

        // ── DELETE ────────────────────────────────────────
        public IEnumerator Delete(string endpoint,
            Action<string> onSuccess, Action<string> onError = null)
        {
            using var request = UnityWebRequest.Delete($"{baseUrl}{endpoint}");
            request.downloadHandler = new DownloadHandlerBuffer();
            ApplyAuth(request);
            yield return request.SendWebRequest();
            HandleResponse(request, onSuccess, onError);
        }

        // ── Helpers ───────────────────────────────────────
        private void ApplyAuth(UnityWebRequest request)
        {
            if (!string.IsNullOrEmpty(_token))
                request.SetRequestHeader("Authorization", $"Bearer {_token}");
        }

        private void HandleResponse(UnityWebRequest request,
            Action<string> onSuccess, Action<string> onError)
        {
            if (request.result == UnityWebRequest.Result.Success)
                onSuccess?.Invoke(request.downloadHandler.text);
            else
                onError?.Invoke(FormatError(request));
        }

        private string FormatError(UnityWebRequest request)
        {
            var body = request.downloadHandler?.text ?? "";
            return $"[{request.responseCode}] {request.error} — {body}";
        }
    }
}
