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
            if (!originalScales.ContainsKey(collision.transform)) originalScales.Add(collision.transform, collision.transform.localScale);
            collision.transform.SetParent(transform, true);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        InteractableObject interactable = collision.gameObject.GetComponent<InteractableObject>();
        if (interactable != null)
        {
            collision.transform.SetParent(null, true);
            if (originalScales.TryGetValue(collision.transform, out Vector3 originalScale))
            {
                collision.transform.localScale = originalScale;
                originalScales.Remove(collision.transform);
            }
        }
    }
}