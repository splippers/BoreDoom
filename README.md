# BoreDOOM: Chorewars 💀🧹
### Turn boring household chores into a game you can win

**BoreDOOM: Chorewars** is a playful, real‑world game that turns everyday household chores into score‑based challenges using augmented reality.

Clean your kitchen. Mow your lawn. See what you actually covered.  
Earn points, streaks, and a small sense of triumph where there was once only dread.

Built for fun first — with real movement and real results.

---

## What is BoreDOOM?

BoreDOOM is a **household role‑playing game**.

Instead of pretending chores don’t exist, it embraces them and asks:
> *What if cleaning counted for something?*

Using *Meta Quest 3 passthrough AR*, BoreDOOM can:
- Visualise where you’ve cleaned or mowed
- Highlight missed spots
- Score coverage, efficiency, and consistency
- Turn physical effort into visible progress

Think *Pokémon Go*, but for the life admin you already have to do.

---

## Why it exists

Chores are boring.  
That doesn’t mean they have to feel pointless.

BoreDOOM is about:
- Making movement visible
- Making effort feel fair
- Making maintenance mildly heroic

No guilt. No nagging. No “productivity optimisation”.
Just a game layered on top of real life.

---

## Core experiences (early prototypes)

### 🏠 BoreDOOM! Indoors
- Vacuuming / hoovering coverage tracking
- Visual overlays showing cleaned vs missed areas
- Simple scoring based on coverage and efficiency

### 🌳 BoreDOOM! Outdoors
- Lawn mowing coverage tracking
- Visual paths and overlap detection
- “You missed a bit” — but make it fun

### 🧠 Chorewars Meta (shared systems)
- Player profiles
- Streaks and history
- Cross‑chore scoring
- Light progression (because number go up good)

---

## Current status

🚧 **Early scaffolding / experimental prototype**

This repository is intentionally public:
- To encourage forks, experiments, and improvements
- To keep the scope honest
- To let someone, somewhere, finish it and finally enjoy doing the dishes

> “I just want to clean my house and get a score for it.”

If that resonates with you — you belong here.

---

## Tech overview (for the curious)

- Unity (LTS 2022/2023 recommended)
- Meta Quest 3
- OpenXR
- Meta XR SDK (passthrough AR)
- C#
- Unity XR Interaction Toolkit (optional)

The goal is **believable feedback**, not technical perfection.

---

## Spatial tracking (prototype)

BoreDOOM experiments with environment mesh capture to:
- Track where movement happened
- Visualise coverage during a chore session
- Export simple spatial snapshots for analysis

Current focus:
- Single‑session feedback
- Immediate visual reward

Multi‑session, whole‑house persistence is a *future* goal — not required to have fun.

---

## Philosophy

This is **not**:
- A productivity app
- A parenting tool
- A surveillance system
- A guilt engine

It *is*:
- A game
- Opt‑in
- Slightly ridiculous
- Surprisingly motivating

If you laugh while hoovering, it’s working.

---

## Getting started

1. Clone the repo
2. Open in Unity (matching the recommended version)
3. Install Meta XR SDK + OpenXR
4. Open `Assets/Chorewars/Scenes/Bootstrap.unity`
5. Build for Quest 3
6. Clean something you were already avoiding

---

## Contributing

Fork it. Break it. Finish it.  
Ideas, PRs, experiments, and wild forks are welcome.

See:
- `CONTRIBUTING.md`
- `ROADMAP.md`
- `docs/` for build notes and experiments

---

## The future (intentionally vague)

Better feedback.  
More chores.  
More laughter.  
Possibly fewer arguments about whose turn it is.

---

## TL;DR

You already do chores.  
**BoreDOOM makes them count.**
