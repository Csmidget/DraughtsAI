using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Central class for checkers model.
/// </summary>
public class CheckersMain{


    [NonSerialized]
    protected Action boardReset;




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
    /// Constructor. Initialises lists. Sets up events.
    /// </summary>
    public CheckersMain()
    {
        //initialise lists
        prevStates = new List<Board>();
       
    }

    /// <summary>
    /// Generates initial board state and prepares for new game.
    /// </summary>
    /// <returns></returns>
    public bool Init()
    {
        //initialise board (This will generate a board with initial game setup by default)
        boardState = new Board();

        return true;
    }

    public int GetActivePlayer()
    {
        return activePlayer;
    }

}
