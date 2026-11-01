# Плани задач

Основні плани англійською; файли `*.uk.md` є українськими копіями. Коли план змінюється, оновлюй обидві версії.

| # | Задача | Гілка | Які файли можна змінювати |
|---|--------|-------|---------------------------|
| 0001 | [Use Indicator на будівлях](0001-use-indicator.uk.md) | `task/0001-use-indicator` | `Building.cs`, `PlayerMovement.cs` |
| 0002 | [Приховування гравця поза Regular](0002-player-visibility.uk.md) | `task/0002-player-visibility` | новий `PlayerVisibility.cs` |
| 0003 | [Зміна розміру Prey на ліфті](0003-lift-prey-resizing.uk.md) | `task/0003-lift-prey-resizing` | `LiftPlatform.cs`, `Lift.cs`, `InteractableObject.cs` |
| 0004 | [Темрява в коридорах](0004-corridor-darkness.uk.md) | `task/0004-corridor-darkness` | новий `CorridorDarkness.cs`, новий шейдер, `PreyFace/*.mat` |
| 0005 | [Респавн кулі разом із гравцем](0005-death-respawn.uk.md) | `task/0005-death-respawn` | `PlayerStateManager.cs` (лише `Die()`), `BallFollower.cs` |
| 0006 | [Shopping тримається, Catching прибрано](0006-player-states-cleanup.uk.md) | `task/0006-player-states-cleanup` | `PlayerStateManager.cs`, `PlayerMovement.cs`, `PlayerAnimator.cs` — **після злиття #5 і 0005** |
| 0007 | [Продаж лише через Lift](0007-sell-via-lift-only.uk.md) | `task/0007-sell-via-lift-only` | `Store.cs` |
| 0008 | [Екран перемоги в Escape](0008-escape-win-screen.uk.md) | `task/0008-escape-win-screen` | `EscapeSequence.cs`, новий `EscapeExitTrigger.cs`, новий `WinScreen.cs` |
| 0009 | [Спільний UI для Pond](0009-shared-pond-ui.uk.md) | `task/0009-shared-pond-ui` | `Pond.cs`, новий `PondBaitRouter.cs` |
| 0010 | [Teleport переносить і кулю](0010-teleport-with-ball.uk.md) | `task/0010-teleport-with-ball` | `PlayerStateManager.cs` (`UseTeleport()` + метод респавну) — **після злиття #14** |
| 0011 | [Зростання цін Chisel Kit, без Booster](0011-chisel-kit-prices.uk.md) | `task/0011-chisel-kit-prices` | `Store.cs` |
| 0012 | [Locked Pond + вікно розблокування](0012-pond-unlock.uk.md) | `task/0012-pond-unlock` | `Pond.cs`, новий `PondUnlockPopup.cs` |
| 0013 | [Main Menu + стартовий стан](0013-main-menu.uk.md) | `task/0013-main-menu` | `PlayerStateManager.cs` (enum, `Start()`), новий `MainMenu.cs` |
| 0014 | [Туторіал Guidelines + Skip](0014-guidelines-tutorial.uk.md) | `task/0014-guidelines-tutorial` | новий `Guidelines.cs`; хуки в `Store.cs`, `Pond.cs`, `EscapeSequence.cs` — **після злиття 0011–0013** |
| 0015 | [Музичний плеєр](0015-music-player.uk.md) | `task/0015-music-player` | новий `MusicPlayer.cs` |

Задачі змінюють різні файли й жодна не редагує `SampleScene.unity`, тож їх можна виконувати одночасно — окрім 0006, яка чекає на злиття PR #5 (0001) і 0005.

Кожна задача закінчується PR і чеклістом для Inspector, який виконує тестувальник. Відомі проблеми й компроміси: [KNOWN-ISSUES.uk.md](KNOWN-ISSUES.uk.md).

## Промпт для нового чату

Заміни `NNNN-name` на файл задачі:

```
Read AGENTS.md, CONTEXT.md and Assets/ClaudeTasks/docs/NNNN-name.md, then carry out that plan end to end:
create the branch it names from an up-to-date main, change only the files it allows, commit, push, and open a
PR with `gh pr create` that includes the plan's Inspector checklist. Don't touch SampleScene.unity. If something
in the plan contradicts the code, stop and tell me instead of guessing.
```
