using UnityEngine;
using UnityEngine.Events;

public class Building : MonoBehaviour
{
    public UnityEvent OnInteractionBegan;
    public UnityEvent OnInteractionEnded;

    public virtual void BeginInteraction()
    {
        OnInteractionBegan?.Invoke();
    }

    public virtual void EndInteraction()
    {
        OnInteractionEnded?.Invoke();
    }
}
