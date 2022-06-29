using UnityEngine;

public class NPC_Wander : MonoBehaviour
{
    public float LeftXLimit;
    public float RightXLimit;

    public Vector3 MoveInput = new Vector3(1, -0.9f, 0);
    public float MoveSpeed = 2f;
    public float ActionTimer = 0f;

    [SerializeField] SpriteRenderer NPCSprite;
    CharacterController Control;
    Animator Ani;
    CutsceneTrigger CT;

    void Start()
    {
        Control = GetComponent<CharacterController>();
        Ani = GetComponent<Animator>();
        if(transform.childCount > 1)
        {
            CT = transform.Find("Catch Area").GetComponent<CutsceneTrigger>(); //Not all NPCs will have a CT, but interacting with one that does should stop them moving
        }
    }

    bool ObstacleFound()
    {
        //True if a wall is ahead or there's a drop
        if (Physics.Linecast(transform.position, new Vector3(transform.position.x + MoveInput.x / 1.5f, transform.position.y - 0.5f, 0), 1 << 0))
        {
            return true;
        }
        else if (!Physics.Linecast(new Vector3(transform.position.x + MoveInput.x / 2.25f, transform.position.y - 0.75f, 0), new Vector3(transform.position.x + MoveInput.x / 2.25f, transform.position.y - 2f, 0), 1 << 0))
        {
            return true;
        }

        return false;
    }

    bool EndOfRange()
    {
        //Check X limits depending on which way the NPC is moving. If current X is further, return true
        switch (MoveInput.x)
        {
            case 1:
                return transform.position.x >= RightXLimit;
            case -1:
                return transform.position.x <= LeftXLimit;
        }

        return false;
    }

    void MoveAI()
    {
        if ((ObstacleFound() || EndOfRange()) && ActionTimer <= 0)
        {
            MoveSpeed = 0f;
            ActionTimer = Random.Range(1, 3);
        }
        else if (ActionTimer > 0)
        {
            ActionTimer -= Time.deltaTime * Time.timeScale;

            if (ActionTimer <= 0)
            {
                MoveSpeed = 2f;
                MoveInput.x *= -1;
            }
        }

        Control.Move(MoveInput * MoveSpeed * Time.deltaTime);

        Ani.SetBool("moving", MoveSpeed != 0);
        NPCSprite.flipX = MoveInput.x < 0;
    }

    void Update()
    {
        //If no interaction or not playing interaction, move
        if(CT != null)
        {
            //If playing the interaction that this NPC leads to, stop movement
            if(! (CutsceneDirector.CD.CutscenePlaying && CutsceneDirector.CD.CurrentInteraction == CT.InkKnotName))
            {
                MoveAI();
            }
        } else
        {
            MoveAI();
        }        
    }
}
