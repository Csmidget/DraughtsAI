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
    public TileState[] state;

    /// <summary>
    /// No parameters, assume new game, generate default board
    /// </summary>
    public Board()
    {
        state = new TileState[35];

        //Generate empty board
        for (int i = 0; i < 35; i ++)
        {
          state[i] = TileState.Empty;
        }

        //Setup white Pieces
        state[0]  = TileState.BlackPiece;
        state[1]  = TileState.BlackPiece;
        state[2]  = TileState.BlackPiece;
        state[3]  = TileState.BlackPiece;
        state[4]  = TileState.BlackPiece;
        state[5]  = TileState.BlackPiece;
        state[6]  = TileState.BlackPiece;
        state[7]  = TileState.BlackPiece;
        state[9]  = TileState.BlackPiece;
        state[10] = TileState.BlackPiece;
        state[11] = TileState.BlackPiece;
        state[12] = TileState.BlackPiece;

        //Setup black Pieces
        state[34] = TileState.WhitePiece; 
        state[33] = TileState.WhitePiece;
        state[32] = TileState.WhitePiece;
        state[31] = TileState.WhitePiece;
        state[30] = TileState.WhitePiece;
        state[29] = TileState.WhitePiece;
        state[28] = TileState.WhitePiece;
        state[27] = TileState.WhitePiece;
        state[25] = TileState.WhitePiece;
        state[24] = TileState.WhitePiece;
        state[23] = TileState.WhitePiece;
        state[22] = TileState.WhitePiece;

    }

    /// <summary>
    /// Create new Board with existing board state
    /// </summary>
    /// <param name="_boardState">board state to copy</param>
    public Board(TileState[] _boardState)
    {
        //TODO: Test this, may not work
        state = (TileState[])_boardState.Clone();
    }

    public Board Clone()
    {
        return new Board(state);
    }

    public void ResolveMove(StoneMove _move)
    {
        if (state[_move.startPos] == TileState.BlackPiece && _move.endPos < 4)
        {
            state[_move.startPos] = TileState.BlackKing;
        }
        else if (state[_move.startPos] == TileState.WhitePiece && _move.endPos > 30)
        {
            state[_move.startPos] = TileState.WhiteKing;
        }

        state[_move.endPos] = state[_move.startPos];
        state[_move.startPos] = TileState.Empty;

        if (_move.stoneCaptured)
        {
            foreach (int pos in _move.capturedStones)
            {
                TileState cappedStone = state[pos];
                state[pos] = TileState.Empty;
            }
        }
    }

    public int GetOwner(int _pos)
    {
        if (_pos > 34 || _pos < 0)
        {
            return -1;
        }
        if (state[_pos] == TileState.BlackPiece || state[_pos] == TileState.BlackKing)
            return 1;
        else if (state[_pos] == TileState.WhitePiece || state[_pos] == TileState.WhiteKing)
            return 2;
        else
            return 0;
    }

    public int GetNumBlack()
    {
        int count = 0;
        for (int i = 0; i < 34; i++)
        {
            if (i == 8 || i == 17 || i == 26) continue;

            if (state[i] == TileState.BlackKing || state[i] == TileState.BlackPiece)
                count++;
        }
    
        return count;
    }
    public int GetNumWhite()
    {
        int count = 0;
        for (int i = 0; i < 34; i++)
        {
            if(i == 8 || i == 17 || i == 26) continue;

            if (state[i] == TileState.WhiteKing || state[i] == TileState.WhitePiece)
                count++;

        }
        return count;
    }
}
