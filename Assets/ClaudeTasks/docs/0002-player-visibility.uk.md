# 0002: Приховування гравця поза станом Regular

Оригінальне завдання: `Assets/ClaudeTasks/PlayerVisibility.txt`
Гілка: `task/0002-player-visibility` · Англійська версія (основна): `0002-player-visibility.md`
Issue: https://github.com/yakupitman/Hackaton7.0/issues/2

## Мета

На основі `PlayerStateManager`: у будь-якому **Player State**, окрім `Regular`, **Player**, **Ball** і **Chain** не повинні бути видимі на жодній камері (включно з тінями). Щойно стан повертається в `Regular`, вони знову з'являються.

## Перед початком

1. Прочитай `AGENTS.md` і `CONTEXT.md`.
2. `git checkout main && git pull && git checkout -b task/0002-player-visibility`
3. Змінювати можна лише: **створити** `Assets/Scripts/PlayerVisibility.cs` (і його `.meta`, див. `AGENTS.md`). Інші скрипти та `SampleScene.unity` не чіпай: паралельно працюють інші чати.

## Наявний код для повторного використання

- `PlayerStateManager.Instance.OnStateChanged` (`UnityEvent<PlayerState>`) спрацьовує при кожному `SetState`, включно з `Die()`. `CurrentState` повертає поточний стан.
- `PlayerStateManager.Instance` встановлюється в його `Awake`, тож підписуйся в `Start` і відписуйся в `OnDestroy` (так само, як `UIManager`).
- У сцені є `Player` (з дочірніми моделлю й Animator) і `BallAndChain` (з `BallFollower`, `MeshRenderer` і `LineRenderer` для **Chain**). Ball **знищується** після 5-го **Chisel Kit**, тож рендерери можуть бути null.

## Рішення (погоджено)

- Ховати **модель гравця, Ball і Chain** разом.
- Ховати через **`Renderer.enabled = false`**, не через `SetActive(false)`. Так фізика, `BallFollower`, `Animator` і колайдери продовжують працювати, і нічого не ламається. Вимкнений рендерер також не відкидає тіні.
- Ховати в усіх станах, окрім `Regular` (`Interacting`, `Shopping`, `Catching`, `Dead`).

## Завдання

1. Створи `PlayerVisibility : MonoBehaviour` (стиль з `AGENTS.md`) для об'єкта `Player`:
   - `[SerializeField] private Renderer[] targetRenderers;`. Якщо масив порожній, в `Awake` збери рендерери автоматично: `GetComponentsInChildren<Renderer>(true)` на цьому об'єкті плюс усі рендерери під `FindFirstObjectByType<BallFollower>()` (це покриває `MeshRenderer` і `LineRenderer` ланцюга).
   - `Start`: підпишись на `OnStateChanged` і одразу застосуй поточний стан.
   - `OnDestroy`: відпишись (з перевіркою `Instance` на null).
   - `private void ApplyState(PlayerState state)`: `bool visible = state == PlayerState.Regular;`, потім для кожного рендерера `if (r != null) r.enabled = visible;`
2. Системи частинок (напр. `breakParticleSystem` на Ball) не чіпай: вони мають програватися при купівлі Chisel Kit.
3. Перечитай diff на помилки компіляції та null.

## Чекліст в Inspector (робить користувач у редакторі)

- [ ] Додай `PlayerVisibility` на GameObject `Player`. Залиш `Target Renderers` порожнім для автозбору або заповни вручну, якщо автозбір захоплює зайве (UI, UseIndicator).
- [ ] Play: натисни E біля Store, Pond, Lift, і гравець, куля та ланцюг зникають на камері будівлі. Вийди, і вони повертаються. Програй Catch (Death): гравець прихований під час затемнення і повертається після респавну. Купи 5-й Chisel Kit: помилок немає.

## Завершення

Коміт (`feat: hide player, ball and chain outside regular state`), push гілки, потім `gh pr create` з чеклістом.

## Поза межами задачі

- Плавне зникнення та появу. Зміни в машині станів (проблеми `Shopping`/`Catching`, описані в `CONTEXT.md`).

## Ризики та відкриті питання

- Якщо якийсь дочірній рендерер гравця має залишатися видимим (напр. майбутній маркер), його треба виключити через масив в Inspector.
