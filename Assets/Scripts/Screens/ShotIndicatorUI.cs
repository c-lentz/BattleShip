using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShotIndicatorUI : MonoBehaviour
{
    public TextMeshProUGUI shotText = null;
    private const string missMsg = "MISS!";
    private const string hitMsg = "HIT!";
    private const string firingMsg = "Firing...";
    

    public void UpdateFireResult(GridInfo curShotInfo)
    {
        shotText.text = (curShotInfo.hit ? hitMsg : missMsg);
    }

    public void FireMsg()
    {
        shotText.text = firingMsg;
    }
}
