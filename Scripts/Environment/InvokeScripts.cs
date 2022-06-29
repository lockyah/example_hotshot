using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InvokeScripts : MonoBehaviour
{
    public UnityEvent[] events;

    public void Activate()
    {
        foreach(UnityEvent e in events)
        {
            e.Invoke();
        }
    }
}
