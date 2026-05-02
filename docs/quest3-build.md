# Quest 3 build checklist (prototype)

This repo is a scaffold intended to be opened in Unity and extended.

## Unity version
`ProjectSettings/ProjectVersion.txt` pins **6000.4.5f1**. Match this editor version when opening the project.

## Required installs
- Meta XR SDK (import into `Assets/Meta/`)
- Enable OpenXR for Android (Quest) and the Meta runtime features you need (passthrough + meshing/scene)

## Canonical scene wiring (multi-session + multi-room)
Create this hierarchy:

- `XR` (optional grouping)
- `AR`
  - `ScanRoot`
    - `HomeOriginAligner`
      - `providerComponent` → `MetaXrHomeOriginProvider` (recommended) **or** `PersistentAnchors` (stub)
    - `SpatialMeshTracker`
      - set `meshParent` → `ScanRoot` (recommended)
    - `HouseMapRecorder` (optional)
      - assign `meshTracker` → `SpatialMeshTracker`
      - assign `scanRoot` → `ScanRoot`
  - `MetaXrHomeOriginProvider` (can live anywhere; referenced by aligner)

- `ScanSession`
  - `HouseScanSessionController`
    - `spatialMeshTracker` → `AR/ScanRoot/SpatialMeshTracker`
    - `houseMapRecorder` → optional
    - `coverageMap` → your `CoverageMap` component (usually on `ScanRoot` or a sibling)
    - `pathTracker` → optional
    - `homeOriginAligner` → `AR/ScanRoot/HomeOriginAligner`

- `RuntimeUI`
  - `RuntimeScanPanel` (auto-finds components if left empty)

- Optional path visualization:
  - Put `PathTracker` on the tracked controller/hand object
  - Add `LineRenderer` + `PathLineVisualizer`

## Player Settings (Meta XR anchors)
When Meta XR SDK is installed, add scripting define symbol:

- `CHOREWARS_META_XR`

This enables `MetaXrHomeOriginProvider` and the extra UI readouts in `RuntimeScanPanel`.

## On-device flow (prototype)
1. Stand at your chosen “home corner” (same spot across days).
2. Move `ScanRoot` to that physical spot/orientation (or move the object `HomeOriginAligner` is on).
3. Press **Set Home Origin** (saves a persistent spatial anchor UUID).
4. Press **Apply Home Origin** (loads + aligns; may take a moment on cold start).
5. **Start scan**, walk RoomA → RoomB, **Stop scan**, **Export combined OBJ**.
6. Optional: take multiple snapshots across sessions, then press **Merge house-map snapshots → one OBJ** to package everything for offline reconstruction tooling.
7. Optional: press **Zip house-map folder** to create a single `.zip` of `house-map/` (OBJ snapshots + manifests) for easy `adb pull`.
8. If you need to reset anchor state during testing: **Erase Home Origin** (clears persistent storage + local prefs).

## Where exports go
On device, OBJ snapshots are written to:
- `Application.persistentDataPath`
- and/or `Application.persistentDataPath/house-map/` for periodic snapshots

You can retrieve these via Android tooling (ADB) once you know the package name.

See also: `docs/retrieving-exports.md`.

## On-device telemetry log (`MQ3.log`)

Runtime diagnostics append **JSON lines** to:

- `{persistentDataPath}/MQ3.log`

When the file exceeds ~10 MB it is rotated to `MQ3.1.log`. Implemented by `Chorewars.Diagnostics.TelemetryLogger`.

Replace the package id if it differs from **Player Settings → Package Name**.

Typical external-storage path (works on many Quest builds):

```bash
adb pull /sdcard/Android/data/com.DefaultCompany.BoreDoom/files/MQ3.log .
```

If `adb pull` fails (scoped storage), use **Development Build** and log `Application.persistentDataPath` once, or inspect device files with Android Studio / Quest file browser.

