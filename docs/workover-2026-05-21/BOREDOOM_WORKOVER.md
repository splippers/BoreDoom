# BoreDOOM! — Full Design Workover
### The 3D Dollhouse System + 7 Viral AR Chore Scenarios
*Claude Code / 2026-05-21 — based on full codebase read*

---

## What you already have (and it's good)

The existing codebase is further along than the audit suggested. You have real, working infrastructure:

- **`SpatialMeshTracker`** — captures environment meshes per-frame from XRMeshSubsystem
- **`HouseMapRecorder`** — multi-session OBJ snapshots with manifest, zip export, persistent path
- **`CoverageMap`** — 2D grid (25cm cells) tracking visited positions in world-space
- **`PathTracker`** — records movement positions with minimum-distance gating
- **`HomeOriginAligner`** — aligns all scans to a persistent spatial anchor across sessions
- **`ScoringEngine`** — grades S/A/B/C/D from coverage + efficiency + movement
- **`PlayerProfile`** — history of all sessions, lifetime points
- **6 chore modes** already registered: Hoover, Mow, AtticAttack, BasementBust, GarageBarrage, ShedDread

The architecture is clean. `BaseCoverageModeController` handles the common loop. Every new mode is thin.

**The gap:** You have the tracking and the scoring. What you don't have yet is the thing that makes someone post it on TikTok. That is the Dollhouse.

---

## I. The 3D Dollhouse — The Central Idea

### The Concept

As you clean your house wearing the Quest 3, BoreDOOM builds a real-time miniature 3D model of your home. It sits on your coffee table — or floating in front of you — at roughly 1:20 scale. It is your actual house, stylised, alive.

Rooms that are dirty glow red-grey. Rooms mid-clean pulse yellow. Rooms you've finished sparkle green. The miniature hoover trails your real movement inside it.

When you're done, you get a **Dollhouse Snapshot** — a top-down screenshot of your home with colour-coded rooms. That's the share card. That's what goes viral.

### Why it works

- It makes invisible effort **visible** — you can *see* what you did
- It makes your home **legible** — most people have never seen their floor plan
- The before/after is **emotionally satisfying** in the same way pressure-washing videos are
- The share card is **inherently personal** — it's *your* house, not a generic graphic
- It connects the **physical act** (hoovering) to a **digital reward** (beautiful clean dollhouse)

### The Matterport-Killer angle

Meta Quest 3's scene understanding + your `SpatialMeshTracker` already builds a mesh of your home. The dollhouse IS that mesh, just styled and miniaturised. You are building a free, gamified Matterport. Over multiple sessions, persistent anchors (`HomeOriginAligner` already handles this) accumulate a complete house scan.

---

## II. Dollhouse Technical Architecture

### New components needed

```
Assets/Chorewars/Scripts/Dollhouse/
  DollhouseManager.cs          ← Central controller, owns room states
  DollhouseRoomState.cs        ← Enum: Chaotic / InProgress / Clean / Pristine
  DollhouseMiniaturizer.cs     ← Takes world mesh, renders at 1:20 scale
  DollhouseRoomSlicer.cs       ← Segments mesh into named rooms via XR Scene API
  DollhouseCleanShader.shader  ← Animates dirty→clean gradient per room
  DollhouseSnapshotCamera.cs   ← Overhead ortho camera → share card Texture2D
  DollhouseHoverTrail.cs       ← Miniature tool follows real PathTracker positions
```

### DollhouseManager (core)

```csharp
public class DollhouseManager : MonoBehaviour
{
    [SerializeField] private Transform dollhousePivot;   // placed on detected plane
    [SerializeField] private float scale = 0.05f;        // 1:20

    private Dictionary<string, DollhouseRoomState> _roomStates = new();

    public void OnCoverageUpdate(string roomId, float coveragePct)
    {
        _roomStates[roomId] = coveragePct switch
        {
            >= 90f => DollhouseRoomState.Pristine,
            >= 60f => DollhouseRoomState.Clean,
            >= 20f => DollhouseRoomState.InProgress,
            _      => DollhouseRoomState.Chaotic
        };
        RefreshRoomVisual(roomId);
    }

    public Texture2D CaptureShareCard() => GetComponent<DollhouseSnapshotCamera>().Capture();
}
```

