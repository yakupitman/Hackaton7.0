# 0001: Use Indicator on Buildings

Source brief: `Assets/ClaudeTasks/BuildingInteractionIdentificator.txt`
Branch: `task/0001-use-indicator` · Ukrainian copy: `0001-use-indicator.uk.md`
Issue: https://github.com/yakupitman/Hackaton7.0/issues/1

## Goal

Each **Building** (Store, Lift, Pond, …) has a disabled child object named `UseIndicator`. While the **Player** is `Regular` and pressing E right now *would* start an interaction with a Building, that Building's `UseIndicator` must be visible. When pressing E would not start one (the player turned away, walked out of range, isn't `Regular`), it must be hidden.

## Before you start

1. Read `AGENTS.md` (code style, rules for working in parallel) and `CONTEXT.md` (terms).
2. `git checkout main && git pull && git checkout -b task/0001-use-indicator`
3. Files you may change: `Assets/Scripts/Building.cs`, `Assets/Scripts/PlayerMovement.cs`. **Don't** edit `SampleScene.unity` or any other script; other chats are working on them in parallel.

## Existing code to reuse

- `PlayerMovement.TryBeginInteraction()` already decides whether E works: `Physics.Raycast` from `transform.position` along `lastMoveDirection`, range `interactionRange` (4), the collider must be tagged `Building` and have a `Building` component. **The indicator must use exactly this check**, so "indicator visible" and "E works" can never disagree.
- `Building` is the base class of `Store`, `Lift`, `Pond`; it has `BeginInteraction`/`EndInteraction` virtuals.
- The scene already contains 3 `UseIndicator` objects (inactive) under the Buildings.

## Decisions (agreed)

- Shown **only in `Regular`** state. While inside a Building (`Interacting`/`Shopping`/…) every indicator is hidden.
- Show/hide with a **DOTween pop**:
  - Show: `SetActive(true)`, then scale from 0 to its original scale with `Ease.OutBack`, about 0.15 s.
  - Hide: scale to 0 in about 0.1 s, then `SetActive(false)` in `OnComplete`.
  - Call `DOKill()` before each tween.
- At most one indicator is visible at a time (the Building the raycast hits).

## Todos

1. **Building.cs**: add indicator support.
   - `[SerializeField] private GameObject useIndicator;`. If it's null in `Awake`, find a descendant named `UseIndicator` (search recursively, including inactive objects). Cache its original `localScale`. Make sure it starts hidden.
   - Add `public void SetIndicatorVisible(bool visible)`. It does the DOTween pop described above and is a no-op when the state hasn't changed or there is no indicator.
   - Subclasses (`Store`, `Lift`, `Pond`) define their own `Awake` without calling a base one. **Don't** rely on `Building.Awake`. Instead, resolve the indicator lazily on the first `SetIndicatorVisible` call, or in a private method called from there. Don't change the subclasses.
2. **PlayerMovement.cs**: extract the raycast into `private Building FindTargetBuilding()`, which returns the Building that E would interact with, or null. `TryBeginInteraction()` must use it, so its behaviour stays the same.
3. **PlayerMovement.cs**: add `private Building highlightedBuilding;` and an `UpdateUseIndicator()` called from `Update()` after `TrackMoveDirection()`:
   - The target is `FindTargetBuilding()` when the state is `Regular`, otherwise null.
   - If the target differs from `highlightedBuilding`: hide the old one, show the new one, and store it.
4. Follow the `AGENTS.md` style: blank lines only between members, comments in English and on their own line, singletons accessed via `.Instance`.
5. Read your diff once more to check it compiles. Check `using DG.Tweening;` and that there are no null refs when a Building has no indicator.

## Inspector checklist (the user does this in the Editor)

- [ ] Each Building (Store, Lift, Pond1) has a `UseIndicator` child. If it's not found automatically, drag it into the new `Use Indicator` field.
- [ ] Play: walk up to each Building facing it → indicator pops in; turn away or step back → it pops out; press E → it disappears while inside; press E again → it comes back.

## Finish

Commit (conventional message, e.g. `feat: show use indicator when a building is interactable`), `git push -u origin task/0001-use-indicator`, `gh pr create` with a description that includes the Inspector checklist.

## Out of scope

- Indicator art or animation beyond the pop. "Press E to leave" hints. The Lift lever click.

## Risks and open questions

- The raycast starts at the player's pivot (feet). If an indicator flickers near uneven geometry, report it; don't change the interaction raycast in this task.
