# 0003: Prey не повинна змінювати розмір на платформі ліфта

Оригінальне завдання: `Assets/ClaudeTasks/LiftInteractableResizing.txt`
Гілка: `task/0003-lift-prey-resizing` · Англійська версія (основна): `0003-lift-prey-resizing.md`
Issue: https://github.com/yakupitman/Hackaton7.0/issues/3

## Мета

Коли **Prey** потрапляє на **Lift Platform** (яку потім рухає DOTween), вона може різко змінити свій масштаб. Prey має піднятися ліфтом до **Sell Height** і повернутися назад без жодної зміни масштабу чи спотворення.

## Перед початком

1. Прочитай `AGENTS.md` і `CONTEXT.md`.
2. `git checkout main && git pull && git checkout -b task/0003-lift-prey-resizing`
3. Змінювати можна лише: `Assets/Scripts/LiftPlatform.cs`, `Assets/Scripts/Lift.cs` і за потреби `Assets/Scripts/InteractableObject.cs`. `SampleScene.unity` та інші скрипти не чіпай.

## Причина (перевірено)

`LiftPlatform` стоїть у сцені з **нерівномірним масштабом `(5, 0.25, 5)`**. `LiftPlatform.OnCollisionEnter` робить `collision.transform.SetParent(transform, true)`. Повернута Prey під батьком з нерівномірним масштабом перекошується, і `localScale` не може цього передати, тому Prey помітно змінює розмір. Нинішній словник `originalScales` лише відновлює масштаб при виході, а це запізно.

## Рішення (погоджено)

**Не робити Prey дочірньою.** Нести «пасажирів» разом із платформою: на кожному кроці фізики зсувати їх на стільки, на скільки зсунулася платформа. Лише код, сцену не змінюємо.

## Завдання

1. **LiftPlatform.cs**: переписати.
   - Прибери `SetParent` і словник `originalScales`.
   - Тримай `HashSet<Rigidbody> riders`. `OnCollisionEnter`: якщо в об'єкта є `InteractableObject` і `Rigidbody`, додай його. `OnCollisionExit`: видали.
   - Зберігай позицію платформи з минулого кадру. У `FixedUpdate` обчисли `delta = transform.position - lastPosition`, потім для кожного пасажира (пропускай null, бо Prey знищується при **Sell**) зроби `rb.MovePosition(rb.position + delta)`. Онови `lastPosition`. Прибери null-пасажирів.
   - Пропускай пасажирів, яких гравець зараз тримає (**Grab**, див. пункт 3).
2. **Lift.cs**: рух платформи синхронно з фізикою.
   - Додай `.SetUpdate(UpdateType.Fixed)` до послідовності твінів ліфта.
   - Знайдений баг: зворотний твін їде до захардкодженого `0.4125f`, а платформа стартує з y ≈ `0.125`. Збережи `startY = liftPlatform.position.y` в `Start` і повертайся до нього.
3. **InteractableObject.cs** (лише за потреби): `public bool IsHeld => isHeld;`, щоб `LiftPlatform` пропускав Prey, яку тримають (нею керує швидкість з `HoldManager`).
4. Перевір, що Sell працює: платформа піднімається на `liftHeight` (10), а `InteractableObject` продається при y ≥ 10. Пасажири мають справді досягти цієї висоти, не зісковзнути й не провалитися.
5. Перечитай diff на помилки компіляції.

## Чекліст в Inspector (робить користувач у редакторі)

- [ ] Play: поклади 1, потім кілька Prey (повернутих під різними кутами) на платформу й смикни важіль. Масштаб не змінюється. Prey піднімаються, продаються нагорі (Money зростає), платформа повертається на початкову висоту.
- [ ] Візьми Prey з платформи посеред підйому: вона йде за мишею, а не за платформою.

## Завершення

Коміт (`fix: carry prey on lift platform without parenting`), push гілки, потім `gh pr create` з чеклістом.

## Поза межами задачі

- Зміна ієрархії платформи в сцені. Зміна правил продажу.

## Ризики та відкриті питання

- Якщо при швидкому підйомі Prey трясеться або провалюється, спробуй кінематичний `Rigidbody` (доданий у рантаймі) з `MovePosition` або вищий `collisionDetectionMode` у Prey. Опиши вибір у PR.
