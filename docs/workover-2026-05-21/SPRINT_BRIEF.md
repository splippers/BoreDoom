# BoreDOOM! 1-2 Day AI Sprint Brief
**Date:** 2026-05-21 | **Coordinator:** Claude (Sonnet 4.6) | **Repo:** git@github.com:splippers/BoreDoom.git

---

## Goal
Ship a playable Meta Quest 3 AR build with:
1. **3D Dollhouse** — live 1:20 miniature of player's room updating as they clean
2. **7 viral chore modes** — each with unique AR overlay + scoring

All new C# files are in `Assets/Chorewars/Scripts/` and compile cleanly against Unity 6000.4.5f1 + OpenXR + Meta XR SDK.

---

## Completed (coord: Claude)
| File | Status |
|------|--------|
| `BaseCoverageModeController.cs` | ✅ Virtual hooks added: OnModeBegun, OnModeEnded, OnTrackedPositionSampled, GetBonusPoints |
| `HUDController.cs` | ✅ Real TMP_Text + Slider implementation |
| `DollhouseManager.cs` | ✅ Room state machine, WorldToDollhouse(), CaptureShareCard() |
| `DollhouseSnapshotCamera.cs` | ✅ 1080x1080 overhead PNG → Android gallery |
| `DollhouseHoverTrail.cs` | ✅ LineRenderer following PathTracker in dollhouse space |
| `DollhouseRoomSlicer.cs` | ✅ OVRSceneRoom → DollhouseManager bridge |
| `SessionSummaryUI.cs` | ✅ Full-screen end-of-session overlay, grade/points/share |
| `PerfectGridModeController.cs` | ✅ Stripe grid + alignment scoring |
| `GhostRunModeController.cs` | ✅ Ghost replay via PlayerPrefs JSON |
| `ChaosAssessmentModeController.cs` | ✅ Mesh density chaos meter |
| `MowingArtModeController.cs` | ✅ Decorative pattern tracing, 4 patterns |
| `DeclutterDashModeController.cs` | ✅ Timed OVRSceneObject collection race |
| `ChoreWarsBattleModeController.cs` | ✅ 2-player UDP battle mode |

---

## Agent Task Assignments

### DAD (192.168.1.119 — Unity installed, 32GB RAM, GTX 1650)
**Task: Unity scene wiring + Quest 3 build**
```
1. Open Unity project at C:\Projects\BoreDoom (or wherever it's cloned)
2. Pull latest from git@github.com:splippers/BoreDoom.git
3. In Unity Editor via MCP:
   a. Create a "Dollhouse" scene — add DollhouseManager + DollhouseRoomSlicer GameObjects
   b. Add a Camera child to DollhouseManager, orthographic, facing down — assign DollhouseSnapshotCamera
   c. On the DollhouseManager, create 4 room materials: Chaotic (red), InProgress (yellow), Clean (green), Pristine (cyan)
   d. Wire up the HUDController + SessionSummaryUI Canvas in the main AR scene
   e. Add a LineRenderer prefab for DollhouseHoverTrail
4. Build to Quest 3 via OpenXR → run first smoke test
5. Report build result back to CraicKen
```

### opencode (agent)
**Task: ChoreSelectionUI + new mode routing**
```
File: Assets/Chorewars/Scripts/UI/ChoreSelectionUI.cs
Current state: routes to coverage mode only
Required: add buttons for all 7 modes:
  - Standard Coverage (existing BaseCoverageModeController)
  - Perfect Grid (PerfectGridModeController)
  - Ghost Run (GhostRunModeController)
  - Chaos Assessment (ChaosAssessmentModeController — scan-only, no coverage)
  - Mowing Art (MowingArtModeController)
  - Declutter Dash (DeclutterDashModeController)
  - ChoreWars Battle (ChoreWarsBattleModeController — shows lobby/matchmaking first)
Also update: DollhouseManager.CaptureShareCard() call from SessionSummaryUI share button
```

