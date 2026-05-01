# Quest 3 build checklist (prototype)

This repo is a scaffold intended to be opened in Unity and extended.

## Unity version
`ProjectSettings/ProjectVersion.txt` pins `2022.3.24f1` (LTS). Use 2022 LTS unless you have a reason to upgrade.

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

