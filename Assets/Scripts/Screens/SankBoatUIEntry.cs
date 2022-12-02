using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SankBoatUIEntry : MonoBehaviour
{
    private GameObject sankIndicatorObj = null;
    private TextMeshProUGUI textObj = null;

    // Start is called before the first frame update
    void Start()
    {
        if (transform.childCount > 1)
            sankIndicatorObj = transform.GetChild(1).gameObject;

        textObj = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void ResetForNewGame()
    {
        if (sankIndicatorObj)
            sankIndicatorObj.SetActive(false);
    }

    public void ShowSankIndicator()
    {
        if (sankIndicatorObj)
            sankIndicatorObj.SetActive(true);
    }

    public void SetBoatName(string name)
    {
        if (!textObj)
            textObj = GetComponentInChildren<TextMeshProUGUI>();

        if (textObj)
            textObj.text = name;
    }
}
