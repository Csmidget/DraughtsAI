using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Search {

    public static int iterations;

    static public float AlphaBeta(BoardNode _node, int _depth, float _alpha,float  _beta, bool _maximizingPlayer)
    {
        
        iterations++;
        
        int activePlayer = _node.GetActivePlayer();

        float v;

        if (CheckersMain.prevStates.Contains(_node.boardState)) return -Mathf.Infinity;

        if (_depth == 0 || _node.MoveCount() == 0)
        {
            //Generate heuristic
            v = AiBehaviour.EvaluateBoardState(_node, activePlayer);
            return v;
        }


        if (_maximizingPlayer)
        {
            v = -Mathf.Infinity;
            foreach (StoneMove b in _node.GetMoveList())
            {
                Board testBoard = _node.boardState.Clone();
                testBoard.ResolveMove(b);

                if (activePlayer == 1) activePlayer = 2;
                else activePlayer = 1;

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

                if (activePlayer == 1) activePlayer = 2;
                else activePlayer = 1;

                v = Mathf.Min(v, AlphaBeta(new BoardNode(testBoard, activePlayer), _depth - 1, _alpha, _beta, true));
                _beta = Mathf.Min(_beta, v);
                if (_beta <= _alpha)
                    break;
            }
            return v;
        }
    } 
}
