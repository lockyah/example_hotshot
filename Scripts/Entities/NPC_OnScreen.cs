using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC_OnScreen : MonoBehaviour
{
    /*
     * Attached to characters with behaviours to enable or disable them depending on whether they are on-screen.
     * Will be attached to the child object with its sprite renderer, as only that triggers OnBecame(In)Visible events.
     */

    public Behaviour[] components; //Which behaviours should be affected by this?

    private void OnBecameVisible()
    {
        foreach(Behaviour c in components)
        {
            c.enabled = true;
        }
    }

    private void OnBecameInvisible()
    {
        foreach (Behaviour c in components)
        {
            c.enabled = false;
        }
    }
}
