using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FairyTales.Audio;
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
        private ScreenManager _screens;
        private int _pagesWhileRecording;
        private byte[] _recordedAudio;

        public bool IsActive => panel != null && panel.activeSelf;

        private void Awake()
        {
            if (btnRecord) btnRecord.onClick.AddListener(OnRecord);
            if (btnDone) btnDone.onClick.AddListener(OnDone);
        }

        public void Activate(ScreenManager screens)
        {
            _screens = screens;
            _mic = FindAnyObjectByType<MicRecorder>();
            _pagesWhileRecording = 0;
            _recordedAudio = null;

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
            if (_recordedAudio == null) return;

            // Save voice locally — cloning happens later when user
            // picks "parent voice" in the Listen panel.
            Cache.AssetCache.SaveParentVoice(_recordedAudio);
            PlayerPrefs.SetInt("ft_voiceCloned", 1);
            PlayerPrefs.Save();

            Toast.Show(Loc.Get("voice_saved"));
            Deactivate();
            _screens.Show<Library.TaleDetailScreen>();
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
            bool ready = hasAudio && !recording && enoughPages;

            if (btnRecord) btnRecord.interactable = true;
            if (btnDone) btnDone.interactable = ready;
            if (iconDoneEnabled) iconDoneEnabled.SetActive(ready);
            if (iconDoneDisabled) iconDoneDisabled.SetActive(!ready);
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
