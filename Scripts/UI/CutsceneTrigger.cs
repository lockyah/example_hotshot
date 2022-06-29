using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutsceneTrigger : MonoBehaviour
{
    public bool AutomaticStart; //Should cutscene start immediately on entering?
    public string InkKnotName; //Which Ink Knot should be accessed on entry?
    public string PromptName; //What should be shown above the player's head?
    //Add a "AfterCS" thing for whether to disable collider after one dialogue, after speaking a few times, etc.
    private Coroutine Falling; //If a StartOnGround routine is called, save it here to prevent multiple running

    private List<Collider> PlayerColls = new List<Collider>(); //Player has two colliders, so we need to track how many are in to prevent multiple plays
    private ParseInputs Input;
    private bool CanPlay = true; //Mainly for automatic starts, manual doesn't stop interaction


    private void OnTriggerEnter(Collider other)
    {
        //Keep track of player colliders
        PlayerColls.Add(other);
        Input = other.GetComponent<ParseInputs>();

        //Track this as priority cutscene
        if(CutsceneDirector.CD.CurrentInteraction == "")
        {
            CutsceneDirector.CD.CurrentInteraction = InkKnotName;
            CanvasController.CC.SetPrompt(PromptName);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        PlayerColls.Remove(other);

        //If Colls is now empty, both player colls have left, so reset the state!
        if(PlayerColls.Count == 0)
        {
            Input = null;

            CanPlay = true;
            CanvasController.CC.SetPrompt("");

            if(CutsceneDirector.CD.CurrentInteraction == InkKnotName)
            {
                CutsceneDirector.CD.CurrentInteraction = "";
            }
        }
    }

    //Failsafe state reset
    private void OnDisable()
    {
        PlayerColls.Clear();
        CanPlay = true;
        CanvasController.CC.SetPrompt("");
        Falling = null;
    }

    private void Update()
    {
        if(PlayerColls.Count > 0 && CanPlay && CutsceneDirector.CD.CurrentInteraction == "")
        {
            //If previously priority cutscene is removed, try to swap to this one
            CutsceneDirector.CD.CurrentInteraction = InkKnotName;
            CanvasController.CC.SetPrompt(PromptName);
        }


        if(PlayerColls.Count > 0 && CanPlay && CutsceneDirector.CD.CurrentInteraction == InkKnotName)
        {
            //Should only begin on the first collider entry!
            if (!CutsceneDirector.CD.CutscenePlaying)
            {
                if (AutomaticStart)
                {
                    //No prompt/button needed, just begin!
                    CanPlay = false;

                    //Only starts this way once
                    if (Falling == null)
                    {
                        Falling = StartCoroutine(StartOnGround());
                    }
                }
                else
                {
                    //Set or empty prompt based on if player is on ground
                    CanvasController.CC.SetPrompt(CutsceneDirector.CD.PlayerRef.ControllerGrounded() ? PromptName : "");

                    if (CutsceneDirector.CD.PlayerRef.ControllerGrounded() && Input.SecondaryButton == ParseInputs.ButtonState.Pressed)
                    {
                        BeginCutscene();
                    }
                }
            }
            else if (CutsceneDirector.CD.CutscenePlaying)
            {
                //If in area but cutscene director is busy, ignore inputs and hide the prompt
                CanvasController.CC.SetPrompt("");
            }

        }
    }

    IEnumerator StartOnGround()
    {
        PlayerControl PC = CutsceneDirector.CD.PlayerRef;
        //PC.ImmobileTimer = 999f; //No horizontal movements allowed!
        //Add ability to disable actions all in one (dash, attacks, held abilities, etc.)
        //Done this way round so state animations still work!
        //PC.MoveInput = new Vector3(0, -0.9f, 0); //Set to only fall

        //Wait for player to land
        //while (!PC.ControllerGrounded())
        //{
        //    yield return new WaitForSeconds(0.5f);
        //}
        yield return null;

        //After landing, begin!
        BeginCutscene();
        Falling = null;
    }

    private void BeginCutscene()
    {
        //Call CutsceneDirector with the Ink segment to use!

        //Double-check that the player stops moving
        CutsceneDirector.CD.PlayerRef.PlayerCanMove = false;
        CutsceneDirector.CD.PlayerRef.HorizSpeed = 0f;

        CutsceneDirector.CD.BeginCutscene(InkKnotName);
    }
}
