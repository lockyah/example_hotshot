using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class ChangeCamera : MonoBehaviour
{
    public CinemachineVirtualCamera cam; // The new camera to use
    public bool FollowPlayer;

    // Start is called before the first frame update
    void Start()
    {
        cam = transform.GetChild(0).GetComponent<CinemachineVirtualCamera>();
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            //Default cam priority is 10
            cam.Priority = 11;

            if (FollowPlayer)
            {
                cam.Follow = other.transform;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            cam.Priority = 0;
        }
    }
}
