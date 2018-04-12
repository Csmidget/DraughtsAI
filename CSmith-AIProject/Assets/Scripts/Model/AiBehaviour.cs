using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using System.Linq;
using System.Threading;

public class AiBehaviour {

    static public bool PerformTurn(Board _currentBoard, int _aiPlayer,bool _firstTurn, out StoneMove _move, ref NeuralNetwork net)
    {

        Search.iterations = 0;
        _move = new StoneMove();

        List<StoneMove> possibleMoves = FindAllValidMoves(_currentBoard, _aiPlayer);

        System.Random rnd = new System.Random();
        List<StoneMove> shuffledList = possibleMoves.OrderBy(item => rnd.Next()).ToList();

        if (shuffledList.Count > 0)
        {
           // Stopwatch stopWatch = new Stopwatch();
           // stopWatch.Start();

            StoneMove selectedMove = possibleMoves[0];
            float selectedMoveValue = -Mathf.Infinity;

            if (_firstTurn && _aiPlayer == 1)
            {
                _move = shuffledList.First();
                return true;
            }
            else
            {
                foreach (StoneMove m in shuffledList)
                {
                    Board testBoard = _currentBoard.Clone();
                    testBoard.ResolveMove(m);

                    bool prevStateFound = false;
                    foreach (Board b in CheckersMain.prevStates)
                    {
                        FFData data;
                        if (b == testBoard)
                        {
                            prevStateFound = true;
                        }
                    }

                    if (!prevStateFound)
                    {
                        float moveValue = Search.AlphaBeta(new BoardNode(testBoard, 3 - _aiPlayer), 6, Mathf.NegativeInfinity, Mathf.Infinity, false, ref net);
                        if (moveValue > selectedMoveValue)
                        {
                            selectedMove = m;
                            selectedMoveValue = moveValue;
                        }
                    }
                }
            }

           // UnityEngine.Debug.Log("Selected move with value: " + selectedMoveValue);
            // UnityEngine.Debug.Log("iterations: " + Search.iterations);
           // stopWatch.Stop();
            // UnityEngine.Debug.Log("Turn time:" + stopWatch.Elapsed);
            _move = selectedMove;
            return true;
        }
        else
        {
            FFData data;
            GetBoardRating(new BoardNode(_currentBoard, _aiPlayer), _aiPlayer, out data, ref net);
            return false;
        }
    }

    static public List<StoneMove> FindAllValidMoves(Board _board, int _activePlayer)
    {
        List<StoneMove> moves = new List<StoneMove>();
        bool firstCap = true;
        bool captureFound = false;

        //Test every tile on the board
        for (int i = 0; i < 35; i++)
        {
            if (_board.state[i] != TileState.Empty && _activePlayer == _board.GetOwner(i))
            {
                FindValidMoves(_board, i, ref captureFound, ref firstCap, ref moves);
            }
        }
        return moves;
    }

    static public void FindValidMoves(Board _board, int _startPos,ref bool _captureFound, ref bool _firstCap, ref List<StoneMove> _moves)
    {
        int owner = _board.GetOwner(_startPos);
        //Find the state of the current tile. Used to check ownership.
        TileState state = _board.state[_startPos];
        
        if (owner != 1 && owner != 2)
        {
            return;
        }

        //Blacks move up the board. Kings can also move up the board
        if (state == TileState.BlackPiece || state == TileState.BlackKing || state == TileState.WhiteKing)
        {
            TestMove(_board, owner, _startPos, _startPos + 4, ref _captureFound, ref _firstCap, ref _moves);
            TestMove(_board, owner, _startPos, _startPos + 5, ref _captureFound, ref _firstCap, ref _moves);
        }
        //Whites move down the board. Kings can also move down the board
        if (state == TileState.WhitePiece || state == TileState.BlackKing || state == TileState.WhiteKing)
        {
            TestMove(_board, owner, _startPos, _startPos - 4, ref _captureFound, ref _firstCap, ref _moves);
            TestMove(_board, owner, _startPos, _startPos - 5, ref _captureFound, ref _firstCap, ref _moves);
        }
    }

