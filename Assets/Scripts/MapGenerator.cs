using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MapLoader;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class MapGenerator : MonoBehaviour
{
    private System.Random random = new System.Random();

    private SmoothSettings smoothSettings = new SmoothSettings();

    //mistakes
    private int mistakeThreshold;
    public int mistakeTimeout = 50000;
    private int mistakeCount = 0;
    private bool ThresholdReached;

    //map vars
    private int width, height;
    private int[,] map;
    private List<int>[,] domains;

    //tiles
    private int[] tileTypes = { 0, 1, 2, 3, 4 }; // DOMAIN OPTIONS

    // Constraints for tile placement
    private Dictionary<int, HashSet<int>> constraints = new Dictionary<int, HashSet<int>>
    {
        //sand
        { 3, new HashSet<int> { 4 } },

        //water
        { 4, new HashSet<int> { 2, 0, 3 } },

        //grass
        { 1, new HashSet<int> { 2, 0 } },

        //stone
        { 2, new HashSet<int> { 4, 1, 0 } },

        //cactus
        { 0, new HashSet<int> { 0, 1, 2, 4 } }

    };

    //generation settings
    public int lakeCount = 0;

    //==========INITIALISATION METHODS==========
    public int[,] GenerateMap(SmoothSettings smoothMap, int inWidth = 10, int inHeight = 10)
    {
        width = inWidth; height = inHeight;

        map = new int[width, height];
        domains = new List<int>[width, height];
        ResetGenerator();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                domains[x, y] = new List<int>();
                map[x, y] = -1;

                for (int i = 0; i < tileTypes.Length; i++)
                {
                    domains[x, y].Add(i);
                }

            }
        }

        if (PreGenerate())
        {
            Debug.Log("Map generation successful!");
            if (smoothMap.smoothMap) SmoothMap();
            return map;
        }
        else
        {
            Debug.LogError("Map generation failed.");
            return null;
        }
    }

    public int[,] GenerateMap(SmoothSettings smoothMap, int[,] inputMap, bool fill, bool validate)
    {
        map = inputMap;
        width = inputMap.GetLength(0); height = inputMap.GetLength(1);
        domains = new List<int>[width, height];

        ResetGenerator();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                domains[x, y] = new List<int>();
                
                if (map[x,y] != -1) domains[x, y].Add(map[x, y]);
                else
                {
                    for (int i = 0; i < tileTypes.Length; i++)
                    {
                        domains[x, y].Add(i);
                    }
                }
            }
        }

        if (validate) isMapValid();

        if (fill)
        {
            if (Backtrack(0,0))
            {
                Debug.Log("Map generation successful!");
                if (smoothMap.smoothMap) SmoothMap();
                return map;
            }
            else
            {
                Debug.LogError("Map generation failed.");
                return null;
            }
        }
        else
        {
            if (smoothMap.smoothMap) SmoothMap();
            return map;
        }
    }

    private void ResetGenerator()
    {
        lakeCount = (width * height) / 100 * 5 / 20;
        mistakeThreshold = mistakeTimeout;
        ThresholdReached = false;
        mistakeCount = 0;
    }

    private bool PreGenerate()
    {
        Debug.Log("LakeCount: " + lakeCount);
        while (lakeCount > 0)
        {
            int x = random.Next(0, width -1);
            int y = random.Next(0, height -1);
            GenerateWater(x, y, 0);
            lakeCount--;
        }

        foreach(List<int> domain in domains) if (domain.Count != 1 && domain.Contains(4)) domain.Remove(4);
        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                if (domains[x, y].Count == 1 && domains[x, y].Contains(4)) map[x, y] = domains[x, y][0];

            }
        }
        debugExportMap();
        return Backtrack(0, 0);
    }

    //==========GENERATING METHODS==========
    private bool Backtrack(int x, int y)
    {
        if (x == width) { x = 0; y++; }
        if (y == height) { return true; }// Return true if we have reached the end of the grid

        if (map[x, y] == -1 && map[x, y] != 4)
        {
            // Get all the remaining domain options for the current tile
            List<int> domain = new List<int>(domains[x, y]);

            // Shuffle the domain list to introduce randomness
            Shuffle(domain);

            // Iterate through each tile type in the shuffled domain
            foreach (int tile in domain)
            {
                // Check if the chosen tile type is consistent with the current constraints
                if (tile != 4 && isTileValidate(x, y, tile, map))
                {
                    // Assign the chosen tile type to the current tile
                    map[x, y] = tile;
                    // Save the current state of all domains
                    List<int>[,] savedDomains = SaveDomains();

                    // Perform forward checking to ensure neighbors have valid options
                    if (ForwardCheck(x, y, tile))
                    {
                        // Recursively backtrack to the next tile
                        if (Backtrack(x + 1, y))
                        {
                            return true;
                        }
                    }

                    savedDomains[x, y].Remove(tile);
                    if(checkTimeOut()) return false; //timeout
                    mistakeCount++;

                    // Restore the domains if the forward check or further backtracking fails
                    RestoreDomains(savedDomains);
                    // Unassign the tile
                    map[x, y] = -1;  // Changed from 0 to -1 for unassigned state
                }
            }
        }
        else
        {
            // Move to the next tile
            if (Backtrack(x + 1, y))
            {
                return true;
            }
        }
        // Return false if no valid assignment is found for the current tile
        return false;
    }

    private bool ForwardCheck(int x, int y, int tile)
    {
        // Get the neighbors of the current tile
        List<Vector2Int> neighbors = GetNeighbors(x, y);

        // Create a list to track neighbors whose domains are modified
        List<Vector2Int> modifiedNeighbors = new List<Vector2Int>();

        // Iterate through each neighbor
        foreach (Vector2Int neighbor in neighbors)
        {
            // If the tile has specific constraints
            if (constraints.ContainsKey(tile))
            {
                HashSet<int> invalidNeighbors = constraints[tile];

                // Create a temporary list to hold the valid domain options after removing invalid ones
                List<int> newDomain = new List<int>(domains[neighbor.x, neighbor.y]);
                newDomain.RemoveAll(invalidNeighbors.Contains);

                // Check if any domain options were actually removed
                if (newDomain.Count < domains[neighbor.x, neighbor.y].Count)
                {
                    // Update the neighbor's domain
                    domains[neighbor.x, neighbor.y] = newDomain;
                    // Track the neighbor whose domain was modified
                    modifiedNeighbors.Add(neighbor);
                }
            }

            // Check if the neighbor has no domain options left
            if (domains[neighbor.x, neighbor.y].Count == 0)
            {
                // If any neighbor has no domain options left, restore all modified domains
                foreach (Vector2Int modifiedNeighbor in modifiedNeighbors)
                {
                    // Restore the original domain to the neighbor
                    domains[modifiedNeighbor.x, modifiedNeighbor.y].Add(tile);
                }
                // Return false indicating that the forward check failed
                return false;
            }
        }
        // Return true indicating that the forward check was successful
        return true;
    }

    private int GenerateWater(int x, int y, int waterCount)
    {
        // Mark the current tile as water
        map[x, y] = 4;
        domains[x, y].Clear(); // Remove all other options, only water remains
        waterCount++;

        List<Vector2Int> neighbors = GetNeighbors(x, y);
        List<Vector2Int> validNeighbors = new List<Vector2Int>();

        // Remove current tile from neighbors
        neighbors.RemoveAll(n => n.x == x && n.y == y);

        foreach (Vector2Int neighbor in neighbors)
        {
            List<int> neighborDomain = domains[neighbor.x, neighbor.y];

            // If neighbor still has water as an option, it's a valid neighbor
            if (neighborDomain.Contains(4))
                validNeighbors.Add(neighbor);

            // Remove all non-water options from neighbor domain
            neighborDomain.RemoveAll(t => t != 4);
        }

        Shuffle(validNeighbors);

        foreach (Vector2Int neighbor in validNeighbors)
        {
            if (waterCount > 10)
                return waterCount; // Exit condition if enough water tiles generated

            waterCount = GenerateWater(neighbor.x, neighbor.y, waterCount);
        }

        return waterCount;
    }

    //==========HELPER METHODS==========
    // Utility method to shuffle a list
    private void Shuffle<T>(List<T> list)
    {
        int n = list.Count;
        for (int i = 0; i < n; i++)
        {
            int r = i + random.Next(n - i);
            T temp = list[r];
            list[r] = list[i];
            list[i] = temp;
        }
    }

    private List<Vector2Int> GetNeighbors(int x, int y, int depth = 1)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();

        // Loop through the (2 * depth + 1) x (2 * depth + 1) grid centered on (x, y)
        for (int dx = -depth; dx <= depth; dx++)
        {
            for (int dy = -depth; dy <= depth; dy++)
            {
                int nx = x + dx;
                int ny = y + dy;

                // Skip the center tile itself
                if (dx == 0 && dy == 0)
                    continue;

                // Check if the neighbor is within the bounds of the map
                if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                {
                    neighbors.Add(new Vector2Int(nx, ny));
                }
            }
        }

        return neighbors;
    }


    //==========DOMAIN MANAGEMENT METHODS==========
    private List<int>[,] SaveDomains() //copies the new domain
    {
        List<int>[,] savedDomains = new List<int>[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                savedDomains[x, y] = new List<int>(domains[x, y]);
            }
        }
        return savedDomains;
    }

    void RestoreDomains(List<int>[,] savedDomains) //sets the domain to the input
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                domains[x, y] = new List<int>(savedDomains[x, y]);
            }
        }
    }

    //==========VALIDATE METHODS==========
    private bool isTileValidate(int x, int y, int domainchoice, int[,] mapToCheck)
    {
        List<Vector2Int> neighbors = GetNeighbors(x, y);
        if (domainchoice == 0 || domainchoice == 2)
        {
            List<Vector2Int> waterNeighbors = GetNeighbors(x, y, 3);
            foreach (Vector2Int neighbor in waterNeighbors) if (map[neighbor.x, neighbor.y] == 4) return false;
        }

        foreach (Vector2Int neighbor in neighbors)
        {
            // Check constraints based on neighboring cells
            if (mapToCheck[neighbor.x, neighbor.y] != -1 && constraints[domainchoice].Contains(mapToCheck[neighbor.x, neighbor.y]))
            {
                return false; // Assignment violates constraints
            }
        }
        return true; // Assignment is consistent
    }

    private bool isMapValid()
    {

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int tileType = map[x, y];

                // Skip if the tile is unassigned
                if (tileType == -1)
                    continue;

                // Check if the current tile is valid
                if (!isTileValidate(x, y, tileType))
                {
                    Debug.LogError($"Invalid map: Tile at ({x},{y}) with type {tileType} is invalid.");
                    return false;
                }
            }
        }
        // If all tiles are valid, return true
        Debug.Log("Map valid!");
        return true;
    }


    //==========DEBUG METHODS==========
    private bool checkTimeOut()
    {
        if (mistakeThreshold <= 0)
        {
            if (!ThresholdReached)
            {
                Debug.LogError("Backtrack process timed out with " + mistakeCount + " mistakes!");
                debugExportMap("TIMEOUT_MAP");
                ThresholdReached = true;
            }
            
            return true; // Timeout occurred
        }
        mistakeThreshold--;
        return false;
    }

    private void debugExportMap(string name = "DEBUG_MAP")
    {
        CSVWriter writer = new CSVWriter();
        writer.SaveTMX(map, name);
    }

    private void SmoothMap()
    {
        Debug.Log("Start smoothing "+ smoothSettings.smoothIteration + " times");
        for (int i = 0; i < smoothSettings.smoothIteration; i++) // Number of iterations for smoothing
        {
            int[,] smoothedMap = new int[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (map[x, y] == 4 || map[x, y] == 0) {
                        smoothedMap[x, y] = map[x, y];
                        continue;
                    };

                    int[] tileCounts = new int[tileTypes.Length];
                    List<Vector2Int> neighbors = GetNeighbors(x, y);

                    // Count the number of each tile type in the neighbors
                    foreach (Vector2Int neighbor in neighbors)
                    {
                        int neighborTile = map[neighbor.x, neighbor.y];
                        tileCounts[neighborTile]++;
                    }

                    // Find the most frequent tile type among the neighbors
                    int maxCount = -1;
                    int mostFrequentTile = map[x, y]; // Default to current tile type

                    for (int t = 0; t < tileTypes.Length; t++)
                    {
                        if (tileCounts[t] > maxCount)
                        {
                            maxCount = tileCounts[t];
                            mostFrequentTile = t;
                        }
                    }

                    if (!isTileValidate(x, y, mostFrequentTile, smoothedMap)) smoothedMap[x, y] = map[x, y];
                    // Assign the most frequent tile type to the current position
                    smoothedMap[x, y] = mostFrequentTile;
                }
            }

            // Update the map with the smoothed version
            map = smoothedMap;
        }
    }
}