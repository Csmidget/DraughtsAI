using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Search {

    static public float AlphaBeta(BoardNode _node, int _depth, float _alpha,float  _beta, bool _maximizingPlayer)
    {
        
        //Generate heuristic
        int whiteCount = _node.boardState.GetNumWhite();
        int blackCount = _node.boardState.GetNumBlack();

        int activePlayer = _node.GetActivePlayer();

        float v = 0;

        if (activePlayer == 1)
        {
            v = blackCount - whiteCount;
        }
        else
            v = whiteCount - blackCount;



        if (_depth == 0 || _node.MoveCount() == 0)
        {
            return v;
        }

        if (activePlayer == 1) activePlayer = 2;
        else activePlayer = 1;

        if (_maximizingPlayer)
        {
            v = -Mathf.Infinity;
            foreach (StoneMove b in _node.GetMoveList())
            {
                Board testBoard = _node.boardState.Clone();
                testBoard.ResolveMove(b);
                v = Mathf.Max(v, AlphaBeta(new BoardNode(testBoard,activePlayer), _depth - 1, _alpha, _beta, false));
                _alpha = Mathf.Max(_alpha, v);
                if (_beta <= _alpha)
                    break;
            }
            return v;
        }
        else
        {
            v = +Mathf.Infinity;
            foreach (StoneMove b in _node.GetMoveList())
            {
                Board testBoard = _node.boardState.Clone();
                testBoard.ResolveMove(b);
                v = Mathf.Min(v, AlphaBeta(new BoardNode(testBoard, activePlayer), _depth - 1, _alpha, _beta, true));
                _beta = Mathf.Min(_beta, v);
                if (_beta <= _alpha)
                    break;
            }
            return v;
        }
    } 
}
