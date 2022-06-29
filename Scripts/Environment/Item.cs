using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public enum ItemType {Health,Ammo,Special,All}
    public ItemType itemType;
    public int value; //How much of the above type will this recover?

    private void OnTriggerEnter(Collider other)
    {
        //Could alter to be a V3 where if any is not 0, affect the relevant meter?

        if (other.CompareTag("Player"))
        {
            PlayerControl pc = other.gameObject.GetComponent<PlayerControl>();

            if(itemType == ItemType.Health || itemType == ItemType.All)
            {
                //Adds health
                //Add regardless of max health
                Debug.Log("Added Health");

                pc.PlayerHealth.ChangeHealth(value);
            }

            if (itemType == ItemType.Ammo || itemType == ItemType.All)
            {
                //Adds ammo for current weapon
                //(Or gives it to whichever is lowest with an upgrade)
                Debug.Log("Added Ammo");
            }

            if (itemType == ItemType.Special || itemType == ItemType.All)
            {
                //Fills Special Meter
                Debug.Log("Added Special");
            }

            Destroy(gameObject);
        }
    }
}
