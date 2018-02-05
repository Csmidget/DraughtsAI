using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Central class for checkers model.
/// </summary>
public class CheckersMain{

    /// <summary>
    /// Ordered list of all boardstates prior to this one. All completed games are saved.
    /// TODO: Maybe optimise by storing only moves made instead of entire board state after every move.
    /// </summary>
    private List<Board> prevStates;

    private Board boardState;

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


}
