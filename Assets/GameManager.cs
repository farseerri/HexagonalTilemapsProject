using DG.Tweening;
using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
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
    public RectTransform gridTextPanel;
    public GridText gridTextPrefab;


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
    [SerializeField]
    public List<SoilderData> ourSoilderDataList;
    public List<Transform> ourSoilderPointsList;
    public List<MovementController> ourSoilderList;
    public Transform ourSoilderPointsPanel;
    public Transform ourSoildersPanel;
    public int ourSoilderSelectIndex = 0;

    public List<SoilderData> enemySoilderDataList;
    public List<Transform> enemySoilderPointsList;
    public List<MovementController> enemySoilderList;
    public Transform enemySoilderPointsPanel;
    public Transform enemySoildersPanel;



    public void Awake()
    {
        Utools.gameManager = this;
    }

    // Start is called before the first frame update
    void Start()
    {



        pathfindingManager = new PathfindingManager(Utools.gameManager.baseTilemap);

        foreach (GridNode gridNode in pathfindingManager.grid)
        {
            GridText tmpGridText = Instantiate<GridText>(gridTextPrefab, gridTextPanel);
            tmpGridText.stringText.text = gridNode.x.ToString() + "-" + gridNode.y.ToString();
            tmpGridText.name = tmpGridText.stringText.text;
            tmpGridText.x = gridNode.x;
            tmpGridText.y = gridNode.y;
            tmpGridText.transform.position = pathfindingManager.NodeToWorld(gridNode);
        }


        ourSoilderPointsList = new List<Transform>();
        foreach (Transform child in ourSoilderPointsPanel)
        {
            ourSoilderPointsList.Add(child);
        }


        foreach (GridNode gridNode in pathfindingManager.grid)
        {
            fogOfWar.SetTile(fogOfWar.WorldToCell(pathfindingManager.NodeToWorld(gridNode)), null);
        }


        SpawnSoliders(ourSoilderDataList, ourSoilderPointsList);


    }

    public void SpawnSoliders(List<SoilderData> soilderDataList, List<Transform> ourSoilderPointsList)
    {
        ourSoilderList = new List<MovementController>();

        for (int i = 0; i < soilderDataList.Count; i++)
        {
            MovementController controller = null;
            if (soilderDataList[i].soliderType == SoliderType.ondGrid)
            {
                controller = Instantiate<MovementController>(oneGridSoilder, ourSoildersPanel);
                controller.transform.position = ourSoilderPointsList[i].position;
            }
            else if (soilderDataList[i].soliderType == SoliderType.twoGird)
            {
                controller = Instantiate<MovementController>(twoGridSoilder, ourSoildersPanel);
                controller.transform.position = baseTilemap.GetCellCenterWorld(baseTilemap.WorldToCell(ourSoilderPointsList[i].position) + Vector3Int.right);

            }
            controller.gameObject.name = soilderDataList[i].soliderName;
            controller.actionLimit = soilderDataList[i].actionLimit;
            ourSoilderList.Add(controller);
            controller.SetDirection(true);
        }

        ourSoilderList = ourSoilderList.OrderByDescending(item => item.actionLimit).ToList();

        SelectCurrent(ourSoilderList[ourSoilderSelectIndex]);

    }

    public void SelectNextOurSoilder()
    {
        ourSoilderSelectIndex++;
        if (ourSoilderSelectIndex >= ourSoilderList.Count)
        {
            ourSoilderSelectIndex = 0;
        }
        SelectCurrent(ourSoilderList[ourSoilderSelectIndex]);
        pathfindingManager.ResetNodeCosts();
    }

    public void SelectPreOurSoilder()
    {
        ourSoilderSelectIndex--;
        if (ourSoilderSelectIndex < 0)
        {
            ourSoilderSelectIndex = ourSoilderList.Count - 1; // 从尾部开始
        }
        SelectCurrent(ourSoilderList[ourSoilderSelectIndex]);
        pathfindingManager.ResetNodeCosts();
    }




    public void SelectCurrent(MovementController controller)
    {

        foreach (MovementController soilder in ourSoilderList)
        {
            soilder.isSelected = false;
            soilder.selfImage.color = Color.white;
        }

        currentPlayer = controller;
        currentPlayer.isSelected = true;
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
            //float horizontal = Input.GetAxis("Horizontal");
            //float vertical = Input.GetAxis("Vertical");
            //Debug.Log("Horizontal:" + horizontal + " Vertical:" + vertical);
            //Utools.gameManager.currentPlayer.GetMovementDirection(horizontal, vertical);
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
        Utools.gameManager.ClearGridText();
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


    public GridText GetGridText(GridNode gridNode)
    {
        GridText temp = null;

        foreach (Transform child in gridTextPanel)
        {
            GridText gt = child.GetComponent<GridText>();
            if (gt.x == gridNode.x && gt.y == gridNode.y)
            {
                temp = gt;
            }
        }
        return temp;
    }

    public void ClearGridText()
    {
        foreach (Transform child in gridTextPanel)
        {
            GridText gt = child.GetComponent<GridText>();
            gt.stringText.color = Color.black;
        }

    }


}