    static private bool TestMove(Board _board, int _owner, int _startPos, int _movePos, ref bool _captureFound, ref bool _firstCap, ref List<StoneMove> _moves)
    {

        //First ensure target position is within the bounds of the board.
        if (_movePos > 34 || _movePos < 0 || _movePos == 8 || _movePos == 17 || _movePos == 26)
        {
            return false;
        }

        TileState targetState = _board.state[_movePos];

        //If target tile is occupied by a stone owned by the same player, move is not valid.
        if (_owner == 1 && (targetState == TileState.BlackKing || targetState == TileState.BlackPiece) ||
            _owner == 2 && (targetState == TileState.WhiteKing || targetState == TileState.WhitePiece))
        {
            return false;
        }

        //If target tile is empty then move is allowed.
        if (targetState == TileState.Empty)
        {
            if (_captureFound)
                return false;
            _moves.Add (new StoneMove(_startPos, _movePos, false, 0));
            return true;
        }

        //If we have gotten this far then the tile must be occupied by an enemy! Now we test if there is an unoccupied tile behind them
        int endPos = _movePos + (_movePos - _startPos);

        //Double check we're still within the board bounds.
        if (endPos > 34 || endPos < 0 || endPos == 8 || endPos == 17 || endPos == 26)
        {
            return false;
        }

        //Final check, if the tile beyond the enemy is empty then they are capturable! But we have to check for further captures that are possible
        if (_board.state[endPos] == TileState.Empty)
        {
            _captureFound = true;
            if (_firstCap)
            {
                _moves.Clear();
                _firstCap = false;
            }
            StoneMove _move = new StoneMove(_startPos, endPos, true, _movePos);
            if (!TestFurtherMoves(_board,_owner, _move, ref _moves))
                _moves.Add(_move);

            return true;
        }

        //If we get this far then there are no more checks to do. It is not a valid move.
        return false;
    }

    //Tests for any further captures that are possible and returns list of all further necessary captures.
    static private bool TestFurtherMoves(Board _board,int _activePlayer, StoneMove _initialMove, ref List<StoneMove> _foundMoves)
    {

        Board tempBoard = _board.Clone();
        tempBoard.ResolveMove(_initialMove);

        int newPos = _initialMove.endPos;

        TileState state = tempBoard.state[newPos];

        int enemyPlayer = 3 - _activePlayer;

        List<int> tempCaps = new List<int>();
        bool moveFound = false;
        int enemyPos,nextPos;

        for (int i = 4; i <= 5; i ++)
        {
            if (state == TileState.BlackKing || state == TileState.BlackPiece || state == TileState.WhiteKing)
            {
                enemyPos = newPos + i;
                nextPos = enemyPos + i;         
                if (tempBoard.GetOwner(enemyPos) == enemyPlayer && tempBoard.GetOwner(nextPos) == 0)
                {
                    moveFound = true;

                    tempCaps.Clear();
                    tempCaps.AddRange(_initialMove.capturedStones);
                    tempCaps.Add(enemyPos);
                    StoneMove newMove = new StoneMove(_initialMove.startPos, nextPos, true, tempCaps, state);

                    if (!TestFurtherMoves(_board,_activePlayer, newMove, ref _foundMoves))
                        _foundMoves.Add(newMove);
                }
            }

            if (state == TileState.BlackKing || state == TileState.WhitePiece || state == TileState.WhiteKing)
            {
                enemyPos = newPos - i;
                nextPos  = enemyPos - i;

                if (tempBoard.GetOwner(enemyPos) == enemyPlayer && tempBoard.GetOwner(nextPos) == 0)
                {
                    moveFound = true;

                    tempCaps.Clear();
                    tempCaps.AddRange(_initialMove.capturedStones);
                    tempCaps.Add(enemyPos);
                    StoneMove newMove = new StoneMove(_initialMove.startPos, nextPos, true, tempCaps, state);

                    if (!TestFurtherMoves(_board,_activePlayer, newMove, ref _foundMoves))   
                        _foundMoves.Add(newMove);
                }
            }
        }

        if (!moveFound)
        {
            return false;
        }
        else
            return true;
    }

