using DG.Tweening;
using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;
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
    public Tilemap pathTilemap;


    private Vector3 playerPosition;
    private Vector3 targetPosition;
    public MovementController player;
    public TileBase inMovementRangeTileType;
    public TileBase outMovementRangeTileType;
    public IEnumerator enumerator;
    private List<GridNode> path;
    public float currentAtionLimit;
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
                OnClickEvent();
            }
        }

    }

    public void OnClickEvent()
    {
        Utools.gameManager.player.controllerMovingState = Utools.ControllerMovingState.IsUsingMouseClickPause;
        Vector3 mousePosition = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);

        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity, LayerMask.GetMask("PathMap"));

        if (hit.collider == null)
        {
            CreateNewPath();
        }
        else
        {
            targetPosition = hit.point;
            GridNode targetNode = pathfindingManager.WorldToNode(targetPosition);

            if (path[path.Count - 1] == targetNode)
            {
                if (enumerator != null)
                {
                    StopCoroutine(enumerator);
                    enumerator = null;
                }

                enumerator = RunByPath(path, 0.5f);
                StartCoroutine(enumerator);
            }
            else
            {
                CreateNewPath();
            }


        }

    }

    public IEnumerator RunByPath(List<GridNode> path, float time)
    {
        if (path != null)
        {
            // Print the path (for debugging purposes).
            foreach (GridNode node in path)
            {
                if (node.gCost <= pathfindingManager.actionLimit)
                {
                    Vector3Int tilePosition = new Vector3Int(baseTilemap.cellBounds.x + node.x, baseTilemap.cellBounds.y + node.y, 0);
                    player.transform.DOMove(baseTilemap.CellToWorld(tilePosition), time / 2);
                    player.UpdateFogOfWar(baseTilemap.CellToWorld(tilePosition));
                    yield return new WaitForSeconds(time);
                }

            }
            Utools.gameManager.player.controllerMovingState = Utools.ControllerMovingState.IsUsingKeyboardMoving;
            Utools.gameManager.pathTilemap.ClearAllTiles();
        }
    }

    public bool isPressMovingKey()
    {
        return Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.S) || Input.GetKeyUp(KeyCode.D);
    }

    public void ShowPath(List<GridNode> path)
    {

        if (path != null)
        {
            foreach (GridNode node in path)
            {
                Debug.Log("Path: " + node.x + ", " + node.y);
                Vector3Int tilePosition = new Vector3Int(baseTilemap.cellBounds.x + node.x, baseTilemap.cellBounds.y + node.y, 0);

                if (node.isOutOfMovmentRange)
                {
                    pathTilemap.SetTile(tilePosition, Utools.gameManager.outMovementRangeTileType);
                }
                else
                {
                    pathTilemap.SetTile(tilePosition, Utools.gameManager.inMovementRangeTileType);
                }

            }
        }
        else
        {
            Debug.Log("No path found.");
        }
    }

    public void CreateNewPath()
    {
        Vector3 mousePosition = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        Utools.gameManager.pathTilemap.ClearAllTiles();
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity, LayerMask.GetMask("BaseTilemap"));
        if (hit.collider != null)
        {
            // Get the world position of the hit point.
            targetPosition = hit.point;

            // Get player's current world position and mouse click world position.
            playerPosition = player.transform.position;

            Debug.Log("玩家所在格子:" + playerPosition.x + ":" + playerPosition.y);
            Debug.Log("点击的格子:" + targetPosition.x + ":" + targetPosition.y);
            path = pathfindingManager.FindPath(playerPosition, targetPosition, currentAtionLimit);
            ShowPath(path);
        }
    }


    public void NextRound()
    {
        pathfindingManager.ResetNodeCosts();
    }

}
