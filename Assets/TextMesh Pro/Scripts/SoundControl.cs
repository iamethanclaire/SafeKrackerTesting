using UnityEngine;
using UnityEngine.Audio;

public class SoundControl : MonoBehaviour
{
    public static SoundControl instance;

    public AudioMixer gameAudioMixer;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject); // Prevents duplicates
        }
    }

    // Adjusts volume for different sound types
    public void SetVolume(string groupName, float volume)
    {
        float volumeInDecibels = Mathf.Log10(volume) * 20;
        gameAudioMixer.SetFloat(groupName, volumeInDecibels);
    }
}
