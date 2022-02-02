using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvokeScripts : MonoBehaviour
{
    public GameObject ActivateTarget; //Which object does this activate, if any?
    public string[] ActivateScript; //Which script to use?
    public string[] ActivateMethod; //Which method on that object should be used?

    public void Activate()
    {
        for (int i = 0; i < ActivateScript.Length; i++)
        {
            if (ActivateTarget != null && ActivateScript[i] != null && ActivateMethod[i] != null)
            {
                //Errors are handled by Unity, no try/catch needed
                (ActivateTarget.GetComponent(ActivateScript[i]) as MonoBehaviour).Invoke(ActivateMethod[i], 0f);
                Debug.Log("Activated method " + ActivateMethod[i] + " on object " + ActivateTarget.name);
            }
        }
    }
}
