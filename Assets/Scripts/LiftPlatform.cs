using UnityEngine;
using System.Collections.Generic;

public class LiftPlatform : MonoBehaviour
{
    private Dictionary<Transform, Vector3> originalScales = new Dictionary<Transform, Vector3>();

    private void OnCollisionEnter(Collision collision)
    {
        InteractableObject interactable = collision.gameObject.GetComponent<InteractableObject>();
        if (interactable != null)
        {
            // Caches the original scale of the object before parenting to prevent permanent distortion
            if (!originalScales.ContainsKey(collision.transform)) originalScales.Add(collision.transform, collision.transform.localScale);
            // Parents the object to the platform so it moves seamlessly with the DOTween transform animation
            collision.transform.SetParent(transform, true);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        InteractableObject interactable = collision.gameObject.GetComponent<InteractableObject>();
        if (interactable != null)
        {
            // Unparents the object when it leaves the platform to restore independent movement
            collision.transform.SetParent(null, true);
            // Forcibly restores the cached scale to fix any Unity non-uniform scaling artifacts
            if (originalScales.TryGetValue(collision.transform, out Vector3 originalScale))
            {
                collision.transform.localScale = originalScale;
                originalScales.Remove(collision.transform);
            }
        }
    }
}