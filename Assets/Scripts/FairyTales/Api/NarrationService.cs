using System;
using System.Collections;
using FairyTales.Models;
using UnityEngine;

namespace FairyTales.Api
{
    public class NarrationService
    {
        private readonly ApiClient _api;

        public NarrationService(ApiClient api) => _api = api;

        // ── Single page narration ────────────────────────
        public IEnumerator NarratePage(string taleId, int page,
            Action<byte[]> onSuccess, Action<string> onError = null)
        {
            yield return _api.PostForAudio(
                $"/api/tales/{taleId}/narrate?page={page}",
                onSuccess, onError
            );
        }

        // ── Full book narration (async) ──────────────────
        public IEnumerator NarrateAll(string taleId,
            Action<NarrateAllResponse> onSuccess, Action<string> onError = null)
        {
            yield return _api.Post($"/api/tales/{taleId}/narrate-all",
                json =>
                {
                    var response = JsonUtility.FromJson<NarrateAllResponse>(json);
                    onSuccess?.Invoke(response);
                },
                onError
            );
        }

        public IEnumerator GetNarrationStatus(string taleId,
            Action<NarrationStatusResponse> onSuccess, Action<string> onError = null)
        {
            yield return _api.Get($"/api/tales/{taleId}/narration-status",
                json =>
                {
                    var response = JsonUtility.FromJson<NarrationStatusResponse>(json);
                    onSuccess?.Invoke(response);
                },
                onError
            );
        }

        public IEnumerator DownloadNarratedPage(string taleId, int page,
            Action<byte[]> onSuccess, Action<string> onError = null)
        {
            yield return _api.GetAudio(
                $"/api/tales/{taleId}/narration/{page}",
                onSuccess, onError
            );
        }
    }
}
