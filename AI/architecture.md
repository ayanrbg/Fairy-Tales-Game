# Architecture & Development Plan

## Folder Structure
```
Assets/
├── Scripts/
│   └── FairyTales/
│       ├── Api/
│       │   ├── ApiClient.cs
│       │   ├── AuthService.cs
│       │   ├── VoiceService.cs
│       │   ├── TalesService.cs
│       │   └── NarrationService.cs
│       ├── Models/
│       │   ├── AuthModels.cs
│       │   ├── VoiceModels.cs
│       │   ├── TaleModels.cs
│       │   └── DraftModels.cs
│       ├── Audio/
│       │   ├── MicRecorder.cs
│       │   ├── NarrationPlayer.cs
│       │   ├── BackgroundMusicManager.cs
│       │   └── DefaultNarrationProvider.cs
│       ├── UI/
│       │   ├── Core/
│       │   │   ├── ScreenManager.cs
│       │   │   └── BaseScreen.cs
│       │   ├── Onboarding/
│       │   │   ├── LanguageSelectScreen.cs
│       │   │   ├── PersonalizationScreen.cs
│       │   │   └── LoadingScreen.cs
│       │   ├── Library/
│       │   │   ├── LibraryScreen.cs
│       │   │   ├── TaleCard.cs
│       │   │   └── TaleDetailScreen.cs
│       │   ├── Reading/
│       │   │   ├── ReadingScreen.cs
│       │   │   ├── PageNavigator.cs
│       │   │   └── TableOfContentsPopup.cs
│       │   └── Narration/
│       │       ├── NarrationSetupScreen.cs
│       │       ├── VoiceRecordingScreen.cs
│       │       └── NarrationProgressScreen.cs
│       └── FairyTaleManager.cs
├── Audio/
│   ├── Default/          # Pre-baked narration per tale/page
│   └── Music/            # Background music tracks
├── Prefabs/
│   ├── UI/
│   └── Screens/
├── Sprites/
│   ├── UI/
│   └── Tales/            # Illustrations per tale
└── Scenes/
    └── MainScene.unity
```

## Screens Flow
```
LanguageSelect → Personalization → Loading → Library
                                                ↓
                                          TaleDetail
                                         /    |     \
                                   Reading  Listen  NarrationSetup
                                     ↓                    ↓
                              TableOfContents    VoiceRecording
                                                       ↓
                                              NarrationProgress
```

## Development Phases

### Phase 0 — Infrastructure ✅
1. DOTween installed
2. Newtonsoft Json added to manifest
3. Folder structure created

### Phase 1 — Core (API + Data)
5. ApiClient.cs — HTTP client (GET/POST/DELETE/Multipart/Audio)
6. Models (Auth, Voice, Tales, Draft, Profile)
7. AuthService.cs — login/register, session restore
8. TalesService.cs — tales list, details, personalization
9. VoiceService.cs — voice clone, drafts CRUD
10. NarrationService.cs — AI narration + default playback

### Phase 2 — Audio
11. MicRecorder.cs — mic recording → WAV
12. NarrationPlayer.cs — play MP3 from bytes
13. BackgroundMusicManager.cs — global music toggle, per-tale tracks
14. DefaultNarrationProvider.cs — play pre-baked audio from Assets

### Phase 3 — UI Framework
15. ScreenManager.cs — screen switching with DOTween fades
16. BaseScreen.cs — base class (Show/Hide with animations)
17. Shared UI prefabs (buttons, panels, header)

### Phase 4 — Onboarding Screens
18. LanguageSelectScreen
19. PersonalizationScreen
20. LoadingScreen (progress bar + cat animation)

### Phase 5 — Library
21. LibraryScreen — grid of tale cards
22. TaleCard component
23. TaleDetailScreen — 2 states (narrated / not narrated)

### Phase 6 — Reading
24. ReadingScreen — fullscreen illustrations + text
25. PageNavigator — page flip with DOTween fade
26. TableOfContentsPopup — horizontal scroll thumbnails
27. Default narration integration

### Phase 7 — Narration
28. NarrationSetupScreen — new recording / drafts
29. VoiceRecordingScreen — record 4 sentences + timer
30. NarrationProgressScreen — waiting for AI generation
31. Full integration: clone → narrate-all → poll → ready

### Phase 8 — Polish
32. DOTween animations everywhere
33. Localization (RU/KZ/EN)
34. State persistence (PlayerPrefs)
35. Mobile adaptation (Safe Area, resolutions)
36. Monetization placeholder
