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
    public TileState[,] boardState;

    /// <summary>
    /// No parameters, assume new game, generate default board
    /// </summary>
    public Board()
    {
        boardState = new TileState[8, 8];

        //Generate empty board
        for (int i = 0; i < 8; i ++)
        {
            for (int j = 0; j < 8; j++)
            {
                boardState[i, j] = TileState.Empty;
            }
        }

        //Setup white Pieces
        boardState[1, 0] = TileState.WhitePiece;
        boardState[3, 0] = TileState.WhitePiece;
        boardState[5, 0] = TileState.WhitePiece;
        boardState[7, 0] = TileState.WhitePiece;
        boardState[0, 1] = TileState.WhitePiece;
        boardState[2, 1] = TileState.WhitePiece;
        boardState[4, 1] = TileState.WhitePiece;
        boardState[6, 1] = TileState.WhitePiece;
        boardState[1, 2] = TileState.WhitePiece;
        boardState[3, 2] = TileState.WhitePiece;
        boardState[5, 2] = TileState.WhitePiece;
        boardState[7, 2] = TileState.WhitePiece;

        //Setup black Pieces
        boardState[0, 7] = TileState.BlackPiece;
        boardState[2, 7] = TileState.BlackPiece;
        boardState[4, 7] = TileState.BlackPiece;
        boardState[6, 7] = TileState.BlackPiece;
        boardState[1, 6] = TileState.BlackPiece;
        boardState[3, 6] = TileState.BlackPiece;
        boardState[5, 6] = TileState.BlackPiece;
        boardState[7, 6] = TileState.BlackPiece;
        boardState[0, 5] = TileState.BlackPiece;
        boardState[2, 5] = TileState.BlackPiece;
        boardState[4, 5] = TileState.BlackPiece;
        boardState[6, 5] = TileState.BlackPiece;
    }

    /// <summary>
    /// Create new Board with existing board state
    /// </summary>
    /// <param name="_boardState">board state to copy</param>
    public Board(TileState[,] _boardState)
    {
        //TODO: Test this, may not work
        boardState = (TileState[,])_boardState.Clone();
    }

    

}
