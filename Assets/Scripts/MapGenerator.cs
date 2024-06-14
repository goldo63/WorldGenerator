using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    private int width, height;

    private int[,] map;
    private List<int>[,] domains;

    private int[] tileTypes = { 0, 1, 2, 3, 4 }; //DOMAIN OPTIONS

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

    public int[,] GenerateMap(int[,] inputMap)
    {
        map = inputMap;
        width = inputMap.GetLength(0); height = inputMap.GetLength(1);
        domains = new List<int>[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                domains[x, y] = new List<int>();
                if(map[x,y] != -1) domains[x, y].Add(map[x, y]);
                else
                {
                    for (int i = 0; i < tileTypes.Length; i++)
                    {
                        domains[x, y].Add(i);
                    }
                }
            }
        }

        if(Backtrack(0,0))
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

    //==========GENERATING METHODS==========
    private bool Backtrack(int x, int y)
    {
        if (x == width) { x = 0; y++; }
        if (y == height) { return true; } //checks if all tiles have been checked

        List<int> domain = domains[x, y]; //gets all the remaining domain options from the selected Variable

        foreach (int tile in domain)
        {
            if (isTileValidate(x, y, tile)) //checks if the chosen domain option fits the current constraints
            {
                map[x, y] = tile;
                List<int>[,] savedDomains = SaveDomains();

                if (ForwardCheck(x, y, tile))  //checks if the neighbours have any options left after domain option is chosen
                {
                    if (Backtrack(x + 1, y)) //checks the next tile.
                    {
                        return true;
                    }
                }

                RestoreDomains(savedDomains); //restores the domain if the chosen domain option leaves neigbours without options
                map[x, y] = 0;  // Unassign tile
            }
        }
        return false;
    }

    private bool ForwardCheck(int x, int y, int tile)
    {
        return false;
    }

    private List<Vector2Int> GetNeigbours(int x, int y) {
        return null;
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

        return false;
    }

    private bool isMapValid()
    {

        return false;
    }
}
