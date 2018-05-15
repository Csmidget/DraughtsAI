using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class AI {

    public static readonly int[] doubleDiagonals = new int[8] { 0, 4, 5, 9, 25, 29, 30, 34};
    public static readonly int[] centreTiles = new int[2] {11, 23};
    public static readonly int[] centreDoubleDiagonals = new int[6] {10, 14, 15, 19, 20, 24 };
    public static readonly int[] otherTiles = new int[16] { 1, 2, 3, 6, 7, 12, 13, 16, 18, 21, 22, 27, 28, 31, 32, 33 };
    protected static List<BoardNode> boardNodes;
    protected static float[] InitBoardVal = new float[2] { 0, 0 };

    protected NeuralNetwork net;

    protected List<float> playerBoardRatings = new List<float>();
    protected List<float> enemyBoardRatings = new List<float>();
    protected float accuracyMod = 0;
    protected int searchDepth = 6;

    public void NewGame()
    {
        playerBoardRatings = new List<float>();
        enemyBoardRatings = new List<float>();
    }

    public virtual bool PerformTurn(Board _currentBoard, int _aiPlayer, PlayerType otherPlayer, bool _firstTurn, out StoneMove _move, int _presetFirstMove)
    {
        //initialize board node list if it isn't already
        if (boardNodes == null)
            boardNodes = new List<BoardNode>();

        //Placeholder move in case no valid moves are found
        _move = new StoneMove();

        //Generate list of possible moves for board state
        List<StoneMove> possibleMoves = FindAllValidMoves(_currentBoard, _aiPlayer,true);

        //Shuffle the list
        System.Random rnd = new System.Random();
        List<StoneMove> shuffledList = possibleMoves.OrderBy(item => rnd.Next()).ToList();

        if (shuffledList.Count > 0)
        {
            //set selected move to first move in the list & the value to infinity, this will likely be overwritten immediately.
            StoneMove selectedMove = possibleMoves[0];
            float selectedMoveValue = -Mathf.Infinity;

            //If its the first turn, run some extra processing
            if (_firstTurn)
            {
                FFData temp;
                //Find the initial board value for the player. This is used for dynamic difficulty AI's
                float boardVal = GetBoardRating(_currentBoard, _aiPlayer,out temp);
                InitBoardVal[_aiPlayer - 1] = boardVal;
                //If player is black, select the preset first move from the list and exit.
                if (_aiPlayer == 1)
                {
                    if (_presetFirstMove >= 0 && _presetFirstMove < possibleMoves.Count)
                    {
                        _move = possibleMoves[_presetFirstMove];
                    }
                    else
                        _move = shuffledList.First();
                    return true;
                }       
            }
            //Main body
            else
            {
                bool existingNodeFound = false;
                BoardNode baseNode = new BoardNode(_currentBoard, _aiPlayer);
                //If we already have a list of board nodes to use the attempt to locate the current board in the list.
                if (boardNodes.Count > 0)
                {
                    for (int i = 0; i < boardNodes.Count; i++)
                    {
                        if (boardNodes[i].boardState == _currentBoard)
                        {
                            existingNodeFound = true;
                            baseNode = boardNodes[i];
                            break;
                        }
                    }
                }
                //Refresh board node list
                boardNodes = new List<BoardNode>();
                //If we found a node, add the nodes children to the board node list
                if (existingNodeFound && !baseNode.IsEndNode())
                {
                    boardNodes.AddRange(baseNode.GetChildren());
                }
                //otherwise, generate a new node from this board and add that nodes children to the list
                else
                {                   
                    foreach (StoneMove m in shuffledList)
                    {
                        Board testBoard = _currentBoard.Clone();
                        testBoard.ResolveMove(m);
                        BoardNode bn = new BoardNode(testBoard, 3 - _aiPlayer, m);
                        boardNodes.Add(bn);
                    }
                }

                //Go through the board nodes and begin search
                foreach (BoardNode bn in boardNodes)
                {
                    bool prevStateFound = false;
                    foreach (Board b in CheckersMain.prevStates)
                    {
                        if (b == bn.boardState)
                        {
                            prevStateFound = true;
                        }
                    }

                    float moveValue = Mathf.NegativeInfinity;
                    if (!prevStateFound)
                    {
                        moveValue = Search.TraverseNodeList(bn, searchDepth - 1, Mathf.NegativeInfinity, Mathf.Infinity, _aiPlayer, false, this, 0);
                    }


                    if (moveValue > selectedMoveValue)
                    {
                        selectedMove = bn.GetMoveMade();
                        selectedMoveValue = moveValue;
                    }
                }
                //If the other player is human. Then the next board state will be skipped, so next board node list has to be 2 ply away.
                if (otherPlayer == PlayerType.Human)
                {
                    List<BoardNode> newNodeList = new List<BoardNode>(0);
                    foreach (BoardNode bn in boardNodes)
                    {
                        if (!bn.IsEndNode())
                        {
                            newNodeList.AddRange(bn.GetChildren());
                        }
                    }
                    boardNodes = newNodeList;
                }
            }

            _move = selectedMove;
            return true;
        }
        else
        {
            FFData data;
            GetBoardRating(_currentBoard, _aiPlayer, out data);
            return false;
        }
    }

    public static List<StoneMove> FindAllValidMoves(Board _board, int _activePlayer, bool _capChains)
    {
        List<StoneMove> moves = new List<StoneMove>();
        bool firstCap = true;
        bool captureFound = false;

        //Test every tile on the board
        for (int i = 0; i < 35; i++)
        {
            if (_board.state[i] != TileState.Empty && _activePlayer == _board.GetOwner(i))
            {
                FindValidMoves(_board, i, ref captureFound, ref firstCap,  moves, _capChains);
            }
        }
        return moves;
    }

    public static void FindValidMoves(Board _board, int _startPos,ref bool _captureFound, ref bool _firstCap, List<StoneMove> _moves, bool _capChains)
    {
        int owner = _board.GetOwner(_startPos);
        //Find the state of the current tile. Used to check ownership.
        TileState state = _board.state[_startPos];
        
        if (owner != 1 && owner != 2)
        {
            return;
        }

        //Blacks move up the board. Kings can also move up the board
        if (state == TileState.BlackPiece || state == TileState.BlackKing || state == TileState.WhiteKing)
        {
            TestMove(_board, owner, _startPos, _startPos + 4, ref _captureFound, ref _firstCap,  _moves, _capChains);
            TestMove(_board, owner, _startPos, _startPos + 5, ref _captureFound, ref _firstCap,  _moves, _capChains);
        }
        //Whites move down the board. Kings can also move down the board
        if (state == TileState.WhitePiece || state == TileState.BlackKing || state == TileState.WhiteKing)
        {
            TestMove(_board, owner, _startPos, _startPos - 4, ref _captureFound, ref _firstCap,  _moves,  _capChains);
            TestMove(_board, owner, _startPos, _startPos - 5, ref _captureFound, ref _firstCap,  _moves,  _capChains);
        }
    }

    protected static bool TestMove(Board _board, int _owner, int _startPos, int _movePos, ref bool _captureFound, ref bool _firstCap, List<StoneMove> _moves, bool _capChains)
    {

        //First ensure target position is within the bounds of the board.
        if (_movePos > 34 || _movePos < 0 || _movePos == 8 || _movePos == 17 || _movePos == 26)
        {
            return false;
        }

        TileState targetState = _board.state[_movePos];

        //If target tile is occupied by a stone owned by the same player, move is not valid.
        if (_owner == 1 && (targetState == TileState.BlackKing || targetState == TileState.BlackPiece) ||
            _owner == 2 && (targetState == TileState.WhiteKing || targetState == TileState.WhitePiece))
        {
            return false;
        }

        //If target tile is empty then move is allowed.
        if (targetState == TileState.Empty)
        {
            if (_captureFound)
                return false;
            _moves.Add (new StoneMove(_startPos, _movePos, false, 0));
            return true;
        }

        //If we have gotten this far then the tile must be occupied by an enemy! Now we test if there is an unoccupied tile behind them
        int endPos = _movePos + (_movePos - _startPos);

        //Double check we're still within the board bounds.
        if (endPos > 34 || endPos < 0 || endPos == 8 || endPos == 17 || endPos == 26)
        {
            return false;
        }

        //Final check, if the tile beyond the enemy is empty then they are capturable! But we have to check for further captures that are possible
        if (_board.state[endPos] == TileState.Empty)
        {
            _captureFound = true;
            if (_firstCap)
            {
                _moves.Clear();
                _firstCap = false;
            }
            StoneMove _move = new StoneMove(_startPos, endPos, true, _movePos);

            if (!_capChains)
                _moves.Add(_move);
            else if (!TestFurtherMoves(_board,_owner, _move, ref _moves))
                _moves.Add(_move);

            return true;
        }

        //If we get this far then there are no more checks to do. It is not a valid move.
        return false;
    }

    //Tests for any further captures that are possible and returns list of all further necessary captures.
    static private bool TestFurtherMoves(Board _board,int _activePlayer, StoneMove _initialMove, ref List<StoneMove> _foundMoves)
    {

        Board tempBoard = _board.Clone();
        tempBoard.ResolveMove(_initialMove);

        int newPos = _initialMove.endPos;

        TileState state = tempBoard.state[newPos];

        int enemyPlayer = 3 - _activePlayer;

        List<int> tempCaps = new List<int>();
        bool moveFound = false;
        int enemyPos,nextPos;

        for (int i = 4; i <= 5; i ++)
        {
            if (state == TileState.BlackKing || state == TileState.BlackPiece || state == TileState.WhiteKing)
            {
                enemyPos = newPos + i;
                nextPos = enemyPos + i;         
                if (tempBoard.GetOwner(enemyPos) == enemyPlayer && tempBoard.GetOwner(nextPos) == 0)
                {
                    moveFound = true;

                    tempCaps.Clear();
                    tempCaps.AddRange(_initialMove.capturedStones);
                    tempCaps.Add(enemyPos);
                    StoneMove newMove = new StoneMove(_initialMove.startPos, nextPos, true, tempCaps, state);

                    if (!TestFurtherMoves(_board,_activePlayer, newMove, ref _foundMoves))
                        _foundMoves.Add(newMove);
                }
            }

            if (state == TileState.BlackKing || state == TileState.WhitePiece || state == TileState.WhiteKing)
            {
                enemyPos = newPos - i;
                nextPos  = enemyPos - i;

                if (tempBoard.GetOwner(enemyPos) == enemyPlayer && tempBoard.GetOwner(nextPos) == 0)
                {
                    moveFound = true;

                    tempCaps.Clear();
                    tempCaps.AddRange(_initialMove.capturedStones);
                    tempCaps.Add(enemyPos);
                    StoneMove newMove = new StoneMove(_initialMove.startPos, nextPos, true, tempCaps, state);

                    if (!TestFurtherMoves(_board,_activePlayer, newMove, ref _foundMoves))   
                        _foundMoves.Add(newMove);
                }
            }
        }

        if (!moveFound)
        {
            return false;
        }
        else
            return true;
    }

    public float GetBoardRating(Board board, int _activePlayer,out FFData data)
    {

        //pieceAdvantage        : 0;
        //pieceDisadvantage     : 1;
        //pieceTake             : 2;  
        //pieceThreat           : 3;  
        //doubleDiagonal        : 4;  
        //enemyDoubleDiagonal   : 5;  
        //backRowBridge         : 6;  
        //centreControl         : 7;  
        //enemyCentreControl    : 8;  
        //kingCentreControl     : 9;  
        //enemyKingCentreControl: 10; 
        //stoneCount            : 11;  
        //enemystoneCount       : 12; 
        //kingCount             : 13; 
        //enemyKingCount        : 14; 

        double[] dataArray = new double[15];

        for (int i = 0; i < dataArray.Length; i++)
            dataArray[i] = 0;

        if (_activePlayer == 1)
        {
            if ((board.state[0] == TileState.BlackKing || board.state[0] == TileState.BlackPiece) && (board.state[2] == TileState.BlackKing || board.state[2] == TileState.BlackPiece))
                dataArray[6] = 1;

            for (int i = 0; i < centreTiles.Length; i++)
            {
                TileState state = board.state[centreTiles[i]];
                if (state == 0) continue;              
                if (state == TileState.BlackPiece)      {dataArray[7]++; dataArray[11]++; }
                else if (state == TileState.WhitePiece) {dataArray[8]++; dataArray[12]++;}
                else if (state == TileState.BlackKing)  {dataArray[9]++; dataArray[13]++;}
                else if (state == TileState.WhiteKing)  {dataArray[10]++;dataArray[14]++; }
            }

            for (int i = 0; i < doubleDiagonals.Length; i++)
            {
                TileState state = board.state[doubleDiagonals[i]];
                if (state == 0) continue;
                if (state == TileState.BlackPiece) { dataArray[4]++; dataArray[11]++; }
                else if (state == TileState.WhitePiece) { dataArray[5]++; dataArray[12]++; }
                else if (state == TileState.BlackKing) { dataArray[4]++; dataArray[13]++; }
                else if (state == TileState.WhiteKing) { dataArray[5]++; dataArray[14]++; }
            }

            for (int i = 0; i < centreDoubleDiagonals.Length; i++)
            {
                TileState state = board.state[centreDoubleDiagonals[i]];
                if (state == 0) continue;
                if (state == TileState.BlackPiece)      {dataArray[7]++; dataArray[4]++; dataArray[11]++; }
                else if (state == TileState.WhitePiece) {dataArray[8]++; dataArray[5]++; dataArray[12]++;}
                else if (state == TileState.BlackKing)  {dataArray[9]++; dataArray[4]++; dataArray[13]++;}
                else if (state == TileState.WhiteKing)  {dataArray[10]++; dataArray[5]++;dataArray[14]++;}
            }

            for (int i = 0; i < otherTiles.Length; i++)
            {
                TileState state = board.state[otherTiles[i]];
                if (state == 0) continue;
                if (state == TileState.BlackPiece)      {dataArray[11]++; }
                else if (state == TileState.WhitePiece) {dataArray[12]++; }
                else if (state == TileState.BlackKing)  {dataArray[13]++; }
                else if (state == TileState.WhiteKing)  {dataArray[14]++; }
            }
        }
        else
        {
            if ((board.state[34] == TileState.WhiteKing || board.state[34] == TileState.WhitePiece) && (board.state[32] == TileState.WhiteKing || board.state[32] == TileState.WhitePiece))
                dataArray[6] = 1;

            for (int i = 0; i < centreTiles.Length; i++)
            {
                TileState state = board.state[centreTiles[i]];
                if (state == 0) continue;
                if (state == TileState.BlackPiece)      {dataArray[8]++; dataArray[12]++;}
                else if (state == TileState.WhitePiece) {dataArray[7]++; dataArray[11]++;}
                else if (state == TileState.BlackKing)  {dataArray[10]++;dataArray[14]++;}
                else if (state == TileState.WhiteKing)  {dataArray[9]++; dataArray[13]++;}
            }

            for (int i = 0; i < doubleDiagonals.Length; i++)
            {
                TileState state = board.state[doubleDiagonals[i]];
                if (state == 0) continue;

                if (state == TileState.BlackPiece)      { dataArray[5]++; dataArray[12]++; }
                else if (state == TileState.WhitePiece) { dataArray[4]++; dataArray[11]++;}
                else if (state == TileState.BlackKing)  { dataArray[5]++; dataArray[14]++;}
                else if (state == TileState.WhiteKing)  { dataArray[4]++; dataArray[13]++;}
            }

            for (int i = 0; i < centreDoubleDiagonals.Length; i++)
            {
                TileState state = board.state[centreDoubleDiagonals[i]];
                if (state == 0) continue;
                if (state == TileState.BlackPiece)      {dataArray[8]++; dataArray[5]++; dataArray[12]++; }
                else if (state == TileState.WhitePiece) {dataArray[7]++; dataArray[4]++; dataArray[11]++; }
                else if (state == TileState.BlackKing)  {dataArray[10]++; dataArray[5]++;dataArray[14]++;}
                else if (state == TileState.WhiteKing)  {dataArray[9]++; dataArray[4]++; dataArray[13]++;}
            }

            for (int i = 0; i < otherTiles.Length; i++)
            {
                TileState state = board.state[otherTiles[i]];
                if (state == 0) continue;
                if (state == TileState.BlackPiece)      {dataArray[12]++; }
                else if (state == TileState.WhitePiece) {dataArray[11]++; }
                else if (state == TileState.BlackKing)  {dataArray[14]++; }
                else if (state == TileState.WhiteKing)  {dataArray[13]++; }
            }
        }

        dataArray[0] = dataArray[11] + dataArray[13] - dataArray[12] - dataArray[14];
        dataArray[1] = -dataArray[0];

        if (dataArray[0] < 0) dataArray[0] = 0;
        if (dataArray[1] < 0) dataArray[1] = 0;

        dataArray[2] = board.GetCapThreats(_activePlayer);
        dataArray[3] = board.GetCapThreats(3 - _activePlayer);

        data = net.FeedForward(dataArray);

        int winner;
        if (CheckersMain.CheckForWinner(board, _activePlayer, out winner))
        {
            if (winner == _activePlayer) return 1;
            else if (winner == 3 - _activePlayer) return 0;
        }

        return (float)data.a3[0, 0];
    }

    static public void LogInputFeatures(FFData data)
    {
        UnityEngine.Debug.Log("pieceAdvantage: "            + data.input[0] );
        UnityEngine.Debug.Log("pieceDisadvantage: "         + data.input[1] );
        UnityEngine.Debug.Log("pieceTake: "                 + data.input[2] ); 
        UnityEngine.Debug.Log("pieceThreat: "               + data.input[3] ); 
        UnityEngine.Debug.Log("doubleDiagonal: "            + data.input[4] ); 
        UnityEngine.Debug.Log("enemyDoubleDiagonal: "       + data.input[5] ); 
        UnityEngine.Debug.Log("backRowBridge: "             + data.input[6] ); 
        UnityEngine.Debug.Log("centreControl: "             + data.input[7] ); 
        UnityEngine.Debug.Log("enemyCentreControl: "        + data.input[8] ); 
        UnityEngine.Debug.Log("kingCentreControl: "         + data.input[9] ); 
        UnityEngine.Debug.Log("enemyKingCentreControl: "    + data.input[10]);
        UnityEngine.Debug.Log("stoneCount: "                + data.input[11]);
        UnityEngine.Debug.Log("enemystoneCount: "           + data.input[12]);
        UnityEngine.Debug.Log("kingCount: "                 + data.input[13]);
        UnityEngine.Debug.Log("enemyKingCount: "            + data.input[14]);
    }

    public virtual float RecalculateAccuracyMod() { return 0; }

    public virtual void ADRASInit(Board _board, int _activeSide) { }

    public virtual void ProcessDynamicAI(Board _board, int _activeSide, bool _isActivePlayer) { }

    public virtual void PrintAverageDifference() { }

    public void SetSearchDepth(int _searchDepth)
    {
        searchDepth = _searchDepth;
    }

    public void SetNeuralNetwork()
    {
        net = new NeuralNetwork();
    }

    public void SetNeuralNetwork(string _netName)
    {
        net = new NeuralNetwork(_netName);
    }

    public void SetNeuralNetwork(NeuralNetwork _net)
    {
        net = _net.Copy();
    }

    public NeuralNetwork GetNeuralNetwork()
    {
        return net;
    }

    public void ResetNNTraceValues()
    {
        net.ResetTraceValues();
    }

    public void SaveNetToFile(string _filename)
    {
        net.SaveToFile(_filename);
    }

    public virtual PlayerType GetType()
    {
        return PlayerType.AI;
    }

    public void TrainNet(FFData _prevRun, FFData _currRun, double _alpha, double _lambda)
    {
        net.BackPropagate(_prevRun, _currRun, _alpha, _lambda);
    }

    public float GetAverageDifference()
    {
        float returnVal = FindAverageDiff(playerBoardRatings, enemyBoardRatings);
        if (float.IsNaN(returnVal)) returnVal = 0;
        return returnVal;
    }

    /// <summary>
    /// Finds the average difference between values of two lists
    /// </summary>
    /// <param name="_list1"></param>
    /// <param name="_list2"></param>
    /// <returns></returns>
    public static float FindAverageDiff(List<float> _list1, List<float> _list2)
    {
        float returnVal = 0;
        for (int i = 0; i < _list1.Count && i < _list2.Count; i++)
        {
            returnVal += _list1[i] - _list2[i];
        }
        return returnVal / _list1.Count;
    }
}
