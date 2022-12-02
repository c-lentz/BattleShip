using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMgr : MonoBehaviour
{
    // ship data.
    private readonly int[] shipLengths = new int[] { 5, 4, 3, 3, 2 };
    private readonly string[] shipNames = new string[] { "Carrier", "Battleship", "Cruiser", "Submarine", "Destroyer" };


    // grid size.
    private const int MAX_ROW = 10;
    private const int MAX_COL = 10;

    // each player's grid, which has their ships.
    private GridInfo[,] localPlayerGrid = new GridInfo[10, 10];
    private GridInfo[,] aiPlayerGrid = new GridInfo[10, 10];

    // Current shot being taken by either local player or AI.
    private GridInfo curShotInfo;


    private bool gameHasStarted = false;
    private bool isPlayerTurn = true;
    private bool isPlayerFiring = false;
    private bool isGameOver = false;

    private GameStatTracker statTracker = new GameStatTracker();


    // Singleton instance.
    private static GameMgr _instance = null;

    // Singleton accessor.
    public static GameMgr Instance()
    {
        if (!_instance)
        {
            _instance = FindObjectOfType<GameMgr>();
        }
        return _instance;
    }

    // Start is called before the first frame update
    void Start()
    {
        // Create AI and Player grids.
        for (short col = 0; col < MAX_COL; col++)
        {
            for (short row = 0; row < MAX_ROW; row++)
            {
                GridInfo info = new GridInfo();
                info.row = row;
                info.col = col;
                info.localPlayerOwned = true;
                localPlayerGrid[row, col] = info;

                GridInfo aiInfo = new GridInfo();
                aiInfo.row = row;
                aiInfo.col = col;
                aiPlayerGrid[row, col] = aiInfo;
            }
        }

        
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameHasStarted)
        {
            gameHasStarted = true;
            StartGame();
        }
    }

    public void ResetForNewGame()
    {
        // clear stats.
        statTracker.Reset();

        curShotInfo = null;

        isGameOver = false;
        isPlayerTurn = true;
        isPlayerFiring = false;

        ClearShipGridInfo(localPlayerGrid);
        ClearShipGridInfo(aiPlayerGrid);

        // Pick starting ship locations.
        // Player will have the opportunity to change theirs.
        RandomizeShipLocations(aiPlayerGrid);
        RandomizeShipLocations(localPlayerGrid);
    }

    public bool CheckForGameOverState()
    {
        if (statTracker.GetSankShips(true) == GetMaxShipCount() || statTracker.GetSankShips(false) == GetMaxShipCount())
        {
            isGameOver = true;
        }

        return isGameOver;
    }

    public bool IsGameOver()
    {
        return isGameOver;
    }

    public void ChangeTurn()
    {
        isPlayerTurn = !isPlayerTurn;
        UIMgr.Instance().StartNewTurn();
    }

    public bool IsPlayerTurn()
    {
        return isPlayerTurn;
    }

    public void SetPlayerFiring(bool firing)
    {
        isPlayerFiring = firing;
    }

    public bool IsPlayerFiring()
    {
        return isPlayerFiring;
    }    

    public int GetMaxShipCount()
    {
        return shipLengths.Length;
    }

    public int[] GetShipLengths()
    {
        return shipLengths;
    }

    public string[] GetShipNames()
    {
        return shipNames;
    }

    public GameStatTracker GetStatTracker()
    {
        return statTracker;
    }

    private void StartGame()
    {
        UIMgr.Instance().ShowTitleScreen();
        ResetForNewGame();
    }

    public void DoGameOver()
    {
        // Sanity check.
        if (!isGameOver) return;

        // Do UI-related gameover stuff.
        UIMgr.Instance().DoGameOver();
    }

    private bool IsValidTileLocation(short row, short col)
    {
        if (row < 0 || row >= MAX_ROW) return false;
        if (col < 0 || col >= MAX_COL) return false;

        return true;
    }

    // Can this grid tile be targeted for a shot (ie. not already marked miss/hit)?
    public bool CanTileBeTargeted(short row, short col, bool playerGrid)
    {
        if (!IsValidTileLocation(row, col)) return false;

        if (playerGrid)
            return localPlayerGrid[row, col].CanBeTargeted();
        else
            return aiPlayerGrid[row, col].CanBeTargeted();
    }

    // Does this grid tile have a ship on it?
    public bool HasShip(short row, short col, bool playerGrid)
    {
        if (!IsValidTileLocation(row, col)) return false;

        if (playerGrid)
            return localPlayerGrid[row, col].ship;
        else
            return aiPlayerGrid[row, col].ship;
    }

    private GridInfo AIPickTarget()
    {
        // Get all the tiles that could be targeted.
        List<GridInfo> possibleMoves = new List<GridInfo>();
        for (short col = 0; col < MAX_COL; col++)
        {
            for (short row = 0; row < MAX_ROW; row++)
            {
                GridInfo curInfo = localPlayerGrid[row, col];
                if (curInfo.CanBeTargeted())
                    possibleMoves.Add(curInfo);
            }
        }

        // pick a random one.
        int moveIndex = Random.Range(0, possibleMoves.Count);
        return possibleMoves[moveIndex];
    }

    public void StartAITurn()
    {
        // pick a tile
        GridInfo aiShotInfo = AIPickTarget();

        // fire and update UI to show what happens, etc.
        // Also ends turn after the appropriate delays.
        UIMgr.Instance().AITakeTurn(aiShotInfo);
    }

    // Used by both player and AI, fire at the specified grid location.
    public GridInfo FireOnLocation(short row, short col, bool playerGrid)
    {
        if (!IsValidTileLocation(row, col)) return new GridInfo();

        curShotInfo = (playerGrid ? localPlayerGrid[row, col] : aiPlayerGrid[row, col]);
        curShotInfo.FireOn();

        if (DidCurShotSinkShip())
        {
            curShotInfo.sank = true;

            UIMgr.Instance().UpdateSankShipUI(curShotInfo);
        }

        statTracker.RecordShot(curShotInfo);

        return curShotInfo;
    }

    // Determine if curShot sank a ship.
    public bool DidCurShotSinkShip()
    {
        if (curShotInfo.ship && curShotInfo.hit)
        {
            GridInfo rootShipPiece = GetCurShotRootShipPiece();
            bool horizontalShip = rootShipPiece.horizontalShip;
                        
            // starting with root ship piece, check all pieces of this ship to see if they are all hit.
            for (int i = 0; i < rootShipPiece.shipLen; i++)
            {
                int curRow = rootShipPiece.row + (horizontalShip ? 0 : i);
                int curCol = rootShipPiece.col + (horizontalShip ? i : 0);

                if (curShotInfo.localPlayerOwned)
                {
                    if (localPlayerGrid[curRow, curCol].hit == false) 
                        return false;
                }
                else
                {
                    if (aiPlayerGrid[curRow, curCol].hit == false) 
                        return false;
                }
            }
            return true;
        }
        
        return false;
    }

    // Given curShotInfo, which has a ship, find the corresponding index=0 ship piece
    private GridInfo GetCurShotRootShipPiece()
    {
        GridInfo returnInfo = curShotInfo;
        while (returnInfo.shipPieceIndex != 0 && returnInfo.shipPieceIndex != -1)
        {
            // go backwards a tile.
            int newCol = returnInfo.col - (returnInfo.horizontalShip ? 1 : 0);
            int newRow = returnInfo.row - (returnInfo.horizontalShip ? 0 : 1);

            returnInfo = (curShotInfo.localPlayerOwned ? localPlayerGrid[newRow, newCol] : aiPlayerGrid[newRow, newCol]);
        }

        return returnInfo;
    }

    // Reset a grid tile, for example, so it no longer has a ship on it,
    // so that ship can be placed elsewhere.
    private void ClearShipGridInfo(GridInfo[,] grid)
    {
        for (short col = 0; col < MAX_COL; col++)
        {
            for (short row = 0; row < MAX_ROW; row++)
            {
                grid[row, col].Reset();
            }
        }
    }

    // public accessor to choosing new random locations for the LOCAL PLAYER's ships.
    public void RerandomizePlayerShips()
    {
        RandomizeShipLocations(localPlayerGrid);
    }

    // Do the actual work of choosing locations for player's (or AI's) ships.
    private void RandomizeShipLocations(GridInfo[,] grid)
    {
        // Clear grid info.
        ClearShipGridInfo(grid);

        // go thru all ship lengths to place them.
        for (int ship = 0; ship < shipLengths.Length; ship++)
        {
            // flip a coin to randomize horz/vert ship orientation for this ship
            bool horizontal = Random.Range(0, 2) == 0;

            // A ship of this length can only be placed so far to the right (if horz) or far down (if vert).
            int shipLen = shipLengths[ship]; 
            int rangeCap = (horizontal ? MAX_COL - shipLen : MAX_ROW - shipLen);

            int tileCol = 0;
            int tileRow = 0;
            bool found = false;
            int tryCounter = 0;

            // Only try 10 random locations.
            while (!found && tryCounter < 10)
            {
                tryCounter++;
                if (horizontal)
                {
                    tileCol = Random.Range(0, rangeCap+1);
                    tileRow = Random.Range(0, MAX_ROW);
                }
                else
                {
                    tileCol = Random.Range(0, MAX_COL);
                    tileRow = Random.Range(0, rangeCap+1);
                }
                found = CanFitShip(grid, tileRow, tileCol, shipLen, horizontal);
            }

            // Place the ship at the found location.
            if (found)
            {
                //Debug.Log($"placing shipLen {shipLen} at {tileRow}{tileCol} h {horizontal}, tryCounter = {tryCounter}");
                PlaceShip(grid, tileRow, tileCol, shipLen, ship, horizontal);
            }
            else
            {
                // result to brute force placement.
                BruteForceFindAndPlaceShip(grid, shipLen, ship);
            }
        }
    }

    // If randomly picking board locations doesn't work, call your old friend the Brute..
    private bool BruteForceFindAndPlaceShip(GridInfo[,] grid, int shipLen, int shipID)
    {
        // Literally just go through every possible tile until a valid location is found.
        for (short col = 0; col < MAX_COL; col++)
        {
            for (short row = 0; row < MAX_ROW; row++)
            {
                // try horizontal.
                if (CanFitShip(grid, row, col, shipLen, true))
                {
                    PlaceShip(grid, row, col, shipLen, shipID, true);
                    return true;
                }
                // try vertical.
                if (CanFitShip(grid, row, col, shipLen, false))
                {
                    PlaceShip(grid, row, col, shipLen, shipID, false);
                    return true;
                }
            }
        }

        return false;
    }

    private bool CanFitShip(GridInfo[,] grid, int row, int col, int shipLength, bool horizontalShip)
    {
        // check that current row/col is empty, plus all the tiles ship would need to take up in the
        // horizontal or vertical directions, based on ship length.
        for (int i = 0; i < shipLength; i++)
        {
            int curRow = row + (horizontalShip ? 0 : i);
            int curCol = col + (horizontalShip ? i : 0);

            if (curCol >= MAX_COL) return false;
            if (curRow >= MAX_ROW) return false;

            if (grid[curRow, curCol].ship)
                return false;
        }
        
        return true;
    }

    private bool PlaceShip(GridInfo[,] grid, int row, int col, int shipLength, int shipID, bool horizontalShip)
    {
        // mark current row/col as having this ship, plus all the tiles ship would need
        // to take up in the horizontal or vertical directions, based on ship length.
        for (int i = 0; i < shipLength; i++)
        {
            int curRow = row + (horizontalShip ? 0 : i);
            int curCol = col + (horizontalShip ? i : 0);

            if (curCol >= MAX_COL) return false;
            if (curRow >= MAX_ROW) return false;

            grid[curRow, curCol].ship = true;
            grid[curRow, curCol].shipID = shipID;
            grid[curRow, curCol].shipLen = shipLength;
            grid[curRow, curCol].shipPieceIndex = i;
            grid[curRow, curCol].horizontalShip = horizontalShip;
        }

        return true;
    }
}