### Room segmentation

Meta Quest 3 Scene API (`OVRScene`) already understands rooms. `OVRSceneRoom` gives you a room volume. Use that to:
1. At session start: enumerate rooms → create room state entries in `DollhouseManager`
2. During session: for each position in `PathTracker.worldPositions`, find which `OVRSceneRoom` contains it → update that room's coverage

The Scene API is already in your Meta XR SDK. No new scanning needed.

### The Clean Shader

Simple URP/Built-in shader that lerps between a dirty (grey, desaturated) and clean (warm, bright) state per room material based on a `_CleanProgress` float [0..1]:

```hlsl
// DollhouseClean.shader (simplified)
half4 frag(v2f i) : SV_Target {
    half4 dirty = SAMPLE_TEXTURE2D(_DirtyTex, sampler_DirtyTex, i.uv);
    half4 clean = SAMPLE_TEXTURE2D(_CleanTex,  sampler_CleanTex,  i.uv);
    half sparkle = _CleanProgress > 0.8 ? sin(i.uv.x * 40 + _Time.y * 8) * 0.1 : 0;
    return lerp(dirty, clean, _CleanProgress) + sparkle;
}
```

### The Share Card

`DollhouseSnapshotCamera` renders the dollhouse from directly above at the session end:
- 1080×1080 overhead orthographic
- Rooms colour-coded by state
- Score + grade overlaid (use Unity UI `RenderTexture`)
- Saved to `Application.persistentDataPath/share-{sessionId}.png`
- Accessible via ADB or from a companion phone app

---

## III. The 7 Viral AR Chore Scenarios

These are modes. Each inherits from `BaseCoverageModeController` and adds a twist.

---

### 1. THE PERFECT GRID (Hoovering)
**`PerfectGridModeController : BaseCoverageModeController`**

The challenge: hoover your room in **perfectly parallel stripes** like a football pitch.

AR renders a ghost grid of parallel lines across your floor. Hoovering along each line turns it from white to green. Miss a stripe = stays red.

**Why viral:** Satisfying completion content. The aesthetic is already beloved (carpet stripes, pressure washing). The BoreDOOM version adds competitive stakes: *can you do it perfectly?* The final grid screenshot — every stripe green — is an inherently shareable image.

**Scoring tweak:** 
- Coverage score ×1 (standard)
- Alignment score ×1.5 (how well your path follows the grid lines)
- Perfection bonus: if every stripe >95% covered = S+ grade

**Share card:** Overhead view of the striped carpet with your path overlaid. The stripes are instantly recognisable.

**Unlock:** Pattern library — diagonal, herringbone, spiral, concentric squares.

---

### 2. SPEED RUN (Any chore)
**`SpeedRunModeController : BaseCoverageModeController`**

Beat the clock. Each room has a par time based on its area (roughly `area_m² × 45 seconds`).

AR HUD shows a split timer per zone. A ghost from your personal best runs ahead of you.

**Why viral:** Competitive content. People share their times — "Kitchen: 4:23, beat that." The ghost adds the racing game feeling to real housework. Streamers will compete live.

**Global leaderboard hook:** Anonymised kitchen-under-10m² world record. You're not just racing yourself.

**Scoring tweak:** Time bonus = `(parTime / actualTime) × 200 points` added on top of coverage.

**Share card:** Split times per room + world rank.

---

### 3. CHAOS METER (Scanning / Assessment)
**`ChaosAssessmentModeController : MonoBehaviour`** *(not coverage-based)*

This isn't a chore mode — it's a **scan mode**. Put on the Quest, walk through your house without cleaning anything. BoreDOOM's spatial mesh + object density estimation scores each room's current chaos level.

- 🟢 0–3: Minimalist — suspicious.
- 🟡 4–6: Lived in — honest.
- 🟠 7–8: Chaos adjacent.
- 🔴 9–10: DOOMED.

