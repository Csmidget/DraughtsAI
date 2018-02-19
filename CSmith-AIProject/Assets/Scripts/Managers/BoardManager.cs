using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour {

    static private BoardManager activeManager;

    private Board displayedBoard;
    private GameObject[] visualBoard;

    private GameManager gameManager;

    private List<GameObject> moveOverlays;
    private List<GameObject> movableStoneOverlays;

    private List<StoneMove> activeMoves;
    private List<StoneMove> validMoves;

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
    [SerializeField]
    private GameObject tileOverlayBlue;


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
        EventManager.RegisterToEvent("turnOver", SpawnMovableStoneOverlays);

        visualBoard = new GameObject[35];
        moveOverlays = new List<GameObject>();
        activeMoves = new List<StoneMove>();
        movableStoneOverlays = new List<GameObject>();
    }

    void OnEnable()
    {
        gameManager = GameManager.GetActive();
    }



    void SetupBoard()
    {

        displayedBoard = gameManager.GetBoardState();
        Debug.Log(displayedBoard);
        for (int i = 0; i < 35; i++)
        {

                TileState t = displayedBoard.state[i];

                visualBoard[i] = SetTile(i, t);
        }

        SpawnMovableStoneOverlays();
    }

    void UpdateBoard()
    {
        SpawnMovableStoneOverlays();

        Board newBoard = gameManager.GetBoardState();
        for (int i = 0; i < 35; i++)
        {
                if (newBoard.state[i] != displayedBoard.state[i])
                {
                    visualBoard[i] = SetTile(i, newBoard.state[i]);
                }
        }
        displayedBoard = newBoard;
    }

    GameObject SetTile(int _tilePos,TileState _newState)
    {
        GameObject currentGo = visualBoard[_tilePos];

        if (currentGo != null)
        {
            Destroy(currentGo);
        }

        Vector3 spawn = BoardToWorldSpace(_tilePos, 1);


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
        if (newGo != null) newGo.GetComponent<Stone>().boardPos = _tilePos;
        return newGo;
    }

    GameObject AddTileOverlay(int _tilePos, string colour)
    {
        //TODO: Clean this up
        if (_tilePos < 35 && _tilePos >= 0)
        {

            

            Vector3 spawn = BoardToWorldSpace(_tilePos, 5);

            switch (colour)
            {
                case "blue":
                case "Blue":
                    return Instantiate(tileOverlayBlue, spawn, Quaternion.identity);
                case "green":
                case "Green":
                    return Instantiate(tileOverlayGreen, spawn, Quaternion.identity);
            }           
        }
        return null;
    }

    public void SpawnMovableStoneOverlays()
    {
        foreach (GameObject go in movableStoneOverlays)
        {
            Destroy(go);
        }
        movableStoneOverlays.Clear();

        validMoves = gameManager.GetAllValidMoves();
        List<int> placedOverlays = new List<int>();

        if (gameManager.GetActivePlayerType() == PlayerType.Human)
        {
            foreach (StoneMove sm in validMoves)
            {
                if (!placedOverlays.Contains(sm.startPos))
                {
                    movableStoneOverlays.Add(AddTileOverlay(sm.startPos, "blue"));
                    placedOverlays.Add(sm.startPos);
                }
            }
        }
    }

    public void StonePicked(GameObject _stoneGO)
    {

        for (int i = 0; i < 35; i++)
        {
                if (visualBoard[i] == _stoneGO)
                {                 
                    
                    activeMoves = gameManager.GetValidMoves(i);
                    foreach (StoneMove sm in activeMoves)
                    {
                        moveOverlays.Add(AddTileOverlay(sm.endPos, "green"));
                    }
                }
            }
    }

    public Vector3 BoardToWorldSpace(int _boardPos,float z)
    {
        float mod = 0;
        if (_boardPos > 8) mod -= 0.25f;
        if (_boardPos > 17) mod -= 0.25f;
        if (_boardPos > 26) mod -= 0.25f;

        int yPos = -(int)((_boardPos / 4f) + mod);
        int xPos = -((_boardPos * 2) + (1 + (10 * yPos) - yPos) ) % 8;

        return new Vector3(xPos + 3.5f, -3.5f - yPos, z);

    }

    public void StoneDropped(GameObject _stoneGO)
    {
        //Attempt to move stone to new pos
        Vector3 dropPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        int convertedPos = -1;

        int whiteCheck = ((int)(dropPos.x + 4f) + (int)(-4f - dropPos.y)) % 2;

        if (whiteCheck == 0)
        {

            convertedPos = ((int)(-dropPos.x + 5f) + (8 * (int)(3.99f + dropPos.y) + 1)) / 2;

            int mod = 1;
            if (convertedPos - mod >= 8) mod -= 1;
            if (convertedPos - mod >= 17) mod -= 1;
            if (convertedPos - mod >= 26) mod -= 1;

            convertedPos -= mod;

        }

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
