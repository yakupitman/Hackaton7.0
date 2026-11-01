# 0005: Respawn the Player and the Ball together on Death

Source: known issue (no brief). See `KNOWN-ISSUES.md`.
Branch: `task/0005-death-respawn` · Ukrainian copy: `0005-death-respawn.uk.md`
Issue: https://github.com/yakupitman/Hackaton7.0/issues/9

## Goal

After a **Death** (failed **Catch** or caught by the **Ghost**), the **Player** respawns at the world centre `(0, 0, 0)` as today. The **Ball** must arrive with them instead of being left behind, so the **Chain** doesn't drag it across the map. The Player must also always return to `Regular`, even if the death fade isn't wired up.

## Before you start

1. Read `AGENTS.md` and `CONTEXT.md`.
2. Create a worktree `../Hackaton7.0-0005` on branch `task/0005-death-respawn` from up-to-date `origin/main` (skill `ni-todo-worktree`).
3. Files you may change: `Assets/Scripts/PlayerStateManager.cs` (**only** the `Die()` method and anything it needs), `Assets/Scripts/BallFollower.cs`. Don't edit `SampleScene.unity` or other scripts. Task 0006 also edits `PlayerStateManager.cs` (the enum and `Update`), so stay inside `Die()` to avoid merge conflicts.

## What's wrong today (verified)

- `Die()` sets `playerTransform.position = Vector3.zero`, but the Player moves with a `Rigidbody`. Setting the transform skips the physics body, so velocity and interpolation can fight the teleport.
- `Die()` turns off a `CharacterController` that the Player doesn't have. The code is dead, but harmless.
- The Ball (`BallFollower`, its own `Rigidbody`) stays where the Player died. `BallFollower.ApplyRopeForce` then pulls it towards the Player with a force that grows with distance, so the Ball flies across the map.
- If `fadeScreen` or `playerTransform` is unassigned, `Die()` sets `Dead` and **never** returns to `Regular`, which leaves the game stuck. In the current scene both are assigned, but this must not be possible.

## Decisions (agreed)

- The respawn position stays at the world centre `(0, 0, 0)`.
- The Ball is teleported with the Player and both stop moving (their velocities are set to zero).

## Todos

1. **BallFollower.cs**: add `public void TeleportTo(Vector3 position)`.
   - Set `ballRigidbody.position` and `transform.position`.
   - Zero `linearVelocity` and `angularVelocity`.
   - The rope visual updates itself in `LateUpdate`, so nothing else is needed.
2. **PlayerStateManager.cs**, in `Die()`:
   - Teleport through the Player's `Rigidbody`: set `rb.position`, `transform.position`, and zero its velocities. Remove the `CharacterController` code.
   - Find the Ball with `FindFirstObjectByType<BallFollower>()` and call `TeleportTo(respawn + small offset behind the Player)`, e.g. `Vector3.back * 1f`. The Ball doesn't exist after the 5th **Chisel Kit**, so check for null.
   - Fallback: if `fadeScreen` or `playerTransform` is null, still teleport whatever can be teleported, log a warning, and return to `Regular` after about 1 s, e.g. with `DOVirtual.DelayedCall`.
3. Re-read the diff for compile errors and null safety.

## Inspector checklist (the tester does this in the Editor)

- No wiring needed.
- [ ] Fail a Catch, and separately get caught by the Ghost. In both cases the Player and the Ball appear together at the centre, the Ball doesn't fly across the map, and the Player can move again after the fade.
- [ ] After buying the 5th Chisel Kit (the Ball is gone), die once: no errors.

## Finish

Use `ni-git-workflow`: commit (e.g. `fix: respawn ball with player on death`), push, then `gh pr create` with "Closes #N" and the checklist. Don't merge. Remove the worktree.

## Out of scope

- A different respawn point. Losing Money or Bait on Death.

## Risks and open questions

- If `(0, 0, 0)` lies inside geometry or below the ground, the Player may fall or get stuck. Mention in the PR what you saw in the scene.
