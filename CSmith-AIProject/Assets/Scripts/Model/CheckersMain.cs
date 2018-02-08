using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;




/// <summary>
/// Central class for checkers model.
/// </summary>
public class CheckersMain{


    private List<StoneMove> validMoves;

    /// <summary>
    /// Ordered list of all boardstates prior to this one. All completed games are saved.
    /// TODO: Maybe optimise by storing only moves made instead of entire board state after every move.
    /// </summary>
    private List<Board> prevStates;

    private Board cachedBoardState;

    /// <summary>
    /// The current board state.
    /// </summary>
    private Board boardState;

    /// <summary>
    /// The player that is currently taking their action (1 or 2);
    /// </summary>
    private int activePlayer;

    /// <summary>
    /// Set to true when a turn has been completed. Tells the model to process the next turn.
    /// </summary>
    private bool turnComplete;

    /// <summary>
    /// Constructor. Initialises lists. Sets up events.
    /// </summary>
    public CheckersMain()
    {
        //initialise lists
        prevStates = new List<Board>();
        validMoves = new List<StoneMove>();
        EventManager.CreateEvent("turnOver");
        EventManager.CreateEvent("gameReset");
        EventManager.CreateEvent("boardUpdated");
    }

    /// <summary>
    /// Generates initial board state and prepares for new game.
    /// </summary>
    /// <returns></returns>
    public bool Init()
    {
        //initialise board (This will generate a board with initial game setup by default)
        boardState = new Board();
        cachedBoardState = boardState.Clone();
        activePlayer = 1;

        GenerateValidMoveList();
        
        EventManager.TriggerEvent("gameReset");
        
        return true;
    }

    /// <summary>
    /// Called from Game Manager, central update function of internal model.
    /// </summary>
    /// <returns></returns>
    public bool Update()
    {
        if (turnComplete)
        {
            turnComplete = false;

            //Switch active player
            if (activePlayer == 1)
                activePlayer = 2;
            else
                activePlayer = 1;

            GenerateValidMoveList();
            prevStates.Add(cachedBoardState.Clone());
            cachedBoardState = boardState.Clone();
            EventManager.TriggerEvent("turnOver");
        }

        return true;
    }

