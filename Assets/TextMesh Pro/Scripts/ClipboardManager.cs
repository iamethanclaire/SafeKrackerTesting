using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ClipboardManager : MonoBehaviour
{
    public GameObject clipboardButtonPrefab;
    public Transform gridParent;

    public GameObject imageObject; 

    private bool isVisible = false;

    public GameObject clipboard;

    public int columns = 10;

    private List<ClipboardButton> numberButtons = new List<ClipboardButton>();

    public void Start()
    {
        imageObject.SetActive(true);
        GenerateGrid();
        imageObject.SetActive(false);
    }

    
    public void ToggleImage()
    {
        isVisible = !isVisible;
        imageObject.SetActive(isVisible);
    }

    void GenerateGrid()
    {
        int currentRow = 0;
        for (int i = 10; i <= 99; i++)
        {
            GameObject buttonGO = Instantiate(clipboardButtonPrefab, gridParent);
            ClipboardButton cb = buttonGO.GetComponent<ClipboardButton>();
            int index = i - 10;
            int row = index / columns;

            cb.SetupNumberButton(i, row);
            cb.clipboardManager = this;

            numberButtons.Add(cb);

            if ((index + 1) % columns == 0)
            {
                GameObject xGO = Instantiate(clipboardButtonPrefab, gridParent);
                ClipboardButton xBtn = xGO.GetComponent<ClipboardButton>();
                xBtn.SetupRowXButton(row);
                xBtn.clipboardManager = this;
            }
        }
    }

    public void ToggleLineRed(int row)
    {
        int start = row * columns;
        bool shouldTurnRed = false;

        for (int i = 0; i < columns; i++)
        {
            int idx = start + i;
            if (idx < numberButtons.Count && !numberButtons[idx].IsMarkedRed)
            {
                shouldTurnRed = true;
                break;
            }
        }

        for (int i = 0; i < columns; i++)
        {
            int idx = start + i;
            if (idx < numberButtons.Count)
            {
                numberButtons[idx].SetRed(shouldTurnRed);
            }
        }
    }


}
