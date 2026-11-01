# 0002: Hide the Player outside Regular state

Source brief: `Assets/ClaudeTasks/PlayerVisibility.txt`
Branch: `task/0002-player-visibility` · Ukrainian copy: `0002-player-visibility.uk.md`
Issue: https://github.com/yakupitman/Hackaton7.0/issues/2

## Goal

Based on `PlayerStateManager`: in every **Player State** except `Regular`, the **Player**, the **Ball** and the **Chain** must be invisible to every camera (including shadows). As soon as the state goes back to `Regular` they reappear.

## Before you start

1. Read `AGENTS.md` and `CONTEXT.md`.
2. `git checkout main && git pull && git checkout -b task/0002-player-visibility`
3. Files you may change: **create** `Assets/Scripts/PlayerVisibility.cs` (plus its `.meta`, see `AGENTS.md`). Don't edit other scripts or `SampleScene.unity`; other chats are working in parallel.

## Existing code to reuse

- `PlayerStateManager.Instance.OnStateChanged` (`UnityEvent<PlayerState>`) fires on every `SetState`, `Die()` included. `CurrentState` gives the current value.
- `PlayerStateManager.Instance` is set in its `Awake`, so subscribe in `Start` and unsubscribe in `OnDestroy` (the same pattern as `UIManager`).
- In the scene: `Player` (has the model and Animator children), and `BallAndChain` (has `BallFollower`, a `MeshRenderer`, and a `LineRenderer` for the **Chain**). The Ball is **destroyed** after the 5th **Chisel Kit**, so expect null renderers.

## Decisions (agreed)

- Hide **Player model + Ball + Chain** together.
- Hide by setting **`Renderer.enabled = false`**, not with `SetActive(false)`. This way physics, `BallFollower`, `Animator` and colliders keep running and nothing breaks. Disabled renderers also stop casting shadows.
- Every non-`Regular` state hides (`Interacting`, `Shopping`, `Catching`, `Dead`).

## Todos

1. Create `PlayerVisibility : MonoBehaviour` (style per `AGENTS.md`) to put on the `Player` object:
   - `[SerializeField] private Renderer[] targetRenderers;`. If it's empty in `Awake`, collect them automatically: `GetComponentsInChildren<Renderer>(true)` on this object, plus all renderers under `FindFirstObjectByType<BallFollower>()` (that covers the `MeshRenderer` and the `LineRenderer` Chain).
   - `Start`: subscribe to `OnStateChanged`, then apply the current state right away.
   - `OnDestroy`: unsubscribe (null-check `Instance`).
   - `private void ApplyState(PlayerState state)`: `bool visible = state == PlayerState.Regular;` then for each renderer, `if (r != null) r.enabled = visible;`
2. Leave particle systems (e.g. the Ball's `breakParticleSystem`) alone. They should still play when a Chisel Kit is bought.
3. Re-read the diff for compile errors and null safety.

## Inspector checklist (the user does this in the Editor)

- [ ] Add `PlayerVisibility` to the `Player` GameObject. Leave `Target Renderers` empty for auto-collect, or fill it in by hand if auto-collect picks up something it shouldn't (e.g. UI, the UseIndicator).
- [ ] Play: press E at the Store, Pond and Lift → the player, ball and chain vanish in the building camera; leave → they're back. Fail a Catch (Death) → hidden during the fade, back after respawn. Buy the 5th Chisel Kit → no errors.

## Finish

Commit (`feat: hide player, ball and chain outside regular state`), push the branch, then `gh pr create` with the checklist.

## Out of scope

- Fading in or out. Changing the state machine (the `Shopping`/`Catching` issues listed in `CONTEXT.md`).

## Risks and open questions

- If any child renderer of Player should stay visible (e.g. a future world-space marker), it has to be excluded through the Inspector array.
