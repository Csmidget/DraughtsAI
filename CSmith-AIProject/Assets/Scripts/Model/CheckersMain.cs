using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Central class for checkers model.
/// </summary>
/// 

public class CheckersMain
{
    AI p1, p2;

    public bool gameActive;

    #region TournamentVariables
    //Tournament variables
    private bool tournament = false;
    int maxGames = 14;
    int p1Wins = 0;
    int p2Wins = 0;
    float p1LastAvgRatingDiff;
    float p2LastAvgRatingDiff;
    int presetFirstMove = -1;
    #endregion

    private int gamesComplete = 0;

    #region  TrainingVariables
    NeuralNetwork netBackup;

    private bool training = false;
    private int maxIterations = 1000;

    private int trainingWins = 0;
    private int controlWins = 0;
    private int controlUpdates = 0;

    private int totalTrainingWins = 0;
    private int totalControlWins = 0;

    int turnCount = 0;
    int maxTurnCount = 70;
    double alpha = 0.05; //learning rate
    double lambda = 0.85; //eligibility trace decay

    List<FFData> prevRuns;
    FFData currRun;
    #endregion

    //Defines which side player 1 is currently playing on (1 = black, 2 = white)
    int p1Side = 1;

    private List<StoneMove> validMoves;

    // Ordered list of all boardstates prior to this one. All completed games are saved.
    static public List<Board> prevStates;

    //True for first full turn of the game (2 Plys) used to randomize first move and to delay backprop whilst there is insufficient data
    private bool firstTurn;

    //flag indicating the current game has finished
    private bool matchOver;

    //True if no moves were found when attempting to perform AI move.
    private bool noMovesFound;

    // The current board state.
    private Board boardState;

    // The side that is currently taking their action (1 or 2);
    private int activeSide;

    //Which SIDE (not player) has won the game
    private int winner;

    // Set to true when a turn has been completed. Tells the model to process the next turn.
    private bool turnComplete;

    /// <summary>
    /// Generates initial board state and prepares for new game.
    /// </summary>
    /// <returns></returns>
    public bool InitTournament(PlayerType _p1Type, PlayerType _p2Type, int _p1SearchDepth, int _p2SearchDepth, string _p1nnFileName, string _p2nnFileName, int _gameCount)
    {
        tournament = true;
        SetupPlayers(_p1Type, _p2Type);
        p1.SetSearchDepth(_p1SearchDepth);
        p2.SetSearchDepth(_p2SearchDepth);
        maxGames = _gameCount;

        if (p1.GetType() != PlayerType.Human)
        {
            p1.SetNeuralNetwork(_p1nnFileName);
        }
        if (p2.GetType() != PlayerType.Human)
        {
            p2.SetNeuralNetwork(_p2nnFileName);
        }

        p1LastAvgRatingDiff = 0;
        p2LastAvgRatingDiff = 0;

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
        presetFirstMove = 0;
        EventManager.TriggerEvent("boardUpdated");
        EventManager.TriggerEvent("gameReset");
        return true;
    }

    /// <summary>
    /// Initialize the model in training mode
    /// </summary>
    /// <param name="_alpha">The learning rate of the neural network</param>
    /// <param name="_lambda">Eligibility trace decay</param>
    /// <param name="_maxIterations">Number of matches to play before stopping</param>
    /// <param name="_nnFileName">file name of neural network to train</param>
    /// <param name="_SearchDepth">size of search tree to use</param>
    public void InitTraining(double _alpha, double _lambda, int _maxIterations, string _nnFileName, int _SearchDepth)
    {
        p1LastAvgRatingDiff = 0;
        p2LastAvgRatingDiff = 0;
        SetupPlayers(PlayerType.AI, PlayerType.AI);
        alpha = _alpha;
        lambda = _lambda;
        p1.SetSearchDepth(_SearchDepth);
        p2.SetSearchDepth(_SearchDepth);
        maxIterations = _maxIterations;
        p1.SetNeuralNetwork(_nnFileName);
        InitNewGame();
        p2.SetNeuralNetwork(p1.GetNeuralNetwork());
        training = true;
        p1Side = 1;
        netBackup = p1.GetNeuralNetwork().Copy();
        turnComplete = false;
        presetFirstMove = 0;
        EventManager.TriggerEvent("gameReset");
    }

    /// <summary>
    /// Reset the board and variables for new match
    /// </summary>
    public void InitNewGame()
    {
        p1.NewGame();
        p2.NewGame();

        turnCount = 0;
        prevRuns = new List<FFData>();
        currRun = new FFData();
        prevStates = new List<Board>();
        matchOver = false;
        noMovesFound = false;
        turnComplete = false;
        firstTurn = true;

        //Reset elgibility trace 
        if (training)
            p1.ResetNNTraceValues();

        //initialise board (This will generate a board with initial game setup by default)
        boardState = new Board();
        activeSide = 1;

        //If player is human, generate valid move list
        if ((activeSide == p1Side && p1.GetType() == PlayerType.Human) || (activeSide == (3 - p1Side) && p2.GetType() == PlayerType.Human))
        {
            validMoves = GenerateValidMoveList(boardState, activeSide);
            if (validMoves.Count == 0)
            {
                turnComplete = true;
            }
        }
        EventManager.TriggerEvent("gameReset");
    }

