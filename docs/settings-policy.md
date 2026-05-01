## Settings policy (BoreDOOM)

This repo intentionally commits only a small, stable subset of Unity settings so collaborators can build reliably without committing machine-specific or noisy files.

### Tracked

- `ProjectSettings/ProjectVersion.txt`
  - Pins the Unity editor version (`6000.4.5f1`).
- `ProjectSettings/EditorBuildSettings.asset`
  - Defines which scenes are included in builds and the startup order.

### Not tracked

Everything else under `ProjectSettings/` is ignored by default. These files tend to:

- Change frequently and create noisy diffs
- Encode environment-specific defaults
- Sometimes reflect Unity Services / project linkage (`UnityConnectSettings.asset`)

If we later need to make a particular setting reproducible (Quest build quirks, XR loader defaults, etc.), we can explicitly un-ignore and commit a specific file.

