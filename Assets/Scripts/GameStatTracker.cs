using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStatTracker
{
    private int playerShots = 0;
    private int playerHits = 0;
    private int playerMisses = 0;
    private int playerSankShips = 0;

    private int aiShots = 0;
    private int aiHits = 0;
    private int aiMisses = 0;
    private int aiSankShips = 0;

    public void Reset()
    {
        playerShots = 0;
        playerHits = 0;
        playerMisses = 0;
        playerSankShips = 0;

        aiShots = 0;
        aiHits = 0;
        aiMisses = 0;
        aiSankShips = 0;
    }

    public void RecordShot(GridInfo curShotInfo)
    {
        // if grid is owned by the local player, that means it was an AI shot.
        if (curShotInfo.localPlayerOwned)
        {
            aiShots++;
            aiHits += curShotInfo.hit ? 1 : 0;
            aiMisses += curShotInfo.miss ? 1 : 0;
            aiSankShips += curShotInfo.sank ? 1 : 0;
        }
        else
        {
            playerShots++;
            playerHits += curShotInfo.hit ? 1 : 0;
            playerMisses += curShotInfo.miss ? 1 : 0;
            playerSankShips += curShotInfo.sank ? 1 : 0;
        }
    }

    public int GetShots(bool localPlayer)
    {
        return localPlayer ? playerShots : aiShots;
    }

    public int GetHits(bool localPlayer)
    {
        return localPlayer ? playerHits : aiHits;
    }

    public int GetMisses(bool localPlayer)
    {
        return localPlayer ? playerMisses : aiMisses;
    }

    public int GetSankShips(bool localPlayer)
    {
        return localPlayer ? playerSankShips: aiSankShips;
    }
}
