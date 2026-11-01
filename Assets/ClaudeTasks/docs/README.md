# Task plans

English plans are the source of truth; `*.uk.md` files are Ukrainian copies. Keep both in sync when a plan changes.

| # | Task | Branch | Files it may touch |
|---|------|--------|--------------------|
| 0001 | [Use Indicator on Buildings](0001-use-indicator.md) | `task/0001-use-indicator` | `Building.cs`, `PlayerMovement.cs` |
| 0002 | [Hide Player outside Regular](0002-player-visibility.md) | `task/0002-player-visibility` | new `PlayerVisibility.cs` |
| 0003 | [Lift Prey resizing](0003-lift-prey-resizing.md) | `task/0003-lift-prey-resizing` | `LiftPlatform.cs`, `Lift.cs`, `InteractableObject.cs` |
| 0004 | [Corridor Darkness](0004-corridor-darkness.md) | `task/0004-corridor-darkness` | new `CorridorDarkness.cs`, new shader, `PreyFace/*.mat` |
| 0005 | [Respawn Ball with Player](0005-death-respawn.md) | `task/0005-death-respawn` | `PlayerStateManager.cs` (`Die()` only), `BallFollower.cs` |
| 0006 | [Shopping sticks, Catching removed](0006-player-states-cleanup.md) | `task/0006-player-states-cleanup` | `PlayerStateManager.cs`, `PlayerMovement.cs`, `PlayerAnimator.cs` — **after #5 and 0005 merge** |
| 0007 | [Sell via Lift only](0007-sell-via-lift-only.md) | `task/0007-sell-via-lift-only` | `Store.cs` |
| 0008 | [Escape win screen](0008-escape-win-screen.md) | `task/0008-escape-win-screen` | `EscapeSequence.cs`, new `EscapeExitTrigger.cs`, new `WinScreen.cs` |
| 0009 | [Shared Pond UI](0009-shared-pond-ui.md) | `task/0009-shared-pond-ui` | `Pond.cs`, new `PondBaitRouter.cs` |

Tasks touch different files and none edit `SampleScene.unity`, so they can run at the same time — except 0006, which waits for PR #5 (0001) and 0005 to merge.

Each task ends with a PR plus an Inspector checklist for the tester. Known issues and trade-offs: [KNOWN-ISSUES.md](KNOWN-ISSUES.md).

## Prompt to paste into a new chat

Replace `NNNN-name` with the task file:

```
Read AGENTS.md, CONTEXT.md and Assets/ClaudeTasks/docs/NNNN-name.md, then carry out that plan end to end:
create the branch it names from an up-to-date main, change only the files it allows, commit, push, and open a
PR with `gh pr create` that includes the plan's Inspector checklist. Don't touch SampleScene.unity. If something
in the plan contradicts the code, stop and tell me instead of guessing.
```
