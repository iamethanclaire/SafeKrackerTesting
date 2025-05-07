using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
[assembly: InternalsVisibleTo("Tests")]

public class QuestionManager : MonoBehaviour
{
    public AudioClip questionSound;
    public AudioClip cursedSound;
    public AudioClip correctNumberSound;
    private bool isHeartbeatPlaying = false;

    public TextMeshProUGUI[] questionTexts;
    public TextMeshProUGUI responseText;
    public string responseMessage;
    private Coroutine yesNoCoroutine;
    public TextMeshProUGUI timerText;
    public Button[] questionButtons;
    public NumberScroller[] numberScrollers;
    [SerializeField] internal GameObject[] numberScrollerObjects;
    public Button submitButton;
    public HeartManager heartManager;
    public SafeReaction safeReaction;
    [SerializeField] internal SafeShaker safeShaker;
    [SerializeField] internal UnityEngine.UI.Image[] heartImages;
    [SerializeField] internal GameObject[] numberBoxButtons;
    [SerializeField] internal Animator safeAnimator;
    public ClipboardManager Clipboard;


    internal int[] correctNumbers = new int[3];
    internal bool[] locks = new bool[3];
    private List<int> cursedQuestionIndices = new List<int>();
    private string[] numberFoundMessages = { "A chill creeps down your spine as the first number sets.", "The second number has been found, but something feels off", "The third number has been found." };
    internal int currentIndex = 0;

    internal float timeRemaining = 300f;
    private bool timerRunning = false;
    private int correctNumber;


    internal static readonly Dictionary<string, Func<int, bool>> AllQuestions = new Dictionary<string, Func<int, bool>>();

    private static readonly Dictionary<string, int> AtomicNumbers = new Dictionary<string, int>
    {
        { "Gold", 79},
        { "Silver", 47},
        { "Platnium", 78}
    };

    internal List<string> availableQuestions;

    private void Start()
    {
        ResetGame();

        if (safeAnimator == null)
        {
            safeAnimator = FindObjectOfType<Animator>();
        }

        if (safeShaker == null)
        {
            safeShaker = FindObjectOfType<SafeShaker>();  // Automatically find the SafeShaker if not manually assigned
        }
    }

    public void ResetGame()
    {
        // Reset the heart manager
        if (heartManager != null)
            heartManager.ResetHearts();


        // Reset the timer
        timeRemaining = StartMenu.selectedTime;
        timerRunning = true;
        StartCoroutine(UpdateTimer());

        // Reset the index and locks
        currentIndex = 0;
        locks = new bool[3];

        // Reset the number scrollers
        foreach (NumberScroller scroller in numberScrollers)
        {
            scroller.ResetScroller();
        }

        // Reset reactions
        if (safeReaction != null)
            safeReaction.ResetResponses();

        // Generate new numbers FIRST
        GenerateNewNumbers();

        // Reset and generate new questions
        PopulateQuestions();
        availableQuestions = new List<string>(AllQuestions.Keys); // Ensuring the list is refreshed
        GenerateAllQuestions();

        // Update scrollers
        UpdateActiveScroller();

        submitButton.onClick.RemoveAllListeners();
        submitButton.onClick.AddListener(CheckGuess);
        submitButton.onClick.AddListener(GenerateAllQuestions);

    }

