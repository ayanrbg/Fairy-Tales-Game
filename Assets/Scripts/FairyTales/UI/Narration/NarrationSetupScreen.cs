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

        [Header("Drafts")]
        [SerializeField] private Transform draftsContainer;
        [SerializeField] private GameObject draftItemPrefab;

        private ScreenManager _screens;
        private VoiceService _voice;
        private TaleSummary _tale;

        private void Awake()
        {
            _screens = GetComponentInParent<ScreenManager>();
            var api = FindAnyObjectByType<ApiClient>();
            _voice = new VoiceService(api);

            if (btnStart) btnStart.onClick.AddListener(OnStart);
            if (btnBack) btnBack.onClick.AddListener(OnBack);
            if (btnTabNew) btnTabNew.onClick.AddListener(() => ShowTab(true));
            if (btnTabDrafts) btnTabDrafts.onClick.AddListener(() => ShowTab(false));
        }

        public void SetTale(TaleSummary tale) => _tale = tale;

        protected override void OnShown()
        {
            if (titleText) titleText.text = _tale?.title ?? "";

            var cover = _tale != null ? CoverProvider.Get(_tale.id) : null;
            if (coverImage && cover) coverImage.sprite = cover;

            ShowTab(true);
            StartCoroutine(LoadDrafts());
        }

        private void ShowTab(bool isNew)
        {
            if (panelNew) panelNew.SetActive(isNew);
            if (panelDrafts) panelDrafts.SetActive(!isNew);
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
