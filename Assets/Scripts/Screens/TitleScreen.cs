using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TitleScreen : Screen
{

    public void OnBtnClickStart()
    {
        GameMgr.Instance().ResetForNewGame();
        SetActive(false);
        UIMgr.Instance().ShowPlaceShipsScreen();
    }
}