The dollhouse appears with fire/smoke effects on the high-chaos rooms.

**Why viral:** This is a personality test disguised as a housework tool. People share embarrassing things about themselves constantly (Credit scores, Spotify Wrapped, personality tests). "My house is 8.4 chaos. The kitchen is on fire." This format prints content.

**Technical hook:** Use `SpatialMeshTracker` mesh density (mesh triangle count per m²) as a chaos proxy. Dense meshes = lots of stuff on surfaces = chaos.

**Share card:** Dollhouse with flame/smoke effects. Chaos score prominently displayed. "YOUR HOUSE IS DOOMED."

---

### 4. BATTLE MODE — ChoreWars Multiplayer
**`ChoreWarsBattleModeController`**

Two or more players, same house, competing in real time. Each player's path is a different colour. Zones are contested — hover over a zone to "claim" it.

**Household variants:**
- **Parent vs Child:** Children earn screen time minutes by covering zones. Parents can steal zones back.
- **Timed Blitz:** 5 minutes, who covers more?
- **King of the Room:** One room is contested. Most coverage at the end wins.

**Why viral:** Family content. TikTok parent-child dynamics. Creators will film themselves literally sprinting to clean zones before their kids do. The overhead dollhouse view showing two colour trails converging on the kitchen is visually compelling.

**Technical:** `LocalNetworkAPI.cs` already exists. Extend it to broadcast player positions + coverage events via UDP multicast. No server needed.

**Share card:** Overhead view of the house with two colour-coded coverage maps overlaid.

---

### 5. THE MOWING MASTERPIECE (Outdoors)
**`MowingArtModeController : BaseCoverageModeController`**

Mow your lawn in the pattern of a design: **sports club badge, famous artwork, Celtic knot, custom initials, spiral.**

AR overlays the target pattern on your lawn. Following the guide lines turns them green. Deviation from the pattern reduces your art score.

**Why viral:** Lawn art already goes viral on its own. BoreDOOM makes it accessible to anyone with a mower and a Quest. The before/after aerial (drone shot or just the share card from above) is shareable content in a format people already love.

**Pattern library:** Basic patterns free. Club badges, custom design upload = premium.

**Share card:** Overhead view of the lawn pattern. Grade based on accuracy. "I mowed a shamrock. Grade A."

---

### 6. THE GHOST RUN (Any chore — next-session feature)
**`GhostRunModeController : BaseCoverageModeController`**

Your previous best session plays back as a **translucent ghost** in the AR. You can see where Past You walked, how fast, which zones they hit first. Race your ghost.

**Why viral:** The "ghost race" is a beloved game mechanic (Mario Kart, any racing game). Applying it to cleaning is absurdist enough to be funny, useful enough to actually work.

**Technical:** `PlayerProfile.history` already stores sessions. `PathTracker.worldPositions` is a recorded path. Replay it as a `LineRenderer`/translucent character moving at the recorded pace.

**Share card:** Your path vs your ghost path, side by side. Time improvement shown.

---

### 7. THE DECLUTTER DASH (New chore type — object pickup)
**`DeclutterDashModeController : MonoBehaviour`** *(not coverage-based)*

Instead of area coverage, this mode is about **picking up items**. 

On session start, Quest 3 scene understanding identifies surfaces and estimates object presence. Each detected object gets an AR floating star/ring above it in the Quest view. Pick up 20 items in 3 minutes to complete the challenge.

Each pickup = **satisfying particle burst + sound + points**. The rhythm of pickup-burst-points becomes addictive.

**Daily challenge:** Today's challenge is always 20 items in 3 minutes. Streak maintained by completing it daily.

**Why viral:** Two things: (1) the visual of AR halos floating over real clutter in your home is instantly compelling when shared. (2) The satisfaction mechanic — burst-points — is the same dopamine loop as Pokémon GO catching. People will film their AR cluttered living room and post it.

**Technical:** `OVRSceneAnchor` + `OVRSceneObject` can tag detected tables, shelves, floors. Density of mesh triangles above a plane = clutter score. Approximate object count from depth data.

