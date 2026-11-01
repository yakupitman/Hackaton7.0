# 0008: Перемога в Escape: екран перемоги і Restart

Джерело: відома проблема (без брифу). Див. `KNOWN-ISSUES.uk.md`.
Гілка: `task/0008-escape-win-screen` · Англійська версія (основна): `0008-escape-win-screen.md`

## Мета

Досягти **Exit** під час **Escape** означає перемогти. Гра зупиняється, плавно з'являється панель «You escaped!», а кнопка Restart перезавантажує сцену в чисту нову гру.

## Перед початком

1. Прочитай `AGENTS.md` і `CONTEXT.md`.
2. Створи worktree `../Hackaton7.0-0008` на гілці `task/0008-escape-win-screen` з актуального `origin/main` (скіл `ni-todo-worktree`).
3. Змінювати можна лише:
   - `Assets/Scripts/EscapeSequence.cs`
   - новий `Assets/Scripts/EscapeExitTrigger.cs`
   - новий `Assets/Scripts/WinScreen.cs`
   - `.meta` для нових скриптів

   `SampleScene.unity` та інші скрипти не чіпай. Панель UI тестувальник збирає в редакторі.

## Що не так зараз (перевірено)

- Об'єкт `EscapeSuccessTrigger` у сцені має лише Transform і колайдер. `EscapeSequence.SuccessfulEscape()` ніхто не викликає, тож досягнення Exit **нічого не робить**.
- Навіть якщо його викликати, `SuccessfulEscape()` лише пише `"WIN!"` у лог.
- Пастка з Restart: `PlayerStateManager`, `InputManager`, `HoldManager` і `UIManager` є синглтонами з `DontDestroyOnLoad`. Звичайний `SceneManager.LoadScene` лишить *старі* менеджери, які посилаються на знищені об'єкти старої сцени (затемнення, HUD, гравець), і знищить нові. Restart буде зламаний.

## Рішення (погоджено)

- При перемозі: зупинити гру, плавно показати просту панель перемоги і дати кнопку **Restart**, яка перезавантажує сцену.

## Завдання

1. **EscapeExitTrigger.cs** (на `EscapeSuccessTrigger`):
   - `[SerializeField] private EscapeSequence escapeSequence;`; якщо поле порожнє, `FindFirstObjectByType<EscapeSequence>()`.
   - `OnTriggerEnter`: якщо в об'єкта тег `Player`, виклич `escapeSequence.SuccessfulEscape()`.
2. **EscapeSequence.cs**: додай `public UnityEvent OnEscaped;` і викликай його в `SuccessfulEscape()` після наявного скидання. Прибери `Debug.Log("WIN!")`.
3. **WinScreen.cs**:
   - Серіалізовані поля: `CanvasGroup panel`, `Button restartButton`, `EscapeSequence escapeSequence` (для останнього запасний варіант `FindFirstObjectByType`).
   - `Start`: сховай панель (alpha 0, не інтерактивна, без raycast), підпишись на `OnEscaped` і додай слухача Restart у коді.
   - `Show()`: `Time.timeScale = 0`, потім плавно покажи панель через DOTween з `.SetUpdate(true)`, щоб твін працював, поки гра зупинена. Потім зроби панель інтерактивною.
   - `Restart()`:
     - `Time.timeScale = 1` і `DOTween.KillAll()`.
     - Знищ `gameObject` кожного наявного постійного синглтона (`PlayerStateManager`, `InputManager`, `HoldManager`, `UIManager`).
     - Потім `SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex)`.
     - Додай коментар, чому менеджери знищуються.
4. Перечитай diff на помилки компіляції. Перевір using: `UnityEngine.Events`, `UnityEngine.UI`, `UnityEngine.SceneManagement`, `DG.Tweening`.

## Чекліст в Inspector (робить тестувальник у редакторі)

- [ ] Додай `EscapeExitTrigger` на `EscapeSuccessTrigger`. Переконайся, що в його колайдера ввімкнено **Is Trigger** і що гравець має тег `Player`.
- [ ] На головному Canvas збери панель `WinScreen`:
  - темне Image на весь екран з `CanvasGroup`
  - TMP-текст «You escaped!»
  - кнопка `Restart`
  - компонент `WinScreen` з призначеними `Panel` і `Restart Button`
- [ ] Переконайся, що `SampleScene` є в *File → Build Profiles → Scene List*. Без цього Restart не спрацює.
- [ ] Play: запусти Escape і добіжи до Exit. Привид зникає, гра зупиняється, плавно з'являється панель. Restart: нова гра з 500 Money, HUD працює, E працює, затемнення смерті працює.

## Завершення

Через `ni-git-workflow`: коміт (напр. `feat: add escape win screen with restart`), push, потім `gh pr create` з "Closes #N" і чеклістом. Не мерджити. Видали worktree.

## Поза межами задачі

- Головне меню, Quit, титри чи збереження прогресу.

## Ризики та відкриті питання

- Якщо є ще якісь об'єкти з `DontDestroyOnLoad` (перевір `Assets/Scripts`, лише читання), Restart має знищувати й їх. Перелічи їх у PR.
