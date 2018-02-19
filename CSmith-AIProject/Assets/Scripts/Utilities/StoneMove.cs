using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoneMove
{
    public int startPos;
    public int endPos;
    public bool stoneCaptured;
    public TileState endState;
    public List<int> capturedStones;

    public StoneMove()
    {

    }

    public StoneMove(int _startPos, int _endPos, bool _stoneCaptured, List<int> _capturedStones, TileState _endState = TileState.Empty)
    {
        startPos = _startPos;
        endPos = _endPos;
        stoneCaptured = _stoneCaptured;
        capturedStones = new List<int>();
        capturedStones.AddRange(_capturedStones);
        endState = _endState;
    }

    public StoneMove(int _startPos, int _endPos, bool _stoneCaptured, int _capturedStone, TileState _endState = TileState.Empty)
    {
        startPos = _startPos;
        endPos = _endPos;
        stoneCaptured = _stoneCaptured;
        capturedStones = new List<int>();
        capturedStones.Add(_capturedStone);
        endState = _endState;
    }

    public StoneMove Clone()
    {
        return new StoneMove(startPos, endPos, stoneCaptured, capturedStones);
    }

    public void AddCapture(int _cappedStone)
    {
        capturedStones.Add(_cappedStone);
    }

    public static bool operator ==(StoneMove sm1, StoneMove sm2)
    {
        bool stoneCap = false;
        if (sm1.stoneCaptured == true && sm2.stoneCaptured == true && sm1.capturedStones != sm2.capturedStones)
            stoneCap = true;
        else if (sm1.stoneCaptured == false && sm2.stoneCaptured == false)
            stoneCap = true;
        
        return (sm1.startPos == sm2.startPos && sm1.endPos == sm2.endPos && stoneCap && sm1.endState == sm2.endState);
    }

    public static bool operator !=(StoneMove sm1, StoneMove sm2)
    {
        return (!(sm1 == sm2));
    }
}