# Плани задач

Основні плани англійською; файли `*.uk.md` є українськими копіями. Коли план змінюється, оновлюй обидві версії.

| # | Задача | Гілка | Які файли можна змінювати |
|---|--------|-------|---------------------------|
| 0001 | [Use Indicator на будівлях](0001-use-indicator.uk.md) | `task/0001-use-indicator` | `Building.cs`, `PlayerMovement.cs` |
| 0002 | [Приховування гравця поза Regular](0002-player-visibility.uk.md) | `task/0002-player-visibility` | новий `PlayerVisibility.cs` |
| 0003 | [Зміна розміру Prey на ліфті](0003-lift-prey-resizing.uk.md) | `task/0003-lift-prey-resizing` | `LiftPlatform.cs`, `Lift.cs`, `InteractableObject.cs` |
| 0004 | [Темрява в коридорах](0004-corridor-darkness.uk.md) | `task/0004-corridor-darkness` | новий `CorridorDarkness.cs`, новий шейдер, `PreyFace/*.mat` |

Чотири задачі змінюють різні файли й жодна не редагує `SampleScene.unity`, тож їх можна виконувати одночасно. Кожна закінчується PR і чеклістом для Inspector, який виконує користувач.

## Промпт для нового чату

Заміни `NNNN-name` на файл задачі:

```
Read AGENTS.md, CONTEXT.md and Assets/ClaudeTasks/docs/NNNN-name.md, then carry out that plan end to end:
create the branch it names from an up-to-date main, change only the files it allows, commit, push, and open a
PR with `gh pr create` that includes the plan's Inspector checklist. Don't touch SampleScene.unity. If something
in the plan contradicts the code, stop and tell me instead of guessing.
```
