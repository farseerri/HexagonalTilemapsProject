using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using DG.Tweening;
using TMPro;
using UnityEditor.Experimental.GraphView;
using static Utools;
using UnityEngine.UI;

public class MovementController : MonoBehaviour
{
    public Image selfImage;
    public bool isSelected = false;

    private Vector2 movementInput;
    private Vector3 direction;
    public float moveSpeed = 2.0f; // 移动速度
    public int vision = 1;
    public Utools.ControllerMovingState controllerMovingState;
    public Utools.SoliderType soliderType;
    public int actionLimit;
    public bool faceRight;
    public float timer = 1.0f;
    public bool colorTurn = false;

    public IEnumerator Start()
    {
        controllerMovingState = Utools.ControllerMovingState.IsUsingMouseClickPause;
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(ChangeColorCoroutine(0.5f));
        UpdateFogOfWar(transform.position);
    }


    void Update()
    {

    }

    private IEnumerator ChangeColorCoroutine(float time)
    {
        while (true) // 无限循环，可以根据需要添加退出条件
        {
            yield return new WaitForSeconds(0.5f); // 每隔0.5秒执行以下代码
            if (isSelected)
            {
                colorTurn = !colorTurn; // 翻转布尔变量
                if (colorTurn)
                {
                    selfImage.color = Color.red;
                }
                else
                {
                    selfImage.color = Color.white;
                }
            }

        }
    }


    public void GetMovementDirection(float horizontal, float vertical)
    {

        if (horizontal > 0)
        {
            direction.x = 1.0f;
        }
        else if (horizontal < 0)
        {
            direction.x = -1.0f;
        }
        else
        {
            direction.x = 0.0f;
        }

        if (vertical > 0)
        {
            direction.y = 1.0f;
        }
        else if (vertical < 0)
        {
            direction.y = -1.0f;
        }
        else
        {
            direction.y = 0.0f;
        }

        // 计算目标位置
        Vector3 targetPosition = transform.position + direction;

        if (controllerMovingState != Utools.ControllerMovingState.IsUsingMouseClickMoving)
        {
            // 检查目标格子是否包含碰撞Tile
            Vector3Int targetTile = Utools.gameManager.collisionTilemap.WorldToCell(targetPosition);
            if (Utools.gameManager.collisionTilemap.GetTile(targetTile) == null && Utools.gameManager.waterTilemap.GetTile(targetTile) == null)
            {
                controllerMovingState = ControllerMovingState.IsUsingMouseClickMoving;
                // 使用DOTween平滑移动
                transform.DOMove(targetPosition, 0.5f / moveSpeed).OnComplete(() => UpdateFogOfWar(targetPosition));
            }
        }

    }

    public void UpdateFogOfWar(Vector3 position)
    {
        if (Utools.gameManager.fogOfWar != null)
        {
            Vector3Int currentPlayerTile = Utools.gameManager.fogOfWar.WorldToCell(position);

            // 清除周围格子的雾效
            for (int x = -vision; x <= vision; x++)
            {
                for (int y = -vision; y <= vision; y++)
                {
                    Utools.gameManager.fogOfWar.SetTile(currentPlayerTile + new Vector3Int(x, y, 0), null);
                }
            }

        }
        controllerMovingState = ControllerMovingState.IsUsingMouseClickPause;
    }

    public void SetDirection(bool faceRight)
    {
        if (faceRight)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
    }


    public bool UpdateDirection(bool needTurn)
    {
        if (needTurn)
        {
            if (faceRight)
            {
                transform.localScale = new Vector3(-1, 1, 1);
            }
            else
            {
                transform.localScale = new Vector3(1, 1, 1);
            }
        }
        return faceRight;
    }

    public bool UpdateDirection(Vector3 targetPosition, bool needTurn)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        faceRight = direction.x > 0 ? true : false;
        if (needTurn)
        {
            if (faceRight)
            {
                transform.localScale = new Vector3(-1, 1, 1);
            }
            else
            {
                transform.localScale = new Vector3(1, 1, 1);
            }
        }
        return faceRight;
    }


    public IEnumerator RunByPath(List<GridNode> path, float time)
    {
        if (path != null)
        {
            // Print the path (for debugging purposes).
            foreach (GridNode node in path)
            {
                if (node.gCost <= actionLimit)
                {
                    Vector3Int tilePosition = new Vector3Int(Utools.gameManager.baseTilemap.cellBounds.x + node.x, Utools.gameManager.baseTilemap.cellBounds.y + node.y, 0);
                    Vector3 targetPosition = Utools.gameManager.baseTilemap.CellToWorld(tilePosition);
                    UpdateDirection(targetPosition, true);

                    transform.DOMove(targetPosition, time / 2);
                    UpdateFogOfWar(targetPosition);
                    yield return new WaitForSeconds(time);
                }

            }
            controllerMovingState = Utools.ControllerMovingState.IsUsingMouseClickPause;
            Utools.gameManager.pathTilemap.ClearAllTiles();
 
        }
    }

}
