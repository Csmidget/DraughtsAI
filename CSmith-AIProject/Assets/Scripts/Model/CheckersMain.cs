using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Central class for checkers model.
/// </summary>
public class CheckersMain
{
    PlayerType p1Type, p2Type;

    public bool gameActive;

    private NeuralNetwork p1NeuralNetwork;
    private NeuralNetwork p2NeuralNetwork;
    private int p1SearchDepth = 6;
    private int p2SearchDepth = 6;

    //Tournament variables
    private bool tournament = false;
    int maxGames = 15;
    int p1Wins = 0;
    int p2Wins = 0;
    //end of Tournament variables

    private int gamesComplete = 0;

    //Training variables
    NeuralNetwork netBackup;

    private bool training = false;
    private int maxIterations = 1000;
    private int gamesPerCheck = 20;

    private int trainingWins = 0;
    private int controlWins = 0;
    private int controlUpdates = 0;

    private int totalTrainingWins = 0;
    private int totalControlWins = 0;

    int turnCount = 0;
    int maxTurnCount = 70;
    double alpha = 0.05; //learning rate
    double lambda = 0.85; //arbitrary constant for partial derivatives of previous runs

    List<FFData> prevRuns;
    FFData currRun;
    //end of Training Variables

    //Defines which side player 1 is currently playing on (1 = black, 2 = white)
    int p1Side = 1;

    private List<StoneMove> validMoves;

    /// <summary>
    /// Ordered list of all boardstates prior to this one. All completed games are saved.
    /// TODO: Maybe optimise by storing only moves made instead of entire board state after every move.
    /// </summary>
    static public List<Board> prevStates;

    private float aiTurnDelay;

    //True for first full turn of the game (2 Plys) used to randomize first move and to delay backprop whilst there is insufficient data
    private bool firstTurn;
    private bool matchOver;

    //True if no moves were found when attempting to perform AI move.
    private bool noMovesFound;

    /// <summary>
    /// The current board state.
    /// </summary>
    private Board boardState;

    /// <summary>
    /// The side that is currently taking their action (1 or 2);
    /// </summary>
    private int activeSide;

    //Which SIDE (not player) has won the game
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
    public bool InitTournament(PlayerType _p1Type, PlayerType _p2Type, int _p1SearchDepth, int _p2SearchDepth, string _p1nnFileName, string _p2nnFileName)
    {
        tournament = true;
        p1Type = _p1Type;
        p2Type = _p2Type;
        p1SearchDepth = _p1SearchDepth;
        p2SearchDepth = _p2SearchDepth;

        if (p1Type == PlayerType.AI)
        {
            p1NeuralNetwork = new NeuralNetwork(_p1nnFileName);
        }
        if (p2Type == PlayerType.AI)
        {
            p2NeuralNetwork = new NeuralNetwork(_p2nnFileName);
        }

        training = false;
        prevStates = new List<Board>();
        validMoves = new List<StoneMove>();
        noMovesFound = false;
        firstTurn = true;
        matchOver = false;
        //initialise board (This will generate a board with initial game setup by default)
        boardState = new Board();
        activeSide = 1;
        winner = 0;
        turnComplete = false;
        validMoves = GenerateValidMoveList(boardState, activeSide);

        EventManager.TriggerEvent("boardUpdated");
        EventManager.TriggerEvent("gameReset");
        return true;
    }

    public void InitTraining(double _alpha, double _lambda, int _maxIterations, string _nnFileName, int _SearchDepth)
    {
        alpha = _alpha;
        lambda = _lambda;
        p1SearchDepth = _SearchDepth;
        p2SearchDepth = _SearchDepth;
        maxIterations = _maxIterations;
        p1NeuralNetwork = new NeuralNetwork(_nnFileName);
        InitNewGame();
        p2NeuralNetwork = p1NeuralNetwork.Copy();
        training = true;
        p1Side = 1;
        netBackup = p1NeuralNetwork.Copy();
        turnComplete = false;
        EventManager.TriggerEvent("gameReset");
    }

    public void InitNewGame()
    {
        turnCount = 0;
        prevRuns = new List<FFData>();
        currRun = new FFData();
        prevStates = new List<Board>();
        matchOver = false;
        noMovesFound = false;
        turnComplete = false;
        firstTurn = true;

        if (training)
            p1NeuralNetwork.ResetTraceValues();

        //initialise board (This will generate a board with initial game setup by default)
        boardState = new Board();
        activeSide = 1;

        //If player is human, generate valid move list
        if ((activeSide == p1Side && p1Type == PlayerType.Human) || (activeSide == (3 - p1Side) && p2Type == PlayerType.Human))
        {
            validMoves = GenerateValidMoveList(boardState, activeSide);
        }


        EventManager.TriggerEvent("gameReset");
    }

