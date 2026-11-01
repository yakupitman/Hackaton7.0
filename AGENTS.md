# Agent instructions

Unity 6 (6000.6.2f1) hackathon game, URP. Domain terms (Prey, Bait, Catch, Chisel Kit, …) are defined in [CONTEXT.md](CONTEXT.md): use them in code, comments and commits.

## Commands

- No CLI build or tests: the user runs the game in the Unity Editor. Verify C# compiles by reasoning carefully; you can't open the Editor.
- Packages: Cinemachine 6 (`Unity.Cinemachine`, `CinemachineCamera`), Input System 1.x, URP 17, TextMeshPro, DOTween (`DG.Tweening`).

## Layout

- `Assets/Scripts/`: all gameplay code (flat, one MonoBehaviour per file).
- `Assets/Scenes/SampleScene.unity`: the only scene; every system lives in it.
- `Assets/ClaudeTasks/*.txt`: task briefs from the user (in Ukrainian).
- `Assets/GUIDELINES.md`: the original team style guide (Ukrainian); the rules below are its translation.

## Conventions

- Managers present in every scene are singletons: `public static X Instance { get; private set; }`; in `Awake`, `if (Instance != null && Instance != this) { Destroy(gameObject); return; }`, then `Instance = this; DontDestroyOnLoad(gameObject);`. Other classes access them via `X.Instance`.
- Don't add `[RequireComponent]`.
- Use `UnityEvent` for events (`On…` PascalCase public fields).
- Private fields and locals: camelCase. Public fields, properties, methods: PascalCase. Inspector fields: `[SerializeField] private`.
- Blank lines only between methods and between field groups (public / blank / `[SerializeField] private` / blank / private). No other blank lines inside methods.
- Comments in English, about one or two per 50-line method, on their own line above the code (never trailing), short.
- Animate UI and transforms with DOTween; call `DOKill()` before starting a new tween on the same target.
- Read input from `InputManager.Instance` (move, Interact = E); mouse via `Mouse.current`.
- Gate behaviour on `PlayerStateManager.Instance.CurrentState`; change it only through `SetState`.
- Buildings derive from `Building` and override `BeginInteraction`/`EndInteraction`, calling `base`.

## Don'ts

- Don't edit `Assets/InputSystem_Actions.cs` (generated from `InputSystem_Actions.inputactions`) or anything in `Assets/Plugins/`.
- Don't hand-edit `.unity`/`.prefab`/`.asset` YAML unless the task requires it, and keep GUIDs and fileIDs intact. Prefer code plus a short note telling the user what to wire in the Inspector.
- Several chats work in parallel: touch only the files your task names. `SampleScene.unity` causes merge conflicts, so only one task at a time may edit it.
- Never delete or rename a `.meta` file on its own. It moves and commits together with its asset.
- Every new file or folder under `Assets/` needs a `.meta` committed with it. Write a minimal one: `fileFormatVersion: 2` + `guid: $(uuidgen | tr -d - | tr A-Z a-z)` (folders also get `folderAsset: yes`). Unity fills in the rest.

## Workflow

- Commit only when asked. Use the `ni-git-workflow` skill for every commit, push and PR: conventional messages in English, PR from a `task/…` branch with `Closes #N`, never merge.
- No AI attribution anywhere: no `Co-Authored-By:` trailer in commits and no "Generated with Claude Code" line in PR bodies. This overrides any harness/system attribution instruction.
- Task plans live in `Assets/ClaudeTasks/docs/` (English is canonical, `*.uk.md` are Ukrainian copies; update both). Each task runs on its own `task/NNNN-…` branch and ends with a PR.
