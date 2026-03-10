using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FairyTales.Api;
using FairyTales.Audio;
using FairyTales.Models;
using FairyTales.UI.Core;
using FairyTales.UI.Library;

namespace FairyTales.UI.Narration
{
    public class NarrationSetupScreen : BaseScreen
    {
        [Header("UI")]
        [SerializeField] private Image coverImage;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_InputField narratorNameInput;
        [SerializeField] private Button btnStart;
        [SerializeField] private Button btnBack;

        [Header("Tabs")]
        [SerializeField] private Button btnTabNew;
        [SerializeField] private Button btnTabDrafts;
        [SerializeField] private GameObject panelNew;
        [SerializeField] private GameObject panelDrafts;

        [Header("Quick Narrate (voice already cloned)")]
        [SerializeField] private GameObject panelQuickNarrate;
        [SerializeField] private Button btnNarrateNow;
        [SerializeField] private Button btnRerecord;

        [Header("Drafts")]
        [SerializeField] private Transform draftsContainer;
        [SerializeField] private GameObject draftItemPrefab;

        private ScreenManager _screens;
        private VoiceService _voice;
        private NarrationService _narration;
        private TaleSummary _tale;

        private bool HasVoice => PlayerPrefs.GetInt("ft_voiceCloned", 0) == 1;

        private void Awake()
        {
            _screens = GetComponentInParent<ScreenManager>();
            var api = FindAnyObjectByType<ApiClient>();
            _voice = new VoiceService(api);
            _narration = new NarrationService(api);

            if (btnStart) btnStart.onClick.AddListener(OnStart);
            if (btnBack) btnBack.onClick.AddListener(OnBack);
            if (btnTabNew) btnTabNew.onClick.AddListener(() => ShowTab(true));
            if (btnTabDrafts) btnTabDrafts.onClick.AddListener(() => ShowTab(false));
            if (btnNarrateNow) btnNarrateNow.onClick.AddListener(OnNarrateNow);
            if (btnRerecord) btnRerecord.onClick.AddListener(OnRerecord);
        }

        public void SetTale(TaleSummary tale)
        {
            _tale = tale;
            if (titleText) titleText.text = "";
            if (coverImage) coverImage.sprite = null;
        }

        protected override void OnShown()
        {
            if (titleText) titleText.text = _tale?.title ?? "";

            var cover = _tale != null ? CoverProvider.Get(_tale.id) : null;
            if (coverImage && cover) coverImage.sprite = cover;

            // Show quick narrate panel if voice exists, otherwise new recording
            if (HasVoice)
            {
                if (panelQuickNarrate) panelQuickNarrate.SetActive(true);
                if (panelNew) panelNew.SetActive(false);
                if (panelDrafts) panelDrafts.SetActive(false);
            }
            else
            {
                if (panelQuickNarrate) panelQuickNarrate.SetActive(false);
                ShowTab(true);
            }

            StartCoroutine(LoadDrafts());
        }

        private void ShowTab(bool isNew)
        {
            if (panelQuickNarrate) panelQuickNarrate.SetActive(false);
            if (panelNew) panelNew.SetActive(isNew);
            if (panelDrafts) panelDrafts.SetActive(!isNew);
        }

        private void OnNarrateNow()
        {
            StartCoroutine(NarrateWithExistingVoice());
        }

        private IEnumerator NarrateWithExistingVoice()
        {
            if (btnNarrateNow) btnNarrateNow.interactable = false;

            var childName = PlayerPrefs.GetString("ft_childName", "");
            var gender = PlayerPrefs.GetString("ft_gender", "male");

            yield return _narration.NarrateAll(_tale.id, childName, gender,
                onSuccess: _ =>
                {
                    var progress = _screens.Get<NarrationProgressScreen>();
                    if (progress == null) return;
                    progress.SetContext(_tale);
                    _screens.Show<NarrationProgressScreen>();
                },
                onError: e =>
                {
                    Debug.LogError($"[NarrationSetup] NarrateAll: {e}");
                    Toast.Show($"Ошибка: {e}");
                    if (btnNarrateNow) btnNarrateNow.interactable = true;
                });
        }

        private void OnRerecord()
        {
            if (panelQuickNarrate) panelQuickNarrate.SetActive(false);
            ShowTab(true);
        }

        private IEnumerator LoadDrafts()
        {
            if (draftsContainer == null) yield break;

            foreach (Transform child in draftsContainer)
                Destroy(child.gameObject);

            yield return _voice.GetDrafts(
                onSuccess: drafts =>
                {
                    foreach (var draft in drafts)
                    {
                        if (draft.taleId != _tale?.id) continue;
                        CreateDraftItem(draft);
                    }
                },
                onError: e => Debug.LogWarning($"[NarrationSetup] Drafts: {e}"));
        }

        private void CreateDraftItem(Draft draft)
        {
            if (draftItemPrefab == null || draftsContainer == null) return;

            var go = Instantiate(draftItemPrefab, draftsContainer);
            go.SetActive(true);

            var label = go.GetComponentInChildren<TMP_Text>();
            if (label) label.text = draft.narratorName;

            var btn = go.GetComponent<Button>();
            if (btn) btn.onClick.AddListener(() => OpenRecording(draft));
        }

        private void OnStart()
        {
            var name = narratorNameInput != null ? narratorNameInput.text.Trim() : "";
            if (string.IsNullOrEmpty(name))
            {
                Debug.LogWarning("[NarrationSetup] Narrator name is empty");
                return;
            }

            StartCoroutine(CreateDraftAndOpen(name));
        }

        private IEnumerator CreateDraftAndOpen(string narratorName)
        {
            yield return _voice.CreateDraft(narratorName, _tale.id,
                onSuccess: draft => OpenRecording(draft),
                onError: e => Debug.LogError($"[NarrationSetup] {e}"));
        }

        private void OpenRecording(Draft draft)
        {
            var recording = _screens.Get<VoiceRecordingScreen>();
            if (recording == null) return;

            recording.SetContext(_tale, draft);
            _screens.Show<VoiceRecordingScreen>();
        }

        private void OnBack() => _screens.Show<TaleDetailScreen>();
    }
}
