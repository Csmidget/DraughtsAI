using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public struct StoneMove
{
    public BoardPos startPos;
    public BoardPos endPos;
    public bool     stoneCaptured;
    public BoardPos capturedStone;

    public StoneMove(BoardPos _startPos, BoardPos _endPos, bool _stoneCaptured, BoardPos _capturedStone)
    {
        startPos      = _startPos;
        endPos        = _endPos;
        stoneCaptured = _stoneCaptured;
        capturedStone = _capturedStone;
    }

    public StoneMove Clone()
    {
        return new StoneMove(startPos, endPos, stoneCaptured, capturedStone);
    }
}

public struct BoardPos
{
    public int x;
    public int y;

    public BoardPos(int _x, int _y)
    {
        x = _x;
        y = _y;
    }

    public static BoardPos operator +(BoardPos b1, BoardPos b2)
    {
        return new BoardPos(b1.x + b2.x, b1.y + b2.y);
    }

    public static BoardPos operator -(BoardPos b1, BoardPos b2)
    {
        return new BoardPos(b1.x - b2.x, b1.y - b2.y);
    }

}

/// <summary>
/// Central class for checkers model.
/// </summary>
public class CheckersMain{

    /// <summary>
    /// Ordered list of all boardstates prior to this one. All completed games are saved.
    /// TODO: Maybe optimise by storing only moves made instead of entire board state after every move.
    /// </summary>
    private List<Board> prevStates;

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
        activePlayer = 1;

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
            EventManager.TriggerEvent("turnOver");
        }

        return true;
    }

    public List<StoneMove> GetValidMoves(int _startX,int _startY)
    {
        int owner;

        TileState state = boardState.state[_startX, _startY];
        if (state == TileState.BlackKing || boardState.state[_startX, _startY] == TileState.BlackPiece)
        {
            owner = 1;
        }
        else if (state == TileState.WhiteKing || boardState.state[_startX, _startY] == TileState.WhitePiece)
        {
            owner = 2;
        }
        else
        {
            Debug.LogError("Attempted to get valid moves for unoccupied tile");
            return null;
        }

        StoneMove move;
        List<StoneMove> validMoves = new List<StoneMove>();
        BoardPos startPos = new BoardPos(_startX, _startY);

        //Blacks move up the board. Kings can also move up the board
        if (state == TileState.BlackPiece || state == TileState.BlackKing || state == TileState.WhiteKing)
        {
            if (TryMove(owner, startPos, new BoardPos(-1, -1), out move)) validMoves.Add(move.Clone());
            if (TryMove(owner, startPos, new BoardPos( 1, -1), out move)) validMoves.Add(move.Clone());
        }
        //Whites move down the board. Kings can also move down the board
        if (state == TileState.WhitePiece || state == TileState.BlackKing || state == TileState.WhiteKing)
        {
            if (TryMove(owner, startPos, new BoardPos(-1, 1), out move)) validMoves.Add(move.Clone());
            if (TryMove(owner, startPos, new BoardPos( 1, 1), out move)) validMoves.Add(move.Clone());
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
        if (_movePos.x > 8 || _movePos.x < 0 || _movePos.y > 8 || _movePos.y < 0)
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
        if (movePos.x > 8 || movePos.x < 0 || movePos.y > 8 || movePos.y < 0)
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
        return boardState;
    }


    public void Destroy()
    {

    }

}
