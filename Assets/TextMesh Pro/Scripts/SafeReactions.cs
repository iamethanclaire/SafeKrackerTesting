using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class SafeReaction : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI responseText;
    [SerializeField] public CanvasGroup flickerCanvas; 
    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private float flickerDuration = 3f; 
    [SerializeField] private int flickerCount = 5;

    public AudioSource audioSource;
    public AudioClip knockSound;

    private List<string> mistakeResponses = new List<string>()
    {
        //"The safe can smell your greed.",
        //"Your vision starts to haze, you can tell you're one step closer to the end.",
        //"You hear a whisper in your ear. You turn around, but nothing meets your eyes.",
        //"Pandora's Box remained closed for a reason.",
        //"The safe remains locked and you begin to understand why.",
        //"Your hands feel heavier now as if the safe is telling you to resist touching it.",
        "You hear a creak in the distance, something is very wrong.",
        "The smart thing to do would be to run away.",
        "Your bones feel like they're turning to iron, these past few minutes have felt like days.",
        "You hear a noise from the box that sounds alive.",
        "You hear a knock come from within the walls",
        "The lights begin to flicker"
    };

    private List<string> availableResponses;
    private Coroutine typingCoroutine;

    private void Awake()
    {
        ResetResponses();

        if (flickerCanvas != null)
        {
            flickerCanvas.alpha = 0;
        }
    }


    public void ResetResponses()
    {
        availableResponses = new List<string>(mistakeResponses);
    }

    public virtual void DisplayReaction(string message)
    {
        if (responseText != null)
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }
            typingCoroutine = StartCoroutine(TypeText(message));

            if (message.Trim() == "The lights begin to flicker")
            {
                StartCoroutine(FlickerEffect());
            }
            if(message.Trim() == "You hear a knock come from within the walls")
            {
                AudioManager.Instance.PlayKnockSound(knockSound);
            }
        }
    }

    public void DisplayRandomMistakeReaction()
    {
        if (availableResponses.Count > 0)
        {
            int index = Random.Range(0, availableResponses.Count);
            string selectedResponse = availableResponses[index];
            availableResponses.RemoveAt(index);

            DisplayReaction(selectedResponse);
        }
    }

    public void DisplayCursedReaction(string message)
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        typingCoroutine = StartCoroutine(TypeText(message));
    }

    private IEnumerator TypeText(string message)
    {
        responseText.text = "";

        foreach (char letter in message.ToCharArray())
        {
            responseText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        typingCoroutine = null;
    }

    private IEnumerator FlickerEffect()
    {
        Debug.Log("FlickerEffect() Coroutine Started");

        float elapsedTime = 0f;
        int flickers = 0;

        while (elapsedTime < flickerDuration)
        {
            flickerCanvas.alpha = 1; // Screen goes dark
            Debug.Log("Flickering On");
            yield return new WaitForSeconds(Random.Range(0.05f, 0.15f));

            flickerCanvas.alpha = 0; // Screen goes back to normal
            Debug.Log("Flickering Off");
            yield return new WaitForSeconds(Random.Range(0.05f, 0.25f));

            elapsedTime += Time.deltaTime;
            flickers++;

            if (flickers >= flickerCount)
                break;
        }

        flickerCanvas.alpha = 0; // Ensure the canvas is invisible after the flicker
        Debug.Log("FlickerEffect() Coroutine Ended");
    }



}