    internal void PopulateQuestions()
    {
        AllQuestions.Clear();

        for(int i = 2; i < 12; i++)
        {
            int divisor = i;
            AllQuestions.Add("Is this number divisible by " + i + "?", number => number % divisor == 0);
        }

        for (int i = 0; i < 9; i++)
        {
            int digit = i;
            AllQuestions.Add("Does this number contain the digit " + i + "?", number => number%10 == digit || (int)number/10 == digit);
        }

        // US questions
        AllQuestions.Add("Is this number greater than the number of stars on the U.S. flag?", number => number > 50);
        AllQuestions.Add("Is this number less than the number of stars on the U.S. flag?", number => number < 50);
        AllQuestions.Add("Is this number the same as the number of U.S. states?", number => number == 50);
        AllQuestions.Add("Is this number found in US currency?", number => number == 10 || number == 25 || number == 50);
        AllQuestions.Add("Is this number greater than the number of stripes on the U.S. flag?", number => number > 13);

        // math questions

        AllQuestions.Add("Is this number a perfect square?", number => Mathf.Sqrt(number) % 1 == 0);
        AllQuestions.Add("Is this number greater than the freezing point of water in Fahrenheit?", number => number > 32);
        AllQuestions.Add("Is this number greater than the number of minutes in an hour", number => number > 60);
        AllQuestions.Add("Is this number greater than the number of hours in a day?", number => number > 24);
        AllQuestions.Add("Is this number an even number?", number => number % 2 == 0);
        AllQuestions.Add("Is this number an odd number?", number => number % 2 != 0);
        AllQuestions.Add("Is this number a prime number?", number => IsPrime(number));
        AllQuestions.Add("Is this number greater than the number of sides on a dodecahedron?", number => number > 12);


        // weird anatomy questions
        AllQuestions.Add("Is this number less than the number of vertebrae in the human spine? ", number => number < 33);
        AllQuestions.Add("Is this number greater than the number of a teeth in an adult human mouth? ", number => number > 32);
        AllQuestions.Add("Is this number less than the number of ribs in the human body? ", number => number < 24);
        AllQuestions.Add("Is this number greater than the number of bones in the human face? ", number => number > 14);
        AllQuestions.Add("Is this number greater than the number of bones in the human hand?", number => number > 27);
        AllQuestions.Add("Is this number less than the number of muscles in the human face?", number => number < 43);
        AllQuestions.Add("Is this number greater than the number of bones in the human foot?", number => number > 26);
        AllQuestions.Add("Is this number less than the number of bones in the human skull?", number => number < 22);

        // misc questions
        AllQuestions.Add("Is this number less than the number of squares on a chessboard?", number => number < 64);
        AllQuestions.Add("Is this number less than the number of pieces in a chess set?", number => number < 32);
        AllQuestions.Add("Is this number greater than the number of keys on a piano", number => number > 88);
        AllQuestions.Add("Is this number greater than the number of cards in a standard deck (no jokers)?", number => number > 52);
        AllQuestions.Add("Is this number greater than the number of days in a month?", number => number > 30);
        // atomic number questions
        foreach (var element in AtomicNumbers)
        {
            AllQuestions.Add($"Is this number greater than the atomic number of {element.Key}?", number => number > element.Value);
            AllQuestions.Add($"Is this number less than the atomic number of {element.Key}?", number => number < element.Value);
        }
    }

    public void DisplayYesNoResponse(bool isCorrect)
    {
        responseMessage = isCorrect ? "The Safe Says: \"Yes\"" : "The Safe Says: \"No\"";

        if (yesNoCoroutine != null)
        {
            StopCoroutine(yesNoCoroutine);
        }


        yesNoCoroutine = StartCoroutine(TypeYesNoResponse(responseMessage));
    }

    private IEnumerator TypeYesNoResponse(string message)
    {
        responseText.text = ""; // Clear previous text

        foreach (char letter in message.ToCharArray())
        {
            responseText.text += letter;
            yield return new WaitForSeconds(0.05f);
        }

        yesNoCoroutine = null; // Reset the coroutine reference when finished
    }
    public void GenerateAllQuestions()
    {
        availableQuestions = new List<string>(AllQuestions.Keys);
        cursedQuestionIndices.Clear();

        int numQuestions = Mathf.Min(questionTexts.Length, questionButtons.Length);

        for (int i = 0; i < numQuestions; i++)
        {
            questionButtons[i].onClick.RemoveAllListeners();  // Clear previous listeners

            // Decide if a cursed question will appear (10% chance)
            bool isCursed = UnityEngine.Random.Range(0, 20) < 2;

            if (isCursed)
            {
                int cursedIndex = UnityEngine.Random.Range(0, CursedQuestions.CursedQuestionPool.Count);
                string cursedQuestion = CursedQuestions.CursedQuestionPool[cursedIndex];

                questionTexts[i].text = cursedQuestion;
                questionTexts[i].color = cursedColor;
                cursedQuestionIndices.Add(i);

                questionButtons[i].onClick.AddListener(() => PlayCursedSound());
                questionButtons[i].onClick.AddListener(() => CursedQuestionClicked(cursedQuestion));
                questionButtons[i].onClick.AddListener(GenerateAllQuestions);
            }
            else
            {
                if (availableQuestions.Count == 0)
                    break;

                int questionIndex = UnityEngine.Random.Range(0, availableQuestions.Count);

                string questionTemplate = availableQuestions[questionIndex];
                availableQuestions.RemoveAt(questionIndex);

                // FIXED: Use the correct number for the current box being guessed
                int number = correctNumbers[currentIndex];
                bool isCorrect = AllQuestions[questionTemplate].Invoke(number);

                questionTexts[i].text = questionTemplate.Replace("{0}", number.ToString());
                questionTexts[i].color = Color.white; // Normal question text color

                bool correctCopy = isCorrect;

                int index = i;  // Capture the current value of i
                questionButtons[index].onClick.AddListener(PlayQuestionSound);
                questionButtons[index].onClick.AddListener(() => QuestionButtonClicked(correctCopy, questionTexts[index].text));
                questionButtons[index].onClick.AddListener(GenerateAllQuestions); // Ensures questions reset after clicking
            }
        }
    }

