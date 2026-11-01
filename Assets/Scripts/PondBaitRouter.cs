using UnityEngine;

public class PondBaitRouter : MonoBehaviour
{
    // Called from the shared Bait buttons; forwards to the Pond the Player is in
    public void SelectBait(int baitType)
    {
        if (Pond.ActivePond == null)
        {
            Debug.LogWarning("PondBaitRouter: no active Pond, Bait click ignored", this);
            return;
        }
        Pond.ActivePond.SelectBait(baitType);
    }
}
