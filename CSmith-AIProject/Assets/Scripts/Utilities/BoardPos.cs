using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public struct BoardPos
{
    public int x;
    public int y;

    public BoardPos(int _x, int _y)
    {
        x = _x;
        y = _y;
    }

    public static BoardPos operator +(BoardPos b1, BoardPos b2)
    {
        return new BoardPos(b1.x + b2.x, b1.y + b2.y);
    }

    public static BoardPos operator -(BoardPos b1, BoardPos b2)
    {
        return new BoardPos(b1.x - b2.x, b1.y - b2.y);
    }

    public static bool operator ==(BoardPos b1, BoardPos b2)
    {
        return (b1.x == b2.x && b1.y == b2.y);
    }

    public static bool operator !=(BoardPos b1, BoardPos b2)
    {
        return (!(b1 == b2));
    }

}
