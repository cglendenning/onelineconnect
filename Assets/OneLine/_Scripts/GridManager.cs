using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager
{
    private static GridManager gridmanager = null;

    public static GridManager GetGridManger()
    {
        if (gridmanager == null)
        {
            gridmanager = new GridManager();
        }
        return gridmanager;
    }

    public Vector3 GetPosForGrid(int gridNum)
    {
        float width = 600 / 100f;
        float height = width / 9f * 13 * 0.7f;
        float size = width / 9f;
        float posX = -width / 2 + size / 2 + (gridNum % 10) * size;
        float posY = height / 2 + size * 1 - (gridNum - 1) / 10 * size * 0.7f;

        return new Vector3(posX, posY, 0);
    }
}
