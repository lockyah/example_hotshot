using UnityEngine;
using Ink.Runtime;
using UnityEngine.Playables;

public class CutsceneDirector : MonoBehaviour
{
    public static CutsceneDirector CD;
    public int SaveSlot = 0; //Determines which Story file to write/read from

    public TextAsset InkAsset;
    private Story InkStory;
    private PlayableDirector TL;

    public bool CutscenePlaying = false; //Add another parameter for "current interaction", player controller detects if it's full?
    public string CurrentInteraction = "";

    // DOCUMENTATION: https://github.com/inkle/ink/blob/master/Documentation/RunningYourInk.md#getting-started-with-the-runtime-api
    // WRITING TIPS: https://github.com/inkle/ink/blob/master/Documentation/WritingWithInk.md

    void Start()
    {
        if(CD == null)
        {
            DontDestroyOnLoad(gameObject);
            CD = this;

            InkAsset = Resources.Load<TextAsset>("Story"); //Edit later to be "Story_File1" and have Story as the base to reset to
            InkStory = new Story(InkAsset.text); //The Story is what reads the Ink story to the game!

            //External Functions are automatically used and skipped over, so multiple can be used in a row without filling the text box!
            InkStory.BindExternalFunction("SetPortrait", (string Portrait) => { CanvasController.CC.SetPortrait(Portrait); });
            InkStory.BindExternalFunction("SetSpeaker", (string Position) => { CanvasController.CC.SetSpeaker(Position); });
            InkStory.BindExternalFunction("PlayAnimation", (string Ani) => { PlayAnimation(Ani); });
            InkStory.BindExternalFunction("PlayTimeline", (string Name) => { PlayTimeline(Name); });
            InkStory.BindExternalFunction("ToggleBox", () => { ToggleBox(); });

            TL = GetComponent<PlayableDirector>(); //Used for ingame cutscenes
        } else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        //Handle "continue" prompts
        if (CutscenePlaying && Input.GetButtonDown("Fire1"))
        {
            //If CC.Typing != null, it's still writing, so run function to stop coroutines and just set to expected sentence
            if (CanvasController.CC.IsTyping())
            {
                //* On days like this, kids like you...
                if (!InkStory.currentTags.Contains("NoSkip"))
                {
                    CanvasController.CC.SkipTyping();
                }
            }
            else
            {
                ContinueDialogue();
            }
        }
    }

    void ToggleBox()
    {
        //Show or hide the box and portraits. Handled on CC's end!
        CanvasController.CC.ToggleDialogue();
    }

    void PlayAnimation(string Ani)
    {
        //Format "L Name" to play animation named on left character

        //Set triggers "L", "Name" separately. Animations only play when two triggers are used and multiple PlayAnis in a row should work!
    }

    void PlayTimeline(string Name)
    {
        //Swap from dialogue box to playing an ingame cutscene.
        //Doesn't automatically hide the portraits or box - needs to be a separate call first!
    }

    public void BeginCutscene(string KnotName)
    {
        //Set up the dialogue box for the Ink knot specified if CD is not already busy
        if (!CutscenePlaying)
        {
            //Assume every character is empty at first
            CanvasController.CC.SetPortrait("L Hide");
            CanvasController.CC.SetPortrait("R Hide");

            CutscenePlaying = true;

            InkStory.ChoosePathString(KnotName);
            ContinueDialogue();
        }
    }

    public void EndCutscene()
    {
        CutscenePlaying = false;

        //Allow player to continue
        PlayerControl.PC.PlayerCanMove = true;
        PlayerControl.PC.ImmobileTimer = 0f;
    }

    public void ContinueDialogue()
    {
        if (InkStory.canContinue)
        {
            //Called by inputting space while the current dialogue is not still typing out and not in multiple choice mode
            //(Space while typing skips to the end of the current sentence, unless typing speed isn't 1)

            //Continue the Ink story segment and handle it based on if it's a dialogue or cutscene cue
            InkStory.Continue();

            //Parse tags to see what needs to happen for this line
            if(InkStory.currentTags.Count > 0)
            {
                foreach(string s in InkStory.currentTags)
                {
                    //TEXT SPEED
                    if (s.Substring(0,6) == "Speed ")
                    {
                        CanvasController.CC.TypeSpeed = float.Parse(s.Substring(6));
                    }

                    //More tags for colours ("Col Red 0 5" could make letters 0-5 red), overriding speech sound (Sound Faye), etc?
                }
            }

            //Send line to CC to write out
            CanvasController.CC.ParseLine(InkStory.currentText);

            //Check for CanContinue to set up buttons
        } else
        {
            //If can't continue, set up a multiple choice
            if (InkStory.currentChoices.Count > 0)
            {
                Debug.Log("EEEE");
                //Set up for a multiple choice


                /* empty cc buttons
                 * 
                 * for int i = 0  limit choice number etc etc:
                 *      Choice choice = IS.currentChoices[i]
                 *      add a button to cc
                 *      set button value to i
                 *      set button text to choice.text
                 */

                //Needs a ToggleAnswers() in CanvasController to set up and show buttons on Continue, then again to hide them when MakeChoice is used
            } else
            {
                //If end of the current knot, reset the dialogue box!
                Debug.Log("End!");

                CanvasController.CC.ToggleDialogue();
                EndCutscene();
            }
        }
    }

    public void MakeChoice(int i)
    {
        //Called from CanvasController to answer a multiple choice branch.
        
        InkStory.ChooseChoiceIndex(i);
    }
}
