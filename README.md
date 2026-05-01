# Chorewars: BoreDoom! AR (Meta Quest 3)

100% AR chore game for Meta Quest 3.
Hoover your kitchen, mow your lawn, see your coverage in AR, and score points for real-world movement.

## Vision
- Use Meta Quest 3 passthrough AR to:
  - Track where you've cleaned or mowed
  - Visualise coverage and missed spots
  - Score efficiency, coverage, and consistency
- Deliver real physical benefits (steps, cardio, movement) like Pokémon Go, but for chores.

## Status
Early scaffolding. This repo is intentionally public so others can fork it, extend it, and ship it.

The original creator’s goal:
> “I just want someone to fork the repo and complete it so I can do the dishes and score points.”

## Tech stack
- Unity (LTS, 2022/2023 recommended)
- OpenXR
- Meta XR SDK (Quest 3, passthrough AR)
- C#
- Unity XR Interaction Toolkit (optional but recommended)

## Core modules
- BoreDoom! Indoors – Hoovering coverage tracking
- BoreDoom! Outdoors – Lawn mowing coverage tracking
- Chorewars Meta – Profiles, scoring, streaks, history

## Related repo
- **ChoreWars (meta layer)**: `https://github.com/splippers/ChoreWars`
  - Intended home for profiles, progression, streaks, and cross-mode scoring that can be shared across multiple “BoreDoom!” experiences.

## Getting started
1. Clone the repo
2. Open in Unity (matching the documented version)
3. Install Meta XR SDK + OpenXR
4. Open `Assets/Chorewars/Scenes/Bootstrap.unity`
5. Build for Quest 3

See `CONTRIBUTING.md` and `ROADMAP.md` for how to help.
See `docs/quest3-build.md` and `docs/retrieving-exports.md` for device wiring + pulling exported meshes.

## Spatial mapping / “Matterport-ish” capture (prototype)
This repo includes a starter `XRMeshSubsystem`-based mesh capture script: `SpatialMeshTracker`.

- During a session (e.g. Hoovering), it can poll environment meshes (when available) and export a snapshot OBJ to `Application.persistentDataPath`.
- Notes and next steps are in `docs/spatial-mapping.md`.

## Whole-house reconstruction (contiguous map)
To build a stable, multi-session 3D recreation of your home (Matterport-ish), you need a **persistent reference frame**.

- This repo stubs `PersistentAnchors` as the integration point for Meta XR persistent anchors.
- The current prototype focuses on **capturing meshes and exporting snapshots**; anchor-based alignment is the next step to make scans “stick” across sessions.
