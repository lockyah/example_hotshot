using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public int index = 0; //Which checkpoint is this?
    public bool FacingRight = true;

    private void OnTriggerEnter(Collider other)
    {
        //Doesn't need to check for multiple colliders since it doesn't really matter!
        if(LevelManager.LM.CheckpointIndex < index)
        {
            LevelManager.LM.CheckpointIndex = index;
            LevelManager.LM.CheckpointPosition = transform.position;
            LevelManager.LM.CheckpointFacingRight = FacingRight;
            
            Debug.Log("Checkpoint updated!");
        }
    }
}
