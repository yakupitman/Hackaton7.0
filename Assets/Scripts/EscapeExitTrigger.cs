using UnityEngine;

public class EscapeExitTrigger : MonoBehaviour
{
    [SerializeField] private EscapeSequence escapeSequence;

    private void Awake()
    {
        if (escapeSequence == null) escapeSequence = FindFirstObjectByType<EscapeSequence>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (escapeSequence == null || !other.CompareTag("Player")) return;
        // EscapeSequence ignores this unless the Escape is running
        escapeSequence.SuccessfulEscape();
    }
}
