using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Search {

    public static int iterations;

    static public float TraverseNodeList(BoardNode _node, int _depth, float _alpha, float _beta, int _searchingPlayer, bool _maximizingPlayer,AI _player, float _accuracyMod)
    {
        float v;

        if (_depth == 0 || _node.MoveCount() == 0)
        {
            if (_maximizingPlayer)
            {
                v = (float)_node.GetValue(_player);
                if (_accuracyMod != 0)
                {
                    System.Random random = new MathNet.Numerics.Random.SystemRandomSource();
                    v = v + (float)random.NextDouble() * (2 * _accuracyMod) - _accuracyMod;
                }
                return v;
            }
            else
            {
                return AlphaBeta(_node, _depth, _alpha, _beta, _searchingPlayer, _maximizingPlayer, _player, _accuracyMod);
            }
        }

        if (!_node.IsEndNode())
        {
            if (_maximizingPlayer)
            {
                v = -Mathf.Infinity;
                foreach (BoardNode b in _node.GetChildren())
                {
                    v = Mathf.Max(v, TraverseNodeList(b, _depth - 1, _alpha, _beta,_searchingPlayer, false, _player, _accuracyMod));
                    _alpha = Mathf.Max(_alpha, v);
                    if (_beta <= _alpha)
                        break;
                }
                return v;
            }
            else
            {
                v = 1;
                foreach (BoardNode b in _node.GetChildren())
                {
                    v = Mathf.Min(v, TraverseNodeList(b, _depth - 1, _alpha, _beta, _searchingPlayer, true, _player, _accuracyMod));
                    _beta = Mathf.Min(_beta, v);
                    if (_beta <= _alpha)
                        break;
                }
                return v;
            }
        }
        else
        {
            return AlphaBeta(_node, _depth, _alpha, _beta, _searchingPlayer, _maximizingPlayer, _player, _accuracyMod);
        }
    }

    static public float AlphaBeta(BoardNode _node, int _depth, float _alpha,float  _beta,int _searchingPlayer, bool _maximizingPlayer,AI _player, float _accuracyMod)
    {
        iterations++;

        float v;

        if (_depth == 0 || _node.MoveCount() == 0)
        {
            if (_maximizingPlayer)
            {
                v = (float)_node.GetValue(_player);
                if (_accuracyMod != 0)
                {
                    System.Random random = new MathNet.Numerics.Random.SystemRandomSource();
                    v = v + (float)random.NextDouble() * (2 * _accuracyMod) - _accuracyMod;
                }
                return v;
            }
            else
            {          
                v = 1;

                foreach (StoneMove m in _node.GetMoveList())
                {
                    Board testBoard = _node.boardState.Clone();
                    testBoard.ResolveMove(m);
                    _node.AddChild(new BoardNode(testBoard, _searchingPlayer, m));
                }

                foreach (BoardNode b in _node.GetChildren())
                {
                    v = Mathf.Min(v, AlphaBeta(b, _depth - 1, _alpha, _beta, _searchingPlayer, true, _player, _accuracyMod));
                    _beta = Mathf.Min(_beta, v);
                    if (_beta <= _alpha)
                        break;
                }
                return v;
            }
        }

        if (_maximizingPlayer)
        {
            v = -Mathf.Infinity;

            foreach (StoneMove m in _node.GetMoveList())
            {
                Board testBoard = _node.boardState.Clone();
                testBoard.ResolveMove(m);
                _node.AddChild(new BoardNode(testBoard, 3 - _searchingPlayer, m));
            }

            foreach (BoardNode b in _node.GetChildren())
            {
                v = Mathf.Max(v, AlphaBeta(b, _depth - 1, _alpha, _beta, _searchingPlayer, false,_player, _accuracyMod));
                _alpha = Mathf.Max(_alpha, v);
                if (_beta <= _alpha)
                    break;
            }
            return v;
        }
        else
        {
            v = +Mathf.Infinity;

            foreach (StoneMove m in _node.GetMoveList())
            {
                Board testBoard = _node.boardState.Clone();
                testBoard.ResolveMove(m);
                _node.AddChild(new BoardNode(testBoard, _searchingPlayer, m));
            }

            foreach (BoardNode b in _node.GetChildren())
            {
                v = Mathf.Min(v, AlphaBeta(b, _depth - 1, _alpha, _beta,_searchingPlayer, true,_player, _accuracyMod));
                _beta = Mathf.Min(_beta, v);
                if (_beta <= _alpha)
                    break;
            }
            return v;
        }
    } 
}
