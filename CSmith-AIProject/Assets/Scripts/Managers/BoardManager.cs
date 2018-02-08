﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour {

    static private BoardManager activeManager;

    private Board displayedBoard;
    private GameObject[,] visualBoard;

    private GameManager gameManager;

    private List<GameObject> moveOverlays;
    private List<GameObject> movableStoneOverlays;

    private List<StoneMove> activeMoves;

    [SerializeField]
    private GameObject blackStonePrefab;
    [SerializeField]
    private GameObject whiteStonePrefab;
    [SerializeField]
    private GameObject blackKingPrefab;
    [SerializeField]
    private GameObject whiteKingPrefab;
    [SerializeField]
    private GameObject tileOverlayGreen;

    private void Awake()
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

        EventManager.RegisterToEvent("gameReset", SetupBoard);
        EventManager.RegisterToEvent("boardUpdated", UpdateBoard);

        visualBoard = new GameObject[8, 8];
        moveOverlays = new List<GameObject>();
        activeMoves = new List<StoneMove>();
    }

    void OnEnable()
    {
        gameManager = GameManager.GetActive();
    }



    void SetupBoard()
    {

        displayedBoard = gameManager.GetBoardState();

        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                //Magic formula to return grey tiles
                int k = j * 2 + (1 - (i % 2));

                TileState t = displayedBoard.state[k,i];

                visualBoard[k, i] = SetTile(k, i, t);
            }
        }
    }

    void UpdateBoard()
    {
        Board newBoard = gameManager.GetBoardState();
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (newBoard.state[i,j] != displayedBoard.state[i,j])
                {
                    visualBoard[i, j] = SetTile(i, j, newBoard.state[i, j]);
                }
            }
        }
        displayedBoard = newBoard;
        Debug.Log("UPDATE BOARD");
    }

    GameObject SetTile(int _tileX,int _tileY,TileState _newState)
    {
        GameObject currentGo = visualBoard[_tileX, _tileY];

        if (currentGo != null)
        {
            Destroy(currentGo);
        }

        Vector3 spawn = new Vector3(_tileX - 3.5f, 3.5f -_tileY,1);


        GameObject newGo = null;
        switch (_newState)
        {
            case TileState.BlackPiece:
                newGo = Instantiate(blackStonePrefab, spawn,Quaternion.identity);
                break;
            case TileState.WhitePiece:
                newGo = Instantiate(whiteStonePrefab, spawn, Quaternion.identity);
                break;
            case TileState.BlackKing:
                newGo = Instantiate(blackKingPrefab, spawn, Quaternion.identity);
                break;
            case TileState.WhiteKing:
                newGo = Instantiate(whiteKingPrefab, spawn, Quaternion.identity);
                break;
        }

        return newGo;
    }

    void AddTileOverlay(int _tileX, int _tileY)
    {
        //TODO: Clean this up
        if (_tileX < 8 && _tileY < 8 && _tileX >= 0 && _tileY >= 0 &&
            displayedBoard.state[_tileX,_tileY] == TileState.Empty)
        {
            Vector3 spawn = new Vector3(_tileX - 3.5f, 3.5f - _tileY,5);
            moveOverlays.Add(Instantiate(tileOverlayGreen, spawn, Quaternion.identity));
        }
    }

    public void StonePicked(GameObject _stoneGO)
    {
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                //Magic formula to return grey tiles
                int k = j * 2 + (1 - (i % 2));

                if (visualBoard[k,i] == _stoneGO)
                {                 
                    //   AddTileOverlay(k-1, i-1);
                    //  AddTileOverlay(k - 1, i + 1);
                    //  AddTileOverlay(k + 1, i - 1);
                    //  AddTileOverlay(k + 1, i + 1);
                   activeMoves = gameManager.GetValidMoves(k, i);
                    foreach (StoneMove sm in activeMoves)
                    {
                        AddTileOverlay(sm.endPos.x, sm.endPos.y);
                    }
                }
            }
        }
    }

    public void StoneDropped(GameObject _stoneGO)
    {
        //Attempt to move stone to new pos
        Vector3 dropPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        BoardPos convertedPos = new BoardPos((int)(dropPos.x + 4f), (int)(-(dropPos.y - 4f)));
        //Debug.Log(convertedPos.x+","+convertedPos.y);

        //TODO: Maybe combine these IF blocks? Test if they are always == in size.
        for (int i = 0; i < activeMoves.Count; i++)
        {
            if (activeMoves[i].endPos == convertedPos)
            {
                gameManager.AttemptMove(activeMoves[i]);
            }
        }


        for (int i = 0; i < moveOverlays.Count; i++)
        {
            Destroy(moveOverlays[i]);
        }
        moveOverlays.Clear();
        activeMoves.Clear();
    }

    /// <summary>
    /// Returns the current active BoardManager. Saves looking up within scene.
    /// </summary>
    /// <returns></returns>
    public static BoardManager GetActive()
    {
        return activeManager;
    }
}
