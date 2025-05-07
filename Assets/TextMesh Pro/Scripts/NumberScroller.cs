using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class NumberScroller : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public TextMeshProUGUI numberText;
    public Image numberImage; // Reference to the Image component for visual changes
    public int selectedNumber = 8;
    private bool isDragging = false;
    private float scrollSpeed = 0.005f;
    private bool isLocked = false;

    private Color opaqueColor = new Color(1f, 1f, 1f, 1f); // Fully Opaque (Alpha = 1)
    private Color translucentColor = new Color(1f, 1f, 1f, 0.4f); // Translucent (Alpha = 0.4)

    public void Start()
    {
        if (numberText != null)
            numberText.text = "X";

        if (numberImage != null)
            numberImage.color = translucentColor; // Start as translucent
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isLocked)
            isDragging = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isLocked)
            isDragging = false;
    }

    private float dragAccumulator = 0f;
    public float scrollThreshold = 15f; // Minimum pixels to move before number changes

    public void OnDrag(PointerEventData eventData)
    {
        if (isDragging && !isLocked)
        {
            dragAccumulator += eventData.delta.y;

            if (dragAccumulator >= scrollThreshold)
            {
                selectedNumber = Mathf.Clamp(selectedNumber + 1, 10, 99);
                UpdateDisplay();
                dragAccumulator = 0f;
            }
            else if (dragAccumulator <= -scrollThreshold)
            {
                selectedNumber = Mathf.Clamp(selectedNumber - 1, 10, 99);
                UpdateDisplay();
                dragAccumulator = 0f;
            }
        }
    }


    public void LockNumberBox()
    {
        isLocked = true;

        if (numberImage != null)
            numberImage.color = opaqueColor; 
    }

    public void EnableNumberBox()
    {
        isLocked = false;
        numberText.color = Color.white;

        if (numberImage != null)
            numberImage.color = opaqueColor; 
    }

    public void DisableNumberBox()
    {
        isLocked = true;
        numberText.color = Color.gray;

        if (numberImage != null)
            numberImage.color = translucentColor; 
    }

    public void ResetScroller()
    {
        selectedNumber = 10;
        numberText.text = "X";
        EnableNumberBox();
    }

    private void UpdateDisplay()
    {
        if (numberText != null)
            numberText.text = selectedNumber.ToString();
    }

}