    static public float GetBoardRating(BoardNode _node, int _activePlayer,out FFData data, ref NeuralNetwork net)
    {
        //pieceAdvantage    : 0;
        //pieceDisadvantage : 1;
        //pieceThreat       : 2;
        //pieceTake         : 3;
        //doubleDiagonal    : 4;
        //backRowBridge     : 5;
        //centreControl     : 6;
        //kingCentreControl : 7;
        //stoneCount        : 8;
        //enemystoneCount   : 9;
        //kingCount         : 10;
        //enemyKingCount    : 11;
        double[] dataArray = new double[12];

        for (int i = 0; i < dataArray.Length; i++)
            dataArray[i] = 0;

        Board board = _node.boardState;

        int[] stoneCount = new int[2]; //0 = player 1, 1 = player 2;
        int[] kingCount = new int[2];

        //Test every tile on board
        for (int i = 0; i < 35; i++)
        {
            int owner = board.GetOwner(i);

            //If tile is empty or invalid move on.
            if (owner == 0 || owner == -1) continue;

            TileState state = board.state[i];

            //Increment piece count for owned player
            stoneCount[owner - 1] += 1;

                    
            if (state == TileState.BlackKing || state == TileState.WhiteKing)
            {
                //Increment owner piece count slightly if king
                kingCount[owner - 1] += 1;
            }
           
            if (owner == _activePlayer)
            {
                //If any centre tiles are occupied by player, add to CentreControl;
                switch (i)
                {
                    case 10:
                    case 11:
                    case 14:
                    case 15:
                    case 19:
                    case 20:
                    case 23:
                    case 24:
                        dataArray[6]++;
                        if (state == TileState.WhiteKing || state == TileState.BlackKing) dataArray[7]+= 1;
                        break;
                }
                //double diagonal tiles
                switch (i)       
                {
                    case 30:
                    case 34:
                    case 25:
                    case 29:
                    case 20:
                    case 24:
                    case 19:
                    case 15:
                    case 10:
                    case 14:
                    case 5:
                    case 9:
                    case 0:
                    case 4:
                        dataArray[4] += 1;
                        break;
                }

            }
        }

        if (_activePlayer == 1)
        {
            if ((board.state[0] == TileState.BlackKing || board.state[0] == TileState.BlackPiece) && (board.state[2] == TileState.BlackKing || board.state[2] == TileState.BlackPiece))
                dataArray[5] = 1;
        }
        else
        {
            if ((board.state[34] == TileState.WhiteKing || board.state[34] == TileState.WhitePiece) && (board.state[32] == TileState.WhiteKing || board.state[32] == TileState.WhitePiece))
                dataArray[5] = 1;
        }


        dataArray[2] = _node.GetCapThreats(3 - _activePlayer);
        dataArray[3] = _node.GetCapThreats(_activePlayer);
       

        dataArray[0] = (stoneCount[_activePlayer - 1] + kingCount[_activePlayer - 1]) - (stoneCount[2 - _activePlayer] + kingCount[2 - _activePlayer]);
        if (dataArray[0] < 0) dataArray[0] = 0;

        dataArray[1] = (stoneCount[2 - _activePlayer] + kingCount[2 - _activePlayer]) - (stoneCount[_activePlayer - 1] + kingCount[_activePlayer - 1]);
        if (dataArray[1] < 0) dataArray[1] = 0;

        dataArray[8] = stoneCount[_activePlayer - 1];
        dataArray[9] = stoneCount[2 - _activePlayer];
        dataArray[10] = kingCount[_activePlayer - 1];
        dataArray[11] = kingCount[2 - _activePlayer];

        data = net.FeedForward(dataArray);

        

        //If player 1 wins
        if (stoneCount[_activePlayer - 1] == 0)
        {
            data.a3[0,0] = 1;
        }
        else if (stoneCount[2- _activePlayer] == 0)
        {
            data.a3[0,0] = 0;
        }
        
        return (float)data.a3[0, 0];
    }

