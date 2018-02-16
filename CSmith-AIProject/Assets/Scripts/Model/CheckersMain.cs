using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;




/// <summary>
/// Central class for checkers model.
/// </summary>
public class CheckersMain{

    PlayerType p1Type,p2Type;

    private List<StoneMove> validMoves;

    /// <summary>
    /// Ordered list of all boardstates prior to this one. All completed games are saved.
    /// TODO: Maybe optimise by storing only moves made instead of entire board state after every move.
    /// </summary>
    private List<Board> prevStates;

    private Board cachedBoardState;

    private int aiTurnDelay;

    /// <summary>
    /// The current board state.
    /// </summary>
    private Board boardState;

    /// <summary>
    /// The player that is currently taking their action (1 or 2);
    /// </summary>
    private int activePlayer;
    private int winner;

    /// <summary>
    /// Set to true when a turn has been completed. Tells the model to process the next turn.
    /// </summary>
    private bool turnComplete;

    /// <summary>
    /// Constructor. Initialises lists. Sets up events.
    /// </summary>
    public CheckersMain(PlayerType _p1Type, PlayerType _p2Type)
    {    
        EventManager.CreateEvent("turnOver");
        EventManager.CreateEvent("gameReset");
        EventManager.CreateEvent("gameOver");
        EventManager.CreateEvent("boardUpdated");

        p1Type = _p1Type;
        p2Type = _p2Type;
    }

    /// <summary>
    /// Generates initial board state and prepares for new game.
    /// </summary>
    /// <returns></returns>
    public bool Init()
    {
        prevStates = new List<Board>();
        validMoves = new List<StoneMove>();

        //initialise board (This will generate a board with initial game setup by default)
        boardState = new Board();
        cachedBoardState = boardState.Clone();
        activePlayer = 1;
        winner = 0;

        validMoves = GenerateValidMoveList(boardState, activePlayer);
        
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

            if ((activePlayer == 1 && p1Type == PlayerType.Human) || (activePlayer == 2 && p2Type == PlayerType.Human))
            {
                validMoves = GenerateValidMoveList(boardState, activePlayer);
            }
            else aiTurnDelay = 5;
       
            prevStates.Add(cachedBoardState.Clone());
            cachedBoardState = boardState.Clone();
            
            EventManager.TriggerEvent("turnOver");
        }

        if ((activePlayer == 1 && p1Type == PlayerType.AI) || (activePlayer == 2 && p2Type == PlayerType.AI))
        {
            if (aiTurnDelay > 0)
                aiTurnDelay -= 1;
            else
            {
                StoneMove chosenMove = new StoneMove();
                if (AiBehaviour.PerformTurn(boardState, activePlayer, out chosenMove))
                {
                    boardState.ResolveMove(chosenMove);
                    turnComplete = true;
                    EventManager.TriggerEvent("boardUpdated");
                }
                else
                {
                    Debug.Log("No possible moves found for AI");
                    turnComplete = true;
                }
            }
        }



