using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour {

    private Board displayedBoard;
    private GameObject[,] visualBoard;

    private GameManager activeManager;

    [SerializeField]
    GameObject blackStonePrefab;
    [SerializeField]
    GameObject whiteStonePrefab;
    [SerializeField]
    GameObject blackKingPrefab;
    [SerializeField]
    GameObject whiteKingPrefab;




    void Start ()
    {
        visualBoard = new GameObject[4,8];
        activeManager = GameManager.GetActive();

        EventManager.RegisterToEvent("gameReset", SetupBoard);
        EventManager.RegisterToEvent("boardUpdated", UpdateBoard);
    }

	void Update ()
    {
		
	}

    void SetupBoard()
    {
        displayedBoard = activeManager.GetBoardState();

        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                //Magic formula to return grey tiles
                int k = j * 2 + (1 - (i % 2));

                TileState t = displayedBoard.boardState[k,i];

                visualBoard[j, i] = SetTile(k, i, visualBoard[j, i], t);
            }
        }


        Debug.Log("SETUP BOARD");
    }

    void UpdateBoard()
    {
        Debug.Log("UPDATE BOARD");
    }

    GameObject SetTile(int _tileX,int _tileY,GameObject _currentgo,TileState _newState)
    {
        if (_currentgo != null)
        {
            Destroy(_currentgo);
        }

        Vector2 spawn = new Vector2(_tileX - 3.5f, 3.5f -_tileY);


        switch (_newState)
        {

            case TileState.BlackPiece:
                _currentgo = Instantiate(blackStonePrefab, spawn,Quaternion.identity);
                break;
            case TileState.WhitePiece:
                _currentgo = Instantiate(whiteStonePrefab, spawn, Quaternion.identity);
                break;
            case TileState.BlackKing:
                _currentgo = Instantiate(blackKingPrefab, spawn, Quaternion.identity);
                break;
            case TileState.WhiteKing:
                _currentgo = Instantiate(blackKingPrefab, spawn, Quaternion.identity);
                break;
            default:
                _currentgo = null;
                break;

        }

        return null;
    }
}
