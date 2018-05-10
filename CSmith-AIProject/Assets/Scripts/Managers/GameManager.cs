using System.Collections.Generic;
using UnityEngine;

public enum PlayerType {Human,AI,ADRNG,DROSAS,Dynamic3}


public class GameManager : MonoBehaviour {

    private static GameManager activeManager;

    private CheckersMain model = new CheckersMain(PlayerType.AI,PlayerType.AI);

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


    private bool training = false;
    private bool tournament = false;

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
        EventManager.CreateEvent("trainingComplete");
        EventManager.CreateEvent("tournamentComplete");
    }
	
	void Update ()
    {
        if (model.gameActive)
        {
            model.Update();
        }
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
    public int GetActiveSide()
    {
        return model.GetActiveSide();
    }

    public PlayerType GetActivePlayerType()
    {
        if (!training)
        {

            if (GetP1Side() == GetActiveSide()) return player1;
            else return player2;
            //if (model.GetActiveSide() == 1) return player1;
           // else return player2;
        }
        else
        {
            return PlayerType.AI;
        }
    }

    public void RevertTurn()
    {
        model.RevertTurn();
    }

    public Board GetBoardState()
    {
            return model.GetBoardState();
    }

    public List<StoneMove> GetValidMoves(int _startPos)
    {
            return model.GetValidMoves(_startPos);
    }

    public void AttemptMove(StoneMove _move)
    {
            model.AttemptMove(_move);
    }

    public List<StoneMove> GetAllValidMoves()
    {
        return model.GetAllValidMoves();
    }

    public int GetWinner()
    {
         return model.GetWinner();
    }

    public void NewGame()
    {
         model.InitNewGame();
    }

    public void Quit()
    {
        System.Diagnostics.Process.GetCurrentProcess().Kill();
    }

    public int GetTrainingWins()
    {
        return model.GetTrainingWins();
    }
    public int GetControlWins()
    {
        return model.GetControlWins();
    }
    public int GetTotalTrainingWins()
    {
        return model.GetTotalTrainingWins();
    }
    public int GetTotalControlWins()
    {
        return model.GetTotalControlWins();
    }
    public int GetGamesComplete()
    {
        return model.GetGamesComplete();
    }
    public int GetCurrentTrainingSide()
    {

        return model.GetCurrentTrainingSide();
    }

    public int GetControlUpdates()
    {
        return model.GetControlUpdates();
    }

    public bool IsTraining()
    {
        return training;
    }

    public bool IsTournament()
    {
        return tournament;
    }

    public PlayerType GetP1Type()
    {
        return player1;
    }

    public PlayerType GetP2Type()
    {
        return player2;
    }

    public void InitTournament(PlayerType _p1Type, PlayerType _p2Type, int _p1SearchDepth, int _p2SearchDepth, string _p1nnFileName, string _p2nnFileName,int _gameCount)
    {
        model = new CheckersMain(PlayerType.AI, PlayerType.AI);
        player1 = _p1Type;
        player2 = _p2Type;
        training = false;
        tournament = true;
        model.InitTournament(_p1Type,_p2Type, _p1SearchDepth, _p2SearchDepth,  _p1nnFileName,  _p2nnFileName,_gameCount);
        model.gameActive = true;
    }

    public void InitTrainingGame(double _alpha,double _lambda,int _maxIterations,string _nnFileName,int _searchDepth)
    {
        model = new CheckersMain(PlayerType.AI, PlayerType.AI);
        player1 = PlayerType.AI;
        player2 = PlayerType.AI;
        training = true;
        tournament = false;
        model.InitTraining(_alpha, _lambda, _maxIterations, _nnFileName, _searchDepth);
        model.gameActive = true;
    }

    public int GetP1Wins()
    {
        return model.GetP1Wins();
    }
    public int GetP2Wins()
    {
        return model.GetP2Wins();
    }

    public int GetP1Side()
    {
        return model.GetP1Side();
    }
}
