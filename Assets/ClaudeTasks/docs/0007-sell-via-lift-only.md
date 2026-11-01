# 0007: Sell Prey only through the Lift

Source: known issue (no brief). See `KNOWN-ISSUES.md`.
Branch: `task/0007-sell-via-lift-only` · Ukrainian copy: `0007-sell-via-lift-only.uk.md`
Issue: https://github.com/yakupitman/Hackaton7.0/issues/11

## Goal

**Selling** has exactly one way: raise a **Prey** to the **Sell Height** (y ≥ 10) with the **Lift**. Dropping Prey into the **Store** no longer sells it.

## Before you start

1. Read `AGENTS.md` and `CONTEXT.md`.
2. Create a worktree `../Hackaton7.0-0007` on branch `task/0007-sell-via-lift-only` from up-to-date `origin/main` (skill `ni-todo-worktree`).
3. Files you may change: `Assets/Scripts/Store.cs`. Don't edit `SampleScene.unity` or other scripts.

## What's wrong today (verified)

- `Store.OnTriggerEnter` sells any `InteractableObject` that enters the Store's trigger: it adds `Price` and destroys it.
- `InteractableObject.CheckSellCondition` *also* sells at y ≥ 10, which is the Lift path.

## Decisions (agreed)

- The Lift is the only way to Sell. Store-trigger selling is removed.

## Todos

1. **Store.cs**: delete `OnTriggerEnter`. Leave `BuyBait`, `BuyChiselKit` and the interaction code unchanged.
2. Check read-only whether the Store's trigger collider is used for anything else, by searching the scripts and `SampleScene.unity`. Report it in the PR so the tester can decide whether to remove the collider.
3. Re-read the diff for compile errors.

## Inspector checklist (the tester does this in the Editor)

- [ ] Drop a Prey into the Store: nothing happens, and Money doesn't change.
- [ ] Put a Prey on the Lift and pull the lever: it Sells at the top and Money goes up.
- [ ] Optional: if the PR says the Store's trigger collider is now unused, remove or disable it.

## Finish

Use `ni-git-workflow`: commit (e.g. `feat: sell prey only through the lift`), push, then `gh pr create` with "Closes #N" and the checklist. Don't merge. Remove the worktree.

## Out of scope

- Changing Prey prices or the Sell Height.

## Risks and open questions

- If a Prey falls into the Store's trigger area, it now just stays there. Should the trigger push it back out? Not decided; report it if it's a problem.
