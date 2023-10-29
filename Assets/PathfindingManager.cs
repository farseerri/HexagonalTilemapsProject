using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.WSA;

public class PathfindingManager
{
    private GridNode[,] grid;
    public float actionLimit;

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
                bool isWalkable = IsTileWalkable(tile, tilePosition);

                // Create a GridNode and set its properties.
                grid[x, y] = new GridNode(x, y, isWalkable);
            }
        }
    }

    private bool IsTileWalkable(TileBase fogTile, Vector3Int tilePosition)
    {
        bool isWalkable = true;

        if (Utools.gameManager.collisionTilemap.GetTile(tilePosition) != null || Utools.gameManager.waterTilemap.GetTile(tilePosition) != null)
        {
            isWalkable = false;
        }

        return isWalkable;
    }



    public List<GridNode> FindPath(Vector3 startPosition, Vector3 targetPosition, float actionLimit)
    {
        this.actionLimit = actionLimit;
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
                if (!neighbor.isWalkable || closedSet.Contains(neighbor))
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

                if (neighbor.gCost > this.actionLimit)
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



}
