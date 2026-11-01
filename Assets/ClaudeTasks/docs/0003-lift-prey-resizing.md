# 0003: Stop Prey resizing on the Lift Platform

Source brief: `Assets/ClaudeTasks/LiftInteractableResizing.txt`
Branch: `task/0003-lift-prey-resizing` · Ukrainian copy: `0003-lift-prey-resizing.uk.md`
Issue: https://github.com/yakupitman/Hackaton7.0/issues/3

## Goal

When a **Prey** lands on the **Lift Platform** (which DOTween then moves), it can suddenly change its scale. Prey must ride the Lift up to the **Sell Height** and back without any change in scale or distortion.

## Before you start

1. Read `AGENTS.md` and `CONTEXT.md`.
2. `git checkout main && git pull && git checkout -b task/0003-lift-prey-resizing`
3. Files you may change: `Assets/Scripts/LiftPlatform.cs`, `Assets/Scripts/Lift.cs`, and if needed `Assets/Scripts/InteractableObject.cs`. Don't edit `SampleScene.unity` or other scripts.

## Root cause (verified)

`LiftPlatform` sits in the scene at **non-uniform scale `(5, 0.25, 5)`**. `LiftPlatform.OnCollisionEnter` does `collision.transform.SetParent(transform, true)`. Parenting a rotated Prey under a non-uniformly scaled parent skews it, and `localScale` can't represent that, so the Prey visibly changes size. The current `originalScales` dictionary only restores scale on exit, which is too late.

## Decision (agreed)

**Stop parenting.** Carry riders with the platform: each physics step, move them by however far the platform moved. This is code only, with no scene change.

## Todos

1. **LiftPlatform.cs**: rewrite.
   - Remove `SetParent` and the `originalScales` dictionary.
   - Keep a `HashSet<Rigidbody> riders`. `OnCollisionEnter`: if the other object has `InteractableObject` and a `Rigidbody`, add it. `OnCollisionExit`: remove it.
   - Store the platform's position from the last frame. In `FixedUpdate`, compute `delta = transform.position - lastPosition`, then for each rider (skip nulls, because a Prey is destroyed when it's **Sold**): `rb.MovePosition(rb.position + delta)`. Then update `lastPosition`. Remove riders that are null.
   - Skip riders that are currently **Grabbed** (see step 3).
2. **Lift.cs**: make the platform move in step with physics.
   - Add `.SetUpdate(UpdateType.Fixed)` to the lift tween sequence.
   - Bug found while investigating: the return tween goes to a hard-coded `0.4125f`, but the platform starts at y ≈ `0.125`. Cache `startY = liftPlatform.position.y` in `Start` and return to it.
3. **InteractableObject.cs** (only if needed): expose `public bool IsHeld => isHeld;` so `LiftPlatform` can skip a Prey the player is holding (it's driven by `HoldManager` velocity).
4. Make sure Selling still works: the platform rises `liftHeight` (10), and `InteractableObject` Sells at y ≥ 10. Riders must actually reach that height and not slide off or fall through.
5. Re-read the diff for compile errors.

## Inspector checklist (the user does this in the Editor)

- [ ] Play: drop 1 and then several Prey (rotated at different angles) on the Lift Platform, then pull the lever. They must not change scale. They should rise, Sell at the top (Money goes up), and the platform should come back to its original height.
- [ ] Grab a Prey off the platform mid-ride → it follows the mouse, not the platform.

## Finish

Commit (`fix: carry prey on lift platform without parenting`), push the branch, then `gh pr create` with the checklist.

## Out of scope

- Restructuring the platform hierarchy in the scene. Changing the Sell rules.

## Risks and open questions

- If a fast rise makes Prey jitter or sink through the platform, try a kinematic `Rigidbody` added at runtime with `MovePosition`, or raise the Prey's `collisionDetectionMode`. Describe what you chose in the PR.
