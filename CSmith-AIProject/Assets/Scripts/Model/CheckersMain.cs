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
    static public List<Board> prevStates;

    private Board cachedBoardState;

    private int aiTurnDelay;

    private bool firstTurn;

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

    //static public Board lastState;

    /// <summary>
    /// Constructor. Initialises lists. Sets up events.
    /// </summary>
    public CheckersMain(PlayerType _p1Type, PlayerType _p2Type)
    {    
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

        firstTurn = true;

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
            firstTurn = false;
            turnComplete = false;

            //Switch active player
            activePlayer = 3 - activePlayer;

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
            NeuralNetwork net = new NeuralNetwork();
            if (aiTurnDelay > 0)
                aiTurnDelay -= 1;
            else
            {
                StoneMove chosenMove = new StoneMove();
                
                if (AiBehaviour.PerformTurn(boardState, activePlayer,firstTurn, out chosenMove,ref net))
                {
                    boardState.ResolveMove(chosenMove);
                    turnComplete = true;
                    EventManager.TriggerEvent("boardUpdated");
                }
                else
                {
                    Debug.Log("No possible moves found for AI");
                    winner = 3 - activePlayer;
                    EventManager.TriggerEvent("gameOver");
                }
            }

            AiBehaviour.PrintCurrentBoardFeatures(new BoardNode(boardState, 3 - activePlayer), 3 - activePlayer,ref net);
        }



        return true;
    }

    //TODO: Optimise
    static public List<StoneMove> GenerateValidMoveList(Board _board, int _activePlayer)
    {

        //initialize return list
        List<StoneMove> moves = new List<StoneMove>();
        //
        bool firstCap = true;
        bool captureFound = false;

        for (int i = 0; i < 35; i++)
        {

            TileState state = _board.state[i];
            if (state != TileState.Empty && _activePlayer == _board.GetOwner(i))
            {
                //List<StoneMove> newMoves = FindValidMoves(_board, i,ref captureFound);
                FindValidMoves(_board, i, ref captureFound, ref firstCap, ref moves);
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

            bool captureFound = false;
            bool furtherCaps = true;

            List<StoneMove> moveCheck = new List<StoneMove>();

            boardState.ResolveMove(_move);

            if (_move.stoneCaptured)
            {
                foreach (int pos in _move.capturedStones)
                {
                    FindValidMoves(boardState, _move.endPos,ref captureFound,ref furtherCaps,ref moveCheck);
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

            if (captureFound)
            {
                validMoves = moveCheck;
            }
            else
            {
                turnComplete = true;
            }

            EventManager.TriggerEvent("boardUpdated");
        }
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

    /// <summary>
    /// TODO: Optimise?
    /// </summary>
    /// <param name="_owner"></param>
    /// <param name="_startPos"></param>
    /// <param name="_movePos"></param>
    /// <param name="_move"></param>
    /// <returns></returns>
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
            _moves.Add(new StoneMove(_startPos, _movePos, false, 0));
            return true;
        }

        //If we have gotten this far then the tile must be occupied by an enemy! Now we test if there is an unoccupied tile behind them
        int endPos = _movePos + (_movePos - _startPos);

        //Double check we're still within the board bounds.
        if (endPos > 34 || endPos < 0 || endPos == 8 || endPos == 17 || endPos == 26)
        {
            return false;
        }

        //Final check, if the tile beyond the enemy is empty then they are capturable!
        if (_board.state[endPos] == TileState.Empty)
        {
            _captureFound = true;
            if (_firstCap)
            {
                _moves.Clear();
                _firstCap = false;
            }
            _moves.Add(new StoneMove(_startPos, endPos, true, _movePos));
            return true;
        }

        //If we get this far then there are no more checks to do. It is not a valid move.
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
