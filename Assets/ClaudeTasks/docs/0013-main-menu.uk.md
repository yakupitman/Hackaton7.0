# 0013: Main Menu і стартовий стан нової гри

Оригінальне завдання: `Assets/ClaudeTasks/ImplementingFullGameplayCycle.txt`
Гілка: `task/0013-main-menu` · Англійська версія (основна): `0013-main-menu.md`
Issue: https://github.com/yakupitman/Hackaton7.0/issues/23

## Мета

Сцена відкривається на **Main Menu**: вид зі спеціальної камери меню, з кнопками Start і Quit. Start затемнює екран, після чого починається нова гра. **Player** з'являється в `(0, 0, 0)` зі швидкістю 2 і початковими **Money**. Quit закриває гру. Restart на екрані перемоги перезавантажує сцену, і гра знову потрапляє в Main Menu.

## Перед початком

1. Прочитай `AGENTS.md` і `CONTEXT.md`.
2. Створи worktree `../Hackaton7.0-0013` на гілці `task/0013-main-menu` з актуального `origin/main` (скіл `ni-todo-worktree`).
3. Змінювати можна лише: `Assets/Scripts/PlayerStateManager.cs` (лише enum `PlayerState`, `Start()` і нові поля), новий `Assets/Scripts/MainMenu.cs` і його `.meta`. `SampleScene.unity` та інші скрипти не чіпай. Задачі 0011, 0012 і 0015 виконуються паралельно.

## Що є зараз (перевірено)

- `PlayerStateManager.Start()` ставить `CurrentState = Regular` і `Money = 500`, обидва значення захардкожені.
- Усі системи вже завмирають поза `Regular`: рух, обертання, E для входу, Grab і Use Indicator. `PlayerVisibility` ховає Player, Ball і Chain поза `Regular`. HUD показується лише в `Regular` і `Shopping`.
- `WinScreen.Restart()` (задача 0008) перезавантажує сцену після знищення постійних менеджерів.
- `PlayerMovement.currentMoveSpeed` у сцені дорівнює 8. За брифом стартова швидкість має бути 2.

## Рішення (погоджено)

- Одна точка входу: Main Menu з **Start** і **Quit**. Restart з екрана перемоги повертає в неї.
- **Окрема MenuCamera**, камера Cinemachine, яку розміщує співрозробник, з найвищим пріоритетом, поки меню відкрите.
- Початкові Money це поле Inspector. Його значення за замовчуванням має точно покривати покупки **Guidelines**: 1-й Chisel Kit + 3 найдешевші Bait + 1-й Pond. Остаточне число ставить співрозробник.
- Стартова швидкість Player дорівнює 2, а швидкість Ghost дорівнює 7. Лише Player із 5 Kit (швидкість 8.0) утече від нього. Це значення Inspector, див. чекліст.
- Тексти англійською.

## Завдання

1. **PlayerStateManager.cs**:
   - Додай `Menu` до `PlayerState` у кінець, щоб наявні значення не зсунулися.
   - У `Start()` початковий стан це `Menu`. Додай `[SerializeField] private int startingMoney = 500;` і використовуй його замість захардкодженого 500.
   - Перевір, що правило HUD у `SetState` ховає HUD у `Menu`. Він показується лише в `Regular` і `Shopping`, тож це вже має працювати.
2. **MainMenu.cs** (на панелі меню):
   - Серіалізовані поля: `CanvasGroup menuPanel`, `Button startButton`, `Button quitButton`, `CinemachineCamera menuCamera` і `CanvasGroup fade` (повноекранна чорна панель; може бути власна або наявний `fadeScreen` смерті, на твій вибір, поясни в PR).
   - Додай `public UnityEvent OnGameStarted;` (задача 0014 запускає Guidelines від нього).
   - `Start()`: покажи меню і ввімкни `menuCamera`. Слухачів додавай у коді.
   - Кнопка Start: затемнення, сховати меню, вимкнути `menuCamera`, `SetState(Regular)`, зникнення затемнення, потім викликати `OnGameStarted`. Player уже в `(0, 0, 0)` завдяки розміщенню в сцені (див. чекліст).
   - Кнопка Quit: `Application.Quit()`, плюс `EditorApplication.isPlaying = false` під `#if UNITY_EDITOR`.
3. Перевір (лише читання), що нічого не ламається в `Menu`, і опиши це в PR: тригери Pond, Store і Lift, `EscapeSequence` (його тригер спрацьовує, лише якщо Player зайде), `BallFollower`, `UIManager` і `PlayerVisibility` (він ховає Player у меню, це нормально).
4. Перечитай diff на помилки компіляції.

## Чекліст в Inspector (робить тестувальник у редакторі)

- [ ] Додай `MenuCamera` (Cinemachine) з гарним видом на кімнату і пріоритетом вищим за ігрову камеру.
- [ ] Збудуй меню під `Canvas`: панель з `CanvasGroup`, заголовок і кнопки Start та Quit. Додай `MainMenu` і признач його поля. Тримай панель активною.
- [ ] Перемісти `Player` (і `BallAndChain` одразу позаду нього) у `(0, 0, 0)`.
- [ ] `PlayerMovement` → `Current Move Speed` = **2**. `EscapeSequence` → `Ghost Speed` = **7**.
- [ ] `PlayerStateManager` → `Starting Money` = ціна 1-го Kit + 3 × ціна найдешевшого Bait + ціна розблокування 1-го Pond.
- [ ] Play: меню показується з MenuCamera, і Player не може рухатися. Start плавно веде в гру зі швидкістю 2. Quit виходить з режиму Play. Виграй Escape і натисни Restart: ти знову в меню.

## Завершення

Через `ni-git-workflow`: коміт (напр. `feat: add main menu and menu start state`), push, потім `gh pr create` з "Closes #N" і чеклістом. Не мерджити. Видали worktree.

## Поза межами задачі

- Туторіал Guidelines (задача 0014). Музика (задача 0015). Налаштування і титри.
