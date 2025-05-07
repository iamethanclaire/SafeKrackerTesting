using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;

public class HeartManager : MonoBehaviour
{
    public Image[] heartImages; 
    public TextMeshProUGUI responseText;

    public AudioSource heartLossAudioSource;
    public AudioClip heartLossSound;

    public static bool IsKnockSoundPlaying = false;
    private int lives;

    private void Start()
    {
        if (heartImages == null || heartImages.Length == 0)
        {
            return;
        }

        if (responseText == null)
        {
            return;
        }

        lives = heartImages.Length; 
        UpdateHearts();
    }

    public bool LoseHeart()
    {
        if (heartImages == null || heartImages.Length == 0)
        {
            return true;
        }

        if(heartLossSound != null)
        {
            AudioManager.Instance.PlayMissedQuestionSound(heartLossSound);
        }

        if (lives > 0)
        {
            lives--; 
            UpdateHearts();

            if (lives == 0)
            {
                return true; 
            }
        }

        return false; 
    }

    private void UpdateHearts()
    {
        if (heartImages == null)
        {
            return;
        }

        for (int i = 0; i < heartImages.Length; i++)
        {
            if (heartImages[i] == null)
            {
                continue;
            }

            heartImages[i].enabled = (i < lives); 
        }
    }

    public bool IsGameOver()
    {
        return lives <= 0; 
    }

    public void ResetHearts()
    {
        lives = heartImages.Length;
        UpdateHearts();
    }
}
