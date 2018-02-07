using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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
