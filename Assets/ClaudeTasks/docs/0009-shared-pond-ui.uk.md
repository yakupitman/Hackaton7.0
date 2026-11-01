# 0009: Спільний UI для Pond, який працює з активним Pond

Джерело: відома проблема (без брифу). Див. `KNOWN-ISSUES.uk.md`.
Гілка: `task/0009-shared-pond-ui` · Англійська версія (основна): `0009-shared-pond-ui.md`

## Мета

**Pond** стане більше. Є одна панель Bait і один HUD **Catch**, і їхні кнопки мають діяти на той Pond, у якому зараз **Player**. Тоді для нового Pond не треба підключати UI.

## Перед початком

1. Прочитай `AGENTS.md` і `CONTEXT.md`.
2. Створи worktree `../Hackaton7.0-0009` на гілці `task/0009-shared-pond-ui` з актуального `origin/main` (скіл `ni-todo-worktree`).
3. Змінювати можна лише: `Assets/Scripts/Pond.cs`, новий `Assets/Scripts/PondBaitRouter.cs` і його `.meta`. `SampleScene.unity` та інші скрипти не чіпай.

## Що не так зараз (перевірено)

- Події `OnClick` кнопок Bait у сцені викликають напряму `Pond1.SelectBait(int)` (у `SampleScene.unity` кілька записів `SelectBait`). Копія Pond однаково надсилала б кліки в Pond1.
- Інші посилання Pond на UI (`availableBaits`, `catchingMinigame`, `bar`, `area`, `needle`) ведуть на спільні об'єкти UI. Копія Pond зберігає ці посилання, тож зламана лише маршрутизація кнопок.

## Рішення (погоджено)

- Лишаємо один спільний UI. Кнопки викликають роутер, а роутер передає виклик активному Pond.

## Завдання

1. **Pond.cs**:
   - Додай `public static Pond ActivePond { get; private set; }`.
   - Встановлюй його в `this` у `BeginInteraction()`. В `EndInteraction()` очищай, лише якщо там досі `this`.
2. **PondBaitRouter.cs** (на спільній панелі Bait): `public void SelectBait(int baitType)` передає виклик у `Pond.ActivePond.SelectBait(baitType)`, якщо активний Pond є. Якщо немає, виведи попередження в лог і нічого не роби.
3. Перевір (лише читання), чи щось ще в `SampleScene.unity` викликає Pond1 напряму через події UI, напр. кнопка «закрити». Перелічи все в PR, щоб тестувальник перепідключив.
4. Перечитай diff на помилки компіляції.

## Чекліст в Inspector (робить тестувальник у редакторі)

- [ ] Додай `PondBaitRouter` на спільну панель Bait (об'єкт `availableBaits`).
- [ ] У кожному записі `OnClick` кнопки Bait, що веде на `Pond1 → Pond.SelectBait`, зміни ціль на `PondBaitRouter.SelectBait` цієї панелі з тим самим int-аргументом.
- [ ] Play біля Pond1: кнопки Bait працюють як раніше.
- [ ] **Як додати новий Pond** (на потім): дублюй `Pond1`. Дай копії власні камери (`Pond Camera`, `Pond Camera 2`), аудіоджерела, ліхтарик, точку появи візуалу Prey і `CorridorDarkness`. Посилання на UI залиш на спільний UI. Тег колайдера `Building`.

## Завершення

Через `ni-git-workflow`: коміт (напр. `feat: route shared bait ui to the active pond`), push, потім `gh pr create` з "Closes #N" і чеклістом. Не мерджити. Видали worktree.

## Поза межами задачі

- Власне додавання Pond2. Зміни мініігри Catch.

## Ризики та відкриті питання

- Кожен Pond виконує свій `Awake` на тих самих спільних об'єктах UI (`SetupUIElement` кешує початковий Y). Це має бути нешкідливо, але підтверди в PR.
