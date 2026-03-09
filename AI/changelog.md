# Changelog

## Session 1 — 2026-03-09

### Phase 0 — Infrastructure ✅

**Packages:**
- Added `com.unity.nuget.newtonsoft-json` (3.2.1) to `Packages/manifest.json`
- DOTween installed via Asset Store (by user)

**Project setup:**
- Renamed `CLAUDE.md1` → `CLAUDE.md`
- Created `AI/` folder for all dev documentation

**Folder structure created in Assets:**
```
Assets/
├── Scripts/FairyTales/
│   ├── Api/
│   ├── Models/
│   ├── Audio/
│   └── UI/
│       ├── Core/
│       ├── Onboarding/
│       ├── Library/
│       ├── Reading/
│       └── Narration/
├── Audio/
│   ├── Default/
│   └── Music/
├── Prefabs/
│   ├── UI/
│   └── Screens/
└── Sprites/
    ├── UI/
    └── Tales/
```

**Documentation created:**
- `AI/architecture.md` — full architecture, folder structure, screen flow, development phases
- `AI/api-requirements.md` — existing + missing server endpoints for backend developer
- `AI/ui-flow.md` — all screens and navigation description based on UI mockups
- `AI/changelog.md` — this file

### Phase 1 — Core (API + Data) ✅

**Models** (5 files in `Scripts/FairyTales/Models/`):
- `AuthModels.cs` — LoginRequest, RegisterRequest, LoginResponse, RegisterResponse, ProfileData, ProfileUpdateRequest/Response
- `TaleModels.cs` — TaleSummary, TaleDetail, PersonalizeRequest/Response
- `VoiceModels.cs` — CloneResponse, DeleteVoiceResponse
- `NarrationModels.cs` — NarrateAllResponse, NarrationStatusResponse
- `DraftModels.cs` — Draft, DraftCreateRequest/Response, DraftDeleteResponse

**API Client** (`Scripts/FairyTales/Api/ApiClient.cs`):
- MonoBehaviour with configurable baseUrl
- Methods: Get, GetAudio, PostJson, Post, PostForAudio, PostMultipart, PutJson, Delete
- Auto JWT auth via Bearer token
- Unified error formatting

**Services** (4 files in `Scripts/FairyTales/Api/`):
- `AuthService.cs` — Login, Register, GetProfile, UpdateProfile, TryRestoreSession, Logout
- `TalesService.cs` — GetTales, GetTale, Personalize
- `VoiceService.cs` — CloneVoice, DeleteVoice, GetDrafts, CreateDraft, GetDraft, DeleteDraft
- `NarrationService.cs` — NarratePage, NarrateAll, GetNarrationStatus, DownloadNarratedPage

---

## Session 2 — 2026-03-09

### Phase 2 — Audio ✅

**Files created** (4 files in `Scripts/FairyTales/Audio/`):

- `MicRecorder.cs` — MonoBehaviour, запись с микрофона → WAV byte[]
  - Auto-stop по maxDuration (30s), events: OnStarted, OnStopped(byte[])
  - WAV encoding (PCM 16-bit, 44100 Hz)

- `NarrationPlayer.cs` — MonoBehaviour, воспроизведение аудио
  - PlayFromBytes(byte[]) — temp file → UnityWebRequestMultimedia (WAV/MP3/OGG)
  - PlayClip(AudioClip) — для предзаписанного аудио
  - Pause/Resume/Stop, Progress, OnFinished event

- `BackgroundMusicManager.cs` — Singleton, DontDestroyOnLoad
  - PlayMenu() — музыка меню (SerializeField)
  - PlayForTale(taleId) — загрузка из Resources/Music/{taleId}
  - SetVolume(), SetMuted(), Stop()

- `DefaultNarrationProvider.cs` — обычный класс (не MonoBehaviour)
  - GetPage(taleId, page) — Resources/Audio/Default/{taleId}/page_{N}
  - HasNarration(), HasAnyNarration()

**Resource paths:**
```
Assets/Resources/Audio/Default/{taleId}/page_0.wav
Assets/Resources/Music/{taleId}.wav
```

### Phase 3 — UI Framework ✅

**Files created** (2 files in `Scripts/FairyTales/UI/Core/`):

- `BaseScreen.cs` — abstract MonoBehaviour, RequireComponent(CanvasGroup)
  - Show()/Hide() — DOTween fade (CanvasGroup alpha)
  - ShowImmediate()/HideImmediate() — без анимации
  - Virtual hooks: OnShown(), OnHidden()

- `ScreenManager.cs` — MonoBehaviour, управление экранами
  - Auto-registers all BaseScreen children in Awake
  - Show<T>() — fade out current → fade in next
  - ShowImmediate<T>(), Get<T>(), Current
  - initialScreen — стартовый экран через Inspector

### Editor Tooling

- `Scripts/FairyTales/Editor/SceneSetup.cs` — menu: FairyTales → Setup Scene
  - Creates [Services] (ApiClient)
  - Creates [BackgroundMusic] (BackgroundMusicManager)
  - Creates [Audio] (MicRecorder + NarrationPlayer)
  - Creates [UICanvas] (Canvas 1080×1920 + ScreenManager + 9 screen stubs)
  - Creates EventSystem if missing
  - Idempotent — safe to run multiple times
  - All operations support Undo

**Next step:** Phase 4 — Onboarding Screens
