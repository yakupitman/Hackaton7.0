using System.Collections.Generic;
using UnityEngine;

public class PlayerVisibility : MonoBehaviour
{
    [SerializeField] private Renderer[] targetRenderers;

    private void Awake()
    {
        if (targetRenderers != null && targetRenderers.Length > 0) return;
        List<Renderer> collected = new List<Renderer>();
        AddRenderers(collected, transform);
        BallFollower ball = FindFirstObjectByType<BallFollower>();
        if (ball != null) AddRenderers(collected, ball.transform);
        targetRenderers = collected.ToArray();
    }

    private void Start()
    {
        if (PlayerStateManager.Instance == null) return;
        PlayerStateManager.Instance.OnStateChanged.AddListener(ApplyState);
        ApplyState(PlayerStateManager.Instance.CurrentState);
    }

    private void OnDestroy()
    {
        if (PlayerStateManager.Instance != null)
        {
            PlayerStateManager.Instance.OnStateChanged.RemoveListener(ApplyState);
        }
    }

    private void AddRenderers(List<Renderer> list, Transform root)
    {
        foreach (Renderer r in root.GetComponentsInChildren<Renderer>(true))
        {
            // Particle systems keep playing (e.g. the Ball's break effect)
            if (r is ParticleSystemRenderer) continue;
            if (!list.Contains(r)) list.Add(r);
        }
    }

    private void ApplyState(PlayerState state)
    {
        bool visible = state == PlayerState.Regular;
        foreach (Renderer r in targetRenderers)
        {
            // The Ball is destroyed by the last Chisel Kit, so entries can be null
            if (r != null) r.enabled = visible;
        }
    }
}
