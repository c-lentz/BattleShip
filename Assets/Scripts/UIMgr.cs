using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMgr : MonoBehaviour
{
    // all the screens.
    private TitleScreen titleScreen = null;
    private PlaceShipsScreen placeShipsScreen = null;
    private MainUIScreen mainUIScreen = null;
    private GameOverScreen gameOverScreen = null;

    // singleton instance to self.
    private static UIMgr _instance = null;

    // singleton accessor.
    public static UIMgr Instance()
    {
        if (!_instance)
        {
            _instance = FindObjectOfType<UIMgr>();
        }
        return _instance;
    }

    // Start is called before the first frame update
    void Start()
    {
        titleScreen = FindObjectOfType<TitleScreen>(true);
        placeShipsScreen = FindObjectOfType<PlaceShipsScreen>(true);
        mainUIScreen = FindObjectOfType<MainUIScreen>(true);
        gameOverScreen = FindObjectOfType<GameOverScreen>(true);
    }

    public void UpdateSankShipUI(GridInfo lastShotInfo)
    {
        if (mainUIScreen)
            mainUIScreen.UpdateSankShipUI(lastShotInfo);
    }

    public void AITakeTurn(GridInfo aiShotInfo)
    {
        if (mainUIScreen)
            mainUIScreen.AIFireAtTile(aiShotInfo);
    }

    public void StartNewTurn()
    {
        if (mainUIScreen)
            mainUIScreen.StartNewTurn();
    }

    public void DoGameOver()
    {
        if (mainUIScreen)
            mainUIScreen.SetActive(false);

        if (gameOverScreen)
            gameOverScreen.SetActive(true);
    }

    public void ShowTitleScreen()
    {
        if (titleScreen)
            titleScreen.SetActive(true);
    }

    public void ShowPlaceShipsScreen()
    {
        if (placeShipsScreen)
            placeShipsScreen.SetActive(true);
    }

    public void ShowMainUI()
    {
        if (mainUIScreen) 
            mainUIScreen.SetActive(true);
    }
}
