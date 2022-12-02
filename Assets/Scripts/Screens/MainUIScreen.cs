using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainUIScreen : Screen
{
    public bool showAIShips = false;

    public GameObject shotsGridObj = null;
    public GameObject shipsGridObj = null;
    public GameObject entryPrefab = null;
    public Button fireButton = null;
    public TextMeshProUGUI targetText = null;
    public SankBoatUI aiLostShipsUI = null;
    public SankBoatUI playerLostShipsUI = null;
    public GameObject aiTurnIndicator = null;
    public GameObject playerTurnIndicator = null;
    public ShotIndicatorUI aiShotResultIndicator = null;
    public ShotIndicatorUI playerShotResultIndicator = null;

    private bool needToStartFirstTurn = true;
    private BattleshipGridEntry curTarget = null;


    private void OnEnable()
    {
        showAIShips = false;
        if (aiLostShipsUI)
            aiLostShipsUI.ResetForNewGame();
        if (playerLostShipsUI)
            playerLostShipsUI.ResetForNewGame();
        if (aiShotResultIndicator)
            aiShotResultIndicator.gameObject.SetActive(false);
        if (playerShotResultIndicator)
            playerShotResultIndicator.gameObject.SetActive(false);
        if (targetText)
            targetText.text = "(click a tile to set)";

        GridLayoutGroup shotsGrid = shotsGridObj.GetComponent<GridLayoutGroup>();
        GridLayoutGroup shipsGrid = shipsGridObj.GetComponent<GridLayoutGroup>();

        foreach (Transform child in shotsGrid.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in shipsGrid.transform)
        {
            Destroy(child.gameObject);
        }

        for (short col = 0; col < 10; col++)
        {
            for (short row = 0; row < 10; row++)
            {
                // Fill up the shots grid.
                GameObject shotsEntry = Instantiate(entryPrefab, shotsGrid.transform);
                shotsEntry.name = $"Entry[{row}][{col}]";

                BattleshipGridEntry shotsGridEntry = shotsEntry.GetComponent<BattleshipGridEntry>();
                shotsGridEntry.gridRow = row;
                shotsGridEntry.gridCol = col;
                shotsGridEntry.screen = this;

                // Fill up the ships grid.
                GameObject shipsEntry = Instantiate(entryPrefab, shipsGrid.transform);
                shipsEntry.name = $"Entry[{row}][{col}]";

                BattleshipGridEntry shipsGridEntry = shipsEntry.GetComponent<BattleshipGridEntry>();
                shipsGridEntry.gridRow = row;
                shipsGridEntry.gridCol = col;
                shipsGridEntry.screen = this;
                shipsGridEntry.localPlayerOwned = true;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (needToStartFirstTurn)
        {
            needToStartFirstTurn = false;
            StartNewTurn();
        }
    }

    public void StartNewTurn()
    {
        UpdateTurnIndicators();
        if (GameMgr.Instance().IsPlayerTurn())
        {
            curTarget = null;
            aiShotResultIndicator.gameObject.SetActive(false);
            GameMgr.Instance().SetPlayerFiring(false);
        }
        else
        {            
            playerShotResultIndicator.gameObject.SetActive(false);
            GameMgr.Instance().StartAITurn();
        }
    }

    private void UpdateTurnIndicators()
    {
        if (!aiTurnIndicator || !playerTurnIndicator) return;

        aiTurnIndicator.SetActive(GameMgr.Instance().IsPlayerTurn() == false);
        playerTurnIndicator.SetActive(GameMgr.Instance().IsPlayerTurn() == true);
    }

    public void UpdateSankShipUI(GridInfo lastShotInfo)
    {
        if (!aiLostShipsUI || !playerLostShipsUI) return;

        if (lastShotInfo.localPlayerOwned)
            playerLostShipsUI.ShowSankShip(lastShotInfo);
        else
            aiLostShipsUI.ShowSankShip(lastShotInfo);
    }       

    public void SetPlayerTarget(BattleshipGridEntry entry)
    {
        if (GameMgr.Instance().IsPlayerTurn() == false || GameMgr.Instance().IsPlayerFiring())
            return;
        
        // cancel old target.
        if (curTarget)
        {
            curTarget.CancelTarget();
        }

        // set NEW target.
        curTarget = entry;

        if (targetText)
            targetText.text = curTarget.GetDisplayName();

        // we have a target during our turn, allow button.
        fireButton.interactable = true;
    }

    public void OnFireBtnClicked()
    {
        // Shouldn't be able to get here without a target, but just in case.
        if (!curTarget) return;

        // reset button for next turn.
        GameMgr.Instance().SetPlayerFiring(true);
        if (fireButton) 
            fireButton.interactable = false;

        // Indicate "firing..."
        if (playerShotResultIndicator)
        {
            playerShotResultIndicator.gameObject.SetActive(true);
            playerShotResultIndicator.FireMsg();
        }

        // delay to showing fire result and gfx.
        StartCoroutine(CoRoFireActions());
    }

    public void AIFireAtTile(GridInfo aiShotInfo)
    {
        // kick off the AI fire decision and action.
        StartCoroutine(CoRoAITakeAim(aiShotInfo));
    }

    private BattleshipGridEntry AIFindAssociatedEntry(GridInfo aiDesiredShotInfo)
    {
        // Grab the entry with the same grid coordinates.
        BattleshipGridEntry[] entryKids = shipsGridObj.transform.GetComponentsInChildren<BattleshipGridEntry>();
        foreach (BattleshipGridEntry entry in entryKids)
        {
            if (entry.gridRow == aiDesiredShotInfo.row && entry.gridCol == aiDesiredShotInfo.col)
                return entry;
        }

        return null;
    }


    //************************************************************CO-ROUTINES***************************************************************************
    IEnumerator CoRoAITakeAim(GridInfo aiShotInfo)
    {
        // delay to have the illusion of thinking..
        yield return new WaitForSeconds(1.0f);

        // Mark entry as targeted.
        curTarget = AIFindAssociatedEntry(aiShotInfo);
        if (curTarget)
            curTarget.SetAITargeted();

        // Indicate "firing..."
        if (aiShotResultIndicator)
        {
            aiShotResultIndicator.gameObject.SetActive(true);
            aiShotResultIndicator.FireMsg();
        }

        // delay to showing fire result and gfx.
        StartCoroutine(CoRoFireActions());
    }
    IEnumerator CoRoFireActions()
    {
        // This function is called right after player/AI picks a target and chooses to fire on it.
        // Let's have a short delay before indicating fire result.
        yield return new WaitForSeconds(1.0f);

        // Fire on the tile and update the tile gfx.
        GridInfo curShotInfo = GameMgr.Instance().FireOnLocation(curTarget.gridRow, curTarget.gridCol, curTarget.localPlayerOwned);
        curTarget.UpdateFireResult(curShotInfo);

        if (curTarget.localPlayerOwned)
        {
            if (aiShotResultIndicator)
                aiShotResultIndicator.UpdateFireResult(curShotInfo);
        }
        else
        {
            if (playerShotResultIndicator)
                playerShotResultIndicator.UpdateFireResult(curShotInfo);
        }

        // Turn is over!
        StartCoroutine(CoRoSwitchTurns());
    }

    IEnumerator CoRoSwitchTurns()
    {
        // This function is called immediately after the player or AI has finished their turn and the miss/hit was indicated
        // so let's introduce a short delay before control switches to the other player.
        yield return new WaitForSeconds(0.75f);

        // Game mustn't be over for us to switch turns.
        if (GameMgr.Instance().CheckForGameOverState())
        {
            GameMgr.Instance().DoGameOver();
        }
        else
        {
            GameMgr.Instance().ChangeTurn();
        }        
    }    
}
