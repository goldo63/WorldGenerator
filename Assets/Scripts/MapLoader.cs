using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapLoader : MonoBehaviour
{
    public string csvMapName = null;
    public int MapHeight, MapWidth = 0;

    public TileBase[] tileTypes; // Array to hold different tile types

    public Tilemap tilemap; // Reference to the Tilemap component
    private int[,] currentMap;

    // Start is called before the first frame update
    void Start()
    {
        int[,] map;

        if (csvMapName != null) map = MapGenerator.GenerateMap(CSVLoader.LoadCSV(csvMapName));
        else if(MapWidth > 0 && MapHeight > 0) map = MapGenerator.GenerateMap(MapWidth, MapHeight);
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
