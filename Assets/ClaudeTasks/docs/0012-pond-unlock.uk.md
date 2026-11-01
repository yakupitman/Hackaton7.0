# 0012: Заблоковані Pond і вікно розблокування

Оригінальне завдання: `Assets/ClaudeTasks/ImplementingFullGameplayCycle.txt`
Гілка: `task/0012-pond-unlock` · Англійська версія (основна): `0012-pond-unlock.md`
Issue: https://github.com/yakupitman/Hackaton7.0/issues/22

## Мета

Є 3 **Pond**, кожен з іншою **Prey**. Кожен Pond починається як **Locked Pond** і має власну ціну розблокування, що зростає від 1-го до 3-го. Натискання E біля Locked Pond відкриває вікно розблокування: "Unlock this Pond for N?" з кнопками Buy і Cancel. Після купівлі Pond працює як зараз. Розблокування Pond це спосіб для **Player** заробляти швидше.

## Перед початком

1. Прочитай `AGENTS.md` і `CONTEXT.md`.
2. Створи worktree `../Hackaton7.0-0012` на гілці `task/0012-pond-unlock` з актуального `origin/main` (скіл `ni-todo-worktree`).
3. Змінювати можна лише: `Assets/Scripts/Pond.cs`, новий `Assets/Scripts/PondUnlockPopup.cs` і його `.meta`. `SampleScene.unity`, префаби та інші скрипти не чіпай. Задачі 0011, 0013 і 0015 виконуються паралельно.

## Що є зараз (перевірено)

- `Pond1` тепер префаб (`Assets/Prefabs/Pond1.prefab`) з двома екземплярами в сцені, названими Pond1 і Pond2.
- Кожен Pond має власні `preyProfiles` (префаб Prey, тривалість Catch, швидкість, Personalities). Кожен префаб Prey має `Price` в `InteractableObject`. Тож "різна Prey на кожному Pond" це налаштування в Inspector.
- `PlayerMovement.TryBeginInteraction()` викликає `building.BeginInteraction()`, потім ставить `Interacting`, якщо стан досі `Regular`. E у стані, відмінному від `Regular`, викликає `EndCurrentInteraction()`, що завершує взаємодію і ставить `Regular`.
- `Pond.ActivePond` (задача 0009) встановлюється в `BeginInteraction` і очищається в `EndInteraction`.

## Рішення (погоджено)

- Pond можна розблоковувати в **будь-якому порядку**. Самі ціни роблять природним порядок 1 → 2 → 3.
- E біля Locked Pond відкриває **вікно**: "Unlock this Pond for N?" з кнопками Buy і Cancel. Buy неактивна, коли Money < N. Після купівлі вікно закривається, і Pond працює нормально при наступному E. Одразу в Pond не заходить.
- Тексти англійською і є полями Inspector.

## Завдання

1. **Pond.cs**:
   - Додай `[SerializeField] private bool isUnlocked;`, `[SerializeField] private int unlockPrice = 30;` і `public bool IsUnlocked => isUnlocked;`.
   - Додай `public UnityEvent OnUnlocked;` і `public void Unlock()`, що встановлює прапорець, є ідемпотентним і викликає `OnUnlocked`. Задача 0014 використовує `Unlock()` для Skip.
   - `BeginInteraction()`: якщо заблоковано, виклич `base.BeginInteraction()` і `PondUnlockPopup.Instance.Show(this, unlockPrice)`, потім return. Не вмикай камери чи UI Bait і не став `ActivePond`. Якщо розблоковано, поводься точно як зараз.
   - `EndInteraction()`: якщо вікно показане для цього Pond, сховай його. Решту не змінюй.
   - Додай `public bool TryBuyUnlock()`: `PlayerStateManager.Instance.SpendMoney(unlockPrice)`, потім `Unlock()`. Повертає true при успіху.
2. **PondUnlockPopup.cs**: одна спільна панель під Canvas, налаштована як сценовий синглтон за `AGENTS.md`.
   - Серіалізовані поля: `CanvasGroup panel`, `TextMeshProUGUI messageText`, `Button buyButton`, `Button cancelButton` і `string messageFormat = "Unlock this Pond for {0}?"`.
   - `Show(Pond pond, int price)`: поява через DOTween, спершу виклич `DOKill`. Buy інтерактивна лише якщо Money ≥ price. Поки вікно відкрите, оновлюй її через `PlayerStateManager.OnMoneyChanged`.
   - Buy: `pond.TryBuyUnlock()`, потім закрити.
   - Cancel: закрити.
   - **Закриття завжди чисто завершує взаємодію:** сховай панель, виклич `pond.EndInteraction()`, потім `PlayerStateManager.Instance.SetState(Regular)`, як це робить `Store.EndInteraction`. Натискання E при відкритому вікні теж має закривати його тим самим шляхом.
   - Додавай слухачі кнопок у коді, а не записами OnClick в Inspector.
3. Перечитай diff на помилки компіляції.

## Чекліст в Inspector (робить тестувальник у редакторі)

- [ ] Збудуй вікно під `Canvas`: панель з `CanvasGroup`, TMP-повідомлення та кнопки Buy і Cancel. Додай компонент `PondUnlockPopup` і признач його поля. Тримай об'єкт панелі активним (скрипт сам її ховає).
- [ ] Зроби 3-й Pond із префаба `Pond1`. На кожному Pond задай `Unlock Price`, що зростає від 1-го до 3-го, і залиш `Is Unlocked` вимкненим. Дай кожному Pond власні `Prey Profiles` з різною Prey та цінами.
- [ ] Play: E біля Locked Pond показує вікно з ціною. Buy неактивна без достатньої кількості Money. Buy витрачає Money, і наступний E заводить у Pond. Cancel і E обидва закривають вікно, і Player знову може ходити.

## Завершення

Через `ni-git-workflow`: коміт (напр. `feat: lock ponds behind an unlock popup`), push, потім `gh pr create` з "Closes #N" і чеклістом. Не мерджити. Видали worktree.

## Поза межами задачі

- Крок Guidelines "купити 1-й Pond" (задача 0014 слухає `OnUnlocked`). Балансування цін.

## Ризики та відкриті питання

- Чи потрібен Use Indicator і на Locked Pond? Він показуватиметься, бо вони все ще Building. Це, імовірно, нормально.