    public void CursedQuestionClicked(string questionText)
    {
        if (questionText == "Have you been here before?")
        {
            SceneManager.LoadScene("StartScreen");
            return;
        }

        Debug.Log("Cursed question asked.");

        // Play cursed sound using AudioManager
        if (cursedSound != null)
        {
            AudioManager.Instance.PlayCursedSound(cursedSound);
        }

       // bool isGameOver = heartManager.LoseHeart();           For if I wanted cursed question to take a heart.

        /*if (isGameOver)
        {
            Debug.Log("All hearts lost. Ending game.");
            LockAllBoxes();
            LoseGame();
            return;
        }*/

        string responseMessage = "The safe remains silent.";

        if (yesNoCoroutine != null)
        {
            StopCoroutine(yesNoCoroutine);
        }
        yesNoCoroutine = StartCoroutine(TypeYesNoResponse(responseMessage));

        int index = UnityEngine.Random.Range(0, CursedQuestions.WarningResponses.Count);
        safeReaction.DisplayReaction(CursedQuestions.WarningResponses[index]);

        // Stop the cursed sound after it finishes playing
        StartCoroutine(StopCursedSoundAfterDelay());
    }

    private IEnumerator StopCursedSoundAfterDelay()
    {
        yield return new WaitForSeconds(cursedSound.length);
        AudioManager.Instance.StopCursedSound();
    }

    public void QuestionButtonClicked(bool isCorrect, string text)
    {
        Debug.Log("Question button clicked. Generating Safe Response.");
        AudioManager.Instance.PlayQuestionSound(questionSound);
        DisplayYesNoResponse(isCorrect);  // This will make the Safe say "Yes" or "No"
       
    }



    public void GenerateNewNumbers()
    {
        for (int i = 0; i < 3; i++)
        {
            correctNumbers[i] = UnityEngine.Random.Range(10, 100);
            locks[i] = false;
            Debug.Log($"Correct Number for Box {i + 1}: {correctNumbers[i]}");
        }
    }

    public void CheckGuess()
    {
        if (heartManager.IsGameOver())
        {
            Debug.Log("Game Over - Cannot Check Guess");
            LockAllBoxes();
            return;
        }

        if (locks[currentIndex])
        {
            Debug.Log("Current index is already locked.");
            return;
        }

        NumberScroller currentScroller = numberScrollers[currentIndex];

        if (currentScroller.selectedNumber == correctNumbers[currentIndex])
        {
            locks[currentIndex] = true;
            currentScroller.LockNumberBox();
            safeReaction.DisplayReaction(numberFoundMessages[currentIndex]);

            if ( correctNumberSound != null)
            {
                AudioManager.Instance.PlayCorrectSound(correctNumberSound);
            }

            Debug.Log($"Correct! Moving to index {currentIndex + 1}");

            currentIndex++;
            if (currentIndex >= numberScrollers.Length)
            {
                Debug.Log("All numbers found. Ending game.");
                WinGame();
                return;
            }

            UpdateActiveScroller();
        }
        else
        {
            bool isGameOver = heartManager.LoseHeart();

            if (isGameOver)
            {
                Debug.Log("All hearts lost. Ending game.");
                LockAllBoxes();
                LoseGame();
            }
            else
            {
                

                safeReaction.DisplayRandomMistakeReaction();
            }
        }
    }

    internal void LockAllBoxes()
    {
        foreach (var scroller in numberScrollers)
        {
            scroller.LockNumberBox(); // Lock all boxes
        }
    }

    private void UpdateActiveScroller()
    {
        // Enable only the current NumberScroller for interaction
        for (int i = 0; i < numberScrollers.Length; i++)
        {
            if (i == currentIndex && !locks[i])
            {
                numberScrollers[i].EnableNumberBox();
            }
            else
            {
                numberScrollers[i].DisableNumberBox();
            }
        }
    }

