using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerType {Human,AI}


public class GameManager : MonoBehaviour {

    private static GameManager activeManager;

    private CheckersMain model;
    private Train trainingModel;

    /// <summary>
    /// Define player 1's player type. This will control how this players turn is executed.
    /// </summary>
    [SerializeField]
    private PlayerType player1;
    /// <summary>
    /// Define player 2's player type. This will control how this players turn is executed.
    /// </summary>
    [SerializeField]
    private PlayerType player2;

    bool gameActive;
    public bool training = false;

    void Awake()
    {
        //Destroy this GameManager if one already exists.
        if (activeManager != null)
        {
            Debug.LogError("GameManager already exists, unable to initialize new GameManager");
            Destroy(this);
            return;
        }
        //set static reference to manager to this manager.
        activeManager = this;

        EventManager.Init();

        EventManager.CreateEvent("turnOver");
        EventManager.CreateEvent("gameReset");
        EventManager.CreateEvent("gameOver");
        EventManager.CreateEvent("boardUpdated");

        //Create Model. Done in awake to allow other managers to register for events within OnEnable/Start
        model = new CheckersMain(player1, player2) ;
        trainingModel = new Train();
    }

	void Start ()
    {

        EventManager.RegisterToEvent("turnOver", TurnOver);
        EventManager.RegisterToEvent("gameReset", GameReset);

        if (!training)
            model.Init();
        else
            trainingModel.Init();
	}
	
	void Update ()
    {
        if (gameActive)
        {
            if (!training)
                model.Update();
            else
                trainingModel.Update();
        }
	}

    void OnDestroy()
    {
        EventManager.UnRegisterFromEvent("turnOver", TurnOver);

        model.Destroy();
    }

    void TurnOver()
    {
    }

    void GameReset()
    {

    }

    /// <summary>
    /// Returns the current active GameManager. Saves looking up within scene.
    /// </summary>
    /// <returns></returns>
    public static GameManager GetActive()
    {
        return activeManager;
    }

    /// <summary>
    /// Returns the currently active player.
    /// </summary>
    /// <returns></returns>
    public int GetActivePlayer()
    {
        if (!training)
            return model.GetActivePlayer();
        else
            return trainingModel.GetActivePlayer();
    }

    public PlayerType GetActivePlayerType()
    {
        if (!training)
        {
            if (model.GetActivePlayer() == 1) return player1;
            else return player2;
        }
        else
        {
            return PlayerType.AI;
        }
    }

    public Board GetBoardState()
    {
        if (!training)
            return model.GetBoardState();
        else
            return trainingModel.GetBoardState();
    }

    public List<StoneMove> GetValidMoves(int _startPos)
    {
        if (!training)
            return model.GetValidMoves(_startPos);
        else
            return new List<StoneMove>();
    }

    public void AttemptMove(StoneMove _move)
    {
        if (!training)
            model.AttemptMove(_move);
    }

    public List<StoneMove> GetAllValidMoves()
    {
        if (!training)
            return model.GetAllValidMoves();
        else
            return trainingModel.GetAllValidMoves();
    }

    public int GetWinner()
    {
        if (!training)
            return model.GetWinner();
        else
            return 1;
    }

    public void NewGame()
    {
        if (!training)
            model.Init();
        else
            trainingModel.Init();
    }

    public void Quit()
    {
        Application.Quit();
    }

    public int GetTrainingWins()
    {
        if (training)
            return trainingModel.GetTrainingWins();
        else
            return 0;
    }
    public int GetControlWins()
    {
        if (training)
            return trainingModel.GetControlWins();
        else
            return 0;
    }
    public int GetTotalTrainingWins()
    {
        if (training)
            return trainingModel.GetTotalTrainingWins();
        else
            return 0;
    }
    public int GetTotalControlWins()
    {
        if (training)
            return trainingModel.GetTotalControlWins();
        else
            return 0;
    }
    public int GetGamesComplete()
    {
        if (training)
            return trainingModel.GetGamesComplete();
        else
            return 0;
    }
    public int GetCurrentTrainingSide()
    {
        if (training)
            return trainingModel.GetCurrentTrainingSide();
        else
            return 0;
    }

    public int GetControlUpdates()
    {
        if (training)
            return trainingModel.GetControlUpdates();
        else
            return 0;
    }

    public void InitTrainingGame(double alpha,double lambda,int maxIterations,string nnFileName)
    {
        trainingModel.Init(alpha, lambda, maxIterations, nnFileName);
        gameActive = true;
    }
}
