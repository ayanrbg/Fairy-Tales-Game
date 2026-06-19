using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FairyTales.Api;
using FairyTales.Cache;
using FairyTales.Models;
using FairyTales.UI.Core;

namespace FairyTales.UI.Library
{
    public class TaleCard : MonoBehaviour
    {
        [SerializeField] private Image coverImage;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private Button button;

        [Header("Overlay (covers entire card)")]
        [SerializeField] private GameObject overlay;
        [SerializeField] private GameObject lockIcon;

        [Header("Coming Soon")]
        [SerializeField] private GameObject comingSoonBadge;
        [SerializeField] private TMP_Text comingSoonText;

        [Header("Download UI (inside overlay)")]
        [SerializeField] private Image circleFill;
        [SerializeField] private Image downloadArrow;
        [SerializeField] private TMP_Text sizeText;
        [SerializeField] private TMP_Text progressText;

        private TaleSummary _tale;
        private Action<TaleSummary> _onClick;
        private ApiClient _api;
        private bool _isLocked;
        private bool _isDownloaded;
        private bool _isDownloading;
        private bool _downloadFailed;
        private bool _isComingSoon;
        private Coroutine _downloadCoroutine;

        public bool IsDownloaded => _isDownloaded;

        public void Init(TaleSummary tale, bool isLocked, bool isDownloaded,
            ApiClient api, Action<TaleSummary> onClick)
        {
            CancelDownload();

            _tale = tale;
            _onClick = onClick;
            _api = api;
            _isComingSoon = tale.comingSoon;
            // Coming-soon tales are never lockable/downloadable — they have no pages.
            _isLocked = isLocked && !_isComingSoon;
            _isDownloaded = isDownloaded;
            _isDownloading = false;
            _downloadFailed = false;

            if (button) button.onClick.RemoveAllListeners();
            if (titleText) titleText.text = tale.GetTitle(Loc.Lang);

            var cached = CoverProvider.GetCached(tale.id);
            if (cached != null)
            {
                if (coverImage) coverImage.sprite = cached;
            }
            else
            {
                StartCoroutine(CoverProvider.GetAsync(tale.id, s =>
                {
                    if (s != null && coverImage) coverImage.sprite = s;
                }, _api));
            }

            if (button) button.onClick.AddListener(OnClick);
            UpdateOverlay();
        }

        /// <summary>
        /// Refresh download state without full re-init (called when returning to library).
        /// </summary>
        public void RefreshDownloadState()
        {
            if (_isDownloading || _isComingSoon) return;
            _isDownloaded = _tale != null && AssetCache.IsTaleDownloaded(_tale.id);
            UpdateOverlay();
        }

        private void UpdateOverlay()
        {
            // Coming soon: badge only, no lock/download UI.
            if (comingSoonBadge) comingSoonBadge.SetActive(_isComingSoon);
            if (_isComingSoon)
            {
                if (comingSoonText) comingSoonText.text = Loc.Get("coming_soon");
                if (overlay) overlay.SetActive(false);
                if (lockIcon) lockIcon.SetActive(false);
                return;
            }

            bool needsDownload = !_isDownloaded && !_isLocked;
            bool showOverlay = _isLocked || needsDownload;

            if (overlay) overlay.SetActive(showOverlay);

            // Lock
            if (lockIcon) lockIcon.SetActive(_isLocked);

            // Download: idle (circle full + arrow + size)
            bool idle = needsDownload && !_isDownloading;

            if (downloadArrow)
                downloadArrow.gameObject.SetActive(idle);

            if (sizeText)
            {
                sizeText.gameObject.SetActive(idle);
                if (idle && _tale != null && _tale.downloadSize > 0)
                {
                    float mb = _tale.downloadSize / (1024f * 1024f);
                    sizeText.text = mb >= 1f ? $"{mb:F1} MB" : $"{_tale.downloadSize / 1024f:F0} KB";
                }
            }

            // Download: in progress (circle fills + percentage)
            if (progressText)
                progressText.gameObject.SetActive(_isDownloading);

            if (circleFill)
            {
                circleFill.gameObject.SetActive(needsDownload);
                circleFill.fillAmount = idle ? 1f : 0f;
            }
        }

        private void OnClick()
        {
            // Coming soon → non-interactive (no open, no download, no narrate).
            if (_isComingSoon) return;

            // Downloading → cancel
            if (_isDownloading)
            {
                CancelDownload();
                _downloadFailed = false;
                UpdateOverlay();
                return;
            }

            // Locked → payment
            if (_isLocked)
            {
                _onClick?.Invoke(_tale);
                return;
            }

            // Not downloaded → start download (skip if last attempt just failed)
            if (!_isDownloaded)
            {
                if (_downloadFailed)
                {
                    _downloadFailed = false;
                    Toast.Show(Loc.Get("error"));
                    return;
                }
                BeginDownload();
                return;
            }

            // Downloaded → open
            _onClick?.Invoke(_tale);
        }

        private void BeginDownload()
        {
            if (_api == null) return;

            // Double-check: maybe it was downloaded since Init
            if (AssetCache.IsTaleDownloaded(_tale.id))
            {
                _isDownloaded = true;
                UpdateOverlay();
                _onClick?.Invoke(_tale);
                return;
            }

            _isDownloading = true;
            _downloadFailed = false;
            UpdateOverlay();
            _downloadCoroutine = StartCoroutine(DownloadRoutine());
        }

        private IEnumerator DownloadRoutine()
        {
            var downloader = new TaleDownloadService(_api);
            bool gotProgress = false;
            bool hadError = false;

            yield return downloader.DownloadTaleContent(_tale,
                (step, total) =>
                {
                    gotProgress = true;
                    float progress = (float)step / total;
                    if (progressText) progressText.text = $"{Mathf.RoundToInt(progress * 100)}%";
                    if (circleFill) circleFill.fillAmount = progress;
                },
                _ => hadError = true);

            _downloadCoroutine = null;
            _isDownloading = false;
            _isDownloaded = AssetCache.IsTaleDownloaded(_tale.id);

            if (!_isDownloaded)
            {
                _downloadFailed = true;
                // No progress at all = server didn't return tale data
                if (!gotProgress)
                    Toast.Show(Loc.Get("error"));
            }

            UpdateOverlay();
        }

        private void CancelDownload()
        {
            if (_downloadCoroutine != null)
            {
                StopCoroutine(_downloadCoroutine);
                _downloadCoroutine = null;
            }
            _isDownloading = false;
        }

        private void OnDestroy()
        {
            CancelDownload();
            if (button) button.onClick.RemoveListener(OnClick);
        }
    }
}
