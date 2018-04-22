using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Search {

    public static int iterations;

    static public float AlphaBeta(BoardNode _node, int _depth, float _alpha,float  _beta,int _searchingPlayer, bool _maximizingPlayer, ref NeuralNetwork net)
    {
        iterations++;

        float v;

        if (_depth == 0 || _node.MoveCount() == 0)
        {
            if (_maximizingPlayer)
            {
                FFData data;
                v = AiBehaviour.GetBoardRating(_node, _searchingPlayer, out data, ref net);
                return v;
            }
            else
            {          
                v = +Mathf.Infinity;
                foreach (StoneMove b in _node.GetMoveList())
                {
                    Board testBoard = _node.boardState.Clone();
                    testBoard.ResolveMove(b);

                    v = Mathf.Min(v, AlphaBeta(new BoardNode(testBoard, _searchingPlayer), 0, _alpha, _beta, _searchingPlayer, true, ref net));
                    _beta = Mathf.Min(_beta, v);
                    if (_beta <= _alpha)
                        break;
                }
                return v;
            }
        }

        int activePlayer = _node.GetActivePlayer();

        if (_maximizingPlayer)
        {

            v = -Mathf.Infinity;
            foreach (StoneMove b in _node.GetMoveList())
            {
                Board testBoard = _node.boardState.Clone();
                testBoard.ResolveMove(b);

                v = Mathf.Max(v, AlphaBeta(new BoardNode(testBoard,3 - _searchingPlayer), _depth - 1, _alpha, _beta,_searchingPlayer, false,ref net));
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

                v = Mathf.Min(v, AlphaBeta(new BoardNode(testBoard,_searchingPlayer), _depth - 1, _alpha, _beta,_searchingPlayer, true,ref net));
                _beta = Mathf.Min(_beta, v);
                if (_beta <= _alpha)
                    break;
            }
            return v;
        }
    } 
}
