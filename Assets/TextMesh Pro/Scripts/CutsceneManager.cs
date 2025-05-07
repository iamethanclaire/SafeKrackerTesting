using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class CutsceneManager : MonoBehaviour
{
    public TextMeshProUGUI cutsceneText;
    public float typingSpeed = 0.05f;
    public float textBlockDelay = 0.5f;

    private string[] cutsceneBlocks = new string[]
    {
        "Pandora’s box stayed closed for a reason." // This is the line we want to be red
    };

    private int currentBlockIndex = 0;
    private bool isTyping = false;
    private Coroutine typingCoroutine;

    private void Start()
    {
        ShowNextBlock();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Left-click to continue
        {
            if (isTyping)
            {
                SkipTyping();
            }
            else
            {
                ShowNextBlock();
            }
        }

        if (Input.GetMouseButtonDown(1)) // Right-click to skip cutscene
        {
            EndCutscene();
        }
    }

    private void ShowNextBlock()
    {
        if (currentBlockIndex >= cutsceneBlocks.Length)
        {
            FadeManager.Instance.FadeToScene("StartScreen");
            return;
        }

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText(cutsceneBlocks[currentBlockIndex]));
        currentBlockIndex++;
    }

    private IEnumerator TypeText(string text)
    {
        isTyping = true;
        cutsceneText.text = "";

        

        for (int i = 0; i < text.Length; i++)
        {
            cutsceneText.text += text[i];
            if (currentBlockIndex == cutsceneBlocks.Length) 
            {
                cutsceneText.color = Color.red;
            }
            yield return new WaitForSeconds(typingSpeed);
        }

        

        isTyping = false;
        yield return new WaitForSeconds(textBlockDelay);
        ShowNextBlock();
    }

    private void SkipTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        if (currentBlockIndex == cutsceneBlocks.Length)
        {
            cutsceneText.color = Color.red; // Make the text red instantly
        }

        cutsceneText.text = cutsceneBlocks[currentBlockIndex - 1];

        

        isTyping = false;
    }

    private void EndCutscene()
    {
        SceneManager.LoadScene("StartScreen"); 
    }
}
