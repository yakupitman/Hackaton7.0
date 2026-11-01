# 0009: Shared Pond UI that targets the active Pond

Source: known issue (no brief). See `KNOWN-ISSUES.md`.
Branch: `task/0009-shared-pond-ui` · Ukrainian copy: `0009-shared-pond-ui.uk.md`

## Goal

More **Ponds** are coming. There is one Bait panel and one **Catch** HUD, and their buttons must act on whichever Pond the **Player** is currently in. Adding a Pond then needs no UI wiring.

## Before you start

1. Read `AGENTS.md` and `CONTEXT.md`.
2. Create a worktree `../Hackaton7.0-0009` on branch `task/0009-shared-pond-ui` from up-to-date `origin/main` (skill `ni-todo-worktree`).
3. Files you may change: `Assets/Scripts/Pond.cs`, new `Assets/Scripts/PondBaitRouter.cs` and its `.meta`. Don't edit `SampleScene.unity` or other scripts.

## What's wrong today (verified)

- The Bait buttons' `OnClick` events in the scene call `Pond1.SelectBait(int)` directly (there are several `SelectBait` entries in `SampleScene.unity`). A duplicated Pond would still send the clicks to Pond1.
- Pond's other UI references (`availableBaits`, `catchingMinigame`, `bar`, `area`, `needle`) point at shared UI objects. A duplicated Pond keeps those references, so only the button routing is broken.

## Decisions (agreed)

- Keep one shared UI. The buttons call a router, and the router forwards to the active Pond.

## Todos

1. **Pond.cs**:
   - Add `public static Pond ActivePond { get; private set; }`.
   - Set it to `this` in `BeginInteraction()`. In `EndInteraction()`, clear it only if it is still `this`.
2. **PondBaitRouter.cs** (goes on the shared Bait panel): `public void SelectBait(int baitType)` forwards to `Pond.ActivePond.SelectBait(baitType)` if there is an active Pond. If there isn't, log a warning and do nothing.
3. Check read-only whether anything else in `SampleScene.unity` calls Pond1 directly through UI events, e.g. a "close" button. List all of them in the PR so the tester can rewire them.
4. Re-read the diff for compile errors.

## Inspector checklist (the tester does this in the Editor)

- [ ] Add `PondBaitRouter` to the shared Bait panel (the `availableBaits` object).
- [ ] For each Bait button `OnClick` entry that targets `Pond1 → Pond.SelectBait`, switch the target to the panel's `PondBaitRouter.SelectBait` and keep the same int argument.
- [ ] Play at Pond1: the Bait buttons work as before.
- [ ] **How to add a new Pond** (for later): duplicate `Pond1`. Give the copy its own cameras (`Pond Camera`, `Pond Camera 2`), audio sources, flashlight, Prey visuals spawn point and `CorridorDarkness`. Leave the UI references pointing at the shared UI. Tag its collider `Building`.

## Finish

Use `ni-git-workflow`: commit (e.g. `feat: route shared bait ui to the active pond`), push, then `gh pr create` with "Closes #N" and the checklist. Don't merge. Remove the worktree.

## Out of scope

- Actually adding Pond2. Changing the Catch minigame.

## Risks and open questions

- Every Pond runs its own `Awake` on the same shared UI objects (`SetupUIElement` caches the original Y). That should be harmless, but confirm it in the PR.
