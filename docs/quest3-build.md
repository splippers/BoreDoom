# Quest 3 build checklist (prototype)

This repo is a scaffold intended to be opened in Unity and extended.

## Unity version
`ProjectSettings/ProjectVersion.txt` pins `2022.3.24f1` (LTS). Use 2022 LTS unless you have a reason to upgrade.

## Required installs
- Meta XR SDK (import into `Assets/Meta/`)
- Enable OpenXR for Android (Quest) and the Meta runtime features you need (passthrough + meshing/scene)

## Prototype wiring (minimal)
In your hoovering scene:

- Create an empty GameObject `AR`
  - Add `ARSessionManager`
  - Add `SpatialMeshTracker`
  - (optional) Add `HomeOriginAligner` and set `providerComponent` to an `IHomeOriginProvider` implementation
  - Add `HouseMapRecorder` (optional) and assign its `meshTracker`

- Create a GameObject `HooverMode`
  - Add `HooverModeController`
  - Assign:
    - `trackedTool` (controller/hand object you want to track)
    - `coverageMap` (add `CoverageMap` to a GameObject and assign)
    - `spatialMeshTracker` (the tracker on `AR`)
    - `houseMapRecorder` (optional)

## On-device home origin + export buttons (debug UI)
To avoid needing editor tweaks every run, add a small world-space Canvas and wire buttons to:
- `HomeOriginDebugUI.SetHomeOriginToCurrent`
- `HomeOriginDebugUI.ApplyHomeOrigin`
- `HomeOriginDebugUI.ExportCombinedObj`

`HomeOriginDebugUI` expects references to:
- `HomeOriginAligner`
- `SpatialMeshTracker`

## Meta XR persistent anchors
This repo includes `MetaXrHomeOriginProvider` behind a compile define so it compiles without Meta packages.

When you install Meta XR SDK, define `CHOREWARS_META_XR` in Player Settings to enable that code path.

## Where exports go
On device, OBJ snapshots are written to:
- `Application.persistentDataPath`
- and/or `Application.persistentDataPath/house-map/` for periodic snapshots

You can retrieve these via Android tooling (ADB) once you know the package name.

