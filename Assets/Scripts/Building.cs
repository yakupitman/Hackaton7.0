using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class Building : MonoBehaviour
{
    public UnityEvent OnInteractionBegan;
    public UnityEvent OnInteractionEnded;

    [SerializeField] private GameObject useIndicator;

    private Vector3 indicatorScale = Vector3.one;
    private bool indicatorResolved;
    private bool indicatorVisible;

    public virtual void BeginInteraction()
    {
        OnInteractionBegan?.Invoke();
    }

    public virtual void EndInteraction()
    {
        OnInteractionEnded?.Invoke();
    }

    public void SetIndicatorVisible(bool visible)
    {
        ResolveIndicator();
        if (useIndicator == null || visible == indicatorVisible) return;
        indicatorVisible = visible;
        Transform indicator = useIndicator.transform;
        // Kill first so a pending hide can't deactivate a re-shown indicator
        indicator.DOKill();
        if (visible)
        {
            // Start from zero only when fully hidden, so a cancelled hide reverses smoothly
            if (!useIndicator.activeSelf)
            {
                useIndicator.SetActive(true);
                indicator.localScale = Vector3.zero;
            }
            indicator.DOScale(indicatorScale, 0.15f).SetEase(Ease.OutBack).SetLink(useIndicator, LinkBehaviour.KillOnDestroy);
        }
        else
        {
            indicator.DOScale(Vector3.zero, 0.1f).SetLink(useIndicator, LinkBehaviour.KillOnDestroy).OnComplete(() => useIndicator.SetActive(false));
        }
    }

    // Subclasses override Awake without calling base, so resolve lazily on first use
    private void ResolveIndicator()
    {
        if (indicatorResolved) return;
        indicatorResolved = true;
        if (useIndicator == null)
        {
            foreach (Transform child in GetComponentsInChildren<Transform>(true))
            {
                if (child != transform && child.name == "UseIndicator")
                {
                    useIndicator = child.gameObject;
                    break;
                }
            }
        }
        if (useIndicator == null) return;
        indicatorScale = useIndicator.transform.localScale;
        useIndicator.SetActive(false);
    }
}
