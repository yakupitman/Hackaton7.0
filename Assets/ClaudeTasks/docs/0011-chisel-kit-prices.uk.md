# 0011: Зростаючі ціни на Chisel Kit і видалення Booster

Оригінальне завдання: `Assets/ClaudeTasks/ImplementingFullGameplayCycle.txt`
Гілка: `task/0011-chisel-kit-prices` · Англійська версія (основна): `0011-chisel-kit-prices.md`
Issue: https://github.com/yakupitman/Hackaton7.0/issues/21

## Мета

Кожен із 5 **Chisel Kit** коштує дорожче за попередній, за фіксованим списком цін. Купити всі 5 і є метою гри. **Store** показує ціну наступного Kit і перестає продавати Kit після 5-го. Недороблений Booster видаляється.

## Перед початком

1. Прочитай `AGENTS.md` і `CONTEXT.md`.
2. Створи worktree `../Hackaton7.0-0011` на гілці `task/0011-chisel-kit-prices` з актуального `origin/main` (скіл `ni-todo-worktree`).
3. Змінювати можна лише: `Assets/Scripts/Store.cs`. `SampleScene.unity` та інші скрипти не чіпай. Задачі 0012, 0013 і 0015 виконуються паралельно на інших файлах.

## Що є зараз (перевірено)

- `Store.chieselKitPrice` це одна ціна (50 у сцені), однакова для кожного Kit. Назва поля з помилкою.
- У `Store` ніщо не рахує Kit. `BallFollower` (`TotalChiselUpgrades = 5`) і `PlayerMovement` (`speedIncreases`) ведуть власні лічильники.
- `Store.BuyBooster()` лише витрачає Money і пише в лог "effect will be implemented later". Співрозробник додав у сцену кнопку Booster і об'єкт `WeightLiftBooster`.

## Рішення (погоджено)

- Ціни Kit беруться з явного списку з 5 значень в Inspector, напр. `[50, 120, 250, 450, 800]`. Числа налаштовує співрозробник.
- Booster видаляється. Часу на його дизайн немає.

## Завдання

1. **Store.cs**:
   - Заміни `chieselKitPrice` на `[SerializeField] private int[] chiselKitPrices = { 50, 120, 250, 450, 800 };`.
   - Додай `private int chiselKitsBought;` і `public int ChiselKitsBought => chiselKitsBought;`, а також `public int NextChiselKitPrice`, що повертає -1, коли куплено все.
   - `BuyChiselKit()`:
     - Якщо куплено всі 5, нічого не роби.
     - Інакше витрать `NextChiselKitPrice`. У разі успіху збільш лічильник, викличи `OnChiselKitBought` і закрий Store (як зараз).
     - Наявну поведінку при невдачі збережи.
   - Додай `[SerializeField] private TextMeshProUGUI chiselKitPriceText;` і `[SerializeField] private Button chiselKitButton;`. Показуй наступну ціну або "Sold out" після 5-го, а коли все продано, роби кнопку неінтерактивною. Оновлюй обидва в `Awake`/`Start` і після кожної покупки.
   - Видали `BuyBooster()` і `boosterPrice`.
2. Перечитай diff на помилки компіляції. Перевір using: `TMPro`, `UnityEngine.UI`.

## Чекліст в Inspector (робить тестувальник у редакторі)

- [ ] Видали кнопку Booster і об'єкт `WeightLiftBooster`. Їхній OnClick вказував би на видалений метод.
- [ ] На `Store`: задай `Chisel Kit Prices` (5 значень) і признач `Chisel Kit Price Text` (TMP-напис на кнопці Kit) та `Chisel Kit Button`.
- [ ] Play: кожен Kit коштує наступну ціну зі списку, напис оновлюється, а після 5-го кнопка показує "Sold out" і вимкнена.

## Завершення

Через `ni-git-workflow`: коміт (напр. `feat: escalate chisel kit prices and remove booster`), push, потім `gh pr create` з "Closes #N" і чеклістом. Не мерджити. Видали worktree.

## Поза межами задачі

- Блокування Store під час **Guidelines** (задача 0014). Ефекти Kit (Ball і швидкість).
