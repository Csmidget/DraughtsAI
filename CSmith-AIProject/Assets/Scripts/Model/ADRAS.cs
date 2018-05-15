﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

//Average Difference Randomized Action Selection
public class ADRAS : AI {

    protected  List<BoardNode> ADRASMoveList = new List<BoardNode>();

    public override bool PerformTurn(Board _currentBoard, int _aiPlayer, PlayerType otherPlayer, bool _firstTurn, out StoneMove _move, int presetFirstMove)
    {
        ADRASMoveList.Clear();

        if (boardNodes == null)
            boardNodes = new List<BoardNode>();

        _move = new StoneMove();

        List<StoneMove> possibleMoves = FindAllValidMoves(_currentBoard, _aiPlayer,true);

        System.Random rnd = new System.Random();
        List<StoneMove> shuffledList = possibleMoves.OrderBy(item => rnd.Next()).ToList();

        if (shuffledList.Count > 0)
        {

            StoneMove selectedMove = possibleMoves[0];
            float selectedMoveValue = -Mathf.Infinity;

            if (_firstTurn)
            {
                FFData temp;
                float boardVal = GetBoardRating(_currentBoard, _aiPlayer, out temp);
                InitBoardVal[_aiPlayer - 1] = boardVal;
                if (_aiPlayer == 1)
                {
                    if (presetFirstMove >= 0 && presetFirstMove < possibleMoves.Count)
                    {
                        _move = possibleMoves[presetFirstMove];
                    }
                    else
                        _move = shuffledList.First();
                    return true;
                }
            }
            else
            {
                bool existingNodeFound = false;
                BoardNode baseNode = new BoardNode(_currentBoard, _aiPlayer);

                if (boardNodes.Count > 0)
                {
                    for (int i = 0; i < boardNodes.Count; i++)
                    {
                        if (boardNodes[i].boardState == _currentBoard)
                        {
                            existingNodeFound = true;
                            baseNode = boardNodes[i];
                            break;
                        }
                    }
                }

                boardNodes = new List<BoardNode>();

                if (existingNodeFound && !baseNode.IsEndNode())
                {
                    boardNodes.AddRange(baseNode.GetChildren());
                }
                else
                {
                    foreach (StoneMove m in shuffledList)
                    {
                        Board testBoard = _currentBoard.Clone();
                        testBoard.ResolveMove(m);
                        BoardNode bn = new BoardNode(testBoard, 3 - _aiPlayer, m);
                        boardNodes.Add(bn);
                    }
                }

                foreach (BoardNode bn in boardNodes)
                {
                    bool prevStateFound = false;
                    foreach (Board b in CheckersMain.prevStates)
                    {
                        if (b == bn.boardState)
                        {
                            prevStateFound = true;
                        }
                    }

                    float moveValue = Mathf.NegativeInfinity;
                    if (!prevStateFound)
                    {
                            moveValue = Search.TraverseNodeList(bn, searchDepth - 1, Mathf.NegativeInfinity, Mathf.Infinity, _aiPlayer, false, this, 0);
                    }

                    if (moveValue >= 1 - accuracyMod)
                    {
                        ADRASMoveList.Add(bn);
                    }

                    if (moveValue > selectedMoveValue)
                    {
                        selectedMove = bn.GetMoveMade();
                        selectedMoveValue = moveValue;
                    }
                }

                if (otherPlayer == PlayerType.Human)
                {
                    List<BoardNode> newNodeList = new List<BoardNode>(0);
                    foreach (BoardNode bn in boardNodes)
                    {
                        if (!bn.IsEndNode())
                        {
                            newNodeList.AddRange(bn.GetChildren());
                        }
                    }
                    boardNodes = newNodeList;
                }
            }

            if (selectedMoveValue > 1 - accuracyMod)
            {
                selectedMove = ADRASMoveList.OrderBy(item => rnd.Next()).First().GetMoveMade();
            }

            _move = selectedMove;
            return true;
        }
        else
        {
            FFData data;
            GetBoardRating(_currentBoard, _aiPlayer, out data);
            return false;
        }
    }

    public override float RecalculateAccuracyMod()
    {
        float avDiff = FindAverageDiff(playerBoardRatings, enemyBoardRatings);
        if (avDiff < 0) avDiff = avDiff / 2;
        accuracyMod = Mathf.Max(0, (0.95f * accuracyMod) + 0.2f * avDiff);
        return accuracyMod;
    }

    public override void ADRASInit(Board _board, int _activeSide)
    {
        FFData temp;
        accuracyMod = 1 - GetBoardRating(_board, _activeSide, out temp);
    }

    public override void ProcessDynamicAI(Board _board,int _activeSide, bool _isActivePlayer)
    {
        FFData temp;

        float boardRating = GetBoardRating(_board, _activeSide, out temp);

        if (_isActivePlayer)
            playerBoardRatings.Add(boardRating);
        else
            enemyBoardRatings.Add(boardRating);
    }

    public override void PrintAverageDifference()
    {
        float avDiff = FindAverageDiff(playerBoardRatings, enemyBoardRatings);
        Debug.Log("AvDiff: " + avDiff);
    }

    public override PlayerType GetType()
    {
        return PlayerType.ADRAS;
    }
}
