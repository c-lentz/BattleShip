using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOverScreen : Screen
{
    [SerializeField]
    private GameObject prefabEntry = null;
    [SerializeField]
    private GridLayoutGroup playerStatGrid = null;
    [SerializeField]
    private GridLayoutGroup aiStatGrid = null;
    [SerializeField]
    private GameObject playerWonBG = null;
    [SerializeField]
    private GameObject aiWonBG = null;


    private void OnEnable()
    {
        if (!playerStatGrid || !aiStatGrid)
            return;

        FillGrid(true);
        FillGrid(false);
        ShowWinnerIcon();
    }

    private void FillGrid(bool localPlayer)
    {
        GameStatTracker stats = GameMgr.Instance().GetStatTracker();
        Transform gridTransform = (localPlayer ? playerStatGrid.transform : aiStatGrid.transform);
        foreach (Transform child in gridTransform)
        {
            Destroy(child.gameObject);
        }

        GameObject shotObj = Instantiate(prefabEntry, gridTransform);
        GameOverResultsEntry shotEntry = shotObj.GetComponent<GameOverResultsEntry>();
        if (shotEntry)
            shotEntry.SetText("Shots:", stats.GetShots(localPlayer).ToString());

        GameObject hitObj = Instantiate(prefabEntry, gridTransform);
        GameOverResultsEntry hitEntry = hitObj.GetComponent<GameOverResultsEntry>();
        if (hitEntry)
            hitEntry.SetText("Hits:", stats.GetHits(localPlayer).ToString());

        GameObject missObj = Instantiate(prefabEntry, gridTransform);
        GameOverResultsEntry missEntry = missObj.GetComponent<GameOverResultsEntry>();
        if (missEntry)
            missEntry.SetText("Misses:", stats.GetMisses(localPlayer).ToString()); ;

        GameObject sankObj = Instantiate(prefabEntry, gridTransform);
        GameOverResultsEntry sankEntry = sankObj.GetComponent<GameOverResultsEntry>();
        if (sankEntry)
            sankEntry.SetText("Ships Sank:", stats.GetSankShips(localPlayer).ToString());
    }

    private void ShowWinnerIcon()
    {
        GameStatTracker stats = GameMgr.Instance().GetStatTracker();
        bool playerWon = stats.GetSankShips(true) > stats.GetSankShips(false);

        if (playerWonBG)
            playerWonBG.SetActive(playerWon);
        if (aiWonBG)
            aiWonBG.SetActive(!playerWon);
    }

    public void OnDoneBtnClicked()
    {
        this.SetActive(false);
        UIMgr.Instance().ShowTitleScreen();
    }
}
