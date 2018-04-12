using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Search {

    public static int iterations;

    static public float AlphaBeta(BoardNode _node, int _depth, float _alpha,float  _beta, bool _maximizingPlayer, ref NeuralNetwork net)
    {
        iterations++;

        float v;
        int activePlayer = _node.GetActivePlayer();
     

        if (_depth == 0 || _node.MoveCount() == 0)
        {
            FFData data;
            if (_maximizingPlayer)
            v = AiBehaviour.GetBoardRating(_node, activePlayer,out data, ref net);
            else
            v = AiBehaviour.GetBoardRating(_node,3 - activePlayer, out data, ref net);

            return v;
        }
        if (_maximizingPlayer)
        {

            v = -Mathf.Infinity;
            foreach (StoneMove b in _node.GetMoveList())
            {
                Board testBoard = _node.boardState.Clone();
                testBoard.ResolveMove(b);

                v = Mathf.Max(v, AlphaBeta(new BoardNode(testBoard,3 - activePlayer), _depth - 1, _alpha, _beta, false,ref net));
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

                v = Mathf.Min(v, AlphaBeta(new BoardNode(testBoard,3 - activePlayer), _depth - 1, _alpha, _beta, true,ref net));
                _beta = Mathf.Min(_beta, v);
                if (_beta <= _alpha)
                    break;
            }
            return v;
        }
    } 
}
