using UnityEngine;
using System.Collections.Generic;

// Runs after DOTween's fixed tick so each step carries riders by the platform's current move
[DefaultExecutionOrder(1000)]
public class LiftPlatform : MonoBehaviour
{
    private Dictionary<Rigidbody, InteractableObject> riders = new Dictionary<Rigidbody, InteractableObject>();
    private List<Rigidbody> soldRiders = new List<Rigidbody>();
    private Vector3 lastPosition;

    private void Start()
    {
        lastPosition = transform.position;
    }

    private void FixedUpdate()
    {
        Vector3 delta = transform.position - lastPosition;
        lastPosition = transform.position;
        if (delta.sqrMagnitude < 1e-10f) return;
        soldRiders.Clear();
        foreach (KeyValuePair<Rigidbody, InteractableObject> rider in riders)
        {
            // Sold Prey are destroyed; a Grabbed Prey follows the mouse, not the platform
            if (rider.Key == null || rider.Value == null) soldRiders.Add(rider.Key);
            else if (!rider.Value.IsHeld) rider.Key.MovePosition(rider.Key.position + delta);
        }
        foreach (Rigidbody sold in soldRiders) riders.Remove(sold);
    }

    private void OnCollisionEnter(Collision collision)
    {
        TryAddRider(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        TryAddRider(collision);
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.rigidbody != null) riders.Remove(collision.rigidbody);
    }

    private void TryAddRider(Collision collision)
    {
        Rigidbody rider = collision.rigidbody;
        if (rider == null || riders.ContainsKey(rider)) return;
        InteractableObject prey = rider.GetComponent<InteractableObject>();
        if (prey != null) riders.Add(rider, prey);
    }
}
