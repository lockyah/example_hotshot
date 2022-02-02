using System.Collections.Generic;
using System;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    public static PlayerControl PC;

    public bool PlayerCanMove = true; //Enable or disable player inputs
    public enum PlayerStates {Idle, Run, Jump, WallSlide, WallJump, Fall, Dodge, Hurt}
    public PlayerStates CurrentState = PlayerStates.Idle;
    public enum PlayerWeapons {Pistol, Bomb, Launcher, Whip, Joke}
    public PlayerWeapons CurrentWeapon = PlayerWeapons.Pistol;
    private float TimeSinceLastShot = 4.9f; //Weapon and idle pose cooldown

    public float ImmobileTimer = 0f; //Dodging and wall jumping immobilise the player's horizontal movement
    private float VertSpeed = -0.75f; //Current jump/gravity speed outside of multipliers
    private float MaxGravity = -5f;
    public float HorizMoveMultiplier = 1f; //Movement can be altered by environmental effects like water slowing horiz movement, or fans boosting jumps
    public float VertMoveMultiplier = 1f;
    private float MoveSpeed = 1f; //Increase to movement speed from dash/dash jumping
    private float FallRate = 1f; //Increase to gravity from releasing jump or falling without jumping

    public Vector3 MoveInput; //Direction to move in
    public bool FacingRight = true;
    public bool BadAim = false; //True if aiming behind on a wall slide or poking gun through a wall

    public Health PlayerHealth;
    private CanvasController GameCanvas;
    private CharacterController Control;
    private GameObject WeaponAim, WeaponObject, WeaponEnd, Reticle, PSJump, PSLand, PSDust;
    private ParticleSystem JumpPS, LandPS, DustPS;
    private SpriteRenderer PlayerSprite;
    private Animator PlayerAni;

    // Start is called before the first frame update
    void Start()
    {
        //Singleton - player will always be needed!
        if(PC == null)
        {
            DontDestroyOnLoad(gameObject); //Player character will always be needed!
            PC = this;

            //Set up health according to save and apply it to the canvas
            PlayerHealth = GetComponent<Health>();
            GameCanvas = CanvasController.CC;
            GameCanvas.SetCurrentHealth(PlayerHealth.CurrentHealth);
            GameCanvas.SetMaxHealth(PlayerHealth.MaxHealth);

            Control = GetComponent<CharacterController>();
            PlayerAni = GetComponent<Animator>();

            Cursor.visible = false;
            Reticle = GameObject.Find("Player Reticle");

            WeaponObject = GameObject.Find("Player Weapon Sprite");
            WeaponAim = GameObject.Find("Player Aim");
            WeaponEnd = GameObject.Find("Player Weapon End");
            PSLand = GameObject.Find("PS Land");
            PSJump = GameObject.Find("PS Jump");
            PSDust = GameObject.Find("PS Dust");
            JumpPS = PSJump.GetComponent<ParticleSystem>();
            LandPS = PSLand.GetComponent<ParticleSystem>();
            DustPS = PSDust.GetComponent<ParticleSystem>();

            PlayerSprite = GameObject.Find("Player Sprite").GetComponent<SpriteRenderer>();
        } else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (PlayerCanMove && CurrentState != PlayerStates.Hurt)
        {
            HandleMoving();
            HandleOrientation();
            HandleWeapons();
            HandleAnimations();
        } else
        {
            //!CanMove is when a cutscene is playing only, so handle gravity and moving cursor only
            //Menus open sets timescale to 0 so nothing will move anyway
            Control.Move(MoveInput * 5f * Time.deltaTime * Time.timeScale);
            Reticle.transform.position = Input.mousePosition;
        }

        UpdateTimers();
        UpdateUI();
    }

    void HandleMoving()
    {
        if(!(CurrentState == PlayerStates.WallJump || CurrentState == PlayerStates.WallSlide || CurrentState == PlayerStates.Dodge))
        {
            if(ImmobileTimer <= 0)
            {
                //Take horiz inputs when not dodging or after wall jump
                MoveInput.x = Mathf.Round(Input.GetAxisRaw("Horizontal")) * HorizMoveMultiplier * MoveSpeed; //Controller can input floats, so this is to keep it as raw input

                if (ControllerGrounded())
                {
                    //Reset move speed if still on ground
                    MoveSpeed = 1;
                }
            }
            

            if (ControllerGrounded())
            {
                //Grounded stuff

                //Handle landing on ground
                if (Control.isGrounded && VertSpeed != -0.9f)
                {
                    VertSpeed = -0.9f; //On landing, reset VertSpeed
                    LandPS.Play(); //Play landing effect, automatically in correct position
                    PlayerAni.SetFloat("WallSlideDirection", 0);
                    FallRate = 1;
                }

                if (MoveInput.x != 0)
                {
                    CurrentState = PlayerStates.Run;
                }
                else
                {
                    CurrentState = PlayerStates.Idle;
                }

                if (Input.GetButtonDown("Dodge"))
                {
                    //If moving, dodge in move direction
                    //If not, move in facing direction

                    //Increase movement speed
                    MoveSpeed = 2.25f;

                    if (MoveInput.x != 0)
                    {
                        MoveInput.x = MoveSpeed * (MoveInput.x > 0 ? 1 : -1) * HorizMoveMultiplier;
                    }
                    else
                    {
                        MoveInput.x = MoveSpeed * (FacingRight ? 1 : -1) * HorizMoveMultiplier;
                    }

                    //Play dust effect from preset position
                    PSDust.transform.localPosition = new Vector3(0, -1.159f, 0);
                    DustPS.Play();

                    ImmobileTimer = 0.33f;

                    CurrentState = PlayerStates.Dodge;
                }

                if (Input.GetButtonDown("Jump"))
                {
                    VertSpeed = 1.75f;
                    CurrentState = PlayerStates.Jump;

                    //Play jump effect from preset position
                    PSJump.transform.localPosition = new Vector3(0, -1.159f, 0);
                    PSJump.transform.localEulerAngles = Vector3.zero;
                    JumpPS.Play();
                }
            }
            else
            {
                //Midair stuff

                //Normal gravity effect
                if (VertSpeed > MaxGravity)
                {
                    VertSpeed = MoveInput.y - (2.5f * Time.deltaTime) * FallRate;

                    if (VertSpeed < MaxGravity)
                    {
                        VertSpeed = MaxGravity;
                    }
                }

                //If jumping and release key or hit peak of jump, begin to fall faster
                if((CurrentState == PlayerStates.Jump && Input.GetButtonUp("Jump")) || VertSpeed <= 0)
                {
                    CurrentState = PlayerStates.Fall;
                    FallRate = 1.75f;
                }
            }

        } else if(CurrentState == PlayerStates.Dodge)
        {
            //If out of time or inputting other direction, return to normal
            //Falling off a ledge won't cancel!
            if (ImmobileTimer <= 0 || (Input.GetAxisRaw("Horizontal") < 0 && MoveInput.x > 0) || (Input.GetAxisRaw("Horizontal") > 0 && MoveInput.x < 0))
            {
                ImmobileTimer = 0; //Exit dash if still moving

                //Reset state, should automatically change if moving or jumping
                CurrentState = PlayerStates.Idle;
                MoveSpeed = 1f; //Return to normal speed

                //Stop dust effect
                DustPS.Stop();
            } else
            {
                if (Input.GetButtonDown("Jump") && ControllerGrounded())
                {
                    ImmobileTimer = 0; //Exit dash if still moving
                    MoveSpeed = 1.5f; //Decrease move speed slightly

                    VertSpeed = 1.75f;
                    CurrentState = PlayerStates.Jump;

                    //Stop dust effect
                    DustPS.Stop();

                    //Play jump effect from preset position
                    PSJump.transform.localPosition = new Vector3(0, -1.159f, 0);
                    PSJump.transform.localEulerAngles = Vector3.zero;
                    JumpPS.Play();
                }
                else if (Input.GetButtonDown("Dodge"))
                {
                    //Or input in opposite direction
                    ImmobileTimer = 0.33f;
                }
            }

        } else
        {
            //Wall Jump and Wall Slide

            if(CurrentState == PlayerStates.WallSlide)
            {
                MoveInput.y = MaxGravity / 10 * VertMoveMultiplier;
                VertSpeed = MoveInput.y;

                FallRate = 1; //Reset gravity rate
                MoveSpeed = 1; //Reset dash movement bonus


                //If no longer in contact with a wall in midair, not holding toward the wall, or have hit the ground, reset wall slide direction for animator
                if (Control.collisionFlags == CollisionFlags.None || Mathf.Round(Input.GetAxisRaw("Horizontal")) != PlayerAni.GetFloat("WallSlideDirection")*-1 || ControllerGrounded())
                {
                    DustPS.Stop();
                    PlayerAni.SetFloat("WallSlideDirection", 0);
                    CurrentState = PlayerStates.Idle; //Set to idle, will sort itself
                }

                if (Input.GetButtonDown("Jump"))
                {
                    CurrentState = PlayerStates.WallJump;
                    DustPS.Stop();

                    PSJump.transform.position = transform.position + new Vector3(PlayerAni.GetFloat("WallSlideDirection") == 1 ? -0.5f : 0.5f,0,0);
                    PSJump.transform.localEulerAngles = new Vector3(0, 0, PlayerAni.GetFloat("WallSlideDirection") == 1 ? -90 : 90);
                    JumpPS.Play();

                    //Jump the direction of the normal and set gravity speed to a full jump
                    MoveInput = new Vector3(PlayerAni.GetFloat("WallSlideDirection"), 1.75f, 0);
                    VertSpeed = 1.75f;

                    //Stop horizontal movement inputs for this amount of seconds
                    ImmobileTimer = 0.33f;

                    //Animation triggers
                    PlayerAni.SetFloat("WallSlideDirection", 0);
                }
            } else
            {
                //Normal gravity effect
                if (VertSpeed > MaxGravity)
                {
                    VertSpeed = MoveInput.y - (2.5f * Time.deltaTime) * FallRate;

                    if (VertSpeed < MaxGravity)
                    {
                        VertSpeed = MaxGravity;
                    }
                }

                //If jumping and release key or hit peak of jump, begin to fall faster
                if (Input.GetButtonUp("Jump") || VertSpeed <= 0)
                {
                    CurrentState = PlayerStates.Fall;
                    FallRate = 1.75f;
                    ImmobileTimer = 0; //Exit jump
                }
                else if (ImmobileTimer <= 0)
                {
                    //Reset state
                    CurrentState = PlayerStates.Fall;
                }
            }

        }

        MoveInput.y = VertSpeed * VertMoveMultiplier;

        Control.Move(MoveInput * 5f * Time.deltaTime * Time.timeScale);

        if(transform.position.z != 0)
        {
            transform.position = transform.position + new Vector3(0, 0, transform.position.z * -1);
        }
    }

    public bool ControllerGrounded()
    {
        bool result = Control.isGrounded;
        
        //If Phys doesn't think it's grounded, check with a raycast downward for normal coyote time
        if(!result && VertSpeed <= -0.5f && Physics.Raycast(Control.bounds.center, Vector3.down, 1.5f,1<<0))
        {
            result = true;
        }

        return result;
    }

    void HandleOrientation()
    {
        //Add check for control scheme in use to position aim
        //When using a controller, this should be a constant distance from the player and rotate in a circle corresponding to the stick
        WeaponAim.transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition + new Vector3(0, 0, 15));
        Reticle.transform.position = Input.mousePosition;


        //If only update aiming if reticle is far enough from the player and not paused
        if(Vector3.Distance(transform.position, WeaponAim.transform.position) > 1f && Time.timeScale != 0)
        {
            float WallDirection = PlayerAni.GetFloat("WallSlideDirection");
            Vector3 AimVector = Vector3.zero;

            if (WallDirection != 0 && !ControllerGrounded())
            {
                //If wall sliding, use an alternate set of calculations
                //Direction = normal of wall; 1 = on left side, -1 = on right

                if((WallDirection == 1 && PlayerSprite.flipX) || (WallDirection == -1 && !PlayerSprite.flipX))
                {
                    InvertSprite();
                }

                if((WallDirection == 1 && WeaponAim.transform.position.x > transform.position.x) || (WallDirection == -1 && WeaponAim.transform.position.x < transform.position.x))
                {
                    BadAim = false;
                } else
                {
                    BadAim = true;
                }
            }
            else
            {
                BadAim = false; //Reset after jumping/leaving wall
                AimVector = (WeaponAim.transform.position - WeaponObject.transform.position).normalized;

                //Swap sprite facing and weapon arm position based on if aiming in front or behind the player
                if ((WeaponAim.transform.position.x < transform.position.x && FacingRight) || (WeaponAim.transform.position.x > transform.position.x && !FacingRight))
                {
                    InvertSprite();
                }
            }

            //If player is aiming the wrong way on a wall/through a wall, or has been 5s since last shot, or weapon is already considered idle, use weapon idle pose
            PlayerAni.SetBool("IdleWeapon", BadAim || TimeSinceLastShot >= 5f);
            if (BadAim || TimeSinceLastShot >= 5f || PlayerAni.GetBool("IdleWeapon"))
            {
                if(WeaponAim.transform.position.x < transform.position.x)
                {
                    //Aiming to left.
                    AimVector = ((WeaponObject.transform.position + new Vector3(-1, 0, 0) - WeaponObject.transform.position).normalized);

                    if(WallDirection == 1)
                    {
                        AimVector.x *= -1;
                    }

                } else if(WeaponAim.transform.position.x > transform.position.x)
                {
                    //Aiming to right
                    AimVector = ((WeaponObject.transform.position + new Vector3(1, 0, 0) - WeaponObject.transform.position).normalized);

                    if (WallDirection == -1)
                    {
                        AimVector.x *= -1;
                    }
                }
            } else
            {
                //Normal aiming
                AimVector = (WeaponAim.transform.position - WeaponObject.transform.position).normalized;
            }

            //Use the vector to update aiming angle
            WeaponObject.transform.localEulerAngles = new Vector3(0, 0, Mathf.Atan2(AimVector.y, AimVector.x) * Mathf.Rad2Deg);
        }        
    }

    private void InvertSprite()
    {
        FacingRight = !FacingRight;
        PlayerSprite.flipX = !PlayerSprite.flipX;
        WeaponEnd.transform.localPosition = new Vector3(WeaponEnd.transform.localPosition.x, WeaponEnd.transform.localPosition.y * -1, 0);
    }

    public void HandleWeapons()
    {
        //Check for changing weapons first so they don't fire the wrong ammo
        //Change where WeaponEnd is per weapon, add a flip at the end by checking FacingRight
        //Can't change during weapon cooldown or while pressing/holding a button

        //Can only change to weapons that are unlocked

        if (!BadAim)
        {
            switch (CurrentWeapon)
            {
                case PlayerWeapons.Pistol:
                    //Mega Buster basic weapon. Fires on clicks, between cooldowns, only when not dodging.
                    if (TimeSinceLastShot >= 0.1f && Input.GetButtonDown("Fire1") && CurrentState != PlayerStates.Dodge)
                    {

                        GameObject bullet = ObjectPooler.OP.RequestPoolItem("Bullet");

                        if (bullet != null)
                        {
                            //Shoot on each press if after cooldown
                            TimeSinceLastShot = 0;

                            //Replicate aiming mechanic
                            Vector3 AimVector = (WeaponAim.transform.position - WeaponObject.transform.position).normalized;
                            //Use the vector to update aiming angle
                            WeaponObject.transform.localEulerAngles = new Vector3(0, 0, Mathf.Atan2(AimVector.y, AimVector.x) * Mathf.Rad2Deg);

                            bullet.GetComponent<WPN_Bullet>().SetData(PlayerHealth.GetHealthType(), 2);
                            bullet.SetActive(true);
                            bullet.transform.position = WeaponEnd.transform.position;
                            bullet.transform.rotation = WeaponObject.transform.localRotation;

                            PlayerAni.SetTrigger("PrimaryFire");
                        }
                    }

                    //Secondary function can happen at same time as firing or dodging
                    if (Input.GetButtonDown("Fire2"))
                    {
                        Debug.Log("Secondary begun!");
                    }
                    else if (Input.GetButtonUp("Fire2"))
                    {
                        Debug.Log("Secondary ended!");
                    }
                    break;
                default:
                    //Unknown or unprogrammed weapon, somehow. Only display error on press.
                    if (Input.GetButtonDown("Fire1") || Input.GetButtonDown("Fire2"))
                    {
                        Debug.Log("No behaviour listed for this weapon!");
                    }
                    break;
            }
        }

        

        //if !Fire2 to recharge special meter slowly
    }

    void UpdateTimers()
    {
        //Timer to return to weapon idle pose
        if (TimeSinceLastShot < 5f)
        {
            TimeSinceLastShot += Time.deltaTime * Time.timeScale;
        }

        if(ImmobileTimer > 0)
        {
            ImmobileTimer -= Time.deltaTime;

            if(CurrentState == PlayerStates.Hurt)
            {
                CurrentState = PlayerStates.Idle;
            }
        }
        
    }

    void UpdateUI()
    {
        //Use the CanvasController to update health, ammo, chosen weapon, etc.

        if (PlayerHealth.MaxHealth != GameCanvas.GetMaxHealth())
        {
            GameCanvas.SetMaxHealth(PlayerHealth.MaxHealth);
        }

        if(PlayerHealth.CurrentHealth != GameCanvas.GetCurrentHealth())
        {
            GameCanvas.SetCurrentHealth(PlayerHealth.CurrentHealth);
        }

        //Add update for special meter when implemented
    }

    void HandleAnimations()
    {
        PlayerAni.SetFloat("HorizSpeed", MoveInput.x);
        PlayerAni.SetInteger("CurrentState", (int)CurrentState);
        PlayerAni.SetBool("FacingRight", FacingRight);
    }

    //Called after taking damage with the origin of the collision - used to apply knockback
    public void TakeDamage(Vector3 vect)
    {
        //Z = damage since axis is unused
        PlayerHealth.ChangeHealth((int)-vect.z);
        vect.z = 0;

        if (!PlayerHealth.IsDead())
        {
            CurrentState = PlayerStates.Hurt;
            ImmobileTimer = 0.5f;

            Debug.Log("Took damage from direction " + vect);

            MoveInput = transform.position - vect;
            VertSpeed = MoveInput.y;
            FallRate = 1f;
        } else
        {
            Debug.Log("DEAD");
            ImmobileTimer = 99f;
            MoveInput = Vector3.zero;
            PlayerCanMove = false;

            LevelManager.LM.ReturnToCheckpoint();
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (PlayerCanMove)
        {
            if(CurrentState == PlayerStates.WallSlide && !(hit.normal.x == -1 || hit.normal.x == 1) && hit.gameObject.layer == 0)
            {
                //If sliding on a wall that isn't flat, end slide
                CurrentState = PlayerStates.Idle;
                PlayerAni.SetFloat("WallSlideDirection", 0);

                DustPS.Stop();
            }

            if (!ControllerGrounded() && Control.collisionFlags == CollisionFlags.Above)
            {
                //If in midair and hit something above, invert vertical speed
                if (VertSpeed > 0)
                {
                    VertSpeed = -VertSpeed;
                }

                MoveInput.y = VertSpeed;
            }
            else if (!Physics.Raycast(transform.position,Vector3.down,3f) && !ControllerGrounded() && (hit.normal.x == -1 || hit.normal.x == 1) && hit.gameObject.layer != 6)
            {
                //Can't wall slide if too close to ground
                //Normals used to ensure flat walls only
                //Layer 6 is for unclimbable surfaces

                if ((Mathf.Round(Input.GetAxisRaw("Horizontal")) < 0 && hit.normal.x > 0) || (Mathf.Round(Input.GetAxisRaw("Horizontal")) > 0 && hit.normal.x < 0))
                {
                    //If inputting toward the wall, trigger wall slide state and save which side the wall is
                    CurrentState = PlayerStates.WallSlide;
                    PlayerAni.SetFloat("WallSlideDirection", hit.normal.x);

                    //Play dust effect from preset position
                    PSDust.transform.localPosition = new Vector3(-0.665f * hit.normal.x, -0.653f, 0);
                    DustPS.Play();
                }
            }
        }
    }
}
