# Hackathon 7.0

A top-down Unity game. You're a prisoner chained to a heavy **Ball**. Lure **Prey** out of dark **Pond** corridors with **Bait**, survive the **Catch** minigame, sell the Prey for **Money**, and buy **Chisel Kits** to break the Ball. Then outrun the **Ghost** in the final **Escape**.

## Setup

- Unity **6000.6.2f1** (Unity 6) with URP.
- Open the project folder in Unity Hub, then open `Assets/Scenes/SampleScene.unity` and press Play.
- Packages: Cinemachine 6, Input System, TextMeshPro, DOTween (in `Assets/Plugins/Demigiant`).

## Controls

| Input | Action |
|-------|--------|
| WASD | Move; during a Catch, steer the Needle |
| E | Enter / leave a Building (Store, Pond, Lift) |
| Left mouse (hold) | Grab and drag Prey; pull the Lift lever |

## Gameplay loop

1. **Store**: buy Bait (3 tiers) or a Chisel Kit.
2. **Pond**: choose Bait. Incense burns, then a Prey bites and the **Catch** starts: keep the Needle inside the moving Area. Fail and you die (respawn, nothing lost).
3. **Sell**: drag the Prey into the Store, or put it on the **Lift** and pull the lever to raise it to the Sell Height.
4. Five Chisel Kits break the Ball and make you faster. Then reach the Exit before the Ghost catches you.

## Project layout

| Path | What's there |
|------|--------------|
| `Assets/Scripts/` | All gameplay code (one MonoBehaviour per file) |
| `Assets/Scenes/SampleScene.unity` | The only scene |
| `Assets/Prefabs/Prey/` | Prey prefabs and their Catch visuals |
| `Assets/ClaudeTasks/` | Task briefs (Ukrainian) |
| `Assets/ClaudeTasks/docs/` | Task plans, English + Ukrainian ([index](Assets/ClaudeTasks/docs/README.md)) |

## Docs

- [CONTEXT.md](CONTEXT.md): game vocabulary and known open questions.
- [AGENTS.md](AGENTS.md): code style and rules for AI agents (Claude reads it via `CLAUDE.md`).
- [Assets/GUIDELINES.md](Assets/GUIDELINES.md): the original team style guide (Ukrainian).

## Working in parallel

Each task in `Assets/ClaudeTasks/docs/` runs on its own `task/NNNN-…` branch and ends with a PR. Only one branch at a time should edit `SampleScene.unity`; scene wiring is done by hand from each plan's Inspector checklist.