---

## IV. The Social Layer — How It Goes Viral

Every session ends with a **Share Card** rendered by `DollhouseSnapshotCamera`:

```
┌─────────────────────────────────────┐
│  [OVERHEAD DOLLHOUSE IMAGE]         │
│                                     │
│  ██ LIVING ROOM  83% clean          │
│  ██ KITCHEN      91% clean          │  
│  ▒▒ BEDROOM     34% (you gave up)   │
│                                     │
│  Grade: A    Points: 847            │
│  Streak: 🔥7 days                   │
│                                     │
│  boredoom.splippers.com             │
└─────────────────────────────────────┘
```

Saved to device as a 1080×1080 PNG. One tap to share.

The overhead dollhouse view is the key. It looks like a beautiful game screenshot — but it's your actual house. That juxtaposition is the content.

---

## V. Room-State Progression (The RPG Layer)

The dollhouse isn't static — it improves as you maintain your home.

| Streak | Room reward |
|--------|-------------|
| 1 clean | Room glows (basic clean state) |
| 3-day streak | Furniture appears in the room |
| 7-day streak | Room gets a decorative skin |
| 14-day streak | Room "levels up" — changes aesthetic tier |
| 30-day streak | Prestige skin unlocked (golden/haunted/retro) |

**This is the RPG loop.** You're not just cleaning. You're building and decorating a digital home. The dollhouse becomes something you want to protect — which means you keep cleaning.

---

## VI. Monetisation

| Revenue line | Mechanic |
|---|---|
| **Free** | Base app, all chore modes, standard dollhouse skin |
| **Skins £1.99** | Dollhouse aesthetics: Retro (70s), Haunted House, Futurist, Ikea Minimal |
| **Pattern Pack £0.99** | Lawn mowing patterns: club badges, Celtic designs, custom upload |
| **ChoreWars Family £2.99/mo** | Up to 6 players, shared household leaderboard, streak bonuses |
| **Streak Savers £0.49** | Miss a day, buy it back |
| **Custom Dollhouse Import £4.99** | Upload your actual floor plan — perfect room labelling from day 1 |

No subscription for solo play. IAP only. App free.

---

## VII. Immediate Build Priorities

Given the existing codebase, here's the order that maximises momentum:

### Sprint 1 (1-2 weeks) — The Dollhouse MVP
1. Add `DollhouseManager.cs` + `DollhouseRoomState.cs`
2. Wire `OVRSceneRoom` room enumeration to `DollhouseManager`
3. Simple clean shader (dirty grey → clean white, no sparkle yet)
4. Place dollhouse on detected floor plane at 1:20 scale
5. Connect `PathTracker` → dollhouse mini-trail
6. **Goal:** You can see a miniature version of your house getting cleaned as you hoover

### Sprint 2 (1 week) — The Share Card
1. `DollhouseSnapshotCamera` overhead render
2. UI overlay (room names, coverage %, score, grade)
3. Save as PNG to device storage
4. Quest share sheet integration
5. **Goal:** Every session ends with a shareable image

### Sprint 3 (1-2 weeks) — Perfect Grid + Speed Run
1. `PerfectGridModeController` with AR stripe overlay
2. `SpeedRunModeController` with par time and ghost trail
3. Both generate share cards with mode-specific stat overlays
4. **Goal:** Two viral-format modes shipping

### Sprint 4 — Chaos Meter + Battle Mode
1. `ChaosAssessmentModeController` using mesh density
2. `ChoreWarsBattleModeController` via `LocalNetworkAPI`
3. Fire/smoke effects on chaos rooms in dollhouse
4. **Goal:** Family content creator bait

---

## VIII. The Pitch Line

> **BoreDOOM** turns your home into a game level.  
> Your hoover is the weapon. Your coverage map is the score.  
> Your dollhouse is the trophy.  
> *You already have to clean. You might as well win.*

---

*Full design workover by Claude Code — 2026-05-21. All C# stubs ready for implementation. Unity 6000.4.5f1 · Meta Quest 3 · Built-in RP.*
