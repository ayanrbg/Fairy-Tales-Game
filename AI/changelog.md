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

---

## Session 3 — 2026-03-09

### Phase 4 — Onboarding Screens ✅

**Files created** (3 files in `Scripts/FairyTales/UI/Onboarding/`):

- `LanguageSelectScreen.cs` — выбор языка (RU/KZ/EN)
  - SerializeField кнопки + визуальные индикаторы выбора
  - Сохраняет `ft_lang` в PlayerPrefs
  - Навигация → PersonalizationScreen

- `PersonalizationScreen.cs` — имя ребёнка + пол
  - TMP_InputField для имени, 2 кнопки пола
  - Кнопка смены языка → назад к LanguageSelectScreen
  - Валидация: не даёт продолжить с пустым именем
  - Сохраняет `ft_childName`, `ft_gender` в PlayerPrefs
  - Навигация → LoadingScreen

- `LoadingScreen.cs` — регистрация + персонализация
  - Slider прогресс-бар + TMP_Text статус
  - Полный flow: Register → GetTales → Personalize каждую сказку
  - Прогресс обновляется по шагам
  - Event OnLoadingComplete для внешней подписки
  - Сохраняет `ft_onboarded` флаг
  - Генерирует userId (GUID) если не существует

**Updated:**
- `Editor/SceneSetup.cs` — теперь добавляет реальные компоненты
  - CreateScreen<T>() — generic метод для экранов с компонентами
  - LanguageSelectScreen как initialScreen в ScreenManager

**PlayerPrefs keys (onboarding):**
- `ft_lang` — язык (ru/kz/en)
- `ft_childName` — имя ребёнка
- `ft_gender` — пол (male/female)
- `ft_userId` — GUID пользователя
- `ft_onboarded` — флаг завершения онбординга (1)

---

## Session 4 — 2026-03-10

### Bugfix — Scene Setup

- `Editor/SceneSetup.cs` — теперь вызывает `OnboardingSetup.Setup()` после создания Canvas, чтобы UI-элементы онбординга создавались автоматически
- `Editor/OnboardingSetup.cs` + `Editor/SceneSetup.cs` — все `new GameObject(...)` теперь создаются с `typeof(RectTransform)` для корректной работы на Canvas (ранее создавались с обычным Transform)

**Next step:** Phase 5 — Library

---

## Session 5 — 2026-03-10

### Phase 5 — Library ✅

**Files created** (4 files in `Scripts/FairyTales/UI/Library/`):

- `LibraryScreen.cs` — грид карточек сказок
  - Загрузка через TalesService.GetTales()
  - GridLayoutGroup 4 колонки, ScrollRect
  - Кнопки: Settings, Music toggle, Unlock All
  - Клик по карточке → TaleDetailScreen

- `TaleCard.cs` — компонент карточки
  - Init(tale, isLocked, onClick) — обложка + название + замок
  - CoverProvider для загрузки обложек

- `TaleDetailScreen.cs` — детали сказки
  - Обложка, название, кол-во страниц
  - 3 кнопки: Читать, Слушать, Озвучить
  - LoadDetail() — GetTale + Personalize + GetNarrationStatus

- `CoverProvider.cs` — статический хелпер
  - Resources/Covers/{taleId}

**Editor:**
- `Editor/LibrarySetup.cs` — menu: FairyTales → Setup Library UI
  - SetupLibraryScreen, SetupTaleDetailScreen, CreateCardPrefab
  - Prefab: `Assets/Prefabs/UI/TaleCard.prefab`

**Bugfixes:**
- `LibrarySetup.cs` — добавлен `using FairyTales.UI.Core` (CS0246)
- `FindOrAddComponent` — сначала `FindAnyObjectByType<T>(FindObjectsInactive.Include)`
- `CreateCardPrefab` — вынесен `AssignCardPrefab()` для назначения при повторном запуске
- ScrollView: заменён `Mask` на `RectMask2D` (Stencil не работает в URP 2D)

### Phase 6 — Reading ✅

**Files created** (4 files in `Scripts/FairyTales/UI/Reading/`):

- `ReadingScreen.cs` — экран чтения
  - Fullscreen иллюстрация + scrollable текстовая панель (30% снизу)
  - Кнопки: Home, TOC, Music (top), Prev/Next (bottom)
  - Авто-озвучка через DefaultNarrationProvider + NarrationPlayer
  - Home → LibraryScreen

- `PageNavigator.cs` — перелистывание страниц
  - DOTween fade только на illustrationImage + pageText (кнопки остаются)
  - ResolveGender() — regex парсер `{m:...|f:...}` плейсхолдеров
  - Init/NextPage/PrevPage/GoToPage + OnPageChanged event

