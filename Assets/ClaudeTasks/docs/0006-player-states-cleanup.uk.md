# 0006: Shopping має триматися, Catching прибрати

Джерело: відома проблема (без брифу). Див. `KNOWN-ISSUES.uk.md`.
Гілка: `task/0006-player-states-cleanup` · Англійська версія (основна): `0006-player-states-cleanup.md`
Issue: https://github.com/yakupitman/Hackaton7.0/issues/10

## Мета

Навести лад у машині станів **Player State**:
- `Shopping` триває весь візит у **Store**, тож HUD з Money і **Bait** видно під час покупок.
- Невикористаний стан `Catching` прибрано.
- Будь-який стан, крім `Regular`, блокує рух, а не лише `Interacting`.

## Залежності

**Починай тільки після злиття PR #5 (задача 0001) і задачі 0005.** 0001 змінює `PlayerMovement.cs`, 0005 змінює `PlayerStateManager.cs`. Якщо почати раніше, будуть конфлікти злиття.

## Перед початком

1. Прочитай `AGENTS.md` і `CONTEXT.md`.
2. Створи worktree `../Hackaton7.0-0006` на гілці `task/0006-player-states-cleanup` з актуального `origin/main` (скіл `ni-todo-worktree`).
3. Змінювати можна лише: `Assets/Scripts/PlayerStateManager.cs`, `Assets/Scripts/PlayerMovement.cs`, `Assets/Scripts/PlayerAnimator.cs`. `SampleScene.unity` та інші скрипти не чіпай.

## Що не так зараз (перевірено)

- `Store.BeginInteraction()` ставить `Shopping`, а `PlayerMovement.TryBeginInteraction()` одразу ставить `Interacting`. Тож `Shopping` не тримається, і його правило в `SetState` (залишати HUD видимим) ніколи не діє.
- `Catching` оголошений, і `PlayerStateManager.CheckCatchingExit()` його обробляє, але в нього ніхто не переходить. Під час **Catch** гравець у стані `Interacting`.
- `PlayerMovement.Move()` блокує рух лише в `Interacting`. Коли `Shopping` почне триматися, гравець зможе ходити під час покупок. Ходити можна й у `Dead`, під час затемнення.
- `PlayerMovement.HandleInteractionInput()` дозволяє E вийти з будівлі лише з `Interacting`.

## Рішення (погоджено)

- `Shopping` тримається. E так само виводить зі Store.
- `Catching` видаляється повністю.

## Завдання

1. **PlayerStateManager.cs**:
   - Прибери `Catching` з `PlayerState`.
   - Прибери `CheckCatchingExit()`, а також `Update()`, якщо він стане порожнім.
   - Пошукай по всьому проєкту (скрипти **і** `SampleScene.unity`, лише читання) використання `Catching` і серіалізовані int-значення `PlayerState`, які зсунуться після зміни enum. Опиши в PR, що знайшов.
2. **PlayerMovement.cs**:
   - `TryBeginInteraction()`: після `BeginInteraction()` викликай `SetState(Interacting)` лише якщо стан досі `Regular`. Якщо будівля сама задала стан (Store ставить `Shopping`), залиш його.
   - `HandleInteractionInput()`: E завершує інтеракцію з будь-якого стану, крім `Regular` і `Dead`.
   - `Move()` і `Rotate()`: не ходити й не повертатися в будь-якому стані, крім `Regular`. Вертикальну швидкість залиш, як зараз.
3. **PlayerAnimator.cs**: idle-анімація (blend 0) в усіх станах, крім `Regular`, а не лише в `Interacting`.
4. Перевір інші місця, що читають стан (лише читання), і опиши, якщо щось ламається:
   - `HoldManager` (Grab лише в `Regular`)
   - Use Indicator у `Building` (лише в `Regular`)
   - `PlayerVisibility` (ховає в усіх станах, крім `Regular`)
   - `Store.BuyChiselKit` (викликає `EndInteraction`, далі стан повертається в `Regular` через `OnChiselKitBought`)
5. Перечитай diff на помилки компіляції.

## Чекліст в Inspector (робить тестувальник у редакторі)

- Нічого підключати не треба.
- [ ] E біля Store: HUD з Money і Bait видно, гравець не ходить, E виводить зі Store.
- [ ] E біля Pond і Lift: працюють як раніше.
- [ ] Під час затемнення смерті гравець не може рухатися.

## Завершення

Через `ni-git-workflow`: коміт (напр. `fix: keep shopping state and remove unused catching state`), push, потім `gh pr create` з "Closes #N" і чеклістом. Не мерджити. Видали worktree.

## Поза межами задачі

- Окремий стан для Catch. Баг, коли невдала покупка Chisel Kit закриває Store (див. `KNOWN-ISSUES.uk.md`).
