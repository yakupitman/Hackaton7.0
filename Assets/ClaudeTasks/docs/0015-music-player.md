# 0015: Music player with shuffled tracks and pauses

Source brief: `Assets/ClaudeTasks/ImplementingGlobalAudio.txt`
Branch: `task/0015-music-player` · Ukrainian copy: `0015-music-player.uk.md`
Issue: https://github.com/yakupitman/Hackaton7.0/issues/25

## Goal

Background music from 3 tracks: pick a random track (never the same one twice in a row), play it, wait a random pause, and repeat. It runs from the moment the scene loads, including the **Main Menu**. It fades out during tense moments (a **Catch**, the **Escape** chase) and comes back afterwards.

## Before you start

1. Read `AGENTS.md` and `CONTEXT.md`.
2. Create a worktree `../Hackaton7.0-0015` on branch `task/0015-music-player` from up-to-date `origin/main` (skill `ni-todo-worktree`).
3. Files you may change: new `Assets/Scripts/MusicPlayer.cs` and its `.meta` **only**. Don't edit other scripts or `SampleScene.unity`. Tasks 0011–0013 run in parallel.

## What exists today (verified)

- There are no music files in the project. `Assets/SFX` has only effects. The co-developer adds the 3 clips.
- Tense audio already exists. Each Pond has an `ambientAudioSource` that loops the intense clip during a Catch. `EscapeSequence` has a `chaseAudioSource` that plays the Ghost spawn sound when the chase starts.
- `WinScreen.Restart()` reloads the scene after destroying the 4 persistent managers.

## Decisions (agreed)

- **Shuffle:** pick a random track each time, never the same one twice in a row. The pause between tracks is random, between a min and a max (Inspector).
- It also plays on the Main Menu.
- It fades out while a Catch or the Escape chase is going on, then fades back in.

## Design choice for this task

- **Ducking without touching other scripts:** `MusicPlayer` has `[SerializeField] private AudioSource[] duckWhilePlaying`. The co-developer assigns each Pond's ambient source and the Escape chase source. Each frame, if any of them `isPlaying`, the music fades to `duckVolume` (default 0); otherwise it fades back up. This keeps the task to one new file, so it can run in parallel.
- **Not `DontDestroyOnLoad`:** this is a deliberate exception to the `AGENTS.md` singleton rule.
  - The music player references scene AudioSources for ducking. A persistent copy would point at destroyed sources after Restart and would need adding to `WinScreen.Restart()`.
  - So it's a plain scene singleton (`Instance` set in `Awake`, no DDOL). Restart simply restarts the music.
  - Explain this in a comment and in the PR.

## Todos

1. **MusicPlayer.cs**:
   - Serialized fields:
     - `AudioClip[] tracks`, `float minPause = 20f`, `float maxPause = 60f`, `float volume = 0.6f`
     - `AudioSource[] duckWhilePlaying`, `float duckVolume = 0f`, `float fadeSpeed = 1f` (volume per second)
   - Create its own `AudioSource` in `Awake` (`playOnAwake = false`, `loop = false`), or use one on the same object.
   - Run a coroutine loop: pick a track ≠ the last one, play it, wait for its length, then wait `Random.Range(minPause, maxPause)`, and repeat. Handle 0 or 1 tracks without errors.
   - `Update`: move the volume towards `volume` or `duckVolume` with `Mathf.MoveTowards`, using **unscaled** time, so it still works while the win screen freezes the game.
   - Use `Random.Range` for both the track and the pause.
2. Re-read the diff for compile errors.

## Inspector checklist (the tester does this in the Editor)

- [ ] Import the 3 music clips (e.g. `Assets/Music/`).
- [ ] Create an empty `MusicPlayer` object in the scene. Add the component, assign `Tracks` (3), and tune the pauses and volume.
- [ ] Add every Pond's `Ambient Audio Source` (`PondAudio2` on each Pond) and the `EscapeSequence` `Chase Audio Source` to `Duck While Playing`.
- [ ] Play: music starts on the menu, tracks change randomly with pauses in between, it fades out during a Catch and the chase, and it comes back after.

## Finish

Use `ni-git-workflow`: commit (e.g. `feat: add shuffled music player with ducking`), push, then `gh pr create` with "Closes #N" and the checklist. Don't merge. Remove the worktree.

## Out of scope

- Volume settings UI. Different music per area.

## Risks and open questions

- The chase source only plays a one-shot spawn sound. If it's short, the music comes back mid-chase. In that case, assign a longer looping chase source, or accept it.
