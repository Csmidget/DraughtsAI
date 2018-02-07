using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerType {Human,AI}


public class GameManager : MonoBehaviour {

    private static GameManager activeManager;

    private CheckersMain model;

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

        //Create Model. Done in awake to allow other managers to register for events within OnEnable/Start
        model = new CheckersMain();
    }

	void Start ()
    {

        EventManager.RegisterToEvent("turnOver", TurnOver);
        EventManager.RegisterToEvent("gameReset", GameReset);

        model.Init();        
	}
	
	void Update ()
    {
        model.Update();
	}

    void OnDestroy()
    {
        EventManager.UnRegisterFromEvent("turnOver", TurnOver);

        model.Destroy();
    }

    void TurnOver()
    {
        Debug.Log("TURN OVER");
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
    /// Returns the model attached to this GameManager
    /// </summary>
    /// <returns></returns>
    public CheckersMain GetModel()
    {
        return model;
    }

    /// <summary>
    /// Returns the currently active player.
    /// </summary>
    /// <returns></returns>
    public int GetActivePlayer()
    {
        return model.GetActivePlayer();
    }

    public Board GetBoardState()
    {
        return model.GetBoardState();
    }

    public List<StoneMove> GetValidMoves(int _startX, int _startY)
    {
        return model.GetValidMoves(_startX,_startY);
    }
}
