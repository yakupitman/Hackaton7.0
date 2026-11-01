# 0014: Туторіал Guidelines зі Skip

Оригінальне завдання: `Assets/ClaudeTasks/ImplementingFullGameplayCycle.txt`
Гілка: `task/0014-guidelines-tutorial` · Англійська версія (основна): `0014-guidelines-tutorial.md`
Issue: https://github.com/yakupitman/Hackaton7.0/issues/24

## Мета

Кожна нова гра (Start у **Main Menu**) запускає **Guidelines**, покрокову панель, що обмежує **Store** і просувається, коли **Player** виконує кожен крок. Кнопка Skip на панелі переносить у **Free Play** в тому самому стані, у якому залишив би завершений туторіал.

## Залежності

**Починати лише після злиття задач 0011, 0012 і 0013.** Ця задача використовує кількість і ціни Kit з 0011, `Pond.OnUnlocked`/`Unlock()` з 0012 та `MainMenu.OnGameStarted` разом зі станом `Menu` з 0013.

## Перед початком

1. Прочитай `AGENTS.md` і `CONTEXT.md`.
2. Створи worktree `../Hackaton7.0-0014` на гілці `task/0014-guidelines-tutorial` з актуального `origin/main` (скіл `ni-todo-worktree`).
3. Змінювати можна лише:
   - новий `Assets/Scripts/Guidelines.cs` і його `.meta`
   - невеликі хуки в `Store.cs` (блокування товарів), `Pond.cs` (подія "Prey caught") і `EscapeSequence.cs` (подія "caught by the Ghost")

   `SampleScene.unity` та інші скрипти не чіпай.

## Кроки (погоджено)

| # | Текст Guidelines (англійською, редагується в Inspector) | Store дозволяє | Просувається, коли |
|---|------|------|------|
| 1 | Buy your first Chisel Kit in the Store. | лише Chisel Kit | `Store.OnChiselKitBought` |
| 2 | Buy 3 of the cheapest Bait. | лише найдешевший Bait (tier 0) | Player має ≥ 3 Bait tier-0 (`OnBaitInventoryChanged`) |
| 3 | Go to the right corner of the room and unlock the first Pond. | нічого | будь-який `Pond.OnUnlocked` |
| 4 | Pick a Bait and start fishing. | нічого | починається Catch (або одразу до 5 у разі успіху) |
| 5 | Catch it! Keep the needle inside the moving area. *(пояснює Catch)* | нічого | успіх Catch (нова `Pond.OnPreyCaught`) |
| 6 | Carry the Prey to the Lift and send it up to sell it. | нічого | Money зростають від Sell |
| 7 | Leave through the main door and run down the corridor. | нічого | Player **спійманий Ghost** (нова `EscapeSequence.OnPlayerCaught`); інші Death не рахуються |
| 8 | "Didn't make it? Try to lighten that heavy thing. Have fun!" показується 15 с після респавну | усе | таймер спливає → **Free Play** |

- Невдалий Catch не змінює крок. У Player досі є Bait. Soft-lock можливий, якщо він витратить усі Bait, і це прийнятно. Вихід це Skip.
- **Free Play:** панель Guidelines схована, весь Store увімкнено.

## Рішення (погоджено)

- Туторіал запускається **у кожній новій грі** і його можна **пропустити** кнопкою Skip **на панелі Guidelines**.
- **Skip = той самий стан, що й завершений туторіал:** застосовано 1-й Chisel Kit, розблоковано Pond 1, 3 найдешевші Bait.
  - Зроби це, завершивши решту покупок туторіалу. Купуй Kit і Bait звичайними методами Store, а найдешевший заблокований Pond розблокуй через `Unlock()`.
  - Плати за них з Money, які за дизайном їх покривають (стартові Money з 0013 покривають саме ці покупки). Результат однаковий, чи пропускаєш на кроці 1, чи на кроці 6.
  - Потім перейди у Free Play без підказки на 15 с.
- Тексти англійською і є полями Inspector.

## Завдання

1. **Store.cs**:
   - Додай серіалізовані посилання на Button для Kit, 3 Bait і Teleport.
   - Додай `public void SetTutorialLock(bool kit, bool bait0, bool others)` і `public void ClearTutorialLock()`. Вони задають інтерактивність кнопок. Блокування має також захищати самі методи Buy, а не лише кнопки.
   - Не повинно конфліктувати з вимкненням кнопки Kit ("Sold out") із задачі 0011.
2. **Pond.cs**: додай `public UnityEvent OnPreyCaught;`, що викликається в `CompleteMinigame(true)`. За бажанням додай `OnCatchStarted` у `StartMinigame()` для кроку 4.
3. **EscapeSequence.cs**: додай `public UnityEvent OnPlayerCaught;`, що викликається в `CheckCatch()` безпосередньо перед `Die()`.
4. **Guidelines.cs**:
   - Серіалізовані поля: панель (`CanvasGroup` + TMP-текст), кнопка Skip, масив текстів кроків, текст підказки та її тривалість (15), а також посилання на `Store`, `MainMenu` і Pond (або `FindObjectsByType<Pond>`).
   - Підпишись на `MainMenu.OnGameStarted` → почати з кроку 1.
   - Керуй кроками через наведені вище події. Відписуйся в `OnDestroy`.
   - Крок 8: після `OnPlayerCaught` дочекайся респавну (стан знову `Regular`), покажи підказку на 15 с, потім перейди у Free Play.
   - Skip: як погоджено вище.
5. Перечитай diff на помилки компіляції. Перевір, що кожну підписку знято в `OnDestroy`, бо Restart перезавантажує сцену.

## Чекліст в Inspector (робить тестувальник у редакторі)

- [ ] Збудуй панель Guidelines під `Canvas`: `CanvasGroup`, TMP-текст і кнопка Skip. Додай `Guidelines` і признач його поля. За бажанням відредагуй тексти кроків.
- [ ] На `Store` признач нові посилання на кнопки Kit, Bait і Teleport.
- [ ] Пройди весь туторіал один раз, крок за кроком. Потім почни нову гру і натисни Skip на кроці 1, а потім ще раз на пізнішому кроці. Обидва випадки ведуть у Free Play з 1 Kit, розблокованим Pond 1 і 3 найдешевшими Bait, а Store повністю відкритий.

## Завершення

Через `ni-git-workflow`: коміт, push, потім `gh pr create` з "Closes #N" і чеклістом. Не мерджити. Видали worktree.

## Поза межами задачі

- Запам'ятовування завершення туторіалу між сесіями. Озвучка чи стрілки, що вказують на об'єкти.

## Ризики та відкриті питання

- Крок 6 ("Money зростають") спрацював би й від будь-якого іншого джерела Money. Зараз Money додає лише Sell.
- Якщо Player помре від Ghost до кроку 7, це не рахується, і крок лишається на місці.
