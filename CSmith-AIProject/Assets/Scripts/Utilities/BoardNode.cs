using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BoardNode
{
    public Board boardState;
    private List<StoneMove> validMoves;
    private int activePlayer;
    private List<BoardNode> childNodes;
    private bool endNode;
    private FFData nnData;
    private StoneMove moveMade;
    private bool valueCalculated;

    public BoardNode(Board _boardState, int _activePlayer)
    {
        boardState = _boardState.Clone();
        activePlayer = _activePlayer;
        childNodes = new List<BoardNode>();
        validMoves = AiBehaviour.FindAllValidMoves(boardState, activePlayer);
        endNode = true;
        moveMade = null;
        valueCalculated = false;
    }

    public BoardNode(Board _boardState, int _activePlayer,StoneMove _moveMade)
    {
        boardState = _boardState.Clone();
        activePlayer = _activePlayer;
        childNodes = new List<BoardNode>();
        validMoves = AiBehaviour.FindAllValidMoves(boardState, activePlayer);
        endNode = true;
        moveMade = _moveMade;
        valueCalculated = false;
    }

    public BoardNode(Board _boardState, int _activePlayer,bool _shuffleMoves)
    {
        boardState = _boardState.Clone();
        activePlayer = _activePlayer;
        childNodes = new List<BoardNode>();
        validMoves = AiBehaviour.FindAllValidMoves(boardState, activePlayer);
        System.Random rnd = new System.Random();
        validMoves = validMoves.OrderBy(item => rnd.Next()).ToList();
        endNode = true;
        moveMade = null;
        valueCalculated = false;
    }

    public int MoveCount()
    {
        return validMoves.Count;
    }
    
    public List<StoneMove> GetMoveList()
    {
        List<StoneMove> moveList = new List<StoneMove>();
        moveList.AddRange(validMoves);
        return validMoves;
    }

    public void AddChild(BoardNode _child)
    {
        if (endNode == true)
            endNode = false;
        childNodes.Add(_child);
    }

    public List<BoardNode> GetChildren()
    {
        List<BoardNode> childList = new List<BoardNode>();
        childList.AddRange(childNodes);
        return childList;
    }

    public StoneMove GetMoveMade()
    {
        return moveMade;
    }

    public bool IsEndNode()
    {
        return endNode;
    }

    public double GetValue(NeuralNetwork net)
    {
        valueCalculated = true;
        return AiBehaviour.GetBoardRating(boardState, activePlayer, out nnData, ref net);
    }
    public FFData GetData()
    {
        if (valueCalculated)
            return nnData;
        else
            return null;
    }

    public void Destroy()
    {
        childNodes.Clear();
        validMoves.Clear();
    }
    public int GetActivePlayer()
    {
        return activePlayer;
    }

}
