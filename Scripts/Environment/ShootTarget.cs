using UnityEngine;

public class ShootTarget : MonoBehaviour
{
    public enum SwitchBehaviour {Hide, Destroy}
    public SwitchBehaviour OnActivate = SwitchBehaviour.Hide; //What to do when the health is depleted?
    
    Health health;

    // Start is called before the first frame update
    void Start()
    {
        health = GetComponent<Health>();
    }

    // Update is called once per frame
    void Update()
    {
        if (health.IsDead())
        {
            //On "death", activate the InvokeScripts script to use the UnityEvents attached to it
            GetComponent<InvokeScripts>().Activate();

            if (OnActivate == SwitchBehaviour.Destroy)
            {
                Destroy(gameObject);
            } else
            {
                gameObject.SetActive(false);
            }

        }
    }
}
