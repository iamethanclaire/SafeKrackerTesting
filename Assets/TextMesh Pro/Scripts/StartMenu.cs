using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class StartMenu : MonoBehaviour
{
    public Button easyButton;
    public Button mediumButton;
    public Button hardButton;
    public Button startButton;
    public TextMeshProUGUI responseText; 
    public float typingSpeed = 0.05f;

    private string selectedMode = "Medium"; 
    public static float selectedTime = 300f; 
    private Coroutine typingCoroutine;
    public static FadeManager Instance;

    public void Start()
    {
        easyButton.onClick.AddListener(() => SetDifficulty(600f, "Easy"));
        mediumButton.onClick.AddListener(() => SetDifficulty(300f, "Medium"));
        hardButton.onClick.AddListener(() => SetDifficulty(60f, "Hard"));
        startButton.onClick.AddListener(StartGame);
    }

    private void SetDifficulty(float time, string mode)
    {
        selectedTime = time;
        selectedMode = mode;

        string message = $"{mode}";

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine = StartCoroutine(TypeText(message));
    }

    private void StartGame() => FadeManager.Instance.FadeToScene("GameScene");

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
}
