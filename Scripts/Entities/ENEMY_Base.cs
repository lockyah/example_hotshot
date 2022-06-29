using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ENEMY_Base : MonoBehaviour
{
    public enum EnemyState { Idle, Moving, Alert, Attacking, Hurt }
    public EnemyState state = EnemyState.Idle;

    public bool FacingRight = false;
    public float InvTime = 0f;
    protected float HorizSpeed = 0;
    protected float VertSpeed = -0.9f;

    protected Collider coll;
    protected CharacterController Control;
    protected Health EnemyHealth;

    private void Start()
    {
        Control = GetComponent<CharacterController>();
        EnemyHealth = GetComponent<Health>();
        coll = GetComponent<Collider>();
    }

    public void DamageKnockback(Vector2 pos)
    {
        state = EnemyState.Hurt;
        EnemyHealth.InvincibleTimer = InvTime;

        //Most enemies don't take knockback, so death animation is the only one to worry about
        if (EnemyHealth.IsDead())
        {
            if (Mathf.Abs(pos.x - transform.position.x) < 0.5f)
            {
                //If damage direction is too similar to enemy (i.e. above/below them), knock back in opposite of moving or aiming position
                if (HorizSpeed != 0)
                {
                    HorizSpeed = HorizSpeed < 0 ? 0.5f : -0.5f;
                }
                else
                {
                    HorizSpeed = !FacingRight ? 0.5f : -0.5f;
                }
            }
            else
            {
                HorizSpeed = pos.x < transform.position.x ? 0.5f : -0.5f;
            }

            StartCoroutine(DeathKnockbackCR());
        } else
        {
            StartCoroutine(DamageCR());
        }
    }

    IEnumerator DamageCR()
    {
        //No knockback, so this just changes states back in most cases
        yield return new WaitForSeconds(InvTime);

        state = EnemyState.Idle;
    }

    IEnumerator DeathKnockbackCR()
    {
        HorizSpeed *= 2f;
        VertSpeed = 1.75f;

        Control.height /= 2; //Become smaller on death
        coll.enabled = false; //No contact damage
        gameObject.layer = 3; //"Untargetable", doesn't contact characters

        while (VertSpeed > 0.5f)
        {
            VertSpeed -= Time.deltaTime * 3f;
            yield return new WaitForEndOfFrame();
        }

        float TimeOut = 0f;

        while (!Control.isGrounded)
        {
            VertSpeed -= Time.deltaTime * 6f;
            TimeOut += Time.deltaTime;

            //Fade out if falling for long enough without hitting the ground
            if (TimeOut >= 5f)
            {
                break;
            }
            else
            {
                yield return new WaitForEndOfFrame();
            }
        }

        if (Control.isGrounded)
        {
            HorizSpeed = 0;
            VertSpeed = 0;
        }

        yield return new WaitForSeconds(3f);

        Destroy(gameObject);
    }
}
