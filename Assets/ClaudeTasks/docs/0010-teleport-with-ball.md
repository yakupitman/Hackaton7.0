# 0010: Teleport item moves the Ball with the Player

Source: known issue (no brief). See `KNOWN-ISSUES.md`.
Branch: `task/0010-teleport-with-ball` · Ukrainian copy: `0010-teleport-with-ball.uk.md`
Issue: https://github.com/yakupitman/Hackaton7.0/issues/17

## Goal

Using the **Teleport** consumable moves the **Player** to their start spot `(11, 0, 7)`. It must move the **Ball** along with them, through both physics bodies, the same way **Death** respawn does after task 0005. One shared helper does both.

## Dependencies

**Start only after PR #14 (task 0005) is merged.** This task reuses `BallFollower.TeleportTo()` and the respawn code that #14 adds to `PlayerStateManager.cs`.

## Before you start

1. Read `AGENTS.md` and `CONTEXT.md`.
2. Create a worktree `../Hackaton7.0-0010` on branch `task/0010-teleport-with-ball` from up-to-date `origin/main` (skill `ni-todo-worktree`).
3. Files you may change: `Assets/Scripts/PlayerStateManager.cs`, but **only** `UseTeleport()` and the respawn helper from #14. Task 0006 edits the enum and `Update()` of the same file, so stay away from them. Don't edit `SampleScene.unity` or other scripts.

## What's wrong today (verified)

- `PlayerStateManager.UseTeleport()` (added by the co-developer and called from a UI button in the scene) sets `playerTransform.position = (11, 0, 7)` directly. That bypasses the Player's `Rigidbody`, and it toggles a `CharacterController` the Player doesn't have.
- The Ball is left behind. `BallFollower.ApplyRopeForce` snaps it to the Player only when they're more than 10 m apart, and only through the transform, so a shorter teleport gets yanked by the rope spring.

## Decisions (agreed)

- Teleporting behaves like the Death respawn: the Player and the Ball move together, and their velocities are set to zero.

## Todos

1. **PlayerStateManager.cs**:
   - Generalise the respawn helper from #14 (`RespawnAtOrigin`) into `private void TeleportPlayerAndBall(Vector3 position)`.
   - `Die()` calls it with `Vector3.zero`. `UseTeleport()` calls it with the start spot.
   - Remove the `CharacterController` code from `UseTeleport()`.
   - Move the start spot `(11, 0, 7)` into a `[SerializeField] private Vector3 teleportPosition` with that default, so the tester can change it.
2. Keep `UseTeleport()`'s existing rules: `Regular` only, consumes one, updates the text.
3. Re-read the diff for compile errors.

## Inspector checklist (the tester does this in the Editor)

- Nothing to wire. `Teleport Position` defaults to `(11, 0, 7)`.
- [ ] Walk far from the start, then use a Teleport. The Player and the Ball arrive together, the Ball doesn't get yanked, and the count goes down by 1.
- [ ] Die once: the respawn still works as in 0005.

## Finish

Use `ni-git-workflow`: commit (e.g. `fix: move ball with player on teleport`), push, then `gh pr create` with "Closes #N" and the checklist. Don't merge. Remove the worktree.

## Out of scope

- Teleport pricing, UI, or new destinations.
