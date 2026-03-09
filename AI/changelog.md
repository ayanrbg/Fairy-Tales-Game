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

**Next step:** Phase 1 — Core (API + Data)