    /// <summary>
    /// Called from Game Manager, central update function of internal model.
    /// </summary>
    /// <returns></returns>
    public bool Update()
    {

        if ((p1Side == activeSide && p1.GetType() != PlayerType.Human) || (3 - p1Side == activeSide && p2.GetType() != PlayerType.Human))
        {
            ProcessDynamicAI();
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
                Debug.Log("MATCH OVER");
                if (training && winner != 0)
                {
                    UpdateTraining();
                }
                else if (tournament)
                {
                    UpdateTournament();
                }
                EventManager.TriggerEvent("gameOver");
                InitNewGame();
            }
        }
        return true;
    }

    void EndTurn()
    {
        if (firstTurn && activeSide == 2) firstTurn = false;

        prevStates.Add(boardState.Clone());
        activeSide = 3 - activeSide;
        turnCount++;
        turnComplete = false;

        //If player is human, generate valid move list
        if ((activeSide == p1Side && p1.GetType() == PlayerType.Human) || (activeSide == (3 - p1Side) && p2.GetType() == PlayerType.Human))
        {
            validMoves = GenerateValidMoveList(boardState, activeSide);
            if (validMoves.Count == 0)
            {
                turnComplete = true;
            }
        }

        EventManager.TriggerEvent("boardUpdated");
        EventManager.TriggerEvent("turnOver");
    }

    /// <summary>
    /// Runs between games when in training mode, updates training variables. 
    /// </summary>
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

        netBackup = p1.GetNeuralNetwork().Copy();

        presetFirstMove++;
        if (presetFirstMove >= 7)
            presetFirstMove = 0;

        p1Side = 3 - p1Side;

        if (trainingWins >= 13 || controlWins > 7)
        {
            Debug.Log("trainingWins: " + trainingWins);
            Debug.Log("controlWins: " + controlWins);

            if (trainingWins >= 13)
            {
                //Savetofile
                System.DateTime sn = System.DateTime.Now;
                p1.SaveNetToFile("NetWeights\\" + sn.ToShortDateString().Replace('/', '-').Trim(' ') + '-' + sn.ToShortTimeString().Replace(':', '-'));
                p2.SetNeuralNetwork(p1.GetNeuralNetwork());
                controlUpdates++;
            }

            //Check win rate and save if improved
            trainingWins = 0;
            controlWins = 0;
        }
        if (gamesComplete >= maxIterations)
        {
            gameActive = false;
            EventManager.TriggerEvent("trainingComplete");
            System.DateTime sn = System.DateTime.Now;
            p1.SaveNetToFile("NetWeights\\" + sn.ToShortDateString().Replace('/', '-').Trim(' ') + '-' + sn.ToShortTimeString().Replace(':', '-') + "_TrainingEnd");
            training = false;
        }
    }

    /// <summary>
    /// Runs between games in tournament mode, updates tournament variables
    /// </summary>
    void UpdateTournament()
    {
        RecalculateAccuracyMod();

        p1.PrintAverageDifference();
        p2.PrintAverageDifference();

        p1LastAvgRatingDiff = p1.GetAverageDifference();
        p2LastAvgRatingDiff = p2.GetAverageDifference();

        if (p1Side == winner)
        {
            p1Wins++;
        }
        else if (3 - p1Side == winner)
        {
            p2Wins++;
        }

        gamesComplete++;
        p1Side = 3 - p1Side;

        if (p1.GetType() != PlayerType.ADRNG && p2.GetType() != PlayerType.ADRNG && p1.GetType() != PlayerType.ADRAS && p2.GetType() != PlayerType.ADRAS)
            presetFirstMove++;

        if (presetFirstMove >= 7)
            presetFirstMove = 0;

        if (gamesComplete >= maxGames)
        {
            gameActive = false;
            EventManager.TriggerEvent("boardUpdated");
            EventManager.TriggerEvent("tournamentComplete");
        }

    }

    /// <summary>
    /// Run any dynamic AI processing during the game
    /// </summary>
    private void ProcessDynamicAI()
    {
        if (activeSide == 1 && gamesComplete == 0 && firstTurn)
        {
            p1.ADRASInit(boardState, activeSide);
            p2.ADRASInit(boardState, 1 - activeSide);
        }

        if (activeSide == p1Side)
        {
            p1.ProcessDynamicAI(boardState, activeSide, true);
            p2.ProcessDynamicAI(boardState, activeSide, false);
        }
        else if (activeSide == 3 - p1Side)
        {
            p1.ProcessDynamicAI(boardState, activeSide, false);
            p2.ProcessDynamicAI(boardState, activeSide, true);
        }
    }

    /// <summary>
    /// Run any dynamic AI processing after each game
    /// </summary>
    private void RecalculateAccuracyMod()
    {
        float p1Acc = p1.RecalculateAccuracyMod();
        float p2Acc = p2.RecalculateAccuracyMod();

        if (p1Acc != 0)
            Debug.Log("P1 Accuracy Mod: " + p1Acc);
        if (p2Acc != 0)
            Debug.Log("P2 Accuracy Mod: " + p2Acc);
    }

    private void ProcessAITurn()
    {
        StoneMove chosenMove;

        if (activeSide == p1Side)
        {
            if (training)
            {
                p1.GetBoardRating(boardState, activeSide, out currRun);
                prevRuns.Add(currRun.Clone());
            }
        }

        PlayerType otherPlayer;
        bool moveFound = false;

        if (activeSide == p1Side)
        {
            otherPlayer = p2.GetType();
            moveFound = p1.PerformTurn(boardState, activeSide, otherPlayer, firstTurn, out chosenMove, presetFirstMove);
        }
        else
        {
            otherPlayer = p1.GetType();
            moveFound = p2.PerformTurn(boardState, activeSide, otherPlayer, firstTurn, out chosenMove, presetFirstMove);
        }

        if (moveFound)
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

    private void SetupPlayers(PlayerType _p1, PlayerType _p2)
    {
        switch (_p1)
        {
            case PlayerType.ADRAS:
                p1 = new ADRAS();
                break;
            case PlayerType.ADRNG:
                p1 = new ADRNG();
                break;
            case PlayerType.DROSAS:
                p1 = new DROSAS();
                break;
            case PlayerType.Human:
                p1 = new Human();
                break;
            default:
                p1 = new AI();
                break;
        }
        switch (_p2)
        {
            case PlayerType.ADRAS:
                p2 = new ADRAS();
                break;
            case PlayerType.ADRNG:
                p2 = new ADRNG();
                break;
            case PlayerType.DROSAS:
                p2 = new DROSAS();
                break;
            case PlayerType.Human:
                p2 = new Human();
                break;
            default:
                p2 = new AI();
                break;
        }
    }

    /// <summary>
    /// Checks the status of the match, tests for winner / draw
    /// </summary>
    public void CheckMatchStatus()
    {
        if (noMovesFound)
        {
            matchOver = true;
            winner = 3 - activeSide;
        }
        else if (turnCount == maxTurnCount)
        {
            winner = 0;
            matchOver = true;
        }
        else
        {
            matchOver = CheckForWinner(boardState, activeSide, out winner);
        }
    }

    static public bool CheckForWinner(Board _boardState, int _activeSide, out int _winner)
    {
        bool winnerFound = false;
        _winner = 0;

        //If, after the current players move, the opponent no longer has any pieces, player wins
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
                p1.TrainNet(prevRuns[prevRuns.Count - 2], prevRuns[prevRuns.Count - 1], alpha, lambda);
            }
        }
        else
        {
            if (!firstTurn && p1Side == activeSide)
                p1.TrainNet(prevRuns[prevRuns.Count - 2], prevRuns[prevRuns.Count - 1], alpha, lambda);
        }

        if (matchOver && winner == 0)
            p1.SetNeuralNetwork(netBackup);
    }

    /// <summary>
    /// Returns a list of all valid moves for the active side.
    /// </summary>
    /// <param name="_board"></param>
    /// <param name="_activeSide"></param>
    /// <returns></returns>
    public List<StoneMove> GenerateValidMoveList(Board _board, int _activeSide)
    {

        //initialize return list
        List<StoneMove> moves = new List<StoneMove>();

        bool firstCap = true;
        bool captureFound = false;

        for (int i = 0; i < 35; i++)
        {

            TileState state = _board.state[i];
            if (state != TileState.Empty && _activeSide == _board.GetOwner(i))
            {
                AI.FindValidMoves(_board, i, ref captureFound, ref firstCap, moves, false);
            }
        }
        return moves;
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
                    AI.FindValidMoves(boardState, _move.endPos, ref captureFound, ref furtherCaps, moveCheck, false);
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

    /// <summary>
    /// Reverts to the board state at the start of the previous turn
    /// </summary>
    public void RevertTurn()
    {
        if (prevStates != null && prevStates.Count > 2)
        {
            boardState = prevStates[prevStates.Count - 3].Clone();
            prevStates.RemoveRange(prevStates.Count - 2, 2);

            if ((activeSide == p1Side && p1.GetType() == PlayerType.Human) || (activeSide == (3 - p1Side) && p2.GetType() == PlayerType.Human))
            {
                validMoves = GenerateValidMoveList(boardState, activeSide);
                if (validMoves.Count == 0)
                {
                    turnComplete = true;
                }
            }

            EventManager.TriggerEvent("turnOver");
            EventManager.TriggerEvent("boardUpdated");
        }
    }

    #region Getters

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

    public float GetP1LastAvgRatingDiff()
    {
        return p1LastAvgRatingDiff;
    }

    public float GetP2LastAvgRatingDiff()
    {
        return p2LastAvgRatingDiff;
    }

    #endregion
}