using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SankBoatUI : MonoBehaviour
{
    [SerializeField]
    private GameObject entryPrefab = null;

    private GridLayoutGroup sankGrid = null;

    // Start is called before the first frame update
    void Start()
    {
        int[] shipLens = GameMgr.Instance().GetShipLengths();
        string[] shipNames = GameMgr.Instance().GetShipNames();

        sankGrid = gameObject.GetComponent<GridLayoutGroup>();
        if (sankGrid && entryPrefab)
        {
            for (int i = 0; i < shipLens.Length; i++)
            {
                GameObject entryObj = Instantiate(entryPrefab, sankGrid.transform);
                SankBoatUIEntry entry = entryObj.GetComponent<SankBoatUIEntry>();
                if (entry)
                {
                    if (i < shipNames.Length && i < shipLens.Length)
                        entry.SetBoatName($"{shipNames[i]} ({shipLens[i]})");
                }
            }
        }
    }

    public void ResetForNewGame()
    {
        if (!sankGrid) return;

        SankBoatUIEntry[] kidEntries = sankGrid.transform.GetComponentsInChildren<SankBoatUIEntry>();
        foreach (SankBoatUIEntry entry in kidEntries)
        {
            entry.ResetForNewGame();
        }
    }

    public void ShowSankShip(GridInfo lastShotInfo)
    {
        if (!sankGrid) return;

        int shipIndex = lastShotInfo.shipID;
        if (shipIndex < sankGrid.transform.childCount)
        {
            GameObject entryObj = sankGrid.transform.GetChild(shipIndex).gameObject;
            SankBoatUIEntry entry = entryObj.GetComponent<SankBoatUIEntry>();
            if (entry)
                entry.ShowSankIndicator();
        }
    }
}
