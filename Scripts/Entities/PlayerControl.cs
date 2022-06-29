using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControl : MonoBehaviour
{
    public enum PlayerStates { Idle, Run, Jump, WallSlide, Fall, Dodge, Hurt }

    public bool PlayerCanMove = true; //Enable or disable player inputs
    public Health PlayerHealth;
    public float InvTime = 1.5f;
    public bool FacingRight = true;
    public PlayerStates CurrentState = PlayerStates.Idle;

    public float HorizSpeed = 0; //Raw movement input
    public float VertSpeed = -0.9f;
    public Vector3 TeleportPosition = Vector3.zero; //Used when respawning or entering a new area

    Vector3 ControlMove;
    float HorizLockTimer = 0f; //Used on dash or wall jump to keep horizontal movement speed consistent
    float MoveMultiplier, FallMultiplier = 1f;

    GameObject Reticle, WeaponObject, WeaponAim;
    ParseInputs Input;
    CharacterController Control;
    PlayerWeapons Weapons;
    Animator PlayerAni;
    SpriteRenderer PlayerSprite;

    private void Start()
    {
        WeaponAim = transform.Find("Player Aim").gameObject;
        Cursor.visible = false;
        Reticle = GameObject.Find("Player Reticle");
        WeaponObject = GameObject.Find("Player Weapon Sprite");

        PlayerHealth = GetComponent<Health>();
        Input = GetComponent<ParseInputs>();
        Control = GetComponent<CharacterController>();
        Weapons = GetComponent<PlayerWeapons>();
        PlayerAni = GetComponent<Animator>();
        PlayerSprite = GameObject.Find("Player Sprite").GetComponent<SpriteRenderer>();

        CutsceneDirector.CD.PlayerRef = this;
        CutsceneDirector.CD.Input = Input;
        CanvasController.CC.PromptAnchor = transform.GetChild(1); //"Camera Track" object

        CanvasController.CC.SetBlackout(false);
        PlayerCanMove = false;
        if(LevelManager.LM.CheckpointIndex != -1)
        {
            transform.position = LevelManager.LM.CheckpointPosition; //Can only directly set position in start, TeleportPosition needed any other time
            FacingRight = LevelManager.LM.CheckpointFacingRight;
            //CutsceneDirector.CD.BeginCutscene("Respawn");

            //Placeholder, would be reset by CD when it actually plays
            PlayerCanMove = true;
        } else
        {
            CutsceneDirector.CD.BeginCutscene("LevelStart");
        }
        LevelManager.LM.ResetCameraOnPlayer(); //Move camera to player on level intro
    }

    private void Update()
    {
        if(Input.SwapLButton == ParseInputs.ButtonState.Pressed)
        {
            LevelManager.LM.ReturnToCheckpoint();
        }

        if (PlayerCanMove)
        {
            HandleMove();
            HandleWeapons();
        } else
        {
            Control.Move(new Vector3(HorizSpeed * MoveMultiplier, VertSpeed * FallMultiplier, 0) * 5f * Time.deltaTime);
        }

        //Aiming and animation should be done either way, but won't update aim direction if !PlayerCanMove
        HandleAiming();
        HandleAnimations();
    }

    //CharacterController prevents direct transforming in Update, so it's done in LU instead
    private void LateUpdate()
    {
        if(TeleportPosition != Vector3.zero)
        {
            //Player needs to be teleported somewhere, so set their position
            transform.position = TeleportPosition;
            TeleportPosition = Vector3.zero;
        }

        //Course correction if geometry has pushed the player off of the z axis
        if (transform.position.z != 0)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, 0);
        }    
    }


    // ---------- UTILITY FUNCTIONS ----------


    //Double-check the CharacterController's isGrounded parameter
    public bool ControllerGrounded()
    {
        bool result = Control.isGrounded;

        if (!result && VertSpeed <= -0.5f && Physics.Raycast(Control.bounds.center, Vector3.down, 1.5f, 1 << 0))
        {
            result = true;
        }

        return result;
    }

    public void DamageKnockback(Vector2 pos)
    {
        //Called from Health when taking damage - applies a set knockback in the opposite direction of the damage source
        CanvasController.CC.SetCurrentHealth(PlayerHealth.CurrentHealth);
        PlayerCanMove = false;
        CurrentState = PlayerStates.Hurt; //Health value determines whether this is a hurt state or a death state
        PlayerHealth.InvincibleTimer = InvTime;

        HorizLockTimer = 0f;
        MoveMultiplier = 1f;
        FallMultiplier = 1f;

        //Find direction of damage so that knockback effects can use it
        if (Mathf.Abs(pos.x - transform.position.x) < 0.5f)
        {
            //If damage direction is too similar to player (i.e. above/below them), knock back in opposite of moving or aiming position
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

        if((HorizSpeed > 0 && FacingRight) || (HorizSpeed < 0 && !FacingRight))
        {
            //Turn to face damage source
            InvertSprite();
        }

        //If not dead, revert to normal gravity (Fall state can handle speed later)
        //If dead, coroutine handles vertical speed for the effect
        if (!PlayerHealth.IsDead())
        {
            VertSpeed = -0.9f;
            StartCoroutine(KnockbackCR());
        } else
        {
            //Disable ability to use pause menu here!
            StartCoroutine(DeathKnockbackCR());
        }
    }

    IEnumerator KnockbackCR()
    {
        //Damage invincibility gives 1.5 seconds, so this control should be regained slightly earlier to allow moving out of the way
        yield return new WaitForSeconds(0.75f);

        CurrentState = PlayerStates.Idle; //Return to idle, main movement will handle it from here!
        PlayerCanMove = true;
    }

    IEnumerator DeathKnockbackCR()
    {
        //From impact, slow time for a second, then rise and gradually speed up
        Time.timeScale = 0.05f;
        HorizSpeed *= 2f;
        VertSpeed = 1.75f;
        yield return new WaitForSecondsRealtime(1f);
        
        while(Time.timeScale < 1f)
        {
            Time.timeScale += Time.deltaTime * 3f;
            VertSpeed -= Time.deltaTime * 3f;
            yield return new WaitForEndOfFrame();
        }

        float TimeOut = 0f;

        while (!ControllerGrounded())
        {
            VertSpeed -= Time.deltaTime * 6f;
            TimeOut += Time.deltaTime;

            //Fade out if falling for long enough without hitting the ground
            if(TimeOut >= 5f)
            {
                break;
            } else
            {
                yield return new WaitForEndOfFrame();
            }
        }

        if (ControllerGrounded())
        {
            HorizSpeed = 0;
            VertSpeed = 0;
        }

        CanvasController.CC.SetBlackout(true);
        yield return new WaitForSecondsRealtime(3f);

        LevelManager.LM.ReturnToCheckpoint();
    }

    private void InvertSprite()
    {
        PlayerSprite.flipX = !PlayerSprite.flipX;
        Weapons.WeaponEnd.transform.localPosition = new Vector3(Weapons.WeaponEnd.transform.localPosition.x, Weapons.WeaponEnd.transform.localPosition.y * -1, 0);
    }


    // ---------- MAIN FUNCTIONS ----------


    //Calculates move direction to handle CharacterController movement
    void HandleMove()
    {
        if (!(CurrentState == PlayerStates.WallSlide || CurrentState == PlayerStates.Dodge))
        {
            if(HorizLockTimer <= 0)
            {
                if(ControllerGrounded() && MoveMultiplier != 1)
                {
                    MoveMultiplier = 1f;
                }

                //Always update input on keyboard, listen for Aim Button on gamepad
                if ((Input.CurrentControls == "Gamepad" && !(Input.AimButton == ParseInputs.ButtonState.Pressed || Input.AimButton == ParseInputs.ButtonState.Down)) || Input.CurrentControls == "Keyboard&Mouse")
                {
                    HorizSpeed = Mathf.Round(Input.LeftStick.x);
                }
            } else
            {
                HorizLockTimer -= Time.deltaTime * Time.deltaTime;
            }
            

            if (ControllerGrounded())
            {
                //GROUNDED - idle, running

                //On landing, reset falling speed
                if (Control.isGrounded && VertSpeed != -0.9f)
                {
                    VertSpeed = -0.9f;
                    FallMultiplier = 1;
                    HorizLockTimer = 0f; //Reset wall jump/dodge timer
                }

                CurrentState = HorizSpeed != 0 ? PlayerStates.Run : PlayerStates.Idle;

                if(Input.DashButton == ParseInputs.ButtonState.Pressed && CurrentState != PlayerStates.Hurt)
                {
                    MoveMultiplier = 2.25f;
                    HorizLockTimer = 0.33f;
                    CurrentState = PlayerStates.Dodge;

                    if(HorizSpeed == 0)
                    {
                        //Move in aim direction if not moving
                        HorizSpeed = WeaponAim.transform.position.x < transform.position.x ? -1 : 1;
                    }
                }

                if (Input.JumpButton == ParseInputs.ButtonState.Pressed && CurrentState != PlayerStates.Hurt)
                {

                    if (CurrentState == PlayerStates.Dodge)
                    {
                        //If also dashing this frame, move straight to dash-jump speed
                        ControlMove.x /= MoveMultiplier;
                        MoveMultiplier = 1.5f;
                        HorizLockTimer = 0f;
                    }
                    
                    VertSpeed = 1.75f;
                    FallMultiplier = 1;
                    CurrentState = PlayerStates.Jump;
                }

            } else
            {
                //MIDAIR - jump, fall

                if (VertSpeed > -5f)
                {
                    VertSpeed -= (3f * Time.deltaTime) * FallMultiplier;

                    if (VertSpeed < -5f)
                    {
                        VertSpeed = -5f;
                    }
                }

                //If jumping and release key or hit peak of jump, begin to fall faster
                if ((CurrentState == PlayerStates.Jump && Input.JumpButton == ParseInputs.ButtonState.Released) || VertSpeed <= 0)
                {
                    CurrentState = PlayerStates.Fall;
                    FallMultiplier = 1.75f;
                    HorizLockTimer = 0f; //Reset wall jump/dodge timer
                }
            }

        } else
        {
            if(CurrentState == PlayerStates.WallSlide)
            {

            } else
            {
                //DODGE

                if(HorizLockTimer <= 0)
                {
                    MoveMultiplier = 1f;
                    CurrentState = PlayerStates.Idle;
                } else
                {
                    HorizLockTimer -= Time.deltaTime * Time.timeScale;

                    //Continually move in same direction that dash started in, unless input (without aim lock) is in the other direction
                    if(Mathf.Round(Input.LeftStick.x) == HorizSpeed * -1 && !(Input.AimButton == ParseInputs.ButtonState.Pressed || Input.AimButton == ParseInputs.ButtonState.Down))
                    {
                        HorizLockTimer = 0f;
                        CurrentState = PlayerStates.Idle;
                        MoveMultiplier = 1f;
                    } else if (Input.JumpButton == ParseInputs.ButtonState.Pressed && CurrentState != PlayerStates.Hurt && ControllerGrounded())
                    {
                        VertSpeed = 1.75f;
                        MoveMultiplier = 1.5f;
                        FallMultiplier = 1f;
                        HorizLockTimer = 0f;
                        CurrentState = PlayerStates.Jump;
                    } else if(Input.DashButton == ParseInputs.ButtonState.Pressed && ControllerGrounded())
                    {
                        HorizLockTimer = 0.33f;

                        //Check to see if the player changed direction
                        if ((Input.CurrentControls == "Gamepad" && !(Input.AimButton == ParseInputs.ButtonState.Pressed || Input.AimButton == ParseInputs.ButtonState.Down)) || Input.CurrentControls == "Keyboard&Mouse")
                        {
                            HorizSpeed = Mathf.Round(Input.LeftStick.x);
                        }
                    }
                }
            }
        }

        ControlMove.x = HorizSpeed * MoveMultiplier;
        ControlMove.y = VertSpeed;

        Control.Move(ControlMove * 5f * Time.deltaTime);
    }

    void HandleAiming()
    {
        if (Input.CurrentControls == "Keyboard&Mouse")
        {
            //Directly tie to cursor position
            WeaponAim.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Mouse.current.position.ReadValue().x, Mouse.current.position.ReadValue().y, -Camera.main.transform.position.z));
        }
        else
        {
            //Link to same stick as movement
            Vector2 direction = Input.LeftStick;

            if (direction.magnitude > 0.5)
            {
                direction = direction / direction.magnitude;
                WeaponAim.transform.localPosition = direction * Camera.main.transform.position.z / -3;
            }
        }

        //Update aim direction if not overlapping the player character and movement is enabled
        if (Vector3.Distance(transform.position, WeaponAim.transform.position) > 1f && PlayerCanMove)
        {
            if (WeaponAim.transform.position.x < transform.position.x)
            {
                FacingRight = false;
            }
            else
            {
                FacingRight = true;
            }

            //Use the vector to update aiming angle
            Vector2 AimVector = (WeaponAim.transform.position - WeaponObject.transform.position).normalized;


            if (Weapons.WeaponIdle())
            {
                WeaponObject.transform.localEulerAngles = new Vector3(0, 0, Mathf.Atan2(0, FacingRight ? 1 : -1) * Mathf.Rad2Deg);
            }
            else
            {
                WeaponObject.transform.localEulerAngles = new Vector3(0, 0, Mathf.Atan2(AimVector.y, AimVector.x) * Mathf.Rad2Deg);
            }

        } else if (!PlayerCanMove)
        {
            //If in a cutscene, still handle idle weapons
            if (Weapons.WeaponIdle())
            {
                WeaponObject.transform.localEulerAngles = new Vector3(0, 0, Mathf.Atan2(0, FacingRight ? 1 : -1) * Mathf.Rad2Deg);
                PlayerAni.SetBool("IdleWeapon", true);
            }
        }

        if ((FacingRight && PlayerSprite.flipX) || (!FacingRight && !PlayerSprite.flipX))
        {
            InvertSprite();
        }

        Reticle.transform.position = Camera.main.WorldToScreenPoint(WeaponAim.transform.position);
    }

    void HandleWeapons()
    {
        if(CurrentState != PlayerStates.Dodge && CurrentState != PlayerStates.Hurt)
        {
            switch (Weapons.CurrentWeapon)
            {
                case PlayerWeapons.WeaponType.Pistol:
                    Weapons.Pistol(Input.PrimaryButton);
                    break;
            }
        }
    }

    void HandleAnimations()
    {
        PlayerAni.SetFloat("HorizSpeed", ControlMove.x);
        PlayerAni.SetFloat("VertSpeed", VertSpeed);
        PlayerAni.SetInteger("CurrentState", (int)CurrentState);
        PlayerAni.SetBool("FacingRight", FacingRight);
        PlayerAni.SetBool("IdleWeapon", Weapons.WeaponIdle());
        PlayerAni.SetBool("Dead", PlayerHealth.IsDead());
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (!ControllerGrounded())
        {
            if (VertSpeed > 0 && Control.collisionFlags == CollisionFlags.Above)
            {
                //Stop jumps if the player hits something above them
                VertSpeed = 0;
                FallMultiplier = 1.75f;

                if(CurrentState != PlayerStates.Hurt)
                {
                    CurrentState = PlayerStates.Fall;
                }
            }
        }
    }
}
