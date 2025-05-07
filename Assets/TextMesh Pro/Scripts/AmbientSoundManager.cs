using System.Collections;
using UnityEngine;

public class AmbientSoundManager : MonoBehaviour
{
    public AudioSource ambientAudioSource;
    public AudioClip[] ambientSounds; 

    public float minInterval = 10f; // Minimum time between sounds
    public float maxInterval = 30f; // Maximum time between sounds

    public void Start()
    {
        if (ambientAudioSource == null)
        {
            ambientAudioSource = GetComponent<AudioSource>();
        }

        StartCoroutine(PlayRandomAmbientSounds());
    }

    public IEnumerator PlayRandomAmbientSounds()
    {
        while (true)
        {
            if (!ambientAudioSource.isPlaying)
            {
                AudioClip clip = ambientSounds[Random.Range(0, ambientSounds.Length)];
                ambientAudioSource.PlayOneShot(clip);
            }

            float waitTime = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(waitTime);
        }
    }
}
