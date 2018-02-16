using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class AiBehaviour {


    static public bool PerformTurn(Board _currentBoard, int _aiPlayer, out StoneMove _move)
    {
        _move = new StoneMove();
        List<StoneMove> possibleMoves = FindAllValidMoves(_currentBoard, _aiPlayer);
        if (possibleMoves.Count > 0)
        {
            StoneMove selectedMove = possibleMoves[0];
            float selectedMoveValue = -Mathf.Infinity;
            if (possibleMoves.Count > 1)
            {
                foreach (StoneMove m in possibleMoves)
                {
                    Board testBoard = _currentBoard.Clone();
                    testBoard.ResolveMove(m);
                    float moveValue = Search.AlphaBeta(new BoardNode(testBoard, _aiPlayer), 10, -Mathf.Infinity, Mathf.Infinity, true);

                    if (moveValue > selectedMoveValue)
                    {
                        selectedMove = m;
                        selectedMoveValue = moveValue;
                    }
                }
            }
            _move = selectedMove;
            return true;
        }
        else
            return false;
    }

  //  static public float CalculateHeuristic(Board _boardState, List<StoneMove> _possibleMoves)
  //  {
//
  //  }

    static public List<StoneMove> FindAllValidMoves(Board _board, int _activePlayer)
    {
        List<StoneMove> moves = new List<StoneMove>();
        bool captureFound = false;


        for (int x = 0; x < 4; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                //Magic formula to return grey tiles
                int k = x * 2 + (1 - (y % 2));
                TileState state = _board.state[k, y];

                if (state != TileState.Empty &&
                    (
                     (_activePlayer == 1 && (state == TileState.BlackKing || state == TileState.BlackPiece)) ||
                     (_activePlayer == 2 && (state == TileState.WhiteKing || state == TileState.WhitePiece))
                    )
                   )
                {
                    List<StoneMove> newMoves = FindValidMoves(_board, k, y);  //FindValidMoves(_board, k, j);

                    if (!captureFound)
                    {
                        for (int l = 0; l < newMoves.Count; l++)
                        {
                            if (newMoves[l].stoneCaptured && !captureFound)
                            {
                                captureFound = true;
                                moves.Clear();
                                break;
                            }
                        }
                    }

                    if (captureFound)
                    {
                        for (int l = 0; l < newMoves.Count; l++)
                        {
                            if (newMoves[l].stoneCaptured)
                                moves.Add(newMoves[l]);
                        }
                    }
                    else
                    {
                        moves.AddRange(newMoves);
                    }
                }
            }
        }
        return moves;
    }

    static public List<StoneMove> FindValidMoves(Board _board, int _startX, int _startY)
    {
        int owner;
        //Find the state of the current tile. Used to check ownership.
        TileState state = _board.state[_startX, _startY];

        //If black, owner = 1
        if (state == TileState.BlackKing || _board.state[_startX, _startY] == TileState.BlackPiece)
        {
            owner = 1;
        }
        //If white, owner = 2
        else if (state == TileState.WhiteKing || _board.state[_startX, _startY] == TileState.WhitePiece)
        {
            owner = 2;
        }
        //If neither then invalid tile
        else
        {
            return new List<StoneMove>();
        }

        List<StoneMove> moves;
        List<StoneMove> validMoves = new List<StoneMove>();
        BoardPos startPos = new BoardPos(_startX, _startY);

        //Blacks move up the board. Kings can also move up the board
        if (state == TileState.BlackPiece || state == TileState.BlackKing || state == TileState.WhiteKing)
        {
            if (TestMove(_board, owner, startPos, new BoardPos(startPos.x - 1, startPos.y - 1), out moves)) validMoves.AddRange(moves);
            if (TestMove(_board, owner, startPos, new BoardPos(startPos.x + 1, startPos.y - 1), out moves)) validMoves.AddRange(moves);
        }
        //Whites move down the board. Kings can also move down the board
        if (state == TileState.WhitePiece || state == TileState.BlackKing || state == TileState.WhiteKing)
        {
            if (TestMove(_board, owner, startPos, new BoardPos(startPos.x - 1, startPos.y + 1), out moves)) validMoves.AddRange(moves);
            if (TestMove(_board, owner, startPos, new BoardPos(startPos.x + 1, startPos.y + 1), out moves)) validMoves.AddRange(moves);
        }

        return validMoves;
    }

    static private bool TestMove(Board _board, int owner, BoardPos _startPos, BoardPos _movePos, out List<StoneMove> _moves)
    {
        _moves = new List<StoneMove>();

        //First ensure target position is within the bounds of the board.
        if (_movePos.x > 7 || _movePos.x < 0 || _movePos.y > 7 || _movePos.y < 0)
        {
            return false;
        }

        TileState targetState = _board.state[_movePos.x, _movePos.y];

        //If target tile is empty then move is allowed.
        if (targetState == TileState.Empty)
        {
            _moves.Add (new StoneMove(_startPos, _movePos, false, new List<BoardPos>()));
            return true;
        }

        //If target tile is occupied by a stone owned by the same player, move is not valid.
        if (owner == 1 && (targetState == TileState.BlackKing || targetState == TileState.BlackPiece) ||
            owner == 2 && (targetState == TileState.WhiteKing || targetState == TileState.WhitePiece))
        {
            return false;
        }

        //If we have gotten this far then the tile must be occupied by an enemy! Now we test if there is an unoccupied tile behind them
        BoardPos movePos = _movePos + (_movePos - _startPos);

        //Double check we're still within the board bounds.
        if (movePos.x > 7 || movePos.x < 0 || movePos.y > 7 || movePos.y < 0)
        {
            return false;
        }

        //TODO: THIS IS MESSY AS FUCK
        //Final check, if the tile beyond the enemy is empty then they are capturable! But we have to check for further captures that are possible
        if (_board.state[movePos.x, movePos.y] == TileState.Empty)
        {
            List<StoneMove> furtherMoves;
            if (TestFurtherMoves(_board, new StoneMove(_startPos, movePos, true, _movePos), out furtherMoves)) _moves.AddRange(furtherMoves);
            else _moves.Add(new StoneMove(_startPos, movePos, true, _movePos));
            return true;
        }

        //If we get this far then there are no more checks to do. It is not a valid move.
        return false;
    }

    //Tests for any further captures that are possible and returns list of all further necessary captures.
    static private bool TestFurtherMoves(Board _board, StoneMove _initialMove, out List<StoneMove> _foundMoves)
    {
        
        _foundMoves = new List<StoneMove>();

        Board tempBoard = _board.Clone();
        tempBoard.ResolveMove(_initialMove);

        List<StoneMove> furtherMoves = new List<StoneMove>();

        BoardPos newPos = _initialMove.endPos;

        TileState state = tempBoard.state[newPos.x, newPos.y];

        int currPlayer = tempBoard.GetOwner(newPos);
        int enemyPlayer = 0;

        if (currPlayer == 1) enemyPlayer = 2;
        else                 enemyPlayer = 1;


        List<StoneMove> tempMoves = new List<StoneMove>();
        List<BoardPos> tempCaps = new List<BoardPos>();
        bool moveFound = false;
        BoardPos enemyPos,nextPos;

        for (int i = -1; i <= 1; i += 2)
        {
            if (state == TileState.BlackKing || state == TileState.BlackPiece || state == TileState.WhiteKing)
            {
                enemyPos = new BoardPos(newPos.x + i, newPos.y - 1);
                nextPos = new BoardPos(enemyPos.x + i, enemyPos.y - 1);         
                if (tempBoard.GetOwner(enemyPos) == enemyPlayer && tempBoard.GetOwner(nextPos) == 0)
                {
                    moveFound = true;

                    tempCaps.Clear();
                    tempCaps.AddRange(_initialMove.capturedStones);
                    tempCaps.Add(enemyPos);
                    if (TestFurtherMoves(_board, new StoneMove(_initialMove.startPos, new BoardPos(enemyPos.x + i, enemyPos.y - 1), true, tempCaps), out furtherMoves))
                        _foundMoves.AddRange(furtherMoves);
                    else
                        _foundMoves.Add(new StoneMove(_initialMove.startPos, new BoardPos(enemyPos.x + i, enemyPos.y - 1), true, tempCaps));
                }
            }

            if (state == TileState.BlackKing || state == TileState.WhitePiece || state == TileState.WhiteKing)
            {
                enemyPos = new BoardPos(newPos.x + i, newPos.y + 1);
                nextPos = new BoardPos(enemyPos.x + i, enemyPos.y + 1);

                if (tempBoard.GetOwner(enemyPos) == enemyPlayer && tempBoard.GetOwner(nextPos) == 0)
                {
                    moveFound = true;

                    tempCaps.Clear();
                    tempCaps.AddRange(_initialMove.capturedStones);
                    tempCaps.Add(enemyPos);
                    if (TestFurtherMoves(_board, new StoneMove(_initialMove.startPos, new BoardPos(enemyPos.x + i, enemyPos.y + 1), true, tempCaps), out furtherMoves))   
                        _foundMoves.AddRange(furtherMoves);
                    else
                        _foundMoves.Add(new StoneMove(_initialMove.startPos, new BoardPos(enemyPos.x + i, enemyPos.y + 1), true, tempCaps));
                }
            }
        }

        if (!moveFound)
        {
            _foundMoves.Add(_initialMove);
            return false;
        }
        else
            return true;
    }
}
