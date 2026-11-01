# 0004: Темрява в коридорах Pond

Оригінальне завдання: `Assets/ClaudeTasks/Darkness.txt`
Гілка: `task/0004-corridor-darkness` · Англійська версія (основна): `0004-corridor-darkness.md`

## Мета

Кожен **Pond** складається з дверей і коридору за ними. Заповни коридор (лише коридор, не всю локацію) густою темрявою, зімітованою кількома напівпрозорими темними шарами. Єдине, що видно вдалині, це матеріали з **Emission**. Ефект: «перше, що з'являється в темряві, це палаючі очі». Під час **Catch** візуал Prey рухається до камери, тож він має виринати з темряви, починаючи з очей.

## Перед початком

1. Прочитай `AGENTS.md` і `CONTEXT.md`.
2. `git checkout main && git pull && git checkout -b task/0004-corridor-darkness`
3. Змінювати чи створювати можна лише: `Assets/Scripts/CorridorDarkness.cs`, `Assets/Shaders/CorridorDarkness.shader` і матеріали облич Prey `Assets/ShadersMaterials/PreyFace/*.mat` (тільки render queue). Для кожного нового файлу й теки створи `.meta` (див. `AGENTS.md`). **Не** чіпай `SampleScene.unity` та інші скрипти.

## Наявний код і ассети

- Зараз у сцені лише один Pond (`Pond1`), але **буде більше**, тож темрява має бути компонентом, який можна використовувати повторно.
- `Pond.cs` використовує `pondCamera` (вибір Bait) і `pondCamera2` (вигляд Catch вздовж коридору). Візуал Prey рухається від `preyVisualsSpawnPoint` до `pondCamera2`.
- `Assets/ShadersMaterials/PreyFace/face1–6.mat` мають `_EMISSION`. Це і є «очі». Перевір, які саме частини світяться.
- Проєкт на URP 17 (Unity 6).

## Рішення (погоджено)

- **Темрява завжди перемагає.** Ліхтарик Pond її не пробиває, видно лише емісивні матеріали.
- Це компонент, який будує шари процедурно в рантаймі: користувач лише кладе порожній GameObject з ним у кожен коридор. YAML префаба вручну не пишемо, префаб користувач робить сам у редакторі.

## Завдання

1. **Шейдер** `Assets/Shaders/CorridorDarkness.shader`: unlit transparent, сумісний з URP, з властивістю `_Color` (чорний, альфа задає густину). Налаштування: `Blend SrcAlpha OneMinusSrcAlpha`, `ZWrite Off`, `Cull Off`, `Queue = Transparent` (3000), `RenderType = Transparent`, pass `UniversalForward` (HLSL, `Core.hlsl`).
2. **CorridorDarkness.cs** (стиль з `AGENTS.md`):
   - Серіалізовані поля:
     - `Shader darknessShader`; якщо не задано, `Shader.Find("Custom/CorridorDarkness")`.
     - `int layerCount = 8`, `float depth` (довжина коридору вздовж локальної +Z), `Vector2 size` (ширина × висота шару).
     - `AnimationCurve alphaByDepth` (густина від ближнього кінця до дальнього, напр. 0.15 → 0.6).
     - `Color darknessColor = black`.
   - `Awake`: створи один спільний матеріал і `layerCount` дочірніх quad (`GameObject.CreatePrimitive(PrimitiveType.Quad)` без колайдера). Розстав їх рівномірно вздовж локальної Z, лицем уздовж осі коридору. Кожному дай `MaterialPropertyBlock` з власною альфою. Вимкни тіні: `shadowCastingMode = Off`, `receiveShadows = false`.
   - `OnDrawGizmos`: малюй wire-box `size × depth` і площини шарів, щоб користувач міг підігнати його під коридор у Scene view без Play.
3. **Емісивні очі крізь темряву.** У кожному `PreyFace/face*.mat` постав `m_CustomRenderQueue` на `3050` (тільки це поле). Тоді вони рендеряться після шарів темряви й перевіряють глибину відносно непрозорої сцени, тож видні поверх темряви. Якщо світитися має лише частина матеріалу обличчя, опиши проблему в PR замість перебудови мешів.
4. Перечитай усе на помилки компіляції та шейдера. Помилки в шейдері видно лише в редакторі, тож тримай шейдер мінімальним і стандартним.

## Чекліст в Inspector (робить користувач у редакторі)

- [ ] У `Pond1` створи порожній дочірній `CorridorDarkness` на ближньому кінці коридору, +Z дивиться вздовж коридору. Додай компонент і підгони gizmo-box під коридор.
- [ ] Признач `Darkness Shader`, якщо він не знайшовся автоматично. Також додай його в *Project Settings → Graphics → Always Included Shaders*, щоб збірка його знайшла.
- [ ] Перетягни об'єкт у `Assets/Prefabs/`, щоб отримати префаб `CorridorDarkness` для майбутніх Pond.
- [ ] Зіграй Catch: коридор темний, спершу з'являються очі, тіло Prey видно в міру наближення. З головної top-down камери чорних quad поза Pond не видно; якщо видно, зменш або зсунь box.
- [ ] Підкрути `layerCount` і `alphaByDepth`, поки не виглядатиме як треба.

## Завершення

Коміт (`feat: add corridor darkness for ponds`), push гілки, потім `gh pr create` з чеклістом і коротким поясненням трюку з render queue.

## Поза межами задачі

- Дублювання Pond1 у нові Pond. **Увага тому, хто це робитиме:** кнопки Bait і UI мініігри Pond1 прив'язані до `SelectBait` саме Pond1, тож копії Pond знадобиться власна прив'язка UI.
- Ліхтарик, що пробиває темряву. Зміни освітлення чи постобробки в інших місцях.

## Ризики та відкриті питання

- Вищий render queue матеріалів облич змушує їх малюватися поверх інших прозорих об'єктів (напр. частинок Incense). Поки що це прийнятно; якщо виглядає погано, згадай у PR.
