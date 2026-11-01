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
| There are two ways to Sell in code: the Store trigger and the Lift. Agreed: Lift only. PR #16 found the Store has no trigger collider in the scene, so Store selling never actually worked. | `Store.OnTriggerEnter` | [0007](0007-sell-via-lift-only.md) |
| Reaching the Exit does nothing: no script calls `SuccessfulEscape()`, and even if one did, it only logs "WIN!". | `EscapeSequence`, `EscapeSuccessTrigger` in scene | [0008](0008-escape-win-screen.md) |
| Reloading the scene would keep stale `DontDestroyOnLoad` managers that point at destroyed objects. | `PlayerStateManager`, `InputManager`, `HoldManager`, `UIManager` | [0008](0008-escape-win-screen.md) |
| The Bait buttons call `Pond1.SelectBait` directly, so new Ponds can't use the UI. | Scene UI events | [0009](0009-shared-pond-ui.md) |
| The Teleport item moves only the Player's transform: it skips the Rigidbody, toggles a `CharacterController` that doesn't exist, and leaves the Ball behind. | `PlayerStateManager.UseTeleport` | [0010](0010-teleport-with-ball.md) |

## Open bugs without a task yet

- **A failed Chisel Kit purchase closes the Store.** `Store.BuyChiselKit` calls `EndInteraction()` even when `SpendMoney` fails. Since `a2ea2a5` the co-developer's `Store.EndInteraction()` sets `Regular`, so the Player no longer gets stuck. Undecided: should the Store stay open when a purchase fails?

- **Double-click spends two Bait.** Clicking a Bait button twice during the 0.25 s panel fade can call `SelectBait` twice. Found in 0009 (PR #15).

- **A Catch never ends the Pond interaction.** After a successful or failed Catch, `Pond` sets the state but never calls `EndInteraction()`, so `PlayerMovement.currentBuilding` still points at the Pond. Harmless today, because the next E replaces it. Found in 0006 (PR #19).
- **E leaves the Lift mid-ride.** You can leave the Lift while the platform is still moving. Found in 0006 (PR #19).
- **Restart leaks the Input System actions.** Each Restart destroys the `InputManager` without disposing its actions wrapper. Fixing it needs `InputManager.cs`. Found in 0008 (PR #18).
- **The win screen doesn't block all input.** While frozen, E and the mouse still reach gameplay code, and other audio keeps playing. Found in 0008 (PR #18).
- **Restart lists the persistent managers by hand.** Any new `DontDestroyOnLoad` manager must also be added to `WinScreen.Restart()`.

## Trade-offs left by finished tasks

| From | Issue | Fix if it shows up in testing |
|------|-------|-------------------------------|
| 0002 (PR #6) | The Player vanishes the moment they die, before the fade covers the screen. This is the agreed behaviour. | Hide on `Dead` only after the fade, e.g. a delay in `PlayerVisibility`. |
| 0002 (PR #6) | Auto-collect hides every renderer under the Player and the Ball. A future marker placed under the Player would also hide. | Fill `Target Renderers` by hand. |
| 0003 (PR #7) | The Lift Platform now moves in physics steps (50/s), so the ride may look steppy. | Set the Prey prefabs' Rigidbody **Interpolate** to *Interpolate*. |
| 0003 (PR #7) | Only the bottom Prey of a stack is carried; the ones on top are pushed and may wobble. | Accept it, or carry everything that touches a rider. |
| 0003 (PR #7) | The platform peaks at about 10 m/s and Prey could bounce off at the top. | Add a kinematic Rigidbody on the platform, moved with `MovePosition`. |
| 0003 (PR #7) | `[DefaultExecutionOrder(1000)]` assumes DOTween's fixed tick runs at the default order. A wrong order costs at most about 4 mm of lag per step. | No action unless Prey visibly lag. |
| 0005 (PR #14) | The Ball respawns with its centre at y = 0 and may sit half in the floor. | Add a small height offset in the respawn helper. |
| 0005 (PR #14) | The floor and walls at the world centre `(0, 0, 0)` weren't checked (the colliders under `HotelWallsParent`). | Test the respawn spot; move it if needed. |
| 0009 (PR #15) | `Pond.ActivePond` isn't cleared when a Catch ends in success or Death, only on E or destroy. Harmless: the next Pond entered overwrites it. | None. |
| 0010 (PR #20) | Teleport moves the Player instantly with no fade, so the Cinemachine camera may visibly sweep across the map to catch up. | Warp the camera on teleport (e.g. `CinemachineCamera.OnTargetObjectWarped`), or add a short fade like Death. |
| 0004 | The Prey face materials now draw over other transparent effects (e.g. Incense particles). | Lower the faces' queue offset, or raise the particles' queue. |
| 0004 | Builds can't find `Custom/CorridorDarkness` through `Shader.Find` unless it's in *Always Included Shaders* or assigned in the Inspector. | Follow the 0004 Inspector checklist. |

## Process issues

- The task chats added AI attribution to their commits (PRs #5 and #6, already cleaned). `AGENTS.md` now forbids it.
- `/code-review` run from the main folder reviews `main`, not the task branch. Run it inside the worktree on `origin/main...HEAD`.
