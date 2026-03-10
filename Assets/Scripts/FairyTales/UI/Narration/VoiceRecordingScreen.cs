using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FairyTales.Api;
using FairyTales.Audio;
using FairyTales.Models;
using FairyTales.UI.Core;

namespace FairyTales.UI.Narration
{
    public class VoiceRecordingScreen : BaseScreen
    {
        [Header("UI")]
        [SerializeField] private TMP_Text sampleText;
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private Button btnRecord;
        [SerializeField] private Button btnPlay;
        [SerializeField] private Button btnSend;
        [SerializeField] private Button btnBack;

        private static readonly string[] Samples = new[]
        {
            "Жили-были дедушка да бабушка. Была у них внучка, которая очень любила сказки.",
            "Однажды солнечным утром они отправились в волшебный лес за грибами и ягодами.",
            "В лесу они встретили маленького зайчика, который заблудился и искал свою маму.",
            "Все вместе они нашли дорогу домой и с тех пор стали лучшими друзьями."
        };

        private ScreenManager _screens;
        private MicRecorder _mic;
        private NarrationPlayer _player;
        private VoiceService _voice;
        private NarrationService _narration;
        private TaleSummary _tale;
        private Draft _draft;
        private byte[] _recordedAudio;

        private void Awake()
        {
            _screens = GetComponentInParent<ScreenManager>();
            var api = FindAnyObjectByType<ApiClient>();
            _voice = new VoiceService(api);
            _narration = new NarrationService(api);

            if (btnRecord) btnRecord.onClick.AddListener(OnRecord);
            if (btnPlay) btnPlay.onClick.AddListener(OnPlay);
            if (btnSend) btnSend.onClick.AddListener(OnSend);
            if (btnBack) btnBack.onClick.AddListener(OnBack);
        }

        public void SetContext(TaleSummary tale, Draft draft)
        {
            _tale = tale;
            _draft = draft;
            _recordedAudio = null;
        }

        protected override void OnShown()
        {
            _mic = FindAnyObjectByType<MicRecorder>();
            _player = FindAnyObjectByType<NarrationPlayer>();

            // Show all 4 sample sentences
            if (sampleText)
                sampleText.text = string.Join("\n\n", Samples);

            UpdateButtons();
            if (timerText) timerText.text = "00:00";
            if (statusText) statusText.text = "Нажмите запись и прочитайте текст вслух";
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
                _mic.OnStopped -= OnRecordingStopped;
                _mic.OnStopped += OnRecordingStopped;
                _mic.StopRecording();
            }
            else
            {
                _recordedAudio = null;
                _mic.OnStopped -= OnRecordingStopped;
                _mic.OnStopped += OnRecordingStopped;
                _mic.StartRecording();
                if (statusText) statusText.text = "Запись...";
                UpdateButtons();
            }
        }

        private void OnRecordingStopped(byte[] wavData)
        {
            _mic.OnStopped -= OnRecordingStopped;
            _recordedAudio = wavData;

            if (statusText)
                statusText.text = wavData != null
                    ? "Запись завершена. Прослушайте или отправьте."
                    : "Ошибка записи. Попробуйте ещё раз.";

            UpdateButtons();
        }

        private void OnPlay()
        {
            if (_player == null || _recordedAudio == null) return;
            _player.PlayFromBytes(_recordedAudio);
        }

        private void OnSend()
        {
            if (_recordedAudio == null) return;
            StartCoroutine(CloneAndNarrate());
        }

        private IEnumerator CloneAndNarrate()
        {
            if (statusText) statusText.text = "Клонирование голоса...";
            SetInteractable(false);

            string voiceId = null;
            yield return _voice.CloneVoice(_recordedAudio, "voice_sample.wav",
                onSuccess: r => voiceId = r.voiceId,
                onError: e =>
                {
                    Debug.LogError($"[VoiceRecording] Clone: {e}");
                    if (statusText) statusText.text = $"Ошибка: {e}";
                });

            if (voiceId == null)
            {
                SetInteractable(true);
                yield break;
            }

            PlayerPrefs.SetInt("ft_voiceCloned", 1);
            PlayerPrefs.Save();

            if (statusText) statusText.text = "Запуск озвучки...";

            var childName = PlayerPrefs.GetString("ft_childName", "");
            var gender = PlayerPrefs.GetString("ft_gender", "male");
            yield return _narration.NarrateAll(_tale.id, childName, gender,
                onSuccess: r =>
                {
                    var progress = _screens.Get<NarrationProgressScreen>();
                    if (progress == null) return;

                    progress.SetContext(_tale);
                    _screens.Show<NarrationProgressScreen>();
                },
                onError: e =>
                {
                    Debug.LogError($"[VoiceRecording] NarrateAll: {e}");
                    if (statusText) statusText.text = $"Ошибка: {e}";
                    SetInteractable(true);
                });
        }

        private void UpdateButtons()
        {
            bool recording = _mic != null && _mic.IsRecording;
            bool hasAudio = _recordedAudio != null;

            if (btnPlay) btnPlay.interactable = hasAudio && !recording;
            if (btnSend) btnSend.interactable = hasAudio && !recording;
        }

        private void SetInteractable(bool value)
        {
            if (btnRecord) btnRecord.interactable = value;
            if (btnPlay) btnPlay.interactable = value;
            if (btnSend) btnSend.interactable = value;
            if (btnBack) btnBack.interactable = value;
        }

        private void OnBack() => _screens.Show<NarrationSetupScreen>();

        protected override void OnHidden()
        {
            if (_mic != null && _mic.IsRecording)
                _mic.StopRecording();
            if (_player != null && _player.IsPlaying)
                _player.Stop();
        }
    }
}