        return true;
    }

    //TODO: Optimise
    static public List<StoneMove> GenerateValidMoveList(Board _board, int _activePlayer)
    {

        List<StoneMove> moves = new List<StoneMove>();
        bool captureFound = false;


        for (int i = 0; i < 35; i++)
        {

            TileState state = _board.state[i];

            if (state != TileState.Empty &&
                (
                 (_activePlayer == 1 && (state == TileState.BlackKing || state == TileState.BlackPiece)) ||
                 (_activePlayer == 2 && (state == TileState.WhiteKing || state == TileState.WhitePiece))
                )
               )
            {
                List<StoneMove> newMoves = FindValidMoves(_board, i);

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
        return moves;
    }

    public List<StoneMove> GetValidMoves(int _startPos)
    {
        List<StoneMove> returnList = new List<StoneMove>();
        int pos = _startPos;
        for (int i = 0; i < validMoves.Count; i++)
        {
            if(validMoves[i].startPos == pos)
            {
                returnList.Add(validMoves[i]);
            }
        }
        return returnList;
    }

    public List<StoneMove> GetAllValidMoves()
    {
        return validMoves;
    }

    public void AttemptMove(StoneMove _move)
    {
        if (validMoves.Contains(_move))
        {

            bool furtherMoves = false;
            List<StoneMove> moveCheck = new List<StoneMove>();

            boardState.ResolveMove(_move);

            if (_move.stoneCaptured)
            {
                foreach (int pos in _move.capturedStones)
                {
                    moveCheck = FindValidMoves(boardState, _move.endPos);
                    for (int i = 0; i < moveCheck.Count; i++)
                    {
                        if (moveCheck[i].stoneCaptured)
                            furtherMoves = true;
                    }
                }

                int blackRemaining = 0, whiteRemaining = 0;
                for (int i = 0; i < 35; i++)
                {

                        TileState s = boardState.state[i];

                        if (s == TileState.BlackKing || s == TileState.BlackPiece)
                            blackRemaining++;
                        else if (s == TileState.WhiteKing || s == TileState.WhitePiece)
                            whiteRemaining++;     
                }
                if (blackRemaining == 0)
                {
                    winner = 2;
                    EventManager.TriggerEvent("gameOver");
                }
                else if (whiteRemaining == 0)
                {
                    winner = 1;
                    EventManager.TriggerEvent("gameOver");
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

    static private List<StoneMove> FindValidMoves(Board _board, int _startPos)
    {
        int owner;
        //Find the state of the current tile. Used to check ownership.
        TileState state = _board.state[_startPos];

        //If black, owner = 1
        if (state == TileState.BlackKing || _board.state[_startPos] == TileState.BlackPiece)
        {
            owner = 1;
        }
        //If white, owner = 2
        else if (state == TileState.WhiteKing || _board.state[_startPos] == TileState.WhitePiece)
        {
            owner = 2;
        }
        //If neither then invalid tile has somehow been sent to this func
        else
        {
            Debug.LogError("Attempted to get valid moves for unoccupied tile:" + _startPos);
            return new List<StoneMove>();
        }

        StoneMove move;
        List<StoneMove> validMoves = new List<StoneMove>();

        //Blacks move up the board. Kings can also move up the board
        if (state == TileState.BlackPiece || state == TileState.BlackKing || state == TileState.WhiteKing)
        {
            if (TestMove(_board, owner, _startPos, _startPos+4, out move)) validMoves.Add(move.Clone());
            if (TestMove(_board, owner, _startPos, _startPos+5, out move)) validMoves.Add(move.Clone());
        }
        //Whites move down the board. Kings can also move down the board
        if (state == TileState.WhitePiece || state == TileState.BlackKing || state == TileState.WhiteKing)
        {
            if (TestMove(_board, owner, _startPos, _startPos - 4, out move)) validMoves.Add(move.Clone());
            if (TestMove(_board, owner, _startPos, _startPos - 5, out move)) validMoves.Add(move.Clone());
        }

        return validMoves;
    }

    /// <summary>
    /// TODO: Optimise?
    /// </summary>
    /// <param name="_owner"></param>
    /// <param name="_startPos"></param>
    /// <param name="_movePos"></param>
    /// <param name="_move"></param>
    /// <returns></returns>
    static private bool TestMove(Board _board, int _owner, int _startPos, int _movePos, out StoneMove _move)
    {
        //First ensure target position is within the bounds of the board.
        if (_movePos > 34 || _movePos < 0 || _movePos == 8 || _movePos == 17 || _movePos == 26)
        {
            _move = new StoneMove();
            return false;
        }

        TileState targetState = _board.state[_movePos];

        //If target tile is empty then move is allowed.
        if (targetState == TileState.Empty)
        {
            _move = new StoneMove(_startPos, _movePos, false, new List<int>());
            return true;
        }

        //If target tile is occupied by a stone owned by the same player, move is not valid.
        if (_owner == 1 && (targetState == TileState.BlackKing || targetState == TileState.BlackPiece) ||
            _owner == 2 && (targetState == TileState.WhiteKing || targetState == TileState.WhitePiece))
        {
            _move = new StoneMove();
            return false;
        }

        //If we have gotten this far then the tile must be occupied by an enemy! Now we test if there is an unoccupied tile behind them
        int movePos = _movePos + (_movePos - _startPos);

        //Double check we're still within the board bounds.
        if (movePos > 34 || movePos < 0 || movePos == 8 || movePos == 17 || movePos == 26)
        {
            _move = new StoneMove();
            return false;
        }

        //Final check, if the tile beyond the enemy is empty then they are capturable!
        if (_board.state[movePos] == TileState.Empty)
        {
            List<int> cappedStones = new List<int>();
            cappedStones.Add(_movePos);
            _move = new StoneMove(_startPos, movePos, true, cappedStones);
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

    public int GetWinner()
    {
        return winner;
    }

    public void Destroy()
    {

    }

}