### eddie (192.168.1.1)
**Task: DollhouseClean.shader (HLSL)**
```
File: Assets/Chorewars/Shaders/DollhouseClean.shader
Write a URP-compatible UnlitShader (Built-in RP fallback too):
- Property _CleanAmount (0=chaotic, 1=pristine)
- At 0: dark grungy colour + noise overlay
- At 0.5: lerp to mid-tone
- At 1: bright clean colour + subtle sparkle (sin-wave UV distortion on a highlight channel)
- Also add _RoomColour property so each room can have unique base tint
This shader drives DollhouseManager room visual state changes.
```

### lave (192.168.1.65)
**Task: LocalNetworkAPI UDP enhancement for ChoreWars**
```
File: Assets/Chorewars/Scripts/Integration/LocalNetworkAPI.cs
Current: UDP broadcast port 27877, HandleIncomingMessageJson() stub
Required additions:
  1. public event System.Action<float> OnCoverageBroadcastReceived
     — fired when UDP message type == "coverage_update"
  2. public void BroadcastCoverageUpdate(float pct)
     — sends {"type":"coverage_update","pct":47.3,"player":"<device name>"}
  3. Coroutine-based receive loop (UdpClient.ReceiveAsync polling)
  4. Parse incoming JSON and fire OnCoverageBroadcastReceived
```

### gemini (agent)
**Task: DeclutterDashModeController — OVRSceneObject API research + hardening**
```
The DeclutterDashModeController.cs uses OVRSceneObject.Classification.Labels.
Research: is this the correct Meta XR SDK 66+ API for querying classified scene anchors?
If not, document the correct API.
Also: the mode needs a visual AR indicator (floating arrow / glow) above each uncluttered item.
Implement indicator spawning in ScanForClutter() using the collectVfxPrefab as a billboard.
File: /mnt/SANDIEGO/Projects/boredoom/Assets/Chorewars/Scripts/Modes/DeclutterDashModeController.cs
```

### vroomfondel (192.168.1.3)
**Task: PlayerProfile persistence for GhostRunModeController**
```
Current: GhostRunModeController saves to PlayerPrefs (PlayerPrefs.SetString("ghost_hoovering", json))
Required: Create Assets/Chorewars/Scripts/Core/PlayerProfile.cs
  - Singleton with DontDestroyOnLoad
  - Dictionary<string, GhostData> bestGhosts — keyed by choreMode string
  - Save/Load via Application.persistentDataPath JSON file (not PlayerPrefs)
  - Method: TryGetBestGhost(string choreMode, out List<Vector3> path, out float duration)
  - Method: SaveGhost(string choreMode, List<Vector3> path, float duration)
Then update GhostRunModeController to use PlayerProfile instead of PlayerPrefs.
```

### marvin (192.168.1.2 — coordinator)
**Task: git push + CraicKen doc update**
```
1. git add all new files in /mnt/SANDIEGO/Projects/boredoom/Assets/Chorewars/
2. git commit -m "feat: dollhouse system + 7 viral chore modes (AI sprint 2026-05-21)"
3. git push origin main
4. Update CraicKen entry #157 (BoreDOOM workover) with "SPRINT ACTIVE — 7 agents assigned"
5. POST to /api/v1/memory/craic a status update when each agent completes their task
```

---

## Key Interfaces to Respect

```csharp
// BaseCoverageModeController virtual hooks (already live in repo):
protected virtual void OnModeBegun() { }
protected virtual void OnModeEnded(ChoreResult result) { }
protected virtual void OnTrackedPositionSampled(Vector3 worldPos) { }
protected virtual float GetBonusPoints() => 0f;

// DollhouseManager:
public void RegisterRoom(string roomId, Renderer[] renderers)
public void OnCoverageUpdate(string roomId, float coveragePct)
public Vector3 WorldToDollhouse(Vector3 worldPos)  // scale 1:20

// LocalNetworkAPI (after lave update):
public event System.Action<float> OnCoverageBroadcastReceived
public void BroadcastCoverageUpdate(float pct)
```

---

## Definition of Done
- [ ] Quest 3 build launches, shows dollhouse in corner of pass-through view
- [ ] Hoovering session updates dollhouse room colour in real-time
- [ ] Share button captures + saves 1080x1080 card to gallery
- [ ] All 7 modes selectable from ChoreSelectionUI
- [ ] Two phones on same LAN can race ChoreWars Battle mode
- [ ] Ghost Run replays previous best session as translucent AR line
