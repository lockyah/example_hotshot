using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ENEMY_Lobber : ENEMY_Base
{
    /*
     * An enemy that stays in place and periodically throws a physics object at a given angle.
     */

    public float ThrowAngle = 45f;
    public float ThrowPower = 4f;
    float ThrowTimer = 3f;
    [SerializeField] GameObject AmmoGO;

    // ---------- MAIN FUNCTIONS ----------

    private void Update()
    {
        //Enemy does not move or become alert, is always throwing while on-screen.

        //Reset after taking damage
        if (state == EnemyState.Idle)
        {
            state = EnemyState.Attacking;
        }
        
        //Attack as normal
        if(state == EnemyState.Attacking)
        {
            HandleAttacking();
        }
        

        //Doesn't move, so just for death animation and falling
        Control.Move(new Vector3(HorizSpeed, VertSpeed, 0) * 5f * Time.deltaTime);
    }

    void HandleAttacking()
    {
        if (ThrowTimer > 0)
        {
            ThrowTimer -= Time.deltaTime * Time.timeScale;
        }
        else
        {
            if (AmmoGO != null)
            {
                //Assume any object given has a rigidbody to use
                GameObject ammo = Instantiate(AmmoGO, transform.position, Quaternion.Euler(0, 0, 90));
                Rigidbody rb = ammo.GetComponent<Rigidbody>();

                rb.AddForce((Quaternion.Euler(0,0,ThrowAngle) * Vector2.up) * ThrowPower * 100);
            }

            ThrowTimer = 3f;
        }
    }
}