    static public void PrintCurrentBoardFeatures(BoardNode _node, int _activePlayer, ref NeuralNetwork net)
    {
        FFData data;

        GetBoardRating(_node, _activePlayer, out data, ref net);

        UnityEngine.Debug.Log("pieceAdvantage: " + data.input[0]);
        UnityEngine.Debug.Log("pieceDisadvantage: " + data.input[1]);
        UnityEngine.Debug.Log("pieceThreat: " + data.input[2]);
        UnityEngine.Debug.Log("pieceTake: " + data.input[3]);
        UnityEngine.Debug.Log("doubleDiagonal: " + data.input[4]);
        UnityEngine.Debug.Log("backRowBridge: " + data.input[5]);
        UnityEngine.Debug.Log("centreControl: " + data.input[6]);
        UnityEngine.Debug.Log("kingCentreControl: " + data.input[7]);
        UnityEngine.Debug.Log("stoneCount: " + data.input[8]);
        UnityEngine.Debug.Log("enemystoneCount: " + data.input[9]);
        UnityEngine.Debug.Log("kingCount: " + data.input[10]);
        UnityEngine.Debug.Log("enemyKingCount: " + data.input[11]);
    }

    static public float EvaluateBoardState(BoardNode _node, int _activePlayer)
    {
        float value = 0;

        //Test every tile on board
        for (int i = 0; i < 35; i++)
        {
            int owner = _node.boardState.GetOwner(i);

            //If tile is empty or invalid move on.
            if (owner == 0 || owner == -1) continue;


            TileState state = _node.boardState.state[i];


            //Increase value for each piece owned by the player, decrease for each owned by the opponent

            //m is the base modifier (1 for current players piece, -1 for enemy piece)
            float m = 1;
            if (owner != _activePlayer)
            {
                m = -1;
                value += -1.09f;
            }
            else
                value += 1.1f;

            //If the piece is a king increase the value of the piece.
            if (state == TileState.BlackKing || state == TileState.WhiteKing)
                value += (m * 0.4f);

            //If the piece is in the centre increase/decrease value
            if (i == 15 || i == 16 || i == 20 || i == 21)
            {
                value += m * 0.09f;
            }

            int opponent;
            if (owner == 1) opponent = 2;
            else opponent = 1;

            for (int j = 4; j <= 5; j++)
            {
                if (state == TileState.BlackKing || state == TileState.BlackPiece || state == TileState.WhiteKing)
                {
                    int enemyPos = i + j;
                    int nextPos = enemyPos + j;
                    if (_node.boardState.GetOwner(enemyPos) == opponent && _node.boardState.GetOwner(nextPos) == 0)
                    {
                        value += m;
                        List<StoneMove> foundMoves = new List<StoneMove>();

                        TestFurtherMoves(_node.boardState, owner, new StoneMove(i, nextPos, true, enemyPos),ref foundMoves);
                        value += foundMoves.Count;       
                    }
                }
                if (state == TileState.BlackKing || state == TileState.WhitePiece || state == TileState.WhiteKing)
                {
                    int enemyPos = i - j;
                    int nextPos = enemyPos - j;
                    if (_node.boardState.GetOwner(enemyPos) == opponent && _node.boardState.GetOwner(nextPos) == 0)
                    {
                        value += m;
                        List<StoneMove> foundMoves = new List<StoneMove>();

                        TestFurtherMoves(_node.boardState, owner, new StoneMove(i, nextPos, true, enemyPos), ref foundMoves);
                        value += foundMoves.Count;
                    }
                }
            }
        }

        return value;


    }

}
