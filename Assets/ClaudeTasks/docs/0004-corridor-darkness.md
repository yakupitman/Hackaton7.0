# 0004: Corridor Darkness in Ponds

Source brief: `Assets/ClaudeTasks/Darkness.txt`
Branch: `task/0004-corridor-darkness` · Ukrainian copy: `0004-corridor-darkness.uk.md`
Issue: https://github.com/yakupitman/Hackaton7.0/issues/4

## Goal

Each **Pond** is a door with a corridor behind it. Fill the corridor (only the corridor, not the whole map) with thick darkness, faked with several semi-transparent dark layers. The only things visible deep in it should be materials with **Emission**. The feeling to aim for: "the first thing that appears in the darkness is burning eyes." During a **Catch**, the Prey visual moves toward the camera, so it should come out of the darkness eyes-first.

## Before you start

1. Read `AGENTS.md` and `CONTEXT.md`.
2. `git checkout main && git pull && git checkout -b task/0004-corridor-darkness`
3. Files you may change or create: `Assets/Scripts/CorridorDarkness.cs`, `Assets/Shaders/CorridorDarkness.shader`, and the Prey face materials in `Assets/ShadersMaterials/PreyFace/*.mat` (render queue only). Create a `.meta` for every new file and folder (see `AGENTS.md`). **Don't** edit `SampleScene.unity` or any other script.

## Existing code and assets

- Only one Pond exists in the scene right now (`Pond1`), but **more are coming**, so the darkness must be a reusable component.
- `Pond.cs` uses `pondCamera` (Bait choice) and `pondCamera2` (the Catch view down the corridor). The Prey visual travels from `preyVisualsSpawnPoint` toward `pondCamera2`.
- `Assets/ShadersMaterials/PreyFace/face1–6.mat` have `_EMISSION`. These are the "eyes". Check which parts are emissive.
- The project is URP 17 (Unity 6).

## Decisions (agreed)

- **Darkness always wins.** The Pond flashlight doesn't cut through it; only emissive materials show.
- It's a component that builds its layers procedurally at runtime, so the user only has to drop an empty GameObject with it into each corridor. The plan doesn't hand-write prefab YAML; the user turns it into a prefab in the Editor.

## Todos

1. **Shader** `Assets/Shaders/CorridorDarkness.shader`: a URP-compatible unlit transparent shader with a `_Color` property (black, alpha controls density). Settings: `Blend SrcAlpha OneMinusSrcAlpha`, `ZWrite Off`, `Cull Off`, `Queue = Transparent` (3000), `RenderType = Transparent`, and a `UniversalForward` pass (HLSL, `Core.hlsl`).
2. **CorridorDarkness.cs** (style per `AGENTS.md`):
   - Serialized fields:
     - `Shader darknessShader`, falling back to `Shader.Find("Custom/CorridorDarkness")`.
     - `int layerCount = 8`, `float depth` (corridor length along local +Z), `Vector2 size` (width × height of each layer).
     - `AnimationCurve alphaByDepth` (density per layer from the near end to the far end, e.g. 0.15 → 0.6).
     - `Color darknessColor = black`.
   - `Awake`: create one shared material and `layerCount` child quads (`GameObject.CreatePrimitive(PrimitiveType.Quad)` with the collider removed). Spread them evenly along local Z, facing the corridor axis. Give each a `MaterialPropertyBlock` with its own alpha. Turn shadows off: `shadowCastingMode = Off`, `receiveShadows = false`.
   - `OnDrawGizmos`: draw a wire box of `size × depth` and the layer planes, so the user can fit it to the corridor in the Scene view without pressing Play.
3. **Make emissive eyes show through.** Set `m_CustomRenderQueue` to `3050` in each `PreyFace/face*.mat` (only that field). They then render after the darkness layers and depth-test against the opaque scene, so they appear on top of the darkness. If only part of a face material should glow, explain the problem in the PR rather than restructuring meshes.
4. Re-read everything for compile and shader errors. Shader typos only show up in the Editor, so keep the shader minimal and standard.

## Inspector checklist (the user does this in the Editor)

- [ ] In `Pond1`, create an empty child `CorridorDarkness` at the near end of the corridor, with +Z pointing down the corridor. Add the component and fit the gizmo box to the corridor.
- [ ] Assign `Darkness Shader` if it isn't found automatically. Also add it to *Project Settings → Graphics → Always Included Shaders* so builds can find it.
- [ ] Drag it into `Assets/Prefabs/` to make a `CorridorDarkness` prefab, for future Ponds.
- [ ] Play a Catch: the corridor is dark, the eyes appear first, and the Prey body shows up as it approaches. From the main top-down camera, no black quads should be visible outside the Pond. If they are, shrink or move the box.
- [ ] Tune `layerCount` and `alphaByDepth` until it looks right.

## Finish

Commit (`feat: add corridor darkness for ponds`), push the branch, then `gh pr create` with the checklist and a short note on the render-queue trick.

## Out of scope

- Duplicating Pond1 into new Ponds. **Heads-up for whoever does that:** Pond1's Bait buttons and minigame UI are wired to Pond1's `SelectBait`, so a copied Pond will need its own UI wiring.
- Flashlight cutting through the darkness. Lighting or post-processing changes elsewhere.

## Risks and open questions

- Raising the face materials' render queue makes them draw over other transparent objects too (e.g. Incense particles). That's acceptable for now; mention it in the PR if it looks wrong.
