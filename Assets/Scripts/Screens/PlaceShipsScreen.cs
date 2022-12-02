
using UnityEngine;
using UnityEngine.UI;

public class PlaceShipsScreen : Screen
{
    public GameObject shipsGridObj = null;
    public GameObject entryPrefab = null;


    private void OnEnable()
    {
        GridLayoutGroup shipsGrid = shipsGridObj.GetComponent<GridLayoutGroup>();
        foreach (Transform child in shipsGrid.transform)
        {
            Destroy(child.gameObject);
        }

        for (short col = 0; col < 10; col++)
        {
            for (short row = 0; row < 10; row++)
            {
                // Fill up the ships grid.
                GameObject shipsEntry = Instantiate(entryPrefab, shipsGrid.transform);
                shipsEntry.name = $"Entry[{row}][{col}]";

                BattleshipGridEntry shipsGridEntry = shipsEntry.GetComponent<BattleshipGridEntry>();
                shipsGridEntry.gridRow = row;
                shipsGridEntry.gridCol = col;
                shipsGridEntry.localPlayerOwned = true;
            }
        }
    }

    private void RefreshShipDisplay()
    {
        GridLayoutGroup shipsGrid = shipsGridObj.GetComponent<GridLayoutGroup>();
        BattleshipGridEntry[] entryKids = shipsGrid.transform.GetComponentsInChildren<BattleshipGridEntry>();
        foreach (BattleshipGridEntry child in entryKids)
        {
            child.RefreshLocalPlayerHasShip();
        }
    }

    public void OnRandomizeShipsBtnClicked()
    {
        GameMgr.Instance().RerandomizePlayerShips();
        RefreshShipDisplay();
    }

    public void OnExitBtnClicked()
    {
        SetActive(false);
        UIMgr.Instance().ShowMainUI();
    }
}
