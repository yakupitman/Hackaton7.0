using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    // Deliberately NOT DontDestroyOnLoad (exception to the AGENTS.md singleton rule):
    // it references scene AudioSources for ducking, which are destroyed on Restart,
    // so a persistent copy would hold dead references and need adding to WinScreen.Restart().
    // A plain scene singleton just restarts the music with the scene.
    public static MusicPlayer Instance { get; private set; }

    [SerializeField] private AudioClip[] tracks;
    [SerializeField] private float minPause = 20f;
    [SerializeField] private float maxPause = 60f;
    [SerializeField] private float volume = 0.6f;
    // Music fades to duckVolume while any of these is playing (Pond ambient sources, Escape chase source)
    [SerializeField] private AudioSource[] duckWhilePlaying;
    [SerializeField] private float duckVolume = 0f;
    // Volume units per second
    [SerializeField] private float fadeSpeed = 1f;

    private AudioSource source;
    private int lastIndex = -1;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        source = GetComponent<AudioSource>();
        if (source == null) source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
        // 2D, so camera distance doesn't affect it
        source.spatialBlend = 0f;
        source.volume = volume;
    }

    private void Start()
    {
        // A duplicate skipped Awake setup and is about to be destroyed
        if (Instance != this) return;
        StartCoroutine(PlayLoop());
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        if (source == null) return;
        float target = IsDucked() ? duckVolume : volume;
        // Unscaled time keeps fades working while the win screen freezes timeScale
        source.volume = Mathf.MoveTowards(source.volume, target, fadeSpeed * Time.unscaledDeltaTime);
    }

    private bool IsDucked()
    {
        if (duckWhilePlaying == null) return false;
        foreach (AudioSource duck in duckWhilePlaying)
        {
            // Unity's null check also covers destroyed sources
            if (duck != null && duck.isPlaying) return true;
        }
        return false;
    }

    private int PickTrack()
    {
        // Candidates are the non-empty slots, minus the last played one when there is a choice
        List<int> candidates = new List<int>();
        for (int i = 0; i < tracks.Length; i++)
        {
            if (tracks[i] != null) candidates.Add(i);
        }
        if (candidates.Count == 0) return -1;
        if (candidates.Count > 1) candidates.Remove(lastIndex);
        return candidates[Random.Range(0, candidates.Count)];
    }

    private IEnumerator PlayLoop()
    {
        if (tracks == null || tracks.Length == 0)
        {
            Debug.LogWarning("MusicPlayer: no tracks assigned, no music will play.", this);
            yield break;
        }
        while (true)
        {
            int index = PickTrack();
            if (index < 0)
            {
                Debug.LogWarning("MusicPlayer: all track slots are empty, no music will play.", this);
                yield break;
            }
            lastIndex = index;
            source.clip = tracks[index];
            source.Play();
            // Realtime wait of the clip length: unaffected by timeScale (music plays through the win screen)
            // and avoids isPlaying being false for a frame after Play()
            yield return new WaitForSecondsRealtime(tracks[index].length);
            float low = Mathf.Max(0f, Mathf.Min(minPause, maxPause));
            float high = Mathf.Max(0f, Mathf.Max(minPause, maxPause));
            yield return new WaitForSecondsRealtime(Random.Range(low, high));
        }
    }
}