    private Color normalColor = Color.white;
    private Color cursedColor = Color.red;
    private Color hoverColor = Color.black;

    public void OnHoverEnter(int index)
    {
        questionTexts[index].color = hoverColor;
    }
    public void OnHoverExit(int index)
    {
        if (cursedQuestionIndices.Contains(index))
        {
            questionTexts[index].color = cursedColor;
        }
        else
        {
            questionTexts[index].color = normalColor;
        }
    }

    internal static bool IsPrime(int number)
    {
        if (number <= 1) return false;
        for (int i = 2; i <= Math.Sqrt(number); i++)
            if (number % i == 0) return false;
        return true;
    }

    private IEnumerator UpdateTimer()
    {
        while (timerRunning && timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            float timeFraction = timeRemaining / StartMenu.selectedTime;
            float intensity = Mathf.Clamp01(1 - (timeFraction / 0.2f));

            if (timeFraction <= 0.15f)
            {
                if (!isHeartbeatPlaying)
                {
                    HeartbeatManager.Instance.StartHeartbeat();
                    isHeartbeatPlaying = true;
                }
                HeartbeatManager.Instance.UpdateHeartbeat(intensity);
            }

            int minutes = Mathf.FloorToInt(timeRemaining / 60);
            int seconds = Mathf.FloorToInt(timeRemaining % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

            yield return null;
        }

        if (timeRemaining <= 0)
        {
            timerText.text = "00:00";
            timerRunning = false;

            // Stop the heartbeat when the timer stops
            HeartbeatManager.Instance.StopHeartbeat();

            HandleGameOver();
        }
    }

    private void WinGame()
    {
        foreach (var button in questionButtons)
        {
            button.interactable = false;
        }
        timerRunning = false;

        // Save time data for the Win Screen
        PlayerPrefs.SetFloat("TimeRemaining", timeRemaining);
        PlayerPrefs.SetFloat("OriginalTime", StartMenu.selectedTime);
        PlayerPrefs.Save();

        FadeManager.Instance.FadeToScene("WinningScreen");
    }

    internal void LoseGame()
    {
        foreach (var button in questionButtons)
        {
            button.interactable = false;
        }
        timerRunning = false;

        FadeManager.Instance.FadeToScene("LosingScreen");
    }

    private void HandleGameOver()
    {
        Debug.Log("HandleGameOver() has been called.");

        if (submitButton != null)
        {
            submitButton.gameObject.SetActive(false);
        }

        if (timerText != null)
        {
            timerText.enabled = false;
        }

        foreach (var scroller in numberScrollerObjects)
        {
            if (scroller != null)
            {
                scroller.SetActive(false);
            }
        }

        foreach (var button in numberBoxButtons)
        {
            if (button != null)
            {
                button.SetActive(false);
            }
        }

        foreach (var heart in heartImages)
        {
            if (heart != null)
            {
                heart.enabled = false;
            }
        }

        AudioSource[] allAudioSources = FindObjectsOfType<AudioSource>();
        foreach (AudioSource audioSource in allAudioSources)
        {
            audioSource.Stop();
            audioSource.loop = false; // Ensure nothing restarts
        }

        AudioListener.pause = true;

        StartCoroutine(HandleGameOverSequence());
    }

    private IEnumerator HandleGameOverSequence()
    {
        yield return new WaitForSeconds(2f);

        string responseMessage = "The safe is awake.";
        yesNoCoroutine = StartCoroutine(TypeYesNoResponse(responseMessage));

        if (safeAnimator != null)
        {
            safeAnimator.Play("SafeAnimation");
            Debug.Log($"Playing animation: SafeAnimation");
        }

        Invoke("LoseGame", 4f);
    }

    // sound management

    public void PlayQuestionSound()
    {
        if (questionSound != null)
        {
            //AudioManager.Instance.PlayQuestionSound(questionSound);
        }
    }

    public void PlayCursedSound()
    {
        if (cursedSound != null)
        {
            AudioManager.Instance.PlayCursedSound(cursedSound);
            AudioManager.Instance.StopCursedSound();
        }
    }

    public void PlaySafeAnimation(string animationName)
    {
        if (safeAnimator != null)
        {
            safeAnimator.Play(animationName);
            Debug.Log($"Playing animation: {animationName}");
        }
    }



}