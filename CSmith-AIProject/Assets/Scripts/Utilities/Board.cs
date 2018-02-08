using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileState {Empty=0,WhitePiece=1,BlackPiece=2,WhiteKing=3,BlackKing=4 }


/// <summary>
/// Dumb class, just stores board state, does not check if moves are valid.
/// Stores in 8x8 array for simplicity. Could ignore white tiles fully but complicates things.
/// </summary>
public class Board {

    /// <summary>
    /// Current state of each tile on board. [0,0] = top left. ([x,y])
    /// </summary>
    public TileState[,] state;

    /// <summary>
    /// No parameters, assume new game, generate default board
    /// </summary>
    public Board()
    {
        state = new TileState[8, 8];

        //Generate empty board
        for (int i = 0; i < 8; i ++)
        {
            for (int j = 0; j < 8; j++)
            {
                state[i, j] = TileState.Empty;
            }
        }

        //Setup white Pieces
        state[1, 0] = TileState.WhitePiece;
        state[3, 0] = TileState.WhitePiece;
        state[5, 0] = TileState.WhitePiece;
        state[7, 0] = TileState.WhitePiece;
        state[0, 1] = TileState.WhitePiece;
        state[2, 1] = TileState.WhitePiece;
        state[4, 1] = TileState.WhitePiece;
        state[6, 1] = TileState.WhitePiece;
        state[1, 2] = TileState.WhitePiece;
        state[3, 2] = TileState.WhitePiece;
        state[5, 2] = TileState.WhitePiece;
        state[7, 2] = TileState.WhitePiece;

        //state[3, 4] = TileState.WhitePiece;

        //Setup black Pieces
        state[0, 7] = TileState.BlackPiece;
        state[2, 7] = TileState.BlackPiece;
        state[4, 7] = TileState.BlackPiece;
        state[6, 7] = TileState.BlackPiece;
        state[1, 6] = TileState.BlackPiece;
        state[3, 6] = TileState.BlackPiece;
        state[5, 6] = TileState.BlackPiece;
        state[7, 6] = TileState.BlackPiece;
        state[0, 5] = TileState.BlackPiece;
        state[2, 5] = TileState.BlackPiece;
        state[4, 5] = TileState.BlackPiece;
        state[6, 5] = TileState.BlackPiece;
    }

    /// <summary>
    /// Create new Board with existing board state
    /// </summary>
    /// <param name="_boardState">board state to copy</param>
    public Board(TileState[,] _boardState)
    {
        //TODO: Test this, may not work
        state = (TileState[,])_boardState.Clone();
    }

    public Board Clone()
    {
        return new Board(state);
    }


}
