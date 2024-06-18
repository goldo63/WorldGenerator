using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapLoader : MonoBehaviour
{
    [System.Serializable]
    public class InputMapSettings
    {
        public string csvName = "";
        public bool validate = false;
        public bool fill = false;
    }

    public InputMapSettings inputMapSettings;

    [System.Serializable]
    public class SmoothSettings
    {
        public bool smoothMap = true;
        public int smoothIteration = 1;
    }
    public SmoothSettings smoothMapSettings;

    public Vector2Int size = new Vector2Int(10, 10);

    public TileBase[] tileTypes; // Array to hold different tile types

    public Tilemap tilemap;
    public MapGenerator generator;

    private int[,] currentMap;
    private int iterations = 10;

    // Start is called before the first frame update
    void Start()
    {
        int[,] map;

        if (inputMapSettings.csvName != null && inputMapSettings.csvName != "") map = generator.GenerateMap(smoothMapSettings, CSVLoader.LoadCSV(inputMapSettings.csvName), inputMapSettings.fill, inputMapSettings.validate);
        else if(size.x > 0 && size.y > 0) map = generator.GenerateMap(smoothMapSettings, size.x, size.y);
        else map = generator.GenerateMap(smoothMapSettings);

        LoadMap(map);
        currentMap = map;
    }

    // Update is called once per frame
    private void LoadMap(int[,] map)
    {
        if (map == null && iterations > 0) 
        {
            iterations--;
            Debug.Log("Iterations remaining: "+ iterations);
            Start();
        } else if(map == null)
        {
            return;
        }

        int width = map.GetLength(0);
        int height = map.GetLength(1);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int tileIndex = map[x, y];
                if (tileIndex >= 0 && tileIndex < tileTypes.Length)
                {
                    TileBase tile = tileTypes[tileIndex];
                    tilemap.SetTile(new Vector3Int(x, y, 0), tile);
                }
            }
        }
    }

    public int[,] GetCurrentMap() => currentMap;
}
