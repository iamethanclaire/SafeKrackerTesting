using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ClipboardButton : MonoBehaviour
{
    public TextMeshProUGUI buttonText;
    private Image background;

    public bool isRowX = false;
    public int rowIndex;
    public ClipboardManager clipboardManager;

    private bool isRed = false;

    void Awake()
    {
        background = GetComponent<Image>();
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    public void SetupNumberButton(int number, int row)
    {
        isRowX = false;
        rowIndex = row;
        buttonText.text = number.ToString();
        ResetVisual();
    }

    public void SetupRowXButton(int row)
    {
        isRowX = true;
        rowIndex = row;
        buttonText.text = "X";
        buttonText.color = Color.red;
        ResetVisual();
    }

    private void OnClick()
    {
        if (isRowX)
        {
            clipboardManager.ToggleLineRed(rowIndex);
        }
        else
        {
            ToggleRed();
        }
    }

    public void ToggleRed()
    {
        isRed = !isRed;
        UpdateVisual();
    }

    public void SetRed(bool value)
    {
        isRed = value;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        background.color = isRed ? Color.red : Color.white;
        buttonText.color = isRed ? Color.red : (isRowX ? Color.red : Color.black);
    }

    public void ResetVisual()
    {
        SetRed(false);
    }

    public bool IsMarkedRed => isRed;
}