- `TableOfContentsPopup.cs` — popup оглавление
  - CanvasGroup fade in/out
  - Горизонтальный ScrollRect с миниатюрами
  - Текущая страница — cyan border
  - Клик по миниатюре → GoToPage + close

- `IllustrationProvider.cs` — статический хелпер
  - Resources/Illustrations/{taleId}/page_{page}

**Editor:**
- `Editor/ReadingSetup.cs` — menu: FairyTales → Setup Reading UI
  - SetupReadingScreen, CreateTocPopup, CreateThumbnailPrefab
  - Prefab: `Assets/Prefabs/UI/TocThumbnail.prefab`

**Updated:**
- `TaleDetailScreen.cs` — OnRead() навигирует на ReadingScreen с _detail
- `TaleDetailScreen.cs` — LoadDetail() вызывает Personalize() для подмены pages
- `SceneSetup.cs` — ReadingScreen как реальный компонент вместо stub

**Resource paths:**
```
Assets/Resources/Illustrations/{taleId}/page_0.png
Assets/Resources/Illustrations/{taleId}/page_1.png
```

### Flow Change — убран LoadingScreen из основного flow

**Новый flow:**
```
LanguageSelect → Personalization → Library
                                    ↓ (Settings)
                              Personalization
```

**Изменения:**

- `PersonalizationScreen.cs` — OnContinue() теперь → LibraryScreen (вместо LoadingScreen)
- `LibraryScreen.cs` — полный рефактор:
  - Lazy Register: при первом OnShown вызывает AuthService.Register()
  - btnSettings → PersonalizationScreen
  - Всегда перезагружает сказки при OnShown (имя/пол могли измениться)
  - GetOrCreateUserId() перенесён из LoadingScreen
- LoadingScreen остаётся в кодовой базе, но не используется в основном flow

**Next step:** Phase 7 — Narration

---

## Session 6 — 2026-03-10

### Auto-skip Onboarding

**ScreenManager.cs** — добавлено поле `onboardedScreen`:
- В Start() проверяет `PlayerPrefs.GetString("ft_childName")`
- Если имя есть → показывает `onboardedScreen` (LibraryScreen) вместо `initialScreen`
- SceneSetup назначает LibraryScreen в `onboardedScreen`

### Phase 7 — Narration ✅

**Files created** (3 files in `Scripts/FairyTales/UI/Narration/`):

- `NarrationSetupScreen.cs` — экран настройки озвучки
  - Обложка + название сказки
  - 2 вкладки: "Новая запись" / "Черновики"
  - Новая запись: имя рассказчика + кнопка "Начать" → CreateDraft → VoiceRecordingScreen
  - Черновики: список drafts → клик → VoiceRecordingScreen

- `VoiceRecordingScreen.cs` — запись голоса
  - 4 sample sentences для чтения вслух (хардкод)
  - Record/Stop (через MicRecorder), таймер в Update
  - Play (через NarrationPlayer)
  - Send → CloneVoice → NarrateAll → NarrationProgressScreen

- `NarrationProgressScreen.cs` — прогресс AI озвучки
  - Polling narration-status каждые 3 сек
  - Slider progressBar + pagesReady/totalPages
  - Когда done → кнопка "Готово" → TaleDetailScreen

**Editor:**
- `Editor/NarrationSetup.cs` — menu: FairyTales → Setup Narration UI
  - SetupNarrationSetupScreen, SetupVoiceRecordingScreen, SetupNarrationProgressScreen
  - Prefab: `Assets/Prefabs/UI/DraftItem.prefab`

**Updated:**
- `TaleDetailScreen.cs` — OnNarrate() → NarrationSetupScreen.SetTale + Show
- `SceneSetup.cs` — stubs заменены на CreateScreen<T> для narration экранов, onboardedScreen assigned
- `ScreenManager.cs` — onboardedScreen для автоскипа онбординга

**Full narration flow:**
```
TaleDetailScreen → "Озвучить" → NarrationSetupScreen
  → "Начать" → CreateDraft → VoiceRecordingScreen
    → Record → CloneVoice → NarrateAll → NarrationProgressScreen
      → Poll → Done → TaleDetailScreen (re-checks narration status)
```

**Known limitation:**
- Кнопка "Слушать" в TaleDetailScreen — пока Debug.Log заглушка. Реальное воспроизведение (AI/Default/нет озвучки) → Phase 8.

**Next step:** Phase 8 — Listen & Polish
