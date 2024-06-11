using System;
using System.IO;
using System.Text;
using UnityEngine;

public class CSVLoader : MonoBehaviour
{
    // Load the CSV file from the Resources folder
    public static int[,] LoadCSV(string fileName)
    {
        TextAsset csvFile = Resources.Load<TextAsset>(fileName);
        if (csvFile == null)
        {
            Debug.LogError("CSV file not found");
            return null;
        }

        string[] lines = csvFile.text.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        int width = lines[0].Split(',').Length;
        int height = lines.Length;

        int[,] map = new int[width, height];

        for (int y = 0; y < height; y++)
        {
            string[] values = lines[y].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            for (int x = 0; x < width; x++)
            {
                if (int.TryParse(values[x], out int tile))
                {
                    map[x, y] = tile;
                }
                else
                {
                    Debug.LogError($"Error parsing value at ({x}, {y}): {values[x]}");
                }
            }
        }

        return map;
    }
}
