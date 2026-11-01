# Known issues

Last updated: 2026-11-01. Ukrainian copy: `KNOWN-ISSUES.uk.md`. Update this file when a task fixes or adds an issue.

Nothing in this project is compiled or run by the coding agents (no Unity editor on that machine). The co-developer's play-test of each PR is the first real check.

## Open bugs with a task

| Issue | Where | Task |
|-------|-------|------|
| On Death the Ball stays where the Player died, then the Chain yanks it across the map. The teleport also bypasses the Player's Rigidbody and disables a `CharacterController` that doesn't exist. | `PlayerStateManager.Die`, `BallFollower` | [0005](0005-death-respawn.md) |
| If `fadeScreen` or `playerTransform` is unassigned, `Die()` never returns to `Regular`. The game soft-locks with the Player hidden (since 0002). Currently both are assigned in the scene. | `PlayerStateManager.Die` | [0005](0005-death-respawn.md) |
| `Shopping` is overwritten by `Interacting` straight away, so the HUD hides in the Store. | `PlayerMovement.TryBeginInteraction` | [0006](0006-player-states-cleanup.md) |
| `Catching` state is never entered (dead code). | `PlayerStateManager` | [0006](0006-player-states-cleanup.md) |
| The Player can walk in `Dead` (during the death fade). Only `Interacting` blocks movement. | `PlayerMovement.Move` | [0006](0006-player-states-cleanup.md) |
| There are two ways to Sell: the Store trigger and the Lift. Agreed: Lift only. | `Store.OnTriggerEnter` | [0007](0007-sell-via-lift-only.md) |
| Reaching the Exit does nothing: no script calls `SuccessfulEscape()`, and even if one did, it only logs "WIN!". | `EscapeSequence`, `EscapeSuccessTrigger` in scene | [0008](0008-escape-win-screen.md) |
| Reloading the scene would keep stale `DontDestroyOnLoad` managers that point at destroyed objects. | `PlayerStateManager`, `InputManager`, `HoldManager`, `UIManager` | [0008](0008-escape-win-screen.md) |
| The Bait buttons call `Pond1.SelectBait` directly, so new Ponds can't use the UI. | Scene UI events | [0009](0009-shared-pond-ui.md) |

## Open bugs without a task yet

- **A failed Chisel Kit purchase closes the Store.** `Store.BuyChiselKit` calls `EndInteraction()` even when `SpendMoney` fails. The camera and UI close, but the state stays non-`Regular` until E is pressed. Undecided: should the Store stay open when the purchase fails?
- **Dropped Prey in the Store area** (after 0007): Prey that lands in the old Store trigger just stays there. Undecided whether it should be pushed out.

## Trade-offs left by finished tasks

| From | Issue | Fix if it shows up in testing |
|------|-------|-------------------------------|
| 0002 (PR #6) | The Player vanishes the moment they die, before the fade covers the screen. This is the agreed behaviour. | Hide on `Dead` only after the fade, e.g. a delay in `PlayerVisibility`. |
| 0002 (PR #6) | Auto-collect hides every renderer under the Player and the Ball. A future marker placed under the Player would also hide. | Fill `Target Renderers` by hand. |
| 0003 (PR #7) | The Lift Platform now moves in physics steps (50/s), so the ride may look steppy. | Set the Prey prefabs' Rigidbody **Interpolate** to *Interpolate*. |
| 0003 (PR #7) | Only the bottom Prey of a stack is carried; the ones on top are pushed and may wobble. | Accept it, or carry everything that touches a rider. |
| 0003 (PR #7) | The platform peaks at about 10 m/s and Prey could bounce off at the top. | Add a kinematic Rigidbody on the platform, moved with `MovePosition`. |
| 0003 (PR #7) | `[DefaultExecutionOrder(1000)]` assumes DOTween's fixed tick runs at the default order. A wrong order costs at most about 4 mm of lag per step. | No action unless Prey visibly lag. |
| 0004 | The Prey face materials now draw over other transparent effects (e.g. Incense particles). | Lower the faces' queue offset, or raise the particles' queue. |
| 0004 | Builds can't find `Custom/CorridorDarkness` through `Shader.Find` unless it's in *Always Included Shaders* or assigned in the Inspector. | Follow the 0004 Inspector checklist. |

## Process issues

- The task chats added AI attribution to their commits (PRs #5 and #6, already cleaned). `AGENTS.md` now forbids it.
- `/code-review` run from the main folder reviews `main`, not the task branch. Run it inside the worktree on `origin/main...HEAD`.
