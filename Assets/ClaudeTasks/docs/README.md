# Task plans

English plans are the source of truth; `*.uk.md` files are Ukrainian copies. Keep both in sync when a plan changes.

| # | Task | Branch | Files it may touch |
|---|------|--------|--------------------|
| 0001 | [Use Indicator on Buildings](0001-use-indicator.md) | `task/0001-use-indicator` | `Building.cs`, `PlayerMovement.cs` |
| 0002 | [Hide Player outside Regular](0002-player-visibility.md) | `task/0002-player-visibility` | new `PlayerVisibility.cs` |
| 0003 | [Lift Prey resizing](0003-lift-prey-resizing.md) | `task/0003-lift-prey-resizing` | `LiftPlatform.cs`, `Lift.cs`, `InteractableObject.cs` |
| 0004 | [Corridor Darkness](0004-corridor-darkness.md) | `task/0004-corridor-darkness` | new `CorridorDarkness.cs`, new shader, `PreyFace/*.mat` |

The four tasks touch different files and none of them edit `SampleScene.unity`, so they can run at the same time. Each one ends with a PR plus an Inspector checklist for the user.

## Prompt to paste into a new chat

Replace `NNNN-name` with the task file:

```
Read AGENTS.md, CONTEXT.md and Assets/ClaudeTasks/docs/NNNN-name.md, then carry out that plan end to end:
create the branch it names from an up-to-date main, change only the files it allows, commit, push, and open a
PR with `gh pr create` that includes the plan's Inspector checklist. Don't touch SampleScene.unity. If something
in the plan contradicts the code, stop and tell me instead of guessing.
```
