using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour {

    static private BoardManager activeManager;

    private Board displayedBoard;
    private GameObject[,] visualBoard;

    private GameManager gameManager;

    private List<GameObject> tileOverlays;

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

        visualBoard = new GameObject[4, 8];
        tileOverlays = new List<GameObject>();
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
                newGo = Instantiate(blackKingPrefab, spawn, Quaternion.identity);
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
            tileOverlays.Add(Instantiate(tileOverlayGreen, spawn, Quaternion.identity));
        }
    }

    public void StonePicked(GameObject _stoneGO)
    {
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 4; j++)
            {        
                if (visualBoard[j,i] == _stoneGO)
                {
                    //Magic formula to return grey tiles
                    int k = j * 2 + (1 - (i % 2));

                    //   AddTileOverlay(k-1, i-1);
                    //  AddTileOverlay(k - 1, i + 1);
                    //  AddTileOverlay(k + 1, i - 1);
                    //  AddTileOverlay(k + 1, i + 1);
                    List<StoneMove> validMoves = gameManager.GetValidMoves(k, i);
                    Debug.Log(validMoves.Count);
                    foreach (StoneMove sm in validMoves)
                    {
                        AddTileOverlay(sm.endPos.x, sm.endPos.y);
                    }
                }
            }
        }
    }

    public void StoneDropped(GameObject _stoneGO)
    {
        for (int i = 0; i < tileOverlays.Count; i++)
        {
            Destroy(tileOverlays[i]);
        }
        tileOverlays.Clear();
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
