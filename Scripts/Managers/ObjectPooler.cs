using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    public GameObject[] ObjectsToPool;
    public int[] ObjectCounts;


    // Start is called before the first frame update
    void Start()
    {
        //Only instantiate objects if both lists have data and are the same length
        if (ObjectsToPool != null && ObjectCounts != null)
        {
            if (ObjectsToPool.Length == ObjectCounts.Length)
            {
                for (int obj = 0; obj < ObjectsToPool.Length; obj++)
                {
                    //For each object, instantiate the matching number of it
                    for (int num = 0; num < ObjectCounts[obj]; num++)
                    {
                        GameObject child = Instantiate(ObjectsToPool[obj]);
                        child.transform.parent = gameObject.transform;
                        child.SetActive(false);
                        child.name = ObjectsToPool[obj].name;
                    }
                }
            }
            else
            {
                Debug.Log("Pooling error! Lists are not the same length!");
            }
        }
        else
        {
            Debug.Log("Pooling error! One/Both of the lists have nothing inside!");
        }
    }

    public GameObject RequestPoolItem(string name)
    {
        //Ask for a pooled item by its name. An item is only available if it is inactive.

        foreach(Transform child in transform)
        {
            GameObject g = child.gameObject;

            if(g.name == name && g.activeInHierarchy == false)
            {
                return g;
            }
        }

        //If we reach the end, no applicable object was found.
        return null;
    }
}
