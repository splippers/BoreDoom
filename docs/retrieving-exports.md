# Retrieving exports from Quest 3 (Android)

BoreDoom writes exports under Android’s app-specific storage:

- `Application.persistentDataPath`

Typical locations include:
- `/sdcard/Android/data/<your.package.name>/files/`

## What gets written
- **Combined OBJ exports** from `SpatialMeshTracker.ExportCurrentSnapshotAsObj(...)`
- **`house-map/` folder** (snapshots + manifests) from `HouseMapRecorder`

## Pull files with adb (developer machine)

1. Install Android Platform Tools (`adb`) on your PC.
2. Enable **Developer Mode** + **USB debugging** on the headset.
3. Find your Android package name (Unity **Player Settings → Android → Package Name**).

Examples:

```bash
adb shell ls /sdcard/Android/data/<package>/files/
adb pull /sdcard/Android/data/<package>/files/house-map ./house-map
```

## Notes
- Paths differ by device/OS versions; if `/sdcard/Android/data/...` is empty, use `adb shell` to explore under `/storage/emulated/0/Android/data/...`.
- If you can’t see files, confirm the app has storage permissions (Android 13+ rules vary) and that you’re using the correct package name.
