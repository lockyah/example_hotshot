using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ParseInputs : MonoBehaviour
{
    public enum ButtonState { Up, Down, Pressed, Released };

    public PlayerInput Input;

    public bool InputsAllowed = true; //Whether controls that move the character, etc. are allowed to work
    public string CurrentControls;
    public Vector2 LeftStick, RightStick;
    bool JumpBool, DashBool, PrimaryBool, SecondaryBool, AimBool, SwapLBool, SwapRBool;
    public ButtonState JumpButton, DashButton, PrimaryButton, SecondaryButton, AimButton, SwapLButton, SwapRButton;

    // Start is called before the first frame update
    void OnEnable()
    {
        Input = GetComponent<PlayerInput>();
    }

    void Update()
    {
        CurrentControls = Input.currentControlScheme;

        JumpButton = ParsePressed(JumpButton, JumpBool);
        DashButton = ParsePressed(DashButton, DashBool);
        PrimaryButton = ParsePressed(PrimaryButton, PrimaryBool);
        SecondaryButton = ParsePressed(SecondaryButton, SecondaryBool);
        SwapLButton = ParsePressed(SwapLButton, SwapLBool);
        SwapRButton = ParsePressed(SwapRButton, SwapRBool);
        
        if (CurrentControls == "Gamepad")
        {
            AimButton = ParsePressed(AimButton, AimBool);
        } else
        {
            AimButton = ButtonState.Up;
            AimBool = false;
        }
    }

    ButtonState ParsePressed(ButtonState b, bool pressed)
    {
        //Mimic GetButtonDown, GetButtonUp, etc. based on previous state of the button
        if (pressed)
        {
            if (b == ButtonState.Up || b == ButtonState.Released)
            {
                return ButtonState.Pressed;
            }
            else
            {
                return ButtonState.Down;
            }
        }
        else
        {
            if (b == ButtonState.Down || b == ButtonState.Pressed)
            {
                return ButtonState.Released;
            }
            else
            {
                return ButtonState.Up;
            }
        }
    }

    public void InputMove(InputAction.CallbackContext context)
    {
        LeftStick = context.ReadValue<Vector2>();
    }

    public void Jump(InputAction.CallbackContext context)
    {
        JumpBool = context.action.ReadValue<float>() == 1;
    }

    public void Dash(InputAction.CallbackContext context)
    {
        DashBool = context.action.ReadValue<float>() == 1;
    }

    public void PrimaryFire(InputAction.CallbackContext context)
    {
        PrimaryBool = context.action.ReadValue<float>() == 1;
    }

    public void SecondaryFire(InputAction.CallbackContext context)
    {
        SecondaryBool = context.action.ReadValue<float>() == 1;
    }

    public void SwapLeft(InputAction.CallbackContext context)
    {
        SwapLBool = context.action.ReadValue<float>() == 1;
    }

    public void SwapRight(InputAction.CallbackContext context)
    {
        SwapRBool = context.action.ReadValue<float>() == 1;
    }

    // - GAMEPAD ONLY -
    //Used to create QOL changes when using a GamePad.

    public void Aim(InputAction.CallbackContext context)
    {
        AimBool = context.action.ReadValue<float>() == 1;
    }

    public void Look(InputAction.CallbackContext context)
    {
        //Can only be reached by Gamepad controls - creates a weapon wheel around the player
        RightStick = context.ReadValue<Vector2>();
    }
}
