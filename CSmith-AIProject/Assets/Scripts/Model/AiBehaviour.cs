using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using System.Linq;
using System.Threading;

public class AiBehaviour {


    static public bool PerformTurn(Board _currentBoard, int _aiPlayer,bool _firstTurn, out StoneMove _move)
    {
 
        Search.iterations = 0;
        _move = new StoneMove();

        List<StoneMove> possibleMoves = FindAllValidMoves(_currentBoard, _aiPlayer);

        System.Random rnd = new System.Random();
        IEnumerable<StoneMove> shuffledList = possibleMoves.OrderBy(item => rnd.Next());

        UnityEngine.Debug.Log("Valid Moves:" + possibleMoves.Count);
        if (possibleMoves.Count > 0)
        {

            if (_firstTurn)
            {
                _move = shuffledList.First();
                return true;
            }

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            StoneMove selectedMove = possibleMoves[0];
            float selectedMoveValue = -Mathf.Infinity;
            if (possibleMoves.Count > 1)
            {
                foreach (StoneMove m in shuffledList)
                {
                    Board testBoard = _currentBoard.Clone();
                    testBoard.ResolveMove(m);
                    float moveValue = Search.AlphaBeta(new BoardNode(testBoard, _aiPlayer), 9, -Mathf.Infinity, Mathf.Infinity, true);

                    if (moveValue > selectedMoveValue)
                    {
                        selectedMove = m;
                        selectedMoveValue = moveValue;
                    }
                }
            }
            UnityEngine.Debug.Log("iterations: " + Search.iterations);
            stopWatch.Stop();
            UnityEngine.Debug.Log("Turn time:" + stopWatch.Elapsed);
            _move = selectedMove;
         
            return true;
        }
        else
            return false;
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
            if (!TestFurtherMoves(_board,_owner, new StoneMove(_startPos, endPos, true, _movePos), ref _moves)) //furtherMoves)) _moves.AddRange(furtherMoves);
                _moves.Add(new StoneMove(_startPos, endPos, true, _movePos));

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

        int enemyPlayer;

        if (_activePlayer == 1) enemyPlayer = 2;
        else                    enemyPlayer = 1;


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

    static public float EvaluateBoardState(BoardNode _node, int _activePlayer)
    {
        float value = 0;

        for (int i = 0; i < 35; i++)
        {
            int owner = _node.boardState.GetOwner(i);

            if (owner == 0 || owner == -1) continue;

            TileState state = _node.boardState.state[i];
            //Increase value for each piece owned by the player, decrease for each owned by the opponent

            float m = 1;

            if (owner != _activePlayer) m = -1;

            value += (1.1f * m);

            if (state == TileState.BlackKing || state == TileState.WhiteKing)
                value += (m * 1.2f);

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
