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
  - Add `HouseMapRecorder` (optional) and assign its `meshTracker`

- Create a GameObject `HooverMode`
  - Add `HooverModeController`
  - Assign:
    - `trackedTool` (controller/hand object you want to track)
    - `coverageMap` (add `CoverageMap` to a GameObject and assign)
    - `spatialMeshTracker` (the tracker on `AR`)
    - `houseMapRecorder` (optional)

## Where exports go
On device, OBJ snapshots are written to:
- `Application.persistentDataPath`
- and/or `Application.persistentDataPath/house-map/` for periodic snapshots

You can retrieve these via Android tooling (ADB) once you know the package name.

