using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;

public class CanvasController : MonoBehaviour
{
    public static CanvasController CC;

    //Player Prompt
    GameObject PromptBox;
    Transform PromptAnchor; //Position of object to align PromptBox to = Camera Track object, slightly above player head
    TMP_Text PromptText;

    //Dialogue
    CanvasGroup DialogueCG;
    RectTransform NameBox;
    GameObject Continue;
    TMP_Text DialogueText, NameText;
    Animator DialogueUI, CharLeft, CharRight;
    Image ImgLeft, ImgRight;
    string ExpectedLine;
    public float TypeSpeed = 0.05f; //Time to wait between each character typed. Changes apply until the end of typing coroutine or until skip
    public float TextDelay = 0f; //Used when playing intro/end animations and can be set by dialogue
    Coroutine Typing; //null if finished typing, filled if still typing message

    Slider HealthMeter;
    Slider SpecialMeter;

    private void Start()
    {
        if (CC == null)
        {
            DontDestroyOnLoad(gameObject);
            CC = this;

            HealthMeter = transform.Find("Player HUD/Health Meter").GetComponent<Slider>();
            SpecialMeter = transform.Find("Player HUD/Special Meter").GetComponent<Slider>();

            PromptBox = transform.Find("PromptBox").gameObject;
            PromptText = PromptBox.transform.Find("PromptText").GetComponent<TMP_Text>();
            PromptAnchor = GameObject.Find("Camera Track").transform;

            DialogueCG = transform.Find("Dialogue").GetComponent<CanvasGroup>();
            DialogueUI = DialogueCG.GetComponent<Animator>();
            NameBox = GameObject.Find("NameBox").GetComponent<RectTransform>();
            NameText = NameBox.transform.Find("NameText").GetComponent<TMP_Text>();
            DialogueText = GameObject.Find("DialogueText").GetComponent<TMP_Text>();
            ImgLeft = GameObject.Find("PortraitL").GetComponent<Image>();
            ImgRight = GameObject.Find("PortraitR").GetComponent<Image>();
            CharLeft = ImgLeft.GetComponent<Animator>();
            CharRight = ImgRight.GetComponent<Animator>();
            Continue = GameObject.Find("DialogueContinue");
        } else
        {
            Destroy(gameObject);
        }        
    }

    // ------------ DIALOGUE FUNCTIONS ------------

    public void ToggleDialogue()
    {
        //Used to show or hide the box
        //Replace with an animation call based on which way it's up!
        

        if(DialogueCG.alpha < 1)
        {
            //Show box, portraits, etc.
            DialogueUI.SetTrigger("enter");
            CharLeft.SetTrigger("enter");
            //CharRight.SetTrigger("enter");

            AddDelay(0.3f);
        } else
        {
            //Hide box, portraits
            DialogueUI.SetTrigger("exit");
            CharLeft.SetTrigger("exit");
        }
    }

    public void AddDelay(float delay)
    {
        TextDelay = delay;
    }

    //Move the name box and apply colours according to who is speaking
    public void SetSpeaker(string speaker)
    {
        //Anchor guide
        //L = 0 1, 0 1
        //M = .5 1, .5 1
        //R = 1 1, 1 1

        switch (speaker)
        {
            case "L":
                //LEFT
                NameBox.anchorMin = new Vector2(0, 1);
                NameBox.anchoredPosition = new Vector3(25, 32.5f, 0);
                ImgLeft.color = Color.white;
                ImgRight.color = Color.grey;
                break;
            case "R":
                //RIGHT
                NameBox.anchorMin = new Vector2(1, 1);
                NameBox.anchoredPosition = new Vector3(-25, 32.5f, 0);
                ImgLeft.color = Color.grey;
                ImgRight.color = Color.white;
                break;
            case "B":
                //BOTH (middle)
                NameBox.anchorMin = new Vector2(0.5f, 1);
                NameBox.anchoredPosition = new Vector3(0, 32.5f, 0);
                ImgLeft.color = Color.white;
                ImgRight.color = Color.white;
                break;
            case "N":
                //NEITHER
                NameBox.anchorMin = new Vector2(0.5f, 1);
                NameBox.anchoredPosition = new Vector3(0, -1000f, 0);
                break;
        }

        NameBox.pivot = NameBox.anchorMin;
        NameBox.anchorMax = NameBox.anchorMin;
    }

