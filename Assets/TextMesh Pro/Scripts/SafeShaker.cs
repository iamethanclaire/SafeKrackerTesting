using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class SafeShaker : MonoBehaviour
{
    [SerializeField] private List<Sprite> safeSprites;  // List of safe images to choose from
    [SerializeField] private Image safeImage;  // Reference to the Image component displaying the safe
    public float shakeDuration = 0.5f;
    public float shakeMagnitude = 0.1f;

    private Vector3 originalPosition;
    private bool isShaking = false;

    private void Start()
    {
        originalPosition = transform.localPosition; 

        if (safeSprites.Count > 0 && safeImage != null)
        {
            Sprite randomSprite = safeSprites[Random.Range(0, safeSprites.Count)];
            safeImage.sprite = randomSprite;
            Debug.Log("Random Safe Image Selected: " + randomSprite.name);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            ShakeSafe();
        }
    }

    public void ShakeSafe()
    {
        if (!isShaking)
        {
            StartCoroutine(ShakeCoroutine());
        }
    }

    private IEnumerator ShakeCoroutine()
    {
        isShaking = true;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float y = Random.Range(-1f, 1f) * shakeMagnitude;

            transform.localPosition = originalPosition + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPosition;  // Reset to original position
        isShaking = false;
    }
}
