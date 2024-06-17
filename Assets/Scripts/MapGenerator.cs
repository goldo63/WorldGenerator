using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    private int width, height;

    private int[,] map;
    private List<int>[,] domains;

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

    private System.Random random = new System.Random();

    //==========INITIALISATION METHODS==========
    public int[,] GenerateMap(int inWidth = 10, int inHeight = 10)
    {
        width = inWidth; height = inHeight;

        map = new int[width, height];
        domains = new List<int>[width, height];

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

        if (Backtrack(0, 0))
        {
            Debug.Log("Map generation successful!");
            return map;
        }
        else
        {
            Debug.LogError("Map generation failed.");
            return null;
        }
    }

    public int[,] GenerateMap(int[,] inputMap, bool fill, bool validate)
    {
        map = inputMap;
        width = inputMap.GetLength(0); height = inputMap.GetLength(1);
        domains = new List<int>[width, height];

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
            if (Backtrack(0, 0))
            {
                Debug.Log("Map generation successful!");
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
            return map;
        }
        
    }

    //==========GENERATING METHODS==========
    private bool Backtrack(int x, int y)
    {
        // Move to the next row if we have reached the end of the current row
        if (x == width) { x = 0; y++; }
        // Return true if we have reached the end of the grid
        if (y == height) { return true; }


        if (map[x, y] == -1)
        {
            // Get all the remaining domain options for the current tile
            List<int> domain = new List<int>(domains[x, y]);

            // Shuffle the domain list to introduce randomness
            Shuffle(domain);

            // Iterate through each tile type in the shuffled domain
            foreach (int tile in domain)
            {
                // Check if the chosen tile type is consistent with the current constraints
                if (isTileValidate(x, y, tile))
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

                    // Restore the domains if the forward check or further backtracking fails
                    RestoreDomains(savedDomains);
                    // Unassign the tile
                    map[x, y] = -1;  // Changed from 0 to -1 for unassigned state
                }
            }
        }
        else {
            if (Backtrack(x + 1, y))
            {
                return true;
            }
        }
        // Return false if no valid assignment is found for the current tile
        return false;

    }

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

    private List<Vector2Int> GetNeighbors(int x, int y)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();

        // Loop through the 3x3 grid centered on (x, y)
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                // Skip the center tile itself
                if (dx == 0 && dy == 0)
                    continue;

                int nx = x + dx;
                int ny = y + dy;

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
    private bool isTileValidate(int x, int y, int domainchoice)
    {
        List<Vector2Int> neighbors = GetNeighbors(x, y);
        foreach (Vector2Int neighbor in neighbors)
        {
            // Check constraints based on neighboring cells
            if (map[neighbor.x, neighbor.y] != -1 && constraints[domainchoice].Contains(map[neighbor.x, neighbor.y]))
            {
                return false; // Assignment violates constraints
            }
        }
        return true; // Assignment is consistent
    }

    private bool isMapValid()
    {
        // Implement any additional map-wide validation if needed
        return true;
    }
}
