using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FairyTales.Api;
using FairyTales.Audio;
using FairyTales.Models;
using FairyTales.UI.Core;

namespace FairyTales.UI.Reading
{
    public class RecordingOverlay : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private TMP_Text progressText;

        [Header("Record / Rerecord button")]
        [SerializeField] private Button btnRecord;
        [SerializeField] private GameObject iconRecord;
        [SerializeField] private GameObject iconRerecord;

        [Header("Done button")]
        [SerializeField] private Button btnDone;
        [SerializeField] private GameObject iconDoneEnabled;
        [SerializeField] private GameObject iconDoneDisabled;

        private const int RequiredPages = 3;

        private MicRecorder _mic;
        private VoiceService _voice;
        private NarrationService _narration;
        private ScreenManager _screens;
        private TaleSummary _taleSummary;
        private int _pagesWhileRecording;
        private byte[] _recordedAudio;
        private bool _sending;

        public bool IsActive => panel != null && panel.activeSelf;

        private void Awake()
        {
            var api = FindAnyObjectByType<ApiClient>();
            if (api != null)
            {
                _voice = new VoiceService(api);
                _narration = new NarrationService(api);
            }

            if (btnRecord) btnRecord.onClick.AddListener(OnRecord);
            if (btnDone) btnDone.onClick.AddListener(OnDone);
        }

        public void Activate(TaleSummary tale, ScreenManager screens)
        {
            _taleSummary = tale;
            _screens = screens;
            _mic = FindAnyObjectByType<MicRecorder>();
            _pagesWhileRecording = 0;
            _recordedAudio = null;
            _sending = false;

            if (panel) panel.SetActive(true);
            if (timerText) timerText.text = "00:00";
            if (statusText) statusText.text = Loc.Get("rec_hint");
            UpdateProgress();
            ShowRecordIcon(recording: false, hasAudio: false);
            UpdateDoneIcon();
        }

        public void Deactivate()
        {
            StopRecordingIfActive();
            if (panel) panel.SetActive(false);
        }

        public void OnPageVisited(int page)
        {
            if (_mic != null && _mic.IsRecording)
            {
                _pagesWhileRecording++;
                UpdateProgress();
            }
            UpdateDoneIcon();
        }

        private void UpdateProgress()
        {
            if (progressText)
                progressText.text = $"{Mathf.Min(_pagesWhileRecording, RequiredPages)}/{RequiredPages}";
        }

        private void Update()
        {
            if (_mic == null || !_mic.IsRecording) return;

            var elapsed = _mic.ElapsedTime;
            if (timerText)
                timerText.text = $"{(int)elapsed / 60:00}:{(int)elapsed % 60:00}";
        }

        private void OnRecord()
        {
            if (_mic == null) return;

            if (_mic.IsRecording)
            {
                StopMic();
            }
            else
            {
                _recordedAudio = null;
                _pagesWhileRecording = 0;
                if (timerText) timerText.text = "00:00";
                _mic.OnStopped -= OnRecordingStopped;
                _mic.OnStopped += OnRecordingStopped;
                _mic.StartRecording();
                if (statusText) statusText.text = Loc.Get("rec_recording");
                UpdateProgress();
                ShowRecordIcon(recording: true, hasAudio: false);
                UpdateDoneIcon();
            }
        }

        private void StopMic()
        {
            _mic.OnStopped -= OnRecordingStopped;
            _mic.OnStopped += OnRecordingStopped;
            _mic.StopRecording();
        }

        private void OnRecordingStopped(byte[] wavData)
        {
            _mic.OnStopped -= OnRecordingStopped;
            _recordedAudio = wavData;

            if (statusText)
                statusText.text = wavData != null
                    ? Loc.Get("rec_done")
                    : Loc.Get("rec_error");

            ShowRecordIcon(recording: false, hasAudio: wavData != null);
            UpdateDoneIcon();
        }

        private void OnDone()
        {
            if (_recordedAudio == null || _sending) return;
            StartCoroutine(CloneAndNarrate());
        }

        private IEnumerator CloneAndNarrate()
        {
            _sending = true;
            if (statusText) statusText.text = Loc.Get("rec_cloning");
            SetInteractable(false);

            string voiceId = null;
            yield return _voice.CloneVoice(_recordedAudio, "voice_sample.wav",
                onSuccess: r => voiceId = r.voiceId,
                onError: e =>
                {
                    Debug.LogError($"[RecordingOverlay] Clone: {e}");
                    if (statusText) statusText.text = $"{Loc.Get("error")}: {e}";
                });

            if (voiceId == null)
            {
                _sending = false;
                SetInteractable(true);
                yield break;
            }

            PlayerPrefs.SetInt("ft_voiceCloned", 1);
            PlayerPrefs.Save();

            if (statusText) statusText.text = Loc.Get("rec_narrating");

            var childName = PlayerPrefs.GetString("ft_childName", "");
            var gender = PlayerPrefs.GetString("ft_gender", "male");
            yield return _narration.NarrateAll(_taleSummary.id, childName, gender,
                onSuccess: _ =>
                {
                    Deactivate();
                    _screens.Show<Library.TaleDetailScreen>();
                },
                onError: e =>
                {
                    Debug.LogError($"[RecordingOverlay] NarrateAll: {e}");
                    if (statusText) statusText.text = $"{Loc.Get("error")}: {e}";
                    _sending = false;
                    SetInteractable(true);
                });
        }

        /// <summary>
        /// Record btn: iconRecord visible when idle (no audio),
        /// iconRerecord visible when has audio or while recording.
        /// </summary>
        private void ShowRecordIcon(bool recording, bool hasAudio)
        {
            bool showRerecord = recording || hasAudio;
            if (iconRecord) iconRecord.SetActive(!showRerecord);
            if (iconRerecord) iconRerecord.SetActive(showRerecord);
        }

        /// <summary>
        /// Done btn: iconDoneEnabled when ready, iconDoneDisabled otherwise.
        /// </summary>
        private void UpdateDoneIcon()
        {
            bool recording = _mic != null && _mic.IsRecording;
            bool hasAudio = _recordedAudio != null;
            bool enoughPages = _pagesWhileRecording >= RequiredPages;
            bool ready = hasAudio && !recording && enoughPages && !_sending;

            if (btnRecord) btnRecord.interactable = !_sending;
            if (btnDone) btnDone.interactable = ready;
            if (iconDoneEnabled) iconDoneEnabled.SetActive(ready);
            if (iconDoneDisabled) iconDoneDisabled.SetActive(!ready);
        }

        private void SetInteractable(bool value)
        {
            if (btnRecord) btnRecord.interactable = value;
            if (btnDone) btnDone.interactable = value;
        }

        private void StopRecordingIfActive()
        {
            if (_mic != null && _mic.IsRecording)
            {
                _mic.OnStopped -= OnRecordingStopped;
                _mic.StopRecording();
            }
        }
    }
}