    //TODO: Optimise
    public void GenerateValidMoveList()
    {
        validMoves.Clear();
        bool captureFound = false;


        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                //Magic formula to return grey tiles
                int k = i * 2 + (1 - (j % 2));
                TileState state = boardState.state[k,j];

                if (state != TileState.Empty &&
                    (
                     (activePlayer == 1 && (state == TileState.BlackKing || state == TileState.BlackPiece)) ||
                     (activePlayer == 2 && (state == TileState.WhiteKing || state == TileState.WhitePiece))
                    )
                   )
                {
                    List<StoneMove> newMoves = FindValidMoves(k,j);

                    if (!captureFound)
                    {
                        for (int l = 0; l < newMoves.Count; l++)
                        {
                            if (newMoves[l].stoneCaptured && !captureFound)
                            {
                                captureFound = true;
                                validMoves.Clear();
                                break;
                            }
                        }
                    }

                    if (captureFound)
                    {
                        for (int l = 0; l < newMoves.Count; l++)
                        {
                            if (newMoves[l].stoneCaptured)
                                validMoves.Add(newMoves[l]);
                        }
                    }
                    else
                    {
                        validMoves.AddRange(newMoves);
                    }
                }
            }
        }
    }

    public List<StoneMove> GetValidMoves(int _startX, int _startY)
    {
        List<StoneMove> returnList = new List<StoneMove>();
        BoardPos pos = new BoardPos(_startX, _startY);
        for (int i = 0; i < validMoves.Count; i++)
        {
            if(validMoves[i].startPos == pos)
            {
                returnList.Add(validMoves[i]);
            }
        }
        return returnList;
    }

    public void AttemptMove(StoneMove _move)
    {
        if (validMoves.Contains(_move))
        {

            if (boardState.state[_move.startPos.x, _move.startPos.y] == TileState.BlackPiece && _move.endPos.y == 0)
            {
                boardState.state[_move.startPos.x, _move.startPos.y] = TileState.BlackKing;
            }
            else if (boardState.state[_move.startPos.x, _move.startPos.y] == TileState.WhitePiece && _move.endPos.y == 7)
            {
                boardState.state[_move.startPos.x, _move.startPos.y] = TileState.WhiteKing;
            }

            boardState.state[_move.endPos.x, _move.endPos.y] = boardState.state[_move.startPos.x, _move.startPos.y];
            boardState.state[_move.startPos.x, _move.startPos.y] = TileState.Empty;

            bool furtherMoves = false;
            List<StoneMove> moveCheck = new List<StoneMove>();          

            if (_move.stoneCaptured)
            {
                boardState.state[_move.capturedStone.x, _move.capturedStone.y] = TileState.Empty;
                moveCheck = FindValidMoves(_move.endPos.x, _move.endPos.y);
                for (int i = 0; i < moveCheck.Count; i++)
                {
                    if (moveCheck[i].stoneCaptured)
                        furtherMoves = true;
                }
            }

            if (furtherMoves)
            {
                for (int i = moveCheck.Count - 1; i >= 0; i--)
                {
                    if (moveCheck[i].stoneCaptured != true)
                        moveCheck.RemoveAt(i);
                }

                validMoves = moveCheck;

            }
            else
            {
                turnComplete = true;
            }

            EventManager.TriggerEvent("boardUpdated");
        }
    }

    private List<StoneMove> FindValidMoves(int _startX,int _startY)
    {
        int owner;
        //Find the state of the current tile. Used to check ownership.
        TileState state = boardState.state[_startX, _startY];

        //If black, owner = 1
        if (state == TileState.BlackKing || boardState.state[_startX, _startY] == TileState.BlackPiece)
        {
            owner = 1;
        }
        //If white, owner = 2
        else if (state == TileState.WhiteKing || boardState.state[_startX, _startY] == TileState.WhitePiece)
        {
            owner = 2;
        }
        //If neither then invalid tile has somehow been sent to this func
        else
        {
            Debug.LogError("Attempted to get valid moves for unoccupied tile:" + _startX + "," + _startY);
            return new List<StoneMove>();
        }

        StoneMove move;
        List<StoneMove> validMoves = new List<StoneMove>();
        BoardPos startPos = new BoardPos(_startX, _startY);

        //Blacks move up the board. Kings can also move up the board
        if (state == TileState.BlackPiece || state == TileState.BlackKing || state == TileState.WhiteKing)
        {
            if (TryMove(owner, startPos, new BoardPos(startPos.x-1, startPos.y - 1), out move)) validMoves.Add(move.Clone());
            if (TryMove(owner, startPos, new BoardPos(startPos.x+1, startPos.y - 1), out move)) validMoves.Add(move.Clone());
        }
        //Whites move down the board. Kings can also move down the board
        if (state == TileState.WhitePiece || state == TileState.BlackKing || state == TileState.WhiteKing)
        {
            if (TryMove(owner, startPos, new BoardPos(startPos.x - 1, startPos.y + 1), out move)) validMoves.Add(move.Clone());
            if (TryMove(owner, startPos, new BoardPos(startPos.x + 1, startPos.y + 1), out move)) validMoves.Add(move.Clone());
        }

        return validMoves;
    }

    /// <summary>
    /// TODO: Optimise?
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="_startPos"></param>
    /// <param name="_movePos"></param>
    /// <param name="_move"></param>
    /// <returns></returns>
    private bool TryMove(int owner, BoardPos _startPos, BoardPos _movePos, out StoneMove _move)
    {
        //First ensure target position is within the bounds of the board.
        if (_movePos.x > 7 || _movePos.x < 0 || _movePos.y > 7 || _movePos.y < 0)
        {
            _move = new StoneMove();
            return false;
        }

        TileState targetState = boardState.state[_movePos.x, _movePos.y];

        //If target tile is empty then move is allowed.
        if (targetState == TileState.Empty)
        {
            _move = new StoneMove(_startPos, _movePos, false, new BoardPos());
            return true;
        }

        //If target tile is occupied by a stone owned by the same player, move is not valid.
        if (owner == 1 && (targetState == TileState.BlackKing || targetState == TileState.BlackPiece) ||
            owner == 2 && (targetState == TileState.WhiteKing || targetState == TileState.WhitePiece))
        {
            _move = new StoneMove();
            return false;
        }

        //If we have gotten this far then the tile must be occupied by an enemy! Now we test if there is an unoccupied tile behind them
        BoardPos movePos = _movePos + (_movePos - _startPos);

        //Double check we're still within the board bounds.
        if (movePos.x > 7 || movePos.x < 0 || movePos.y > 7 || movePos.y < 0)
        {
            _move = new StoneMove();
            return false;
        }

        //Final check, if the tile beyond the enemy is empty then they are capturable!
        if (boardState.state[movePos.x,movePos.y] == TileState.Empty)
        {
            _move = new StoneMove(_startPos, movePos, true, _movePos);
            return true;
        }

        //If we get this far then there are no more checks to do. It is not a valid move.
        _move = new StoneMove();
        return false;
    }

    public int GetActivePlayer()
    {
        return activePlayer;
    }

    public Board GetBoardState()
    {
        return boardState.Clone();
    }


    public void Destroy()
    {

    }

}
