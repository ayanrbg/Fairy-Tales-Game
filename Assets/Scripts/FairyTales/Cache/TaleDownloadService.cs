using System;
using System.Collections;
using FairyTales.Api;
using FairyTales.Models;
using Newtonsoft.Json;
using UnityEngine;

namespace FairyTales.Cache
{
    public class TaleDownloadService
    {
        private readonly ApiClient _api;
        private long _bytesDownloaded;

        public long BytesDownloaded => _bytesDownloaded;

        public TaleDownloadService(ApiClient api) => _api = api;

        public IEnumerator DownloadCover(TaleSummary tale, Action<string> onError = null)
        {
            if (AssetCache.IsCoverDownloaded(tale.id)) yield break;

            yield return DownloadImage(
                $"/api/tales/{tale.id}/cover",
                AssetCache.CoverKey(tale.id), onError);
        }

        /// <summary>
        /// Downloads illustrations + default narration for a tale.
        /// onProgress(step, totalSteps) reports per-file progress.
        /// BytesDownloaded tracks total bytes for display.
        /// </summary>
        public IEnumerator DownloadTaleContent(TaleSummary tale,
            Action<int, int> onProgress, Action<string> onError = null)
        {
            _bytesDownloaded = 0;

            if (AssetCache.IsTaleDownloaded(tale.id))
            {
                onProgress?.Invoke(1, 1);
                yield break;
            }

            var id = tale.id;
            var lang = tale.lang;

            TaleDetail detail = null;
            yield return _api.Get($"/api/tales/{id}?lang={lang}",
                json => detail = JsonConvert.DeserializeObject<TaleDetail>(json),
                e => onError?.Invoke(e));

            if (detail == null) yield break;

            int totalSteps = detail.totalPages;
            if (tale.hasDefaultNarration) totalSteps += detail.totalPages;
            int step = 0;

            // Illustrations
            for (int p = 0; p < detail.totalPages; p++)
            {
                yield return DownloadImage(
                    $"/api/tales/{id}/illustration/{p}",
                    AssetCache.IllustrationKey(id, p), onError,
                    $"Illustrations/{id}/page_{p}");
                onProgress?.Invoke(++step, totalSteps);
            }

            // Default narration
            if (tale.hasDefaultNarration)
            {
                for (int p = 0; p < detail.totalPages; p++)
                {
                    yield return DownloadFile(
                        $"/api/tales/{id}/default-narration/{p}?lang={lang}",
                        AssetCache.NarrationKey(id, lang, p), onError);
                    onProgress?.Invoke(++step, totalSteps);
                }
            }

            AssetCache.MarkTaleReady(id);
        }

        /// Downloads default narration audio for a tale.
        /// Uses /default-narration?lang= to get available pages, then downloads each.
        public IEnumerator DownloadDefaultNarration(string taleId, string lang,
            Action<int, int> onProgress, Action<string> onError = null)
        {
            // Check which pages are available
            DefaultNarrationInfo info = null;
            yield return _api.Get($"/api/tales/{taleId}/default-narration?lang={lang}",
                json => info = JsonConvert.DeserializeObject<DefaultNarrationInfo>(json),
                e => Debug.LogWarning($"[Download] narration info {taleId}: {e}"));

            if (info == null || !info.available || info.pages == null || info.pages.Length == 0)
                yield break;

            for (int i = 0; i < info.pages.Length; i++)
            {
                int page = info.pages[i];
                yield return DownloadFile(
                    $"/api/tales/{taleId}/default-narration/{page}?lang={lang}",
                    AssetCache.NarrationKey(taleId, lang, page), onError);
                onProgress?.Invoke(i + 1, info.pages.Length);
            }
        }

        /// Downloads image as raw bytes and saves directly to cache.
        /// Server sends optimized JPEG — no re-encoding needed.
        private IEnumerator DownloadImage(string endpoint, string key,
            Action<string> onError, string resourcePath = null)
        {
            if (AssetCache.Exists(key)) yield break;
            if (resourcePath != null && Resources.Load<Sprite>(resourcePath) != null)
                yield break;

            yield return _api.GetBytes(endpoint,
                bytes =>
                {
                    AssetCache.Save(key, bytes);
                    _bytesDownloaded += bytes.Length;
                },
                e =>
                {
                    Debug.LogWarning($"[Download] {endpoint}: {e}");
                    onError?.Invoke(e);
                });
        }

        private IEnumerator DownloadFile(string endpoint, string key,
            Action<string> onError)
        {
            if (AssetCache.Exists(key)) yield break;

            yield return _api.GetBytes(endpoint,
                bytes =>
                {
                    AssetCache.Save(key, bytes);
                    _bytesDownloaded += bytes.Length;
                },
                e =>
                {
                    Debug.LogWarning($"[Download] {endpoint}: {e}");
                    onError?.Invoke(e);
                });
        }
    }
}
