using System.Collections.Generic;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.Tilemaps;
using UnityEngine.WSA;
using static Utools;

public class PathfindingManager
{
    private GridNode[,] grid;

    public Utools.SoliderType soliderType;

    public PathfindingManager(int width, int height)
    {
        grid = new GridNode[width, height];

        // Initialize grid nodes, marking them as walkable or not based on your tilemaps.
        // You will need to adapt this to your specific tilemap setup.
        // For each grid position (x, y), create a new GridNode and set isWalkable accordingly.
    }

    public PathfindingManager(Tilemap inputTilemap)
    {
        // 获取Tilemap的边界范围
        BoundsInt bounds = inputTilemap.cellBounds;
        int x_orgin = bounds.x;
        int y_orgin = bounds.y;

        grid = new GridNode[bounds.size.x, bounds.size.y];

        // 遍历Tilemap中的每个格子
        for (int x = 0; x < bounds.size.x; x++)
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                Vector3Int tilePosition = new Vector3Int(x_orgin + x, y_orgin + y, 0);

                // 获取格子中的瓦片
                TileBase tile = inputTilemap.GetTile(tilePosition);

                // Check if the tile in fogOfWar is walkable, and if there are corresponding colliders or water tiles.


                bool isWalkable = IsTileWalkable(tilePosition);

                // Create a GridNode and set its properties.
                grid[x, y] = new GridNode(x, y, isWalkable);
            }
        }
    }

    private bool IsTileWalkable(Vector3Int tilePosition)
    {
        bool isWalkable = true;


        if (Utools.gameManager.collisionTilemap.GetTile(tilePosition) != null || Utools.gameManager.waterTilemap.GetTile(tilePosition) != null)
        {
            isWalkable = false;

        }

        return isWalkable;
    }



    public List<GridNode> FindPath(Vector3 startPosition, Vector3 targetPosition, MovementController controller)
    {
        this.soliderType = controller.soliderType;
        // Convert world positions to grid positions (grid nodes).
        GridNode startNode = WorldToNode(startPosition);
        GridNode targetNode = WorldToNode(targetPosition);



        List<GridNode> openSet = new List<GridNode>();
        HashSet<GridNode> closedSet = new HashSet<GridNode>();

        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            GridNode currentNode = openSet[0];
            currentNode.neighbors = GetHexagonalNeighbors(currentNode);

            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].FCost < currentNode.FCost || (openSet[i].FCost == currentNode.FCost && openSet[i].hCost < currentNode.hCost))
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == targetNode)
            {
                return RetracePath(startNode, targetNode);
            }


            foreach (GridNode neighbor in currentNode.neighbors)
            {
                bool isTwoGridCanPass = false;
                if (this.soliderType == SoliderType.twoGird)
                {
                    isTwoGridCanPass = CheckTwoGridCanPass(controller, NodeToWorld(neighbor));
                }
                else
                {
                    isTwoGridCanPass = true;
                }

                if (!neighbor.isWalkable || closedSet.Contains(neighbor) || isTwoGridCanPass == false)
                {
                    continue;
                }

                int newCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor);
                if (newCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost = newCostToNeighbor;
                    neighbor.hCost = GetDistance(neighbor, targetNode);
                    neighbor.parent = currentNode;

                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                    }
                }

                if (neighbor.gCost > controller.actionLimit)
                {
                    neighbor.isOutOfMovmentRange = true;
                    // 这个节点的行动值超出上限，可以在这里执行打印操作
                    Debug.Log("Node with excessive action cost: " + neighbor.x + ", " + neighbor.y);
                }
                else
                {
                    neighbor.isOutOfMovmentRange = false;
                }

            }
        }

        return null;
    }

    private List<GridNode> RetracePath(GridNode startNode, GridNode endNode)
    {
        List<GridNode> path = new List<GridNode>();
        GridNode currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }

        path.Reverse();
        Debug.Log(path);
        return path;
    }

    private int GetDistance(GridNode nodeA, GridNode nodeB)
    {
        int distX = Mathf.Abs(nodeA.x - nodeB.x);
        int distY = Mathf.Abs(nodeA.y - nodeB.y);
        int distZ = Mathf.Abs(-nodeA.x - nodeA.y + nodeB.x + nodeB.y);
        int distance = (distX + distY + distZ) / 2;

        return distance;
    }

    public GridNode WorldToNode(Vector3 worldPosition)
    {
        // Check if the world position is within the bounds of the tilemap.
        Vector3Int tilePosition = Utools.gameManager.baseTilemap.WorldToCell(worldPosition);
        BoundsInt tilemapBounds = Utools.gameManager.baseTilemap.cellBounds;

        if (!tilemapBounds.Contains(tilePosition))
        {
            // The world position is outside the tilemap bounds.
            return null;
        }

        // Calculate the GridNode coordinates based on the tile position.
        int x = tilePosition.x - tilemapBounds.x;
        int y = tilePosition.y - tilemapBounds.y;

        return grid[x, y];
    }

    public List<GridNode> GetHexagonalNeighbors(GridNode currentNode)
    {
        List<GridNode> neighbors = new List<GridNode>();

        int x = currentNode.x;
        int y = currentNode.y;

        // Assuming you have an axial coordinate system (x, y) for the hexagons:

        // Right
        AddNeighbor(neighbors, x + 1, y);

        // Left
        AddNeighbor(neighbors, x - 1, y);

        // Up-Right (odd rows)
        if (y % 2 == 1)
        {
            AddNeighbor(neighbors, x, y + 1);
            AddNeighbor(neighbors, x - 1, y + 1);
        }
        // Up-Right (even rows)
        else
        {
            AddNeighbor(neighbors, x + 1, y + 1);
            AddNeighbor(neighbors, x, y + 1);
        }

        // Down-Left (odd rows)
        if (y % 2 == 1)
        {
            AddNeighbor(neighbors, x, y - 1);
            AddNeighbor(neighbors, x - 1, y - 1);
        }
        // Down-Left (even rows)
        else
        {
            AddNeighbor(neighbors, x + 1, y - 1);
            AddNeighbor(neighbors, x, y - 1);
        }

        return neighbors;
    }

    private void AddNeighbor(List<GridNode> neighbors, int x, int y)
    {
        // Assuming grid bounds checking is done elsewhere to avoid out-of-bounds errors.
        if (x >= 0 && x < grid.GetLength(0) && y >= 0 && y < grid.GetLength(1))
        {
            neighbors.Add(grid[x, y]);
        }
    }

    public void ResetNodeCosts()
    {
        foreach (GridNode node in grid)
        {
            node.gCost = 0;
            node.hCost = 0;
            node.parent = null;
            node.isOutOfMovmentRange = false;
        }
    }

    public void ShowPath(List<GridNode> path)
    {

        if (path != null)
        {
            foreach (GridNode node in path)
            {
                Debug.Log("Path: " + node.x + ", " + node.y);
                Vector3Int tilePosition = new Vector3Int(Utools.gameManager.baseTilemap.cellBounds.x + node.x, Utools.gameManager.baseTilemap.cellBounds.y + node.y, 0);

                if (node.isOutOfMovmentRange)
                {
                    Utools.gameManager.pathTilemap.SetTile(tilePosition, Utools.gameManager.outMovementRangeTileType);
                }
                else
                {
                    Utools.gameManager.pathTilemap.SetTile(tilePosition, Utools.gameManager.inMovementRangeTileType);
                }

            }
        }
        else
        {
            Debug.Log("No path found.");
        }
    }
    public Vector3 CreateNewPath(MovementController controller)
    {
        Vector3 targetPosition = Vector3.zero;
        Vector3 mousePosition = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        Utools.gameManager.pathTilemap.ClearAllTiles();
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity, LayerMask.GetMask("BaseTilemap"));
        if (hit.collider != null)
        {
            // Get the world position of the hit point.
            targetPosition = hit.point;

            // Get player's current world position and mouse click world position.
            Vector3 controllerPosition = controller.transform.position;

            Debug.Log("玩家所在格子:" + controllerPosition.x + ":" + controllerPosition.y);
            Debug.Log("点击的格子:" + targetPosition.x + ":" + targetPosition.y);
            Utools.gameManager.path = Utools.gameManager.pathfindingManager.FindPath(controllerPosition, targetPosition, controller);
            Utools.gameManager.pathfindingManager.ShowPath(Utools.gameManager.path);
        }
        return targetPosition;
    }



    public List<GridNode> CalculateReachableArea(MovementController controller)
    {
        List<GridNode> reachableArea = new List<GridNode>();
        Queue<GridNode> frontier = new Queue<GridNode>();
        Dictionary<GridNode, int> costSoFar = new Dictionary<GridNode, int>();

        GridNode startTile = WorldToNode(controller.transform.position);

        frontier.Enqueue(startTile);
        costSoFar[startTile] = 0;

        while (frontier.Count > 0)
        {
            GridNode current = frontier.Dequeue();

            if (costSoFar[current] > controller.actionLimit)
                continue; // Skip tiles that are too far to reach

            reachableArea.Add(current);

            foreach (GridNode neighbor in GetHexagonalNeighbors(current))
            {
                // Check if the neighbor is walkable and does not contain impassable tiles
                bool isNeighborWalkable = neighbor.isWalkable;

                if (this.soliderType == SoliderType.twoGird)
                {
                    if (isNeighborWalkable)
                    {
                        Vector3 neighborWorldPos = NodeToWorld(neighbor);
                        isNeighborWalkable = CheckTwoGridCanPass(controller, neighborWorldPos);
                    }
                }

                int newCost = costSoFar[current] + GetDistance(current, neighbor); // Consider terrain cost
                if (isNeighborWalkable && (!costSoFar.ContainsKey(neighbor) || newCost < costSoFar[neighbor]))
                {
                    costSoFar[neighbor] = newCost;
                    if (newCost <= controller.actionLimit)
                        frontier.Enqueue(neighbor);
                }
            }
        }

        return reachableArea;
    }

    public bool CheckTwoGridCanPass(MovementController controller, Vector3 worldPosition)
    {
        bool isTwoGridCanPass = false;
        bool isFaceRight = controller.UpdateDirection(worldPosition, false);

        //Vector3Int tilePosition = new Vector3Int(Utools.gameManager.baseTilemap.cellBounds.x + gridNode.x, Utools.gameManager.baseTilemap.cellBounds.y + gridNode.y, 0);
        //Vector3 worldPosition = Utools.gameManager.baseTilemap.CellToWorld(tilePosition);
        Vector3Int currentTilePostion = Utools.gameManager.pathTilemap.WorldToCell(worldPosition);
        if (!isFaceRight)
        {
            if (Utools.gameManager.collisionTilemap.GetTile(currentTilePostion) == null && Utools.gameManager.collisionTilemap.GetTile(currentTilePostion + Vector3Int.right) == null)
            {
                isTwoGridCanPass = true;
            }
        }
        else
        {
            if (Utools.gameManager.collisionTilemap.GetTile(currentTilePostion) == null && Utools.gameManager.collisionTilemap.GetTile(currentTilePostion + Vector3Int.left) == null)
            {
                isTwoGridCanPass = true;
            }
        }
        return isTwoGridCanPass;
    }

    public Vector3 NodeToWorld(GridNode node)
    {
        if (node == null)
        {
            return Vector3.zero; // 或者其他适当的默认值
        }

        // 获取基础 Tilemap 的原点位置
        Vector3Int tilemapOrigin = Utools.gameManager.baseTilemap.cellBounds.position;

        // 计算网格节点在 Tilemap 中的位置
        Vector3Int tilePosition = new Vector3Int(tilemapOrigin.x + node.x, tilemapOrigin.y + node.y, 0);

        // 获取网格节点中心的世界坐标
        Vector3 worldPosition = Utools.gameManager.baseTilemap.GetCellCenterWorld(tilePosition);

        return worldPosition;
    }

}
