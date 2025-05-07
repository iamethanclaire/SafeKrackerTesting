using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LosingScreenManager : MonoBehaviour
{
    public void ReturnHome()
    {
        FadeManager.Instance.FadeToScene("StartScreen");
    }
}