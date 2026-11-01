# 0008: Win the Escape: win screen and Restart

Source: known issue (no brief). See `KNOWN-ISSUES.md`.
Branch: `task/0008-escape-win-screen` · Ukrainian copy: `0008-escape-win-screen.uk.md`

## Goal

Reaching the **Exit** during the **Escape** wins the game. The game freezes, a "You escaped!" panel fades in, and a Restart button reloads the scene into a clean new game.

## Before you start

1. Read `AGENTS.md` and `CONTEXT.md`.
2. Create a worktree `../Hackaton7.0-0008` on branch `task/0008-escape-win-screen` from up-to-date `origin/main` (skill `ni-todo-worktree`).
3. Files you may change:
   - `Assets/Scripts/EscapeSequence.cs`
   - new `Assets/Scripts/EscapeExitTrigger.cs`
   - new `Assets/Scripts/WinScreen.cs`
   - the `.meta` files for the new scripts

   Don't edit `SampleScene.unity` or other scripts. The UI panel is built by the tester in the Editor.

## What's wrong today (verified)

- The `EscapeSuccessTrigger` object in the scene has only a Transform and a collider. Nothing calls `EscapeSequence.SuccessfulEscape()`, so reaching the Exit does **nothing**.
- Even when it is called, `SuccessfulEscape()` only logs `"WIN!"`.
- Restart trap: `PlayerStateManager`, `InputManager`, `HoldManager` and `UIManager` are `DontDestroyOnLoad` singletons. A plain `SceneManager.LoadScene` keeps the *old* managers, which point at destroyed objects from the old scene (fade screen, HUD, Player), and destroys the fresh ones. Restart would be broken.

## Decisions (agreed)

- On a win: freeze the game, fade in a simple win panel, and offer a **Restart** button that reloads the scene.

## Todos

1. **EscapeExitTrigger.cs** (goes on `EscapeSuccessTrigger`):
   - `[SerializeField] private EscapeSequence escapeSequence;`, falling back to `FindFirstObjectByType<EscapeSequence>()`.
   - `OnTriggerEnter`: if the other object has the `Player` tag, call `escapeSequence.SuccessfulEscape()`.
2. **EscapeSequence.cs**: add `public UnityEvent OnEscaped;` and invoke it in `SuccessfulEscape()` after the existing reset. Remove the `Debug.Log("WIN!")`.
3. **WinScreen.cs**:
   - Serialized fields: `CanvasGroup panel`, `Button restartButton`, `EscapeSequence escapeSequence` (the last one falls back to `FindFirstObjectByType`).
   - `Start`: hide the panel (alpha 0, not interactable, no raycasts), subscribe to `OnEscaped`, and add the Restart listener in code.
   - `Show()`: `Time.timeScale = 0`, then fade the panel in with DOTween using `.SetUpdate(true)`, which keeps the tween running while the game is frozen. Then make it interactable.
   - `Restart()`:
     - `Time.timeScale = 1` and `DOTween.KillAll()`.
     - Destroy the `gameObject` of every persistent singleton that exists (`PlayerStateManager`, `InputManager`, `HoldManager`, `UIManager`).
     - Then `SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex)`.
     - Add a comment explaining why the managers are destroyed.
4. Re-read the diff for compile errors. Check the usings: `UnityEngine.Events`, `UnityEngine.UI`, `UnityEngine.SceneManagement`, `DG.Tweening`.

## Inspector checklist (the tester does this in the Editor)

- [ ] Add `EscapeExitTrigger` to `EscapeSuccessTrigger`. Make sure its collider has **Is Trigger** on and that the Player has the `Player` tag.
- [ ] On the main Canvas, build a `WinScreen` panel:
  - a full-screen dark Image with a `CanvasGroup`
  - a TMP text "You escaped!"
  - a `Restart` Button
  - the `WinScreen` component, with `Panel` and `Restart Button` assigned
- [ ] Make sure `SampleScene` is in *File → Build Profiles → Scene List*. Restart needs it.
- [ ] Play: trigger the Escape and reach the Exit. The ghost vanishes, the game freezes and the panel fades in. Press Restart: a fresh game starts with 500 Money, the HUD works, E works, the death fade works.

## Finish

Use `ni-git-workflow`: commit (e.g. `feat: add escape win screen with restart`), push, then `gh pr create` with "Closes #N" and the checklist. Don't merge. Remove the worktree.

## Out of scope

- A main menu, Quit, credits, or saving progress.

## Risks and open questions

- If any other `DontDestroyOnLoad` object exists (check `Assets/Scripts` read-only), Restart must destroy it too. List them in the PR.