    /// <summary>
    /// Called from Game Manager, central update function of internal model.
    /// </summary>
    /// <returns></returns>
    public bool Update()
    {

        if (((activeSide == 1 && p1Type == PlayerType.AI) || (activeSide == 2 && p2Type == PlayerType.AI)) && aiTurnDelay <= 0)
        {
            ProcessAITurn();
        }

        if (turnComplete)
        {
            CheckMatchStatus();

            if (training)
            {
                TrainNetwork();
            }

            //Run end of turn logic
            if (!matchOver)
            {
                EndTurn();
            }
            else
            {
                if (training && winner != 0)
                {
                    UpdateTraining();
                }
                else if (tournament && winner != 0)
                {
                    UpdateTournament();
                }
                EventManager.TriggerEvent("gameOver");
                InitNewGame();
            }
        }

        aiTurnDelay -= Time.deltaTime;

        return true;
    }

    void EndTurn()
    {
        aiTurnDelay = 0.0f;

        if (firstTurn && activeSide == 2) firstTurn = false;

        prevStates.Add(boardState.Clone());
        activeSide = 3 - activeSide;
        turnCount++;
        turnComplete = false;

        //If player is human, generate valid move list
        if ((activeSide == p1Side && p1Type == PlayerType.Human) || (activeSide == (3 - p1Side) && p2Type == PlayerType.Human))
        {
            validMoves = GenerateValidMoveList(boardState, activeSide);
        }

        EventManager.TriggerEvent("boardUpdated");
        EventManager.TriggerEvent("turnOver");

    }

    void UpdateTraining()
    {
        if (p1Side == winner)
        {
            trainingWins++;
            totalTrainingWins++;
        }
        else if (3 - p1Side == winner)
        {
            controlWins++;
            totalControlWins++;
        }
        else
            return;

        gamesComplete++;

        netBackup = p1NeuralNetwork.Copy();

        p1Side = 3 - p1Side;

        if (gamesComplete % gamesPerCheck == 0)
        {
            Debug.Log("trainingWins: " + trainingWins);
            Debug.Log("controlWins: " + controlWins);

            if (trainingWins > controlWins + gamesPerCheck / 4)
            {
                //Savetofile
                System.DateTime sn = System.DateTime.Now;
                p1NeuralNetwork.SaveToFile("NetWeights\\" + sn.ToShortDateString().Replace('/', '-').Trim(' ') + '-' + sn.ToShortTimeString().Replace(':', '-'));
                p2NeuralNetwork = p1NeuralNetwork.Copy();
                controlUpdates++;
            }

            //check win rate and save if improved
            trainingWins = 0;
            controlWins = 0;
        }
        if (gamesComplete == maxIterations)
        {
            gameActive = false;
            EventManager.TriggerEvent("trainingComplete");
            System.DateTime sn = System.DateTime.Now;
                p1NeuralNetwork.SaveToFile("NetWeights\\" + sn.ToShortDateString().Replace('/', '-').Trim(' ') + '-' + sn.ToShortTimeString().Replace(':', '-') + "_TrainingEnd");
            training = false;
        }
    }

    void UpdateTournament()
    {
        if (p1Side == winner)
        {
            p1Wins++;         
        }
        else if (3 - p1Side == winner)
        {
            p2Wins++;
        }
        else
            return;

        gamesComplete++;
        p1Side = 3 - p1Side;

        if (p1Wins == 8 || p2Wins == 8)
        {
            gameActive = false;
            EventManager.TriggerEvent("tournamentComplete");
        }
    }

    private void ProcessAITurn()
    {
        StoneMove chosenMove;
        NeuralNetwork net;
        int searchDepth;



        if (activeSide == p1Side)
        {
            net = p1NeuralNetwork;
            searchDepth = p1SearchDepth;
            if (training)
            {
                AiBehaviour.GetBoardRating(new BoardNode(boardState, activeSide), activeSide, out currRun, ref p1NeuralNetwork);
                prevRuns.Add(currRun.Clone());
                Debug.Log("current board rating:" + currRun.a3[0, 0]);
            }
        }
        else
        {
            net = p2NeuralNetwork;
            searchDepth = p2SearchDepth;
        }

        if (AiBehaviour.PerformTurn(boardState, activeSide, firstTurn, out chosenMove, ref net, searchDepth))
        {
            if (chosenMove.stoneCaptured)
            {
                turnCount = 0;
            }
            boardState.ResolveMove(chosenMove);
        }
        else
        {
            Debug.Log("No possible moves found for AI. Assuming loss");
            noMovesFound = true;
        }
        turnComplete = true;
    }

