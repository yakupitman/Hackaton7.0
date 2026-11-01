# 0011: Rising Chisel Kit prices, and remove the Booster

Source brief: `Assets/ClaudeTasks/ImplementingFullGameplayCycle.txt`
Branch: `task/0011-chisel-kit-prices` · Ukrainian copy: `0011-chisel-kit-prices.uk.md`
Issue: https://github.com/yakupitman/Hackaton7.0/issues/21

## Goal

Each of the 5 **Chisel Kits** costs more than the one before, from a fixed price list. Buying all 5 is the goal of the game. The Store shows the next Kit's price and stops selling Kits after the 5th. The unfinished Booster is removed.

## Before you start

1. Read `AGENTS.md` and `CONTEXT.md`.
2. Create a worktree `../Hackaton7.0-0011` on branch `task/0011-chisel-kit-prices` from up-to-date `origin/main` (skill `ni-todo-worktree`).
3. Files you may change: `Assets/Scripts/Store.cs`. Don't edit `SampleScene.unity` or other scripts. Tasks 0012, 0013 and 0015 run in parallel on other files.

## What exists today (verified)

- `Store.chieselKitPrice` is a single price (50 in the scene), the same for every Kit. The field name is misspelt.
- Nothing counts Kits in `Store`. `BallFollower` (`TotalChiselUpgrades = 5`) and `PlayerMovement` (`speedIncreases`) each keep their own count.
- `Store.BuyBooster()` only spends Money and logs "effect will be implemented later". The co-developer added a Booster button and a `WeightLiftBooster` object in the scene.

## Decisions (agreed)

- Kit prices come from an explicit Inspector list of 5, e.g. `[50, 120, 250, 450, 800]`. The co-developer tunes the numbers.
- The Booster is deleted. There isn't time to design it.

## Todos

1. **Store.cs**:
   - Replace `chieselKitPrice` with `[SerializeField] private int[] chiselKitPrices = { 50, 120, 250, 450, 800 };`.
   - Add `private int chiselKitsBought;` and `public int ChiselKitsBought => chiselKitsBought;`, plus `public int NextChiselKitPrice`, which returns -1 when all are bought.
   - `BuyChiselKit()`:
     - If all 5 are bought, do nothing.
     - Otherwise spend `NextChiselKitPrice`. On success, increment the count, invoke `OnChiselKitBought`, and close the Store (as today).
     - Keep the existing behaviour on failure.
   - Add `[SerializeField] private TextMeshProUGUI chiselKitPriceText;` and `[SerializeField] private Button chiselKitButton;`. Show the next price, or "Sold out" after the 5th, and make the button non-interactable when sold out. Refresh both in `Awake`/`Start` and after each purchase.
   - Delete `BuyBooster()` and `boosterPrice`.
2. Re-read the diff for compile errors. Check the usings: `TMPro`, `UnityEngine.UI`.

## Inspector checklist (the tester does this in the Editor)

- [ ] Delete the Booster button and the `WeightLiftBooster` object. Their OnClick would point at a removed method.
- [ ] On `Store`: set `Chisel Kit Prices` (5 values), and assign `Chisel Kit Price Text` (the TMP label on the Kit button) and `Chisel Kit Button`.
- [ ] Play: each Kit costs the next price in the list, the label updates, and after the 5th the button says "Sold out" and is disabled.

## Finish

Use `ni-git-workflow`: commit (e.g. `feat: escalate chisel kit prices and remove booster`), push, then `gh pr create` with "Closes #N" and the checklist. Don't merge. Remove the worktree.

## Out of scope

- Store locking during the **Guidelines** (task 0014). Kit effects (Ball and speed).
