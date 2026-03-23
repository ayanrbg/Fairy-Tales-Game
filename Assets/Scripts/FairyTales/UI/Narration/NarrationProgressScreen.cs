using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FairyTales.Api;
using FairyTales.Models;
using FairyTales.UI.Core;
using FairyTales.UI.Library;

namespace FairyTales.UI.Narration
{
    public class NarrationProgressScreen : BaseScreen
    {
        [SerializeField] private Slider progressBar;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private TMP_Text pagesText;
        [SerializeField] private Button btnDone;
        [SerializeField] private Button btnBack;

        [SerializeField] private float pollInterval = 3f;

        private ScreenManager _screens;
        private NarrationService _narration;
        private TaleSummary _tale;
        private Coroutine _polling;

        private void Awake()
        {
            _screens = GetComponentInParent<ScreenManager>();
            var api = FindAnyObjectByType<ApiClient>();
            _narration = new NarrationService(api);

            if (btnDone) btnDone.onClick.AddListener(OnDone);
            if (btnBack) btnBack.onClick.AddListener(OnBack);
        }

        public void SetContext(TaleSummary tale) => _tale = tale;

        protected override void OnPrepare()
        {
            if (btnDone) btnDone.gameObject.SetActive(false);
            if (progressBar) progressBar.value = 0f;
            if (statusText) statusText.text = "Идёт озвучка...";
            if (pagesText) pagesText.text = "";
        }

        protected override void OnShown()
        {
            _polling = StartCoroutine(PollStatus());
        }

        protected override void OnHidden()
        {
            if (_polling != null)
            {
                StopCoroutine(_polling);
                _polling = null;
            }
        }

        private IEnumerator PollStatus()
        {
            while (true)
            {
                bool done = false;
                bool error = false;

                yield return _narration.GetNarrationStatus(_tale.id,
                    onSuccess: s =>
                    {
                        float progress = s.totalPages > 0
                            ? (float)s.pagesReady / s.totalPages
                            : 0f;

                        if (progressBar) progressBar.value = progress;
                        if (pagesText)
                            pagesText.text = $"{s.pagesReady} / {s.totalPages}";

                        if (s.status == "done")
                        {
                            done = true;
                            if (statusText) statusText.text = "Озвучка завершена!";
                        }
                        else if (s.status == "error")
                        {
                            error = true;
                            if (statusText) statusText.text = "Ошибка озвучки";
                        }
                    },
                    onError: e =>
                    {
                        Debug.LogError($"[NarrationProgress] {e}");
                        error = true;
                        if (statusText) statusText.text = "Ошибка связи";
                    });

                if (done)
                {
                    if (btnDone) btnDone.gameObject.SetActive(true);
                    yield break;
                }

                if (error) yield break;

                yield return new WaitForSeconds(pollInterval);
            }
        }

        private void OnDone()
        {
            // Back to TaleDetailScreen — it will re-check narration status
            _screens.Show<TaleDetailScreen>();
        }

        private void OnBack() => _screens.Show<TaleDetailScreen>();
    }
}
