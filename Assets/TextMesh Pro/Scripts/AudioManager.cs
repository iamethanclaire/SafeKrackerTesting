using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource audioSource;
    public bool isCursedSoundPlaying = false;

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }

    public virtual void PlayCursedSound(AudioClip clip)
    {
        if (clip == null) return;

        Debug.Log("Trying to play cursed sound.");

        // Forcefully stop all other sounds and play the cursed sound
        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();
        isCursedSoundPlaying = true;

        Debug.Log("Cursed sound playing: " + clip.name);
    }

    public virtual void PlayMissedQuestionSound(AudioClip clip)
    {
        if (clip == null || isCursedSoundPlaying) return; // Don't play if cursed sound is playing

        Debug.Log("Trying to play missed question sound.");

        if (!audioSource.isPlaying)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }

        Debug.Log("Missed question sound playing: " + clip.name);
    }

    public virtual void PlayCorrectSound(AudioClip clip)
    {
        Debug.Log("Trying to play asked question sound.");
        if (!audioSource.isPlaying)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }

    }

    public virtual void PlayQuestionSound(AudioClip clip)
    {
        Debug.Log("Trying to play asked question sound.");
        if (!audioSource.isPlaying)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }

    }

    public void PlayKnockSound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.Stop();
            audioSource.PlayOneShot(clip);
        }
    }

    public virtual void StopCursedSound()
    {
        if (isCursedSoundPlaying)
        {
            Debug.Log("Stopping cursed sound.");
            audioSource.Stop();
            isCursedSoundPlaying = false;
        }
    }
}
