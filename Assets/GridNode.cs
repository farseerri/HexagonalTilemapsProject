using System.Collections.Generic;

public class GridNode
{
    public int x;
    public int y;
    public bool isWalkable;
    public List<GridNode> neighbors;
    public GridNode parent;
    public int gCost;
    public int hCost;

    public int FCost
    {
        get { return gCost + hCost; }
    }

    public GridNode(int x, int y, bool isWalkable)
    {
        this.x = x;
        this.y = y;
        this.isWalkable = isWalkable;
        neighbors = new List<GridNode>();
    }
}
