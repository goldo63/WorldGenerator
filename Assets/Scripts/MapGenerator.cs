using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public static int[,] GenerateMap(int width = 10, int height = 10)
    {
        int[,] map = new int[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Simple random generation, replace with your own logic
                map[x, y] = Random.Range(0, 5); // Assuming 3 different tile types (0, 1, 2)
            }
        }

        return map;
    }

    public static int[,] GenerateMap(int[,] inputMap)
    {
        return inputMap;
    }
}
