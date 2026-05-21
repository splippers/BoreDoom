# CLAUDE.md — BoreDoom / Chorewars

## Project
- **What:** AR chore game (hoover/mow coverage, movement, scoring) for **Meta Quest 3**
- **Unity:** 6000.4.5f1, Built-in RP (not URP/HDRP)
- **Platform:** Android / Quest, OpenXR, IL2CPP
- **Repo:** `\\marvin\Projects\boredoom` | GitHub: `github.com:splippers/BoreDoom.git`
- **Local clone:** `C:\Users\Jon\Boredoom`

## Architecture
- Scenes in `Assets/Chorewars/Scenes/` are saved **without cameras/rigs** — runtime bootstraps add XROrigin + camera
- Bootstrap flow: `QuestXrRigBootstrap.cs` → `BootstrapSceneRouter.cs`
- Scenes: `Bootstrap.unity`, `HooverMode.unity`, `MowingMode.unity`

## Source of truth files
| Topic | Path |
|--------|------|
| Runtime XR rig + logs | `Assets/Chorewars/Scripts/Bootstrap/QuestXrRigBootstrap.cs` |
| Scene routing | `Assets/Chorewars/Scripts/Core/BootstrapSceneRouter.cs` |
| XR asset bootstrap (Editor only) | `Assets/Editor/BoreDoomXrManagementBootstrap.cs` |
| Build scenes | `ProjectSettings/EditorBuildSettings.asset` |
| Packages | `Packages/manifest.json` |
| Link / stripping | `Assets/link.xml` |
| Telemetry | `Assets/Chorewars/Scripts/Diagnostics/TelemetryLogger.cs` |

## AI assistant notes
- **MCP:** `com.ivanmurzak.unity.mcp` is in `Packages/manifest.json` — free Unity Editor ↔ AI bridge. Server auto-starts when Unity opens.
- **Connection:** opencode.json in repo root connects to Unity MCP server. Find port in Unity → Window → AI Game Developer.
- **Prerequisite:** Unity Editor must be open with the MCP package resolved for AI to control the editor.
- **Key constraint:** Avoid drive-by refactors. Make minimal, focused changes.
- **Safety:** Full passthrough AR preferred; avoid opaque full-screen overlays.

## Build notes
- IL2CPP backend
- Meta Quest Support + Oculus Touch Controller Profile required in OpenXR
- Build docs: `docs/quest3-build.md`
- Pull logs: `adb exec-out run-as com.boredoom.app cat /storage/emulated/0/Android/data/com.boredoom.app/files/MQ3.log`
