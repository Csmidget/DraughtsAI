using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct StoneMove
{
    public BoardPos startPos;
    public BoardPos endPos;
    public bool stoneCaptured;
    public BoardPos capturedStone;

    public StoneMove(BoardPos _startPos, BoardPos _endPos, bool _stoneCaptured, BoardPos _capturedStone)
    {
        startPos = _startPos;
        endPos = _endPos;
        stoneCaptured = _stoneCaptured;
        capturedStone = _capturedStone;
    }

    public StoneMove Clone()
    {
        return new StoneMove(startPos, endPos, stoneCaptured, capturedStone);
    }

    public static bool operator ==(StoneMove sm1, StoneMove sm2)
    {
        bool stoneCap = false;
        if (sm1.stoneCaptured == true && sm2.stoneCaptured == true && sm1.capturedStone != sm2.capturedStone)
            stoneCap = true;
        else if (sm1.stoneCaptured == false && sm2.stoneCaptured == false)
            stoneCap = true;
        
        return (sm1.startPos == sm2.startPos && sm1.endPos == sm2.endPos && stoneCap);
    }

    public static bool operator !=(StoneMove sm1, StoneMove sm2)
    {
        return (!(sm1 == sm2));
    }
}