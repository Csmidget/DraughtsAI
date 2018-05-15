using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileState:int {Empty=0,WhitePiece=1,BlackPiece=2,WhiteKing=3,BlackKing=4 }


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
        state = (TileState[])_boardState.Clone();
    }

    public Board Clone()
    {
        return new Board(state);
    }

    public void ResolveMove(StoneMove _move)
    {
        if (state[_move.startPos] == TileState.WhitePiece && _move.endPos < 4)
        {
            state[_move.startPos] = TileState.WhiteKing;
        }
        else if (state[_move.startPos] == TileState.BlackPiece && _move.endPos > 30)
        {
            state[_move.startPos] = TileState.BlackKing;
        }
        else if (_move.endState != TileState.Empty)
        {
            state[_move.startPos] = _move.endState;
        }

        state[_move.endPos] = state[_move.startPos];
        state[_move.startPos] = TileState.Empty;

        if (_move.stoneCaptured)
        {
            foreach (int pos in _move.capturedStones)
            {
                state[pos] = TileState.Empty;
            }
        }
    }

    public int GetOwner(int _pos)
    {
        if (_pos > 34 || _pos < 0 || _pos == 8 || _pos == 17 || _pos == 26)
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

    public int GetPieceCount(int player)
    {
        if (player == 1)
            return GetNumBlack();
        else
            return GetNumWhite();
    }

    public int GetNumBlack()
    {
        int count = 0;
        for (int i = 0; i <= 34; i++)
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
        for (int i = 0; i <= 34; i++)
        {
            if(i == 8 || i == 17 || i == 26) continue;

            if (state[i] == TileState.WhiteKing || state[i] == TileState.WhitePiece)
                count++;

        }
        return count;
    }

    //Returns number of stones NOT KINGS
    public int GetNumWhiteStones()
    {
        int count = 0;
        for (int i = 0; i <= 34; i++)
        {
            if (i == 8 || i == 17 || i == 26) continue;

            if (state[i] == TileState.WhitePiece)
                count++;

        }
        return count;
    }

    public int GetNumBlackStones()
    {
        int count = 0;
        for (int i = 0; i <= 34; i++)
        {
            if (i == 8 || i == 17 || i == 26) continue;

            if (state[i] == TileState.BlackPiece)
                count++;

        }
        return count;
    }

    public int GetNumWhiteKings()
    {
        int count = 0;
        for (int i = 0; i <= 34; i++)
        {
            if (i == 8 || i == 17 || i == 26) continue;

            if (state[i] == TileState.WhiteKing)
                count++;

        }
        return count;
    }

    public int GetNumBlackKings()
    {

        int count = 0;
        for (int i = 0; i <= 34; i++)
        {
            if (i == 8 || i == 17 || i == 26) continue;

            if (state[i] == TileState.BlackKing)
                count++;

        }
        return count;

    }

    public int GetCapThreats(int _activePlayer)
    {
        int ret = 0;
        List<StoneMove> foundMoves = AI.FindAllValidMoves(this, _activePlayer,true);
        foreach (StoneMove s in foundMoves)
        {
            if (s.stoneCaptured)
                ret += s.capturedStones.Count;

            Board newBoard = Clone();
            newBoard.ResolveMove(s);
            List<StoneMove> enemyMoves = AI.FindAllValidMoves(newBoard, 3 - _activePlayer,true);
            bool capped = false;
            foreach (StoneMove s2 in enemyMoves)
            {
                if (s2.stoneCaptured && s2.capturedStones.Contains(s.endPos))
                {
                    capped = true;
                }
            }

            if (!capped)
            {
                bool furtherCapFound = false;
                bool firstCap = true;
                List<StoneMove> furtherMoves = new List<StoneMove>();
                AI.FindValidMoves(newBoard, s.endPos, ref furtherCapFound, ref firstCap,  furtherMoves,true);

                if (furtherCapFound)
                {
                    foreach (StoneMove s2 in furtherMoves)
                    {
                        if (s2.stoneCaptured)
                            ret += s2.capturedStones.Count;
                    }
                }
            }

        }
        return ret;
    }


    public int[] ToIntArray()
    {
        int[] returnArr = new int[35];
        for (int i = 0; i <= 34; i++)
        {
            returnArr[i] = (int)state[i];
        }
        return returnArr;
    }

    public static bool operator== (Board a, Board b)
    {
       // if ((object)a == null || (object)b == null)
       // {
       //     if ((object)a == null && (object)b == null)
       //         return true;
       //     else
       //         return false;
       //
       // }


        for (int i = 0; i <= 34; i++)
        {
            if (a.state[i] != b.state[i])
            {
                return false;
            }
        }
        return true;
             
    }

    public static bool operator!= (Board a, Board b)
    {
        for (int i = 0; i <= 34; i++)
        {
            if (a.state[i] != b.state[i])
            {
                return true;
            }
        }
        return false;
    }
}
