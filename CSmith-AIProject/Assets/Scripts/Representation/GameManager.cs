using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerType { Human,AI}


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

        //Create Model. Done in awake to allow other managers to register for events within OnEnable/Start
        model = new CheckersMain();
    }

	void Start () {
        model.Init();
	}
	
	void Update () {
		
	}

    public static GameManager GetActive()
    {
        return activeManager;
    }
}
