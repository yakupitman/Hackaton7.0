# 0014: Guidelines tutorial with Skip

Source brief: `Assets/ClaudeTasks/ImplementingFullGameplayCycle.txt`
Branch: `task/0014-guidelines-tutorial` · Ukrainian copy: `0014-guidelines-tutorial.uk.md`
Issue: https://github.com/yakupitman/Hackaton7.0/issues/24

## Goal

Every new game (Start on the **Main Menu**) runs the **Guidelines**, a step-by-step panel that restricts the **Store** and advances as the **Player** completes each step. A Skip button on the panel jumps to **Free Play** in the same state a finished tutorial would leave.

## Dependencies

**Start only after tasks 0011, 0012 and 0013 are merged.** This task uses the Kit count and prices from 0011, `Pond.OnUnlocked`/`Unlock()` from 0012, and `MainMenu.OnGameStarted` plus the `Menu` state from 0013.

## Before you start

1. Read `AGENTS.md` and `CONTEXT.md`.
2. Create a worktree `../Hackaton7.0-0014` on branch `task/0014-guidelines-tutorial` from up-to-date `origin/main` (skill `ni-todo-worktree`).
3. Files you may change:
   - new `Assets/Scripts/Guidelines.cs` and its `.meta`
   - small hooks in `Store.cs` (item locking), `Pond.cs` (a "Prey caught" event) and `EscapeSequence.cs` (a "caught by the Ghost" event)

   Don't edit `SampleScene.unity` or other scripts.

## The steps (agreed)

| # | Guidelines text (English, Inspector-editable) | Store allows | Advances when |
|---|------|------|------|
| 1 | Buy your first Chisel Kit in the Store. | Chisel Kit only | `Store.OnChiselKitBought` |
| 2 | Buy 3 of the cheapest Bait. | cheapest Bait (tier 0) only | Player owns ≥ 3 tier-0 Bait (`OnBaitInventoryChanged`) |
| 3 | Go to the right corner of the room and unlock the first Pond. | nothing | any `Pond.OnUnlocked` |
| 4 | Pick a Bait and start fishing. | nothing | a Catch starts (or skip straight to 5 on success) |
| 5 | Catch it! Keep the needle inside the moving area. *(explains the Catch)* | nothing | Catch success (new `Pond.OnPreyCaught`) |
| 6 | Carry the Prey to the Lift and send it up to sell it. | nothing | Money increases from a Sell |
| 7 | Leave through the main door and run down the corridor. | nothing | the Player is **caught by the Ghost** (new `EscapeSequence.OnPlayerCaught`); other Deaths don't count |
| 8 | "Didn't make it? Try to lighten that heavy thing. Have fun!" shown for 15 s after the respawn | everything | timer ends → **Free Play** |

- A failed Catch doesn't change the step. The Player still has Bait. A soft-lock is possible if they spend all their Bait, and that's accepted. Skip is the way out.
- **Free Play:** the Guidelines panel is hidden and the whole Store is enabled.

## Decisions (agreed)

- The tutorial runs **every new game** and is **skippable** with a Skip button **on the Guidelines panel**.
- **Skip = the same state as a finished tutorial:** 1st Chisel Kit applied, Pond 1 unlocked, 3 cheapest Bait.
  - Do this by completing the remaining tutorial purchases. Buy the Kit and Bait through the normal Store methods, and unlock the cheapest locked Pond with `Unlock()`.
  - Pay for them from Money, which is affordable by design (0013's starting Money covers exactly these). The result is the same whether you skip at step 1 or step 6.
  - Then go to Free Play without the 15 s hint.
- Texts are in English and are Inspector fields.

## Todos

1. **Store.cs**:
   - Add serialized Button refs for the Kit, the 3 Bait and Teleport.
   - Add `public void SetTutorialLock(bool kit, bool bait0, bool others)` and `public void ClearTutorialLock()`. They set button interactability. The lock must also guard the Buy methods themselves, not only the buttons.
   - Must not fight 0011's "Sold out" disabling of the Kit button.
2. **Pond.cs**: add `public UnityEvent OnPreyCaught;`, invoked in `CompleteMinigame(true)`. Optionally add `OnCatchStarted` in `StartMinigame()` for step 4.
3. **EscapeSequence.cs**: add `public UnityEvent OnPlayerCaught;`, invoked in `CheckCatch()` just before `Die()`.
4. **Guidelines.cs**:
   - Serialized fields: the panel (`CanvasGroup` + TMP text), the Skip button, an array of step texts, the hint text and its duration (15), and refs to `Store`, `MainMenu` and the Ponds (or `FindObjectsByType<Pond>`).
   - Subscribe to `MainMenu.OnGameStarted` → begin at step 1.
   - Drive the steps from the events above. Unsubscribe in `OnDestroy`.
   - Step 8: after `OnPlayerCaught`, wait for the respawn (state back to `Regular`), show the hint for 15 s, then go to Free Play.
   - Skip: as decided above.
5. Re-read the diff for compile errors. Check that every subscription is removed in `OnDestroy`, because Restart reloads the scene.

## Inspector checklist (the tester does this in the Editor)

- [ ] Build the Guidelines panel under `Canvas`: a `CanvasGroup`, a TMP text and a Skip button. Add `Guidelines` and assign its fields. Edit the step texts if wanted.
- [ ] On `Store`, assign the new Kit, Bait and Teleport button refs.
- [ ] Play the whole tutorial once, step by step. Then start a new game and press Skip at step 1 and again at a later step. Both land in Free Play with 1 Kit, Pond 1 unlocked and 3 cheapest Bait, and the Store fully open.

## Finish

Use `ni-git-workflow`: commit, push, then `gh pr create` with "Closes #N" and the checklist. Don't merge. Remove the worktree.

## Out of scope

- Remembering tutorial completion between sessions. Voice-over or arrows pointing at objects.

## Risks and open questions

- Step 6 ("Money increases") would also fire from any other Money source. Today only Selling adds Money.
- If the Player dies to the Ghost before step 7, it doesn't count, and the step stays put.
