using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleHide : MonoBehaviour
{
    public void Toggle()
    {
        gameObject.SetActive(!gameObject.activeInHierarchy);
    }
}
