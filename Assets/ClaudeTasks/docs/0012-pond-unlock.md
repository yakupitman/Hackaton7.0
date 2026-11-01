# 0012: Locked Ponds and the unlock popup

Source brief: `Assets/ClaudeTasks/ImplementingFullGameplayCycle.txt`
Branch: `task/0012-pond-unlock` · Ukrainian copy: `0012-pond-unlock.uk.md`
Issue: https://github.com/yakupitman/Hackaton7.0/issues/22

## Goal

There are 3 **Ponds**, each with different **Prey**. Each Pond starts as a **Locked Pond** and has its own unlock price, rising from the 1st to the 3rd. Pressing E at a Locked Pond opens an unlock popup: "Unlock this Pond for N?" with Buy and Cancel. Once bought, the Pond works as today. Unlocking a Pond is how the Player speeds up earning.

## Before you start

1. Read `AGENTS.md` and `CONTEXT.md`.
2. Create a worktree `../Hackaton7.0-0012` on branch `task/0012-pond-unlock` from up-to-date `origin/main` (skill `ni-todo-worktree`).
3. Files you may change: `Assets/Scripts/Pond.cs`, new `Assets/Scripts/PondUnlockPopup.cs` and its `.meta`. Don't edit `SampleScene.unity`, prefabs or other scripts. Tasks 0011, 0013 and 0015 run in parallel.

## What exists today (verified)

- `Pond1` is now a prefab (`Assets/Prefabs/Pond1.prefab`) with two instances in the scene, named Pond1 and Pond2.
- Each Pond has its own `preyProfiles` (Prey prefab, Catch duration, speed, Personalities). Each Prey prefab has a `Price` on `InteractableObject`. So "different Prey per Pond" is Inspector configuration.
- `PlayerMovement.TryBeginInteraction()` calls `building.BeginInteraction()`, then sets `Interacting` if the state is still `Regular`. E in a non-`Regular` state calls `EndCurrentInteraction()`, which ends the interaction and sets `Regular`.
- `Pond.ActivePond` (task 0009) is set in `BeginInteraction` and cleared in `EndInteraction`.

## Decisions (agreed)

- Ponds can be unlocked in **any order**. The prices alone make 1 → 2 → 3 the natural order.
- E at a Locked Pond opens a **popup**: "Unlock this Pond for N?" with Buy and Cancel. Buy is greyed out when Money < N. After buying, the popup closes and the Pond works normally on the next E. It doesn't go straight into the Pond.
- The texts are in English and are Inspector fields.

## Todos

1. **Pond.cs**:
   - Add `[SerializeField] private bool isUnlocked;`, `[SerializeField] private int unlockPrice = 30;` and `public bool IsUnlocked => isUnlocked;`.
   - Add `public UnityEvent OnUnlocked;` and `public void Unlock()`, which sets it, is idempotent, and invokes `OnUnlocked`. Task 0014 uses `Unlock()` for Skip.
   - `BeginInteraction()`: if locked, call `base.BeginInteraction()` and `PondUnlockPopup.Instance.Show(this, unlockPrice)`, then return. Don't enable the cameras or the Bait UI, and don't set `ActivePond`. If unlocked, behave exactly as today.
   - `EndInteraction()`: if the popup is showing for this Pond, hide it. Leave the rest unchanged.
   - Add `public bool TryBuyUnlock()`: `PlayerStateManager.Instance.SpendMoney(unlockPrice)`, then `Unlock()`. Returns true on success.
2. **PondUnlockPopup.cs**: one shared panel under the Canvas, set up as a scene singleton per `AGENTS.md`.
   - Serialized fields: `CanvasGroup panel`, `TextMeshProUGUI messageText`, `Button buyButton`, `Button cancelButton`, and `string messageFormat = "Unlock this Pond for {0}?"`.
   - `Show(Pond pond, int price)`: fade in with DOTween, call `DOKill` first. Buy is interactable only if Money ≥ price. While open, refresh it from `PlayerStateManager.OnMoneyChanged`.
   - Buy: `pond.TryBuyUnlock()`, then close.
   - Cancel: close.
   - **Close always ends the interaction cleanly:** hide the panel, call `pond.EndInteraction()`, then `PlayerStateManager.Instance.SetState(Regular)`, as `Store.EndInteraction` does. Pressing E while the popup is open must also close it through the same path.
   - Add the button listeners in code, not as Inspector OnClick entries.
3. Re-read the diff for compile errors.

## Inspector checklist (the tester does this in the Editor)

- [ ] Build the popup under `Canvas`: a panel with a `CanvasGroup`, a TMP message, and Buy and Cancel buttons. Add the `PondUnlockPopup` component and assign its fields. Keep the panel object active (the script hides it).
- [ ] Make a 3rd Pond from the `Pond1` prefab. On each Pond, set `Unlock Price`, rising from the 1st to the 3rd, and leave `Is Unlocked` off. Give each Pond its own `Prey Profiles`, with different Prey and prices.
- [ ] Play: E at a Locked Pond shows the popup with the price. Buy is greyed out without enough Money. Buy spends the Money, and the next E enters the Pond. Cancel and E both close the popup, and the Player can walk again.

## Finish

Use `ni-git-workflow`: commit (e.g. `feat: lock ponds behind an unlock popup`), push, then `gh pr create` with "Closes #N" and the checklist. Don't merge. Remove the worktree.

## Out of scope

- The Guidelines step "buy the 1st Pond" (task 0014 listens to `OnUnlocked`). Balancing prices.

## Risks and open questions

- Is the Use Indicator wanted on Locked Ponds too? It will show, because they're still Buildings. That's probably fine.