    public void CheckMatchStatus()
    {
        if (noMovesFound)
        {
            matchOver = true;
            winner = 3 - activeSide;
        }
        else
        {
            matchOver = CheckForWinner(boardState,activeSide, out winner);

            if (!matchOver && turnCount == maxTurnCount)
            {
                winner = 0;
                matchOver = true;
            }
        }
    }

    static public bool CheckForWinner(Board _boardState, int _activeSide, out int _winner)
    {
        bool winnerFound = false;
        _winner = 0;
       
        if (_boardState.GetPieceCount(3 - _activeSide) == 0)
        {
            winnerFound = true;
            _winner = _activeSide;
        }
        //Special case for our version of draughts
        else if (_boardState.GetNumWhiteKings() >= _boardState.GetNumBlack())
        {
            if (_boardState.GetNumWhiteKings() == _boardState.GetNumBlackKings() && _boardState.GetNumWhiteStones() == 0)
            {
                _winner = 0;
            }
            else
            {
                _winner = 2;
            }
            winnerFound = true;
        }
        else if (_boardState.GetNumBlackKings() >= _boardState.GetNumWhite())
        {
            if (_boardState.GetNumWhiteKings() == _boardState.GetNumBlackKings() && _boardState.GetNumBlackStones() == 0)
            {
                _winner = 0;
            }
            else
            {
                _winner = 1;
            }
            winnerFound = true;
        }
        return winnerFound;
    }

    private void TrainNetwork()
    {

        if (matchOver)
        {
            if (winner != 0)
            {
                prevRuns[prevRuns.Count - 1].a3[0, 0] = 1 - Mathf.Abs(p1Side - winner);
                p1NeuralNetwork.BackPropagate(prevRuns.GetRange(0, prevRuns.Count - 1), prevRuns[prevRuns.Count - 1], alpha, lambda);
            }
        }
        else
        {
            if (!firstTurn && p1Side == activeSide)
                p1NeuralNetwork.BackPropagate(prevRuns.GetRange(0, prevRuns.Count - 1), prevRuns[prevRuns.Count - 1], alpha, lambda);
        }

        if (matchOver && winner == 0)
            p1NeuralNetwork = netBackup;
    }

    //TODO: Optimise
    static public List<StoneMove> GenerateValidMoveList(Board _board, int _activeSide)
    {

        //initialize return list
        List<StoneMove> moves = new List<StoneMove>();
        //
        bool firstCap = true;
        bool captureFound = false;

        for (int i = 0; i < 35; i++)
        {

            TileState state = _board.state[i];
            if (state != TileState.Empty && _activeSide == _board.GetOwner(i))
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
            if (validMoves[i].startPos == pos)
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
                    FindValidMoves(boardState, _move.endPos, ref captureFound, ref furtherCaps, ref moveCheck);
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

    static public void FindValidMoves(Board _board, int _startPos, ref bool _captureFound, ref bool _firstCap, ref List<StoneMove> _moves)
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

    public int GetActiveSide()
    {
        return activeSide;
    }

    public Board GetBoardState()
    {
        return boardState.Clone();
    }

    public int GetWinner()
    {
        return winner;
    }

    public int GetCurrentTrainingSide()
    {
        return p1Side;
    }

    public int GetTrainingWins()
    {
        return trainingWins;
    }
    public int GetControlWins()
    {
        return controlWins;
    }

    public int GetTotalTrainingWins()
    {
        return totalTrainingWins;
    }
    public int GetTotalControlWins()
    {
        return totalControlWins;
    }
    public int GetGamesComplete()
    {
        return gamesComplete;
    }

    public int GetControlUpdates()
    {
        return controlUpdates;
    }
    public int GetP1Wins()
    {
        return p1Wins;
    }
    public int GetP2Wins()
    {
        return p2Wins;
    }

    public int GetP1Side()
    {
        return p1Side;
    }

    public void Destroy()
    {

    }

}
