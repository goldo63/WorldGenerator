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

    public Vector2Int size = new Vector2Int(10, 10);

    public TileBase[] tileTypes; // Array to hold different tile types

    public Tilemap tilemap;
    private int[,] currentMap;

    // Start is called before the first frame update
    void Start()
    {
        int[,] map;

        if (inputMapSettings.csvName != null && inputMapSettings.csvName != "") map = MapGenerator.GenerateMap(CSVLoader.LoadCSV(inputMapSettings.csvName));
        else if(size.x > 0 && size.y > 0) map = MapGenerator.GenerateMap(size.x, size.y);
        else map = MapGenerator.GenerateMap();

        LoadMap(map);
        currentMap = map;
    }

    // Update is called once per frame
    private void LoadMap(int[,] map)
    {
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
