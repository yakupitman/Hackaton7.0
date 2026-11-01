# 0006: Make Shopping stick and remove Catching

Source: known issue (no brief). See `KNOWN-ISSUES.md`.
Branch: `task/0006-player-states-cleanup` · Ukrainian copy: `0006-player-states-cleanup.uk.md`

## Goal

Clean up the **Player State** machine:
- `Shopping` lasts for the whole **Store** visit, so the Money and **Bait** HUD stays visible while shopping.
- The unused `Catching` state is removed.
- Every non-`Regular` state blocks movement, not only `Interacting`.

## Dependencies

**Start only after PR #5 (task 0001) and task 0005 are merged.** 0001 changes `PlayerMovement.cs` and 0005 changes `PlayerStateManager.cs`. Starting earlier would cause merge conflicts.

## Before you start

1. Read `AGENTS.md` and `CONTEXT.md`.
2. Create a worktree `../Hackaton7.0-0006` on branch `task/0006-player-states-cleanup` from up-to-date `origin/main` (skill `ni-todo-worktree`).
3. Files you may change: `Assets/Scripts/PlayerStateManager.cs`, `Assets/Scripts/PlayerMovement.cs`, `Assets/Scripts/PlayerAnimator.cs`. Don't edit `SampleScene.unity` or other scripts.

## What's wrong today (verified)

- `Store.BeginInteraction()` sets `Shopping`, then `PlayerMovement.TryBeginInteraction()` immediately sets `Interacting`. So `Shopping` never lasts, and its rule in `SetState` (keep the HUD visible) never applies.
- `Catching` is declared and `PlayerStateManager.CheckCatchingExit()` handles it, but nothing ever enters it. During a **Catch** the Player is `Interacting`.
- `PlayerMovement.Move()` only blocks movement for `Interacting`. Once `Shopping` sticks, the Player could walk around while shopping. They can also walk while `Dead`, during the fade.
- `PlayerMovement.HandleInteractionInput()` only lets E leave a Building from `Interacting`.

## Decisions (agreed)

- `Shopping` sticks. E still leaves the Store.
- `Catching` is deleted entirely.

## Todos

1. **PlayerStateManager.cs**:
   - Remove `Catching` from `PlayerState`.
   - Remove `CheckCatchingExit()`, and `Update()` if it ends up empty.
   - Search the whole project (scripts **and** `SampleScene.unity`, read-only) for uses of `Catching`, and for serialized `PlayerState` int values that would shift when the enum changes. Report what you find.
2. **PlayerMovement.cs**:
   - `TryBeginInteraction()`: after `BeginInteraction()`, call `SetState(Interacting)` only if the state is still `Regular`. If the Building has set its own state (the Store sets `Shopping`), keep that one.
   - `HandleInteractionInput()`: E ends the interaction from any state except `Regular` and `Dead`.
   - `Move()` and `Rotate()`: no walking or turning in any state except `Regular`. Keep the vertical velocity, as now.
3. **PlayerAnimator.cs**: play the idle animation (blend 0) in every state except `Regular`, not only in `Interacting`.
4. Check the other places that read the state, read-only, and report anything that breaks:
   - `HoldManager` (Grab only in `Regular`)
   - `Building`'s Use Indicator (only in `Regular`)
   - `PlayerVisibility` (hides in every state except `Regular`)
   - `Store.BuyChiselKit` (calls `EndInteraction`, then the state goes back to `Regular` through `OnChiselKitBought`)
5. Re-read the diff for compile errors.

## Inspector checklist (the tester does this in the Editor)

- No wiring needed.
- [ ] Press E at the Store: the Money and Bait HUD stays visible, the Player can't walk, and E leaves.
- [ ] Press E at the Pond and the Lift: they behave as before.
- [ ] During the death fade the Player can't move.

## Finish

Use `ni-git-workflow`: commit (e.g. `fix: keep shopping state and remove unused catching state`), push, then `gh pr create` with "Closes #N" and the checklist. Don't merge. Remove the worktree.

## Out of scope

- A dedicated state for a Catch. The bug where a failed Chisel Kit purchase closes the Store (see `KNOWN-ISSUES.md`).
