using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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

    public BoardNode(Board _boardState, int _activePlayer,bool _shuffleMoves)
    {
        boardState = _boardState.Clone();
        activePlayer = _activePlayer;

        validMoves = AiBehaviour.FindAllValidMoves(boardState, activePlayer);
        System.Random rnd = new System.Random();
        validMoves = validMoves.OrderBy(item => rnd.Next()).ToList();

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

    public int GetCapThreats(int _activePlayer)
    {
        int ret = 0;
        List<StoneMove> foundMoves = AiBehaviour.FindAllValidMoves(boardState, _activePlayer);
        foreach (StoneMove s in foundMoves)
        {
            if (s.stoneCaptured)
                ret += s.capturedStones.Count;

            Board newBoard = boardState.Clone();
            newBoard.ResolveMove(s);
            List<StoneMove> enemyMoves =  AiBehaviour.FindAllValidMoves(newBoard, 3 - _activePlayer);
            bool capped = false;
            foreach (StoneMove s2 in enemyMoves)
            {
                if (s2.stoneCaptured && s2.capturedStones.Contains(s.endPos))
                {
                    capped = true;
                }
            }

            if (!capped)
            {
                bool furtherCapFound = false;
                bool firstCap = true;
                List<StoneMove> furtherMoves = new List<StoneMove>();
                AiBehaviour.FindValidMoves(newBoard, s.endPos, ref furtherCapFound, ref firstCap, ref furtherMoves);

                if (furtherCapFound)
                {
                    foreach(StoneMove s2 in furtherMoves)
                    {
                        if (s2.stoneCaptured)
                            ret += s2.capturedStones.Count;
                    }
                }
            }

        }
        return ret;
    }

  //  public int GetEnemyCapThreats(int _enemyPlayer)
  //  {
  //      int ret = 0;
  //      foreach (StoneMove s in validMoves)
  //      {
  //          Board newBoard = boardState.Clone();
  //          newBoard.ResolveMove(s);
  //          List<StoneMove> enemyMoves = AiBehaviour.FindAllValidMoves(newBoard, 3 - _enemyPlayer);
  //          bool capped = false;
  //          foreach (StoneMove s2 in enemyMoves)
  //          {
  //              if (s2.stoneCaptured && s2.capturedStones.Contains(s.endPos))
  //              {
  //                  capped = true;
  //              }
  //          }
  //
  //          if (!capped)
  //          {
  //              bool furtherCapFound = false;
  //              bool firstCap = true;
  //              List<StoneMove> furtherMoves = new List<StoneMove>();
  //              AiBehaviour.FindValidMoves(newBoard, s.endPos, ref furtherCapFound, ref firstCap, ref furtherMoves);
  //
  //              if (furtherCapFound)
  //              {
  //                  foreach (StoneMove s2 in furtherMoves)
  //                  {
  //                      ret += s2.capturedStones.Count;
  //                  }
  //              }
  //          }
  //
  //      }
  //      return ret;
  //  }

    public int GetActivePlayer()
    {
        return activePlayer;
    }

}
