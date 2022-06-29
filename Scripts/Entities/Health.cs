using UnityEngine;

public class Health : MonoBehaviour
{
    public enum HealthType {Player,Ally,Enemy,Boss,Target} //Determines which interactions are allowed to affect it
    public HealthType Type;

    //Variables are changed in the editor and by interactions like taking damage or picking up items!
    public int CurrentHealth;
    public int MaxHealth;
    public float InvincibleTimer; //Changed only by the character controller

    private void Start()
    {
        CurrentHealth = CanvasController.CC.GetCurrentHealth();
        MaxHealth = CanvasController.CC.GetMaxHealth();
    }

    private void Update()
    {
        if(InvincibleTimer > 0)
        {
            InvincibleTimer -= Time.deltaTime * Time.timeScale;
        }
    }

    public bool IsDead()
    {
        return CurrentHealth <= 0;
    }

    public HealthType GetHealthType()
    {
        return Type;
    }

    public void TakeDamage(Vector3 Damage)
    {
        //Damage x and y = impact direction
        //Damage z = impact damage

        //Any damage taken after death is ignored
        if (!IsDead())
        {
            if (InvincibleTimer <= 0)
            {
                ChangeHealth(-(int)Damage.z);

                //Each controller will handle knockback and I-Frames differently, so this activates it
                gameObject.SendMessage("DamageKnockback", (Vector2)Damage);
            }
        }
        
    }

    public void ChangeHealth(int Damage)
    {
        CurrentHealth += Damage;

        if(CurrentHealth > MaxHealth)
        {
            CurrentHealth = MaxHealth;
        }
    }

    //public changeMaxHealth, use a percentage to change current health too

}
