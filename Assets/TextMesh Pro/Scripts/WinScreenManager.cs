using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement; // Import SceneManagement
using TMPro;

public class WinScreenManager : MonoBehaviour
{
    public TextMeshProUGUI winText;
    public TextMeshProUGUI endingText;  // New TextMeshProUGUI for BAD ENDING label
    public float typingSpeed = 0.05f;
    public Color badEndingColor = Color.red; // Red color for special text
    public float delayBeforeTitleScreen = 10f; // Time before returning to title screen

    private string[] generalWinMessages = new string[]
    {
        "You pull on the safe handle, but it won't budge. After a few minutes of work with your crowbar, you pop open the door to the safe. The last thing you remember was the shadows pouring out from within.",
        "The lights go out. You awake to find the safe open and completely empty. You don't think much of it until you feel a cold touch on your shoulder.",
        "Before you have a chance to pull on the handle, the safe is pushed open from within. A red glow meets your eyes as the room turns frigid. You're too stunned to move, and that was their plan all along.",
        "You pull on the safe handle, and it opens. It's empty, but you know it shouldn't be. You reach your hand into the safe to feel for something. You feel a pull.",
        "<color=#FF0000>What have you done?</color>"
    };

    private string fastWinMessage = "Speed has always been your priority, oftentimes over your own safety. You wish you had taken life a little slower as you peek into the cavern of the safe. Hundreds of eyes look back at you.";
    private string slowWinMessage = "You wish you had taken the repeated warnings more seriously as you slowly open the door to the safe. You always hoped of one day seizing riches because of your job. But this, this wasn't what you wanted to see. Pandora's Box stayed closed for a reason.";

    private Coroutine typingCoroutine;

    private void Start()
    {
        float timeRemaining = PlayerPrefs.GetFloat("TimeRemaining", 0);
        float originalTime = PlayerPrefs.GetFloat("OriginalTime", 1);

        string message = SelectMessage(timeRemaining, originalTime);

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine = StartCoroutine(TypeText(message));
    }

    private string SelectMessage(float timeRemaining, float originalTime)
    {
        float timePercentage = (timeRemaining / originalTime) * 100f;

        if (timePercentage > 50f)
        {
            return fastWinMessage;
        }
        else if (timePercentage < 20f)
        {
            DisplayBadEnding();
            return slowWinMessage;
        }
        else
        {
            int index = Random.Range(0, generalWinMessages.Length);

            if (index == generalWinMessages.Length - 1)
            {
                DisplayBadEnding();
            }

            return generalWinMessages[index];
        }
    }

    private IEnumerator TypeText(string message)
    {
        winText.text = ""; 

        bool isTag = false; // Tracks if we are inside a <color> tag
        string currentTag = ""; // Stores the current tag text

        foreach (char letter in message)
        {
            if (letter == '<') isTag = true;
            if (isTag) currentTag += letter;
            if (letter == '>') isTag = false;

            if (!isTag)
            {
                if (currentTag.Length > 0)
                {
                    winText.text += currentTag; // Display the whole tag at once
                    currentTag = "";
                }

                winText.text += letter; 
                yield return new WaitForSeconds(typingSpeed);
            }
        }

        typingCoroutine = null;

        StartCoroutine(ReturnToTitleScreen());
    }

    private void DisplayBadEnding()
    {
        if (endingText != null)
        {
            endingText.text = "<color=#FF0000>BAD ENDING</color>";
            endingText.enabled = true;
        }
    }

    private IEnumerator ReturnToTitleScreen()
    {
        yield return new WaitForSeconds(delayBeforeTitleScreen);
        SceneManager.LoadScene("StartScreen"); 
    }
}
