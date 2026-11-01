# 0001: Індикатор взаємодії (Use Indicator) на будівлях

Оригінальне завдання: `Assets/ClaudeTasks/BuildingInteractionIdentificator.txt`
Гілка: `task/0001-use-indicator` · Англійська версія (основна): `0001-use-indicator.md`

## Мета

Кожна **Building** (Store, Lift, Pond, …) має вимкнений дочірній об'єкт `UseIndicator`. Коли **Player** у стані `Regular` і натискання E *прямо зараз* почало б інтеракцію з будівлею, `UseIndicator` цієї будівлі має бути видимим. Якщо E нічого не зробить (гравець відвернувся, відійшов, не в `Regular`), індикатор вимкнений.

## Перед початком

1. Прочитай `AGENTS.md` (стиль коду, правила паралельної роботи) і `CONTEXT.md` (терміни).
2. `git checkout main && git pull && git checkout -b task/0001-use-indicator`
3. Змінювати можна лише: `Assets/Scripts/Building.cs`, `Assets/Scripts/PlayerMovement.cs`. **Не** чіпай `SampleScene.unity` та інші скрипти: паралельно над ними працюють інші чати.

## Наявний код для повторного використання

- `PlayerMovement.TryBeginInteraction()` уже вирішує, чи спрацює E: `Physics.Raycast` з `transform.position` у напрямку `lastMoveDirection`, дальність `interactionRange` (4), колайдер з тегом `Building` і компонентом `Building`. **Індикатор має використовувати саме цю перевірку**, щоб «індикатор видно» і «E працює» ніколи не розходилися.
- `Building` є базовим класом для `Store`, `Lift`, `Pond`, з віртуальними `BeginInteraction`/`EndInteraction`.
- У сцені вже є 3 вимкнені об'єкти `UseIndicator` під будівлями.

## Рішення (погоджено)

- Показується **лише в стані `Regular`**. Усередині будівлі (`Interacting`/`Shopping`/…) усі індикатори приховані.
- Показ і приховування з **DOTween pop**:
  - Показ: `SetActive(true)`, потім масштаб від 0 до початкового з `Ease.OutBack`, приблизно 0.15 с.
  - Приховування: масштаб до 0 приблизно за 0.1 с, потім `SetActive(false)` в `OnComplete`.
  - Перед кожним твіном викликай `DOKill()`.
- Одночасно видно не більше одного індикатора (тієї будівлі, в яку влучає raycast).

## Завдання

1. **Building.cs**: підтримка індикатора.
   - `[SerializeField] private GameObject useIndicator;`. Якщо поле порожнє, знайди нащадка з іменем `UseIndicator` (рекурсивно, включно з неактивними). Збережи його початковий `localScale`. Він має стартувати прихованим.
   - Додай `public void SetIndicatorVisible(bool visible)` з описаним вище pop. Якщо стан не змінився або індикатора немає, метод нічого не робить.
   - Підкласи (`Store`, `Lift`, `Pond`) мають власний `Awake` без виклику базового. **Не** покладайся на `Building.Awake`: знаходь індикатор ліниво, при першому виклику `SetIndicatorVisible`. Підкласи не змінюй.
2. **PlayerMovement.cs**: винеси raycast у `private Building FindTargetBuilding()`, який повертає будівлю, з якою спрацює E, або null. `TryBeginInteraction()` має використовувати цей метод, тож його поведінка не зміниться.
3. **PlayerMovement.cs**: додай `private Building highlightedBuilding;` і метод `UpdateUseIndicator()`, який викликається в `Update()` після `TrackMoveDirection()`:
   - Ціль: `FindTargetBuilding()`, якщо стан `Regular`, інакше null.
   - Якщо ціль змінилася: сховай стару, покажи нову, запам'ятай її.
4. Дотримуйся стилю з `AGENTS.md`: порожні рядки лише між членами класу, коментарі англійською на окремому рядку, доступ до синглтонів через `.Instance`.
5. Перечитай diff, щоб переконатися, що код компілюється. Перевір `using DG.Tweening;` і що немає null ref, коли будівля без індикатора.

## Чекліст в Inspector (робить користувач у редакторі)

- [ ] Кожна будівля (Store, Lift, Pond1) має дочірній `UseIndicator`. Якщо він не знаходиться автоматично, перетягни його в нове поле `Use Indicator`.
- [ ] Play: підійди до будівлі лицем, і індикатор з'являється. Відвернися або відійди, і він зникає. Натисни E: усередині будівлі його немає. Натисни E ще раз: він повертається.

## Завершення

Коміт (conventional, напр. `feat: show use indicator when a building is interactable`), `git push -u origin task/0001-use-indicator`, `gh pr create` з чеклістом в описі.

## Поза межами задачі

- Арт або анімація індикатора, окрім pop. Підказка «натисни E, щоб вийти». Клік по важелю ліфта.

## Ризики та відкриті питання

- Raycast іде від pivot гравця (ноги). Якщо індикатор мерехтить на нерівній геометрії, опиши це в PR, але не змінюй raycast інтеракції в цій задачі.
