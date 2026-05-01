# Spatial mapping (Quest 3) — capture + export

This project’s goal is a **working on-device prototype** where you can:
- run passthrough AR
- vacuum (walk) while scanning your home
- accumulate a contiguous map over time
- export meshes for reconstruction (e.g. OBJ/PLY → external tools)

## Reality check (what’s implemented here)
The codebase includes a **Unity/OpenXR mesh-capture path** using `XRMeshSubsystem` that:
- requests environment meshes (when the underlying runtime exposes them)
- caches updated meshes during a scan session
- exports an **OBJ** snapshot into `Application.persistentDataPath`

It also includes `HouseMapRecorder`, which can take periodic snapshots into `Application.persistentDataPath/house-map/`
and write a simple manifest.

## What you still must wire in Unity/Meta SDK
Quest spatial mapping typically depends on Meta’s XR features/settings and permissions.
You will likely need to:
- install the **Meta XR SDK** in `Assets/Meta`
- enable the relevant scene/meshing features in Project Settings
- ensure the app has the right runtime permissions on-device

## Contiguous “whole house” mapping
To build a stable, multi-session house model you’ll want:
- **anchors** (persistent spatial anchors) or a saved reference frame
- a way to align newly scanned meshes with prior scans

This repo stubs the mesh capture/export. Anchor persistence + multi-session alignment is the next step.

