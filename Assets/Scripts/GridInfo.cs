using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridInfo
{
    public short row = -1;
    public short col = -1;

    public bool localPlayerOwned = false;
    public bool hit = false;
    public bool miss = false;
    public bool ship = false;
    public bool sank = false;

    public int shipID = -1;
    public int shipLen = -1;
    public int shipPieceIndex = -1;
    public bool horizontalShip = true;

    public void Reset()
    {
        hit = false;
        miss = false;
        ship = false;
        sank = false;

        shipID = -1;
        shipLen = -1;
        shipPieceIndex = -1;
    }

    public bool CanBeTargeted()
    {
        return (!hit && !miss);
    }

    public bool FireOn()
    {
        if (ship)
        {
            hit = true;
            return true;
        }
        else
            miss = true;

        return false;
    }
}