    public void SetPortrait(string Portrait)
    {
        //Parse the string (format "L Name 0") to find out:
        // L: Which side to change
        // Name: Which character to use
        // 0: Which portrait index to use
        //      "L Hide" is to swap to an empty portrait on that side

        string[] commands = Regex.Split(Portrait, " ");
        Sprite sp = null;

        if(commands[1] != "Hide")
        {
            Sprite[] list = Resources.LoadAll<Sprite>("Sprites/Dialogue Ani/" + commands[1]);
            foreach(Sprite s in list)
            {
                if(s.name == commands[1] + "_" + commands[2])
                {
                    sp = s;
                    break;
                }
            }
        } else
        {
            //Add an empty sprite to use instead
        }

        if(commands[0] == "L")
        {
            ImgLeft.sprite = sp;
        } else
        {
            ImgRight.sprite = sp;
        }
    }

    //Show or hide the prompt box based on whether the text given is empty or not
    public void SetPrompt(string s)
    {
        if(PromptBox != null)
        {
            PromptText.text = s;
            PromptBox.SetActive(s != "");
        }
    }

    //When given a dialogue line, split it down and apply the parts in applicable places
    public void ParseLine(string line)
    {
        //0 should be name, 1 should be the dialogue
        string[] parsedLine = Regex.Split(line, ": ");

        //Shouldn't be a 2+, but if there is, add it onto 1 as it's likely part of the dialogue
        if (parsedLine.Length > 2)
        {
            for(int i = 2; i < parsedLine.Length; i++)
            {
                //Re-add the pattern as Regex removes it
                parsedLine[1] += ": " + parsedLine[i];
            }
        }

        NameText.text = parsedLine[0];
        ExpectedLine = parsedLine[1];
        DialogueText.text = "";
        Continue.SetActive(false);
        Typing = StartCoroutine(TypeLine());
    }

    IEnumerator TypeLine()
    {
        while(DialogueText.text != ExpectedLine)
        {
            if(TextDelay > 0)
            {
                float d = TextDelay;
                TextDelay = 0;
                yield return new WaitForSeconds(d);
            } else
            {
                DialogueText.text += ExpectedLine[DialogueText.text.Length];
                yield return new WaitForSeconds(TypeSpeed);
            }            
        }

        Continue.SetActive(true);

        //Reset type speed and finish typing
        TypeSpeed = 0.05f;
        Typing = null;
    }

    public bool IsTyping()
    {
        return Typing != null;
    }

    //Called if trying to continue while a TypeLine coroutine is still running
    public void SkipTyping()
    {
        StopCoroutine(Typing);

        Continue.SetActive(true);

        TypeSpeed = 0.05f;
        Typing = null;

        //Overwrite current progress of the line
        DialogueText.text = ExpectedLine;
    }

    // ------------ INGAME FUNCTIONS ------------

    //Used to check if the player already has an interaction set up
    public bool GetPrompt()
    {
        return PromptBox.activeInHierarchy;
    }

    private void Update()
    {
        PromptBox.transform.position = Camera.main.WorldToScreenPoint(PromptAnchor.position);
    }

    public int GetCurrentHealth()
    {
        return (int)HealthMeter.value;
    }

    public int GetMaxHealth()
    {
        return (int)HealthMeter.maxValue;
    }

    public float GetSpecial()
    {
        return (int)SpecialMeter.value;
    }

    public void SetCurrentHealth(int Current)
    {
        HealthMeter.value = Current;
    }

    public void SetMaxHealth(int NewMax)
    {
        HealthMeter.maxValue = NewMax;
    }

    public void SetSpecial(int i)
    {
        //Empty for now!
    }
}
