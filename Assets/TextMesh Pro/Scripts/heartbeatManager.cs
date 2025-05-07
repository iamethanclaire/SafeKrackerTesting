using UnityEngine;
using UnityEngine.Rendering;

public class HeartbeatManager : MonoBehaviour
{
    public static HeartbeatManager Instance { get; private set; }

    private AudioSource heartbeatAudioSource;
    public AudioClip heartbeatSound;
    public float maxVolume = 1.0f;
    public float minVolume = 0.3f;
    public float minPitch = 0.8f;
    public float maxPitch = 1.5f;

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

        heartbeatAudioSource = gameObject.AddComponent<AudioSource>();
        heartbeatAudioSource.clip = heartbeatSound;
        heartbeatAudioSource.loop = true;
        heartbeatAudioSource.playOnAwake = false;
    }

    public void StartHeartbeat()
    {
        if (!heartbeatAudioSource.isPlaying)
        {
            heartbeatAudioSource.Play();
        }
    }

    public void StopHeartbeat()
    {
        if (heartbeatAudioSource != null && heartbeatAudioSource.isPlaying)
        {
            heartbeatAudioSource.Stop();
        }
    }

    public void UpdateHeartbeat(float intensity)
    {
        intensity = Mathf.Clamp01(intensity); // Ensure intensity is between 0 and 1
        heartbeatAudioSource.volume = Mathf.Lerp(minVolume, maxVolume, intensity);
        heartbeatAudioSource.pitch = Mathf.Lerp(minPitch, maxPitch, intensity);
    }
}
