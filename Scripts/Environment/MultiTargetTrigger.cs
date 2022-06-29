using UnityEngine;

public class MultiTargetTrigger : MonoBehaviour
{
    public Health[] SwitchOrder; //Empty if switches can be used in any order
    public int SwitchIndex = 0;
    
    public void UpdateSwitches()
    {
        //Invoke can't use parameters, so instead we'll check that the order is correct by seeing if each object in order is active

        if(SwitchOrder.Length > 0)
        {
            bool valid = true;
            for (int i = 0; i <= SwitchIndex; i++)
            {
                if(!SwitchOrder[i].IsDead())
                {
                    valid = false;
                    SwitchIndex = 0;
                    //Incorrect animation
                    Debug.Log("Wrong!");

                    foreach(Health h in SwitchOrder)
                    {
                        h.gameObject.SetActive(true);
                        h.ChangeHealth(999);
                    }

                    break;
                }
            }

            if (valid)
            {
                //Buttons in correct order!
                //Add to counter!
                SwitchIndex++;

                Debug.Log("Right!");
            }
        } else
        {
            //Add to counter!
            SwitchIndex++;
        }

        //Finished!
        if(SwitchIndex == SwitchOrder.Length)
        {
            Debug.Log("Multi-target unlocked!");
            GetComponent<InvokeScripts>().Activate();
        }
    }
}
