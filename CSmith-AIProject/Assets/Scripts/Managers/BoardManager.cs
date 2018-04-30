using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour {

    static private BoardManager activeManager;

    //The raw board data for the currently displayed board
    private Board displayedBoard;
    //The gameObjects that compose the displayed board
    private GameObject[] visualBoard;

    //The currently active game manager
    private GameManager gameManager;

    //Green overlays showing possible move locations for selected stone
    private List<GameObject> moveOverlays;
    //Blue overlays showing stones that can be moved
    private List<GameObject> movableStoneOverlays;

    //List of moves for a specific stone
    private List<StoneMove> activeMoves;

    //List of all valid moves for the current ply
    private List<StoneMove> validMoves;

    //Prefabs for game piece board representations
    [SerializeField]
    private GameObject blackStonePrefab;
    [SerializeField]
    private GameObject whiteStonePrefab;
    [SerializeField]
    private GameObject blackKingPrefab;
    [SerializeField]
    private GameObject whiteKingPrefab;

    //Prefabs for tile overlays
    [SerializeField]
    private GameObject tileOverlayGreen;
    [SerializeField]
    private GameObject tileOverlayBlue;

    bool boardSetup;

    private void Awake()
    {
        boardSetup = false;
        //Destroy this GameManager if one already exists.
        if (activeManager != null)
        {
            Debug.LogError("GameManager already exists, unable to initialize new GameManager");
            Destroy(this);
            return;
        }
        //set static reference to manager to this manager.
        activeManager = this;     

        visualBoard = new GameObject[35];
        moveOverlays = new List<GameObject>();
        activeMoves = new List<StoneMove>();
        movableStoneOverlays = new List<GameObject>();
    }

    private void Start()
    {
        gameManager = GameManager.GetActive();
        EventManager.RegisterToEvent("gameReset", SetupBoard);
        EventManager.RegisterToEvent("boardUpdated", UpdateBoard);
        EventManager.RegisterToEvent("turnOver", SpawnMovableStoneOverlays);
    }

    /// <summary>
    /// Initial setup for the visual board representation
    /// </summary>
    void SetupBoard()
    {
        displayedBoard = gameManager.GetBoardState();
        for (int i = 0; i < 35; i++)
        {

                TileState t = displayedBoard.state[i];

                visualBoard[i] = SetTile(i, t);
        }
     
        SpawnMovableStoneOverlays();
        boardSetup = true;
    }

    /// <summary>
    /// Updates the visual representation of the board to match the game managers board.
    /// </summary>
    void UpdateBoard()
    {
        //Spawn overlays for any movable stones
        SpawnMovableStoneOverlays();
        
        //If the visual board has not been initialized, do so.
        if (!boardSetup) SetupBoard();

        //Get the current board state from the game manager
        Board newBoard = gameManager.GetBoardState();
        for (int i = 0; i < 35; i++)
        {
            //If the displayed board does not match the game manager board, update only tiles that do not match
            if (newBoard.state[i] != displayedBoard.state[i])
            {            
                    visualBoard[i] = SetTile(i, newBoard.state[i]);
            }
        }

        displayedBoard = newBoard;
    }

    /// <summary>
    /// Controls spawning of pieces on occupied tiles
    /// </summary>
    /// <param name="_tilePos"></param>
    /// <param name="_newState"></param>
    /// <returns></returns>
    GameObject SetTile(int _tilePos,TileState _newState)
    {
        //Get current tile status
        GameObject currentGo = visualBoard[_tilePos];

        //Clean any previous status
        if (currentGo != null)
        {
            Destroy(currentGo);
        }

        //Find spawn location in world space
        Vector3 spawn = BoardToWorldSpace(_tilePos, 1);

        //Instantiate object that matches tile state.
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

    /// <summary>
    /// Spawns tile overlay of selected colour. Supports blue & green
    /// </summary>
    /// <param name="_tilePos">board position to spawn the tile overlay</param>
    /// <param name="colour">colour of tile overlay</param>
    /// <returns></returns>
    GameObject AddTileOverlay(int _tilePos, string colour)
    {
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

    /// <summary>
    /// Spawns blue overlays representing movable pieces.
    /// </summary>
    public void SpawnMovableStoneOverlays()
    {
        //Clear current list of movable stones
        foreach (GameObject go in movableStoneOverlays)
        {
            Destroy(go);
        }
        movableStoneOverlays.Clear();

        //List of moves from game manager
        validMoves = gameManager.GetAllValidMoves();

        //Tracks overlays that have been placed
        List<int> placedOverlays = new List<int>();

        //Only display possible moves if the current player is human
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

    /// <summary>
    /// Runs when a stone is picked up, displays tiles overlays for that stones possible move locations
    /// </summary>
    /// <param name="_stoneGO"></param>
    public void StonePicked(GameObject _stoneGO)
    {

        for (int i = 0; i < 35; i++)
        {
            //Find selected stone in list
            if (visualBoard[i] == _stoneGO)
            {
                //Get the moves for the stone
                activeMoves = gameManager.GetValidMoves(i);

                //Display tile overlay for each move
                foreach (StoneMove sm in activeMoves)
                {
                    moveOverlays.Add(AddTileOverlay(sm.endPos, "green"));
                }
            }
        }
    }

    /// <summary>
    /// Converts a board position (int 0-34) to a world space vector
    /// </summary>
    /// <param name="_boardPos">The board position to find in world space</param>
    /// <param name="z">the z value for the output vector</param>
    /// <returns></returns>
    public Vector3 BoardToWorldSpace(int _boardPos,float z)
    {
        float mod = 0;
        //tiles 8,17 and 26 are ignored, increase the position modifier to account for this

        if (_boardPos > 26) mod -= 0.75f;
        else if (_boardPos > 17) mod -= 0.5f;
        else if (_boardPos > 8) mod -= 0.25f;

        int yPos = (int)((_boardPos / 4f) + mod);
        int xPos = (1 + (_boardPos * 2) - (9 * yPos) ) % 8;

        return new Vector3(3.5f - xPos, yPos - 3.5f , z);
    }

    /// <summary>
    /// Runs when a stone is dropped, clear tile overlays and sends the move data to the game manager
    /// </summary>
    /// <param name="_stoneGO"></param>
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
