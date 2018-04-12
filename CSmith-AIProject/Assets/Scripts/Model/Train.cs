using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Train {

    int maxIterations = 1000;
    int gamesPerCheck = 20;
    int trainingNetSide; //1 = black, 2 = white

    int gamesComplete;
    int trainingWins = 0;
    int controlWins = 0;
    int controlUpdates = 0;

    int totalTrainingWins = 0;
    int totalControlWins = 0;

    Board boardState;

    int activePlayer;
    int turnCount = 0;
    int maxTurnCount = 70;
    double alpha = 0.05; //learning rate
    double lambda = 0.85; //arbitrary constant for partial derivatives of previous runs
    bool firstTurn;
    bool trainingActive;

    NeuralNetwork controlNet;
    NeuralNetwork trainingNet;
    NeuralNetwork netBackup;

    List<FFData> prevRuns;
    FFData currRun;

    private int aiTurnDelay;

    public void Init()
    {

        trainingNet = new NeuralNetwork();
        InitNewGame();
        controlNet = trainingNet.Copy();//new NeuralNetwork();       
        trainingActive = true;
        trainingNetSide = 1;
    }

    public void Init(double _alpha, double _lambda, int _maxIterations, string _nnFileName)
    {
        alpha = _alpha;
        lambda = _lambda;
        maxIterations = _maxIterations;
        trainingNet = new NeuralNetwork(_nnFileName);
        InitNewGame();
        controlNet = trainingNet.Copy();//new NeuralNetwork();       
        trainingActive = true;
        trainingNetSide = 1;
        netBackup = trainingNet.Copy();
    }

    // Update is called once per frame
    public void Update () {

        if (trainingActive && aiTurnDelay <= 0)
        {
            EventManager.TriggerEvent("boardUpdated");

            StoneMove chosenMove;
            NeuralNetwork net;

            if (activePlayer == trainingNetSide)
            {
                net = trainingNet;

                AiBehaviour.GetBoardRating(new BoardNode(boardState, activePlayer), activePlayer, out currRun, ref net);
                prevRuns.Add(currRun.Clone());
                Debug.Log("current board rating:" + currRun.a3[0,0]);                                            
            }
            else
            {
                net = controlNet;
            }

            if (AiBehaviour.PerformTurn(boardState, activePlayer, firstTurn, out chosenMove, ref net))
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

                if (activePlayer == trainingNetSide)
                {
                    prevRuns[prevRuns.Count - 1].a3[0, 0] = 0;
                    trainingNet.BackPropagate(prevRuns.GetRange(0, prevRuns.Count - 1), prevRuns[prevRuns.Count - 1], alpha, lambda);
                     controlWins++;
                     totalControlWins++;
                }
                else
                {
                    prevRuns[prevRuns.Count - 1].a3[0, 0] = 1;
                    trainingNet.BackPropagate(prevRuns.GetRange(0, prevRuns.Count - 1), prevRuns[prevRuns.Count - 1], alpha, lambda);
                       trainingWins++;
                       totalTrainingWins++;                
                }
                gamesComplete++;
                UpdateTraining();
                InitNewGame();
                return;
            }

            if (boardState.PieceCount(trainingNetSide) == 0)
            {
                prevRuns[prevRuns.Count - 1].a3[0, 0] = 0;
                trainingNet.BackPropagate(prevRuns.GetRange(0, prevRuns.Count - 1), prevRuns[prevRuns.Count - 1], alpha, lambda);
                gamesComplete++;
                controlWins++;
                totalControlWins++;
                UpdateTraining();
                Debug.Log("Control wins!");
                InitNewGame();
                return;
            }
            else if (boardState.PieceCount(3 - trainingNetSide) == 0)
            {
                prevRuns[prevRuns.Count - 1].a3[0, 0] = 1;
                trainingNet.BackPropagate(prevRuns.GetRange(0, prevRuns.Count - 1), prevRuns[prevRuns.Count - 1], alpha, lambda);

                gamesComplete++;
                trainingWins++;
                totalTrainingWins++;
                UpdateTraining();
                Debug.Log("Training wins!"); ;
                InitNewGame();
                return;
            }
            else if (boardState.GetNumBlackKings() > boardState.GetNumWhite() || boardState.GetNumWhiteKings() > boardState.GetNumBlack())                
            {
                int blackKings = boardState.GetNumBlackKings();
                int whiteKings = boardState.GetNumWhiteKings();

                if ((trainingNetSide == 1 && blackKings - whiteKings > 0) || (trainingNetSide == 2 && whiteKings - blackKings > 0))
                {
                    prevRuns[prevRuns.Count - 1].a3[0, 0] = 1;
                    trainingNet.BackPropagate(prevRuns.GetRange(0, prevRuns.Count - 1), prevRuns[prevRuns.Count - 1], alpha, lambda);
                    trainingWins++;
                    totalTrainingWins++;
                }
                else
                {
                    prevRuns[prevRuns.Count - 1].a3[0, 0] = 0;
                    trainingNet.BackPropagate(prevRuns.GetRange(0, prevRuns.Count - 1), prevRuns[prevRuns.Count - 1], alpha, lambda);
                    controlWins++;
                    totalControlWins++;
                }
                gamesComplete++;
                UpdateTraining();
                InitNewGame();
                return;
            }
            else if (turnCount >= maxTurnCount || (boardState.GetNumBlackStones() == 0 && boardState.GetNumWhiteStones() == 0))
            {          
                trainingNet = netBackup.Copy();
                InitNewGame();
                return;
            }
            else if (activePlayer == trainingNetSide && !firstTurn && prevRuns.Count >= 2)
            {
                net.BackPropagate(prevRuns.GetRange(0, prevRuns.Count - 1), prevRuns[prevRuns.Count - 1], alpha, lambda);
            }

            if (firstTurn && activePlayer == 2) firstTurn = false;
            CheckersMain.prevStates.Add(boardState.Clone());
            activePlayer = 3 - activePlayer;
            aiTurnDelay = 3;
            turnCount++;
        }
        else if (aiTurnDelay > 0)
            aiTurnDelay--;
    }

    void UpdateTraining()
    {

        trainingNetSide = 3 - trainingNetSide;
        netBackup = trainingNet.Copy();

        if (gamesComplete % gamesPerCheck == 0)
        {
            Debug.Log("trainingWins: " + trainingWins);
            Debug.Log("controlWins: " + controlWins);

            if (trainingWins > controlWins + gamesPerCheck/4)
            {
                //Savetofile
                System.DateTime sn = System.DateTime.Now;
                trainingNet.SaveToFile("NetWeights\\" + sn.ToShortDateString().Replace('/', '-').Trim(' ') + '-' + sn.ToShortTimeString().Replace(':', '-'));
                controlNet = trainingNet.Copy();               
                controlUpdates++;
            }
           

            //check win rate and save if improved
            trainingWins = 0;
            controlWins = 0;
        }
       if (gamesComplete == maxIterations)
       {
           trainingActive = false;
       }
    }

    void InitNewGame()
    {
        turnCount = 0;
        prevRuns = new List<FFData>();
        currRun = new FFData();
        CheckersMain.prevStates = new List<Board>();

        firstTurn = true;
        trainingNet.ResetTraceValues();
        //initialise board (This will generate a board with initial game setup by default)
        boardState = new Board();
        activePlayer = 1;
        EventManager.TriggerEvent("gameReset");
    }

    public int GetActivePlayer()
    {
        return activePlayer;
    }

    public Board GetBoardState()
    {
        return boardState.Clone();
    }

    public List<StoneMove> GetAllValidMoves()
    {
        return CheckersMain.GenerateValidMoveList(boardState, activePlayer);
    }

    public int GetCurrentTrainingSide()
    {
        return trainingNetSide;
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

}
