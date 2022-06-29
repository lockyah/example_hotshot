using UnityEngine;

public class ENV_Damage : MonoBehaviour
{
    public int Damage = 0;

    private void OnCollisionStay(Collision collision)
    {
        if(collision.gameObject != gameObject)
        {
            //OnStay used so that standing on objects 
            Health h = collision.gameObject.GetComponent<Health>();

            //Z value is ignored, so we can use it to send the damage too!
            Vector3 value = collision.contacts[0].point;
            value.z = Damage;

            if (h != null && h.gameObject != gameObject)
            {
                //On contact with a character, reduce their health
                if (h.GetHealthType() != Health.HealthType.Target && h.GetHealthType() != Health.HealthType.Boss)
                {
                    collision.gameObject.SendMessage("TakeDamage", value);
                }
            }
        }
    }
}
