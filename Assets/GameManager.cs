using DG.Tweening;
using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
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
    public MovementController currentPlayer;

    public MovementController oneGridSoilder;
    public MovementController twoGridSoilder;

    public TileBase inMovementRangeTileType;
    public TileBase outMovementRangeTileType;
    public TileBase areaTileType;
    public IEnumerator enumerator;
    public List<GridNode> path;


    public void Awake()
    {
        Utools.gameManager = this;
    }

    // Start is called before the first frame update
    void Start()
    {

        pathfindingManager = new PathfindingManager(Utools.gameManager.baseTilemap);
        currentPlayer.UpdateDirection(true);
    }

    // Update is called once per frame
    void Update()
    {


        if (Utools.gameManager.isPressMovingKey())
        {
            Utools.gameManager.currentPlayer.controllerMovingState = Utools.ControllerMovingState.IsUsingKeyboardMoving;
        }

        if (Utools.gameManager.currentPlayer.controllerMovingState == Utools.ControllerMovingState.IsUsingKeyboardMoving)
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            Debug.Log("Horizontal:" + horizontal + " Vertical:" + vertical);
            Utools.gameManager.currentPlayer.GetMovementDirection(horizontal, vertical);
        }
        else
        {
            if (Input.GetMouseButtonDown(1))
            {
                OnClickEvent();
            }
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            List<GridNode> reachableAreaGridNodeList = pathfindingManager.CalculateReachableArea(currentPlayer);

            foreach (GridNode gridNode in reachableAreaGridNodeList)
            {
                BoundsInt bounds = baseTilemap.cellBounds;
                int x_orgin = bounds.x;
                int y_orgin = bounds.y;

                Vector3Int tilePosition = new Vector3Int(x_orgin + gridNode.x, y_orgin + gridNode.y, 0);

                pathTilemap.SetTile(tilePosition, Utools.gameManager.areaTileType);
            }

        }

    }

    public void OnClickEvent()
    {
        Utools.gameManager.currentPlayer.controllerMovingState = Utools.ControllerMovingState.IsUsingMouseClickPause;
        Vector3 mousePosition = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);

        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity, LayerMask.GetMask("PathMap"));

        if (hit.collider == null)
        {
            targetPosition = pathfindingManager.CreateNewPath(currentPlayer);
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

                enumerator = currentPlayer.RunByPath(path, 0.5f);
                StartCoroutine(enumerator);
            }
            else
            {
                targetPosition = pathfindingManager.CreateNewPath(currentPlayer);
            }


        }

    }








    public bool isPressMovingKey()
    {
        return Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.S) || Input.GetKeyUp(KeyCode.D);
    }






    public void NextRound()
    {
        pathfindingManager.ResetNodeCosts();
    }


    public void SetCurrentPlayer(MovementController controller)
    {
        currentPlayer = controller;
    }

}
