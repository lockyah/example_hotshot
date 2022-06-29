using UnityEngine;

public class WPN_Bullet : MonoBehaviour
{
    //Ammo behaviour for a normal bullet.

    public Health.HealthType FiredBy; //Which "team" was this fired by?
    public int Damage = 2; //How much damage should this do on contact?
    public float MoveSpeed = 2f; //How fast should the bullet fly?
    TrailRenderer Trail;
    Rigidbody RB;


    private void OnEnable()
    {
        if(RB == null)
        {
            Trail = GetComponent<TrailRenderer>();
            RB = GetComponent<Rigidbody>();
        }
    }

    private void OnDisable()
    {
        Trail.Clear();
    }

    public void Update()
    {
        RB.MovePosition(transform.position + gameObject.transform.right);
    }

    //Called when the bullet leaves the frame
    private void OnBecameInvisible()
    {
        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
    }

    //Called when instantiating to set team/damage
    public void SetData(Health.HealthType team, int dmg)
    {
        FiredBy = team;
        Damage = dmg;
    }

    private void OnTriggerEnter(Collider other)
    {
        //Only work on solid objects
        if (!other.isTrigger)
        {

            if (other.CompareTag("Flammable"))
            {
                other.transform.root.SendMessage("Ignite"); //Failsafe for if a mesh collider is being used on a child object
            }

            Health target = other.gameObject.GetComponent<Health>();

            Vector3 value = other.ClosestPointOnBounds(transform.position);
            value.z = Damage;


            if (target != null)
            {
                //Hit a creature or target! Check against FiredBy to see if it should hit.
                if (FiredBy == Health.HealthType.Player || FiredBy == Health.HealthType.Ally)
                {
                    //From friendly team. Hit only enemies or targets!
                    if(target.Type == Health.HealthType.Target)
                    {
                        //Target objects don't have any knockback, so using ChangeHealth directly is easier than making an empty receiver for KnockbackDamage
                        target.ChangeHealth(-Damage);
                        gameObject.SetActive(false);
                    }
                    else if (target.Type == Health.HealthType.Enemy || target.Type == Health.HealthType.Boss)
                    {
                        target.TakeDamage(value);
                        gameObject.SetActive(false);
                    }
                }
                else
                {
                    //From enemies. Hit player or allies, ignore targets!
                    if (target.Type == Health.HealthType.Player || target.Type == Health.HealthType.Ally)
                    {
                        target.TakeDamage(value);
                        gameObject.SetActive(false);
                    }
                }
            }
            else
            {
                //Hit a wall
                gameObject.SetActive(false);
            }
        }
    }
}
