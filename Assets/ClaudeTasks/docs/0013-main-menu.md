# 0013: Main Menu and new-game start state

Source brief: `Assets/ClaudeTasks/ImplementingFullGameplayCycle.txt`
Branch: `task/0013-main-menu` · Ukrainian copy: `0013-main-menu.uk.md`
Issue: https://github.com/yakupitman/Hackaton7.0/issues/23

## Goal

The scene opens on the **Main Menu**: a view from a dedicated menu camera, with Start and Quit. Start fades to black, then a new game begins. The **Player** spawns at `(0, 0, 0)` with speed 2, holding the starting **Money**. Quit closes the game. Restart on the win screen reloads the scene, which lands back on the Main Menu.

## Before you start

1. Read `AGENTS.md` and `CONTEXT.md`.
2. Create a worktree `../Hackaton7.0-0013` on branch `task/0013-main-menu` from up-to-date `origin/main` (skill `ni-todo-worktree`).
3. Files you may change: `Assets/Scripts/PlayerStateManager.cs` (the `PlayerState` enum, `Start()` and new fields only), new `Assets/Scripts/MainMenu.cs` and its `.meta`. Don't edit `SampleScene.unity` or other scripts. Tasks 0011, 0012 and 0015 run in parallel.

## What exists today (verified)

- `PlayerStateManager.Start()` sets `CurrentState = Regular` and `Money = 500`, both hard-coded.
- Every system already freezes outside `Regular`: movement, rotation, E-to-enter, Grab, and the Use Indicator. `PlayerVisibility` hides the Player, the Ball and the Chain outside `Regular`. The HUD shows only in `Regular` and `Shopping`.
- `WinScreen.Restart()` (task 0008) reloads the scene after destroying the persistent managers.
- `PlayerMovement.currentMoveSpeed` is 8 in the scene. The brief wants a start speed of 2.

## Decisions (agreed)

- One entry point: the Main Menu with **Start** and **Quit**. Restart from the win screen goes back to it.
- A **dedicated MenuCamera**, a Cinemachine camera the co-developer places, with the highest priority while the menu is open.
- Starting Money is an Inspector field. Its default must exactly cover the **Guidelines** purchases: 1st Chisel Kit + 3 cheapest Bait + 1st Pond. The co-developer sets the final number.
- The Player's start speed is 2 and the Ghost's speed is 7. Only a Player with 5 Kits (speed 8.0) outruns it. These are Inspector values; see the checklist.
- The texts are in English.

## Todos

1. **PlayerStateManager.cs**:
   - Add `Menu` to `PlayerState`, at the end, so existing values don't shift.
   - In `Start()`, the initial state is `Menu`. Add `[SerializeField] private int startingMoney = 500;` and use it instead of the hard-coded 500.
   - Check that `SetState`'s HUD rule hides the HUD in `Menu`. It shows only `Regular` and `Shopping`, so it should already.
2. **MainMenu.cs** (on the menu panel):
   - Serialized fields: `CanvasGroup menuPanel`, `Button startButton`, `Button quitButton`, `CinemachineCamera menuCamera`, and `CanvasGroup fade` (a full-screen black panel; it can be its own or the existing death `fadeScreen`, your choice, explain in the PR).
   - Add `public UnityEvent OnGameStarted;` (task 0014 starts the Guidelines from it).
   - `Start()`: show the menu and enable `menuCamera`. Add the listeners in code.
   - Start button: fade to black, hide the menu, disable `menuCamera`, `SetState(Regular)`, fade back, then invoke `OnGameStarted`. The Player is already at `(0, 0, 0)` by scene placement (see the checklist).
   - Quit button: `Application.Quit()`, plus `EditorApplication.isPlaying = false` under `#if UNITY_EDITOR`.
3. Check read-only that nothing breaks while in `Menu`, and report it in the PR: the Pond, Store and Lift triggers, `EscapeSequence` (its trigger only fires if the Player walks in), `BallFollower`, `UIManager`, and `PlayerVisibility` (it hides the Player in the menu, which is fine).
4. Re-read the diff for compile errors.

## Inspector checklist (the tester does this in the Editor)

- [ ] Add a `MenuCamera` (Cinemachine) with a nice view of the room and a priority higher than the gameplay camera.
- [ ] Build the menu under `Canvas`: a panel with a `CanvasGroup`, a title, and Start and Quit buttons. Add `MainMenu` and assign its fields. Keep the panel active.
- [ ] Move the `Player` (and `BallAndChain` just behind it) to `(0, 0, 0)`.
- [ ] `PlayerMovement` → `Current Move Speed` = **2**. `EscapeSequence` → `Ghost Speed` = **7**.
- [ ] `PlayerStateManager` → `Starting Money` = 1st Kit price + 3 × cheapest Bait price + 1st Pond unlock price.
- [ ] Play: the menu shows from MenuCamera and the Player can't move. Start fades to the game at speed 2. Quit exits Play mode. Win the Escape and press Restart: you're back at the menu.

## Finish

Use `ni-git-workflow`: commit (e.g. `feat: add main menu and menu start state`), push, then `gh pr create` with "Closes #N" and the checklist. Don't merge. Remove the worktree.

## Out of scope

- The Guidelines tutorial (task 0014). Music (task 0015). Settings and credits.
