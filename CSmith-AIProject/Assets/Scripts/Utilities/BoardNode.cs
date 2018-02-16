using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardNode
{
    public Board boardState;
    private List<StoneMove> validMoves;
    private int activePlayer;

    public BoardNode(Board _boardState, int _activePlayer)
    {
        boardState = _boardState.Clone();
        activePlayer = _activePlayer;

        validMoves = AiBehaviour.FindAllValidMoves(boardState, activePlayer);

    }

    public int MoveCount()
    {
        return validMoves.Count;
    }
    
    public List<StoneMove> GetMoveList()
    {
        List<StoneMove> moveList = new List<StoneMove>();
        moveList.AddRange(validMoves);
        return moveList;
    }

    public int GetActivePlayer()
    {
        return activePlayer;
    }

}
