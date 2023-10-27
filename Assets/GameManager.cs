using DG.Tweening;
using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using static Utools;

public class GameManager : MonoBehaviour
{
    public PathfindingManager pathfindingManager;
    public Tilemap baseTilemap;
    public Tilemap fogOfWar;
    public Tilemap collisionTilemap;
    public Tilemap waterTilemap;

    private Vector3 playerPosition;
    private Vector3 targetPosition;
    public MovementController player;
    public TileBase maskTile;
    public IEnumerator enumerator;
    public void Awake()
    {
        Utools.gameManager = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        pathfindingManager = new PathfindingManager(Utools.gameManager.baseTilemap);
    }

    // Update is called once per frame
    void Update()
    {


        if (Utools.gameManager.isPressMovingKey())
        {
            Utools.gameManager.player.controllerMovingState = Utools.ControllerMovingState.IsUsingKeyboardMoving;
        }

        if (Utools.gameManager.player.controllerMovingState == Utools.ControllerMovingState.IsUsingKeyboardMoving)
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            Debug.Log("Horizontal:" + horizontal + " Vertical:" + vertical);
            Utools.gameManager.player.GetMovementDirection(horizontal, vertical);
        }
        else
        {
            if (Input.GetMouseButtonDown(1))
            {
                OnLeftClick();
            }
        }

    }

    public void OnLeftClick()
    {
        Utools.gameManager.player.controllerMovingState = Utools.ControllerMovingState.IsUsingMouseClickPause;



        // Get the mouse click position in screen coordinates.
        Vector3 mousePosition = Input.mousePosition;

        // Convert screen coordinates to world coordinates.
        Vector3 worldMousePosition = Camera.main.ScreenToWorldPoint(mousePosition);

        // Convert world coordinates to cell (grid) coordinates for the target tilemap.
        Vector3Int cellPosition = fogOfWar.WorldToCell(worldMousePosition); // Use the appropriate tilemap (fogOfWar in this case).

        if (fogOfWar.GetTile(cellPosition) == null)
        {
            // Get the center of the cell for accuracy (assuming tiles are centered).
            targetPosition = fogOfWar.GetCellCenterWorld(cellPosition); // Use the appropriate tilemap.


            // Get player's current world position and mouse click world position.
            playerPosition = player.transform.position;

            Debug.Log("玩家所在格子:" + playerPosition.x + ":" + playerPosition.y);
            Debug.Log("点击的格子:" + targetPosition.x + ":" + targetPosition.y);
            List<GridNode> path = pathfindingManager.FindPath(playerPosition, targetPosition);

            if (enumerator != null)
            {
                StopCoroutine(enumerator);
                enumerator = null;
            }

            enumerator = RunByPath(path, 0.5f);
            StartCoroutine(enumerator);
        }

    }

    public IEnumerator RunByPath(List<GridNode> path, float time)
    {
        if (path != null)
        {

            // Print the path (for debugging purposes).
            foreach (GridNode node in path)
            {
                Debug.Log("Path: " + node.x + ", " + node.y);
                Vector3Int tilePosition = new Vector3Int(baseTilemap.cellBounds.x + node.x, baseTilemap.cellBounds.y + node.y, 0);
                baseTilemap.SetTile(tilePosition, Utools.gameManager.maskTile);

                player.transform.DOMove(baseTilemap.CellToWorld(tilePosition), time / 2);
                player.UpdateFogOfWar(baseTilemap.CellToWorld(tilePosition));
                //player.transform.position = baseTilemap.CellToWorld(tilePosition);

                yield return new WaitForSeconds(time);
            }
            Utools.gameManager.player.controllerMovingState = Utools.ControllerMovingState.IsUsingKeyboardMoving;
        }
        else
        {
            Debug.Log("No path found.");
        }
    }

    public bool isPressMovingKey()
    {
        return Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.S) || Input.GetKeyUp(KeyCode.D);
    }
}
