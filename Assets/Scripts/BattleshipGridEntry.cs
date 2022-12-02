using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BattleshipGridEntry : MonoBehaviour
    , IPointerEnterHandler
    , IPointerExitHandler    
    , IPointerClickHandler
{
    [SerializeField]
    private GameObject shipBG = null;
    [SerializeField]
    private GameObject targetBG = null;
    [SerializeField]
    private GameObject hitBG = null;
    [SerializeField]
    private GameObject missBG = null;

    public bool localPlayerOwned = false;
    public MainUIScreen screen = null;
    public short gridCol = -1;
    public short gridRow = -1;

    private bool hasShip = false;

    private char[] rowLetters = new char[] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J' };

    private bool targetLocked = false;

    // Start is called before the first frame update
    void Start()
    {
        hasShip = GameMgr.Instance().HasShip(gridRow, gridCol, localPlayerOwned);
        
        if (localPlayerOwned && shipBG)
            shipBG.SetActive(hasShip);
    }

    // Update is called once per frame
    void Update()
    {
        if (!localPlayerOwned && screen && shipBG)
        {
            shipBG.SetActive(screen.showAIShips && hasShip);
        }

        if (localPlayerOwned && shipBG)
        {   
            shipBG.SetActive(hasShip);
        }
    }

    public void RefreshLocalPlayerHasShip()
    {
        // value of hasShip can changed if re-randomizing ships on the Place Ship screen.
        hasShip = GameMgr.Instance().HasShip(gridRow, gridCol, localPlayerOwned);
    }

    public string GetDisplayName()
    {
        return $"{rowLetters[gridRow]}{gridCol+1}";
    }



    public void OnPointerEnter(PointerEventData eventData)
    {
        if (localPlayerOwned) return;
        if (GameMgr.Instance().IsGameOver()) return;

        if (targetBG && GameMgr.Instance().CanTileBeTargeted(gridRow, gridCol, false))
            targetBG.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (localPlayerOwned) return;
        if (GameMgr.Instance().IsGameOver()) return;

        if (targetBG && !targetLocked)
            targetBG.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (localPlayerOwned) return;
        if (GameMgr.Instance().IsPlayerTurn() == false) return;
        if (GameMgr.Instance().IsPlayerFiring()) return;
        if (GameMgr.Instance().IsGameOver()) return;

        if (!targetLocked && GameMgr.Instance().CanTileBeTargeted(gridRow, gridCol, false))
        {
            targetLocked = true;
            if (screen)
                screen.SetPlayerTarget(this);
        }
    }

    public void SetAITargeted()
    {
        targetLocked = true;
        if (targetBG)
            targetBG.SetActive(true);
    }

    public void CancelTarget()
    {
        targetLocked = false;
        if (targetBG)
            targetBG.SetActive(false);
    }

    public void UpdateFireResult(GridInfo shotInfo)
    {
        // remove target gfx.
        CancelTarget();
        
        // show hit/miss icon
        if (hitBG)
            hitBG.SetActive(shotInfo.hit);
        if (missBG)
            missBG.SetActive(shotInfo.miss);

    }
}
