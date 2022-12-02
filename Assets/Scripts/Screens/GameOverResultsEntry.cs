using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameOverResultsEntry : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI label = null;
    [SerializeField]
    private TextMeshProUGUI value = null;

    public void SetText(string labelText, string valueText)
    {
        label.text = labelText;
        value.text = valueText;
    }
}
