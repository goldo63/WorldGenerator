using System;
using System.IO;
using System.Text;
using System.Xml;
using UnityEngine;

public class CSVWriter : MonoBehaviour
{
    public string csvName;
    public MapLoader mapLoader; // Reference to the MapLoader script

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            // Save the current map
            int[,] currentMap = mapLoader.GetCurrentMap();

            if (currentMap != null)
            {
                //SaveCSV(currentMap, csvName);
                SaveTMX(currentMap, csvName);
            }
            else
            {
                Debug.LogError("Current map is null. Cannot save.");
            }
        }
    }

    public void SaveCSV(int[,] map, string fileName)
    {
        StringBuilder csvContent = new StringBuilder();

        int width = map.GetLength(0);
        int height = map.GetLength(1);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                csvContent.Append(map[x, y]);

                if (x < width - 1)
                {
                    csvContent.Append(",");
                }
            }
            csvContent.AppendLine();
        }

        // Define the path to save the file
        string filePath = Path.Combine(Application.dataPath, "Resources/Export", "EXTRACT_" + fileName + ".csv");

        try
        {
            File.WriteAllText(filePath, csvContent.ToString());
            Debug.Log("CSV file saved to " + filePath);
        }
        catch (Exception ex)
        {
            Debug.LogError("Error saving CSV file: " + ex.Message);
        }
    }

    public void SaveTMX(int[,] map, string fileName)
    {
        int width = map.GetLength(0);
        int height = map.GetLength(1);

        XmlDocument xmlDoc = new XmlDocument();

        // Create XML declaration
        XmlDeclaration xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
        xmlDoc.AppendChild(xmlDeclaration);

        // Create map element
        XmlElement mapElement = xmlDoc.CreateElement("map");
        mapElement.SetAttribute("version", "1.0");
        mapElement.SetAttribute("tiledversion", "1.8.4"); // Change to the version of Tiled you are using
        mapElement.SetAttribute("orientation", "orthogonal");
        mapElement.SetAttribute("renderorder", "right-up");
        mapElement.SetAttribute("width", width.ToString());
        mapElement.SetAttribute("height", height.ToString());
        mapElement.SetAttribute("tilewidth", "32"); // Change tile width as needed
        mapElement.SetAttribute("tileheight", "32"); // Change tile height as needed
        xmlDoc.AppendChild(mapElement);

        // Create tileset element
        XmlElement tilesetElement = xmlDoc.CreateElement("tileset");
        tilesetElement.SetAttribute("firstgid", "1");
        tilesetElement.SetAttribute("source", Application.dataPath + "/MapInput/BaseTileSet.tsx"); // Specify the source of your tileset
        mapElement.AppendChild(tilesetElement);

        // Create layer element
        XmlElement layerElement = xmlDoc.CreateElement("layer");
        layerElement.SetAttribute("name", "Tile Layer 1");
        layerElement.SetAttribute("width", width.ToString());
        layerElement.SetAttribute("height", height.ToString());
        mapElement.AppendChild(layerElement);

        // Create data element
        XmlElement dataElement = xmlDoc.CreateElement("data");
        dataElement.SetAttribute("encoding", "csv");
        layerElement.AppendChild(dataElement);

        // Add tile data
        StringBuilder csvContent = new StringBuilder();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                csvContent.Append(map[x, y] + 1);

                if (x < width - 1 || y < height - 1)
                {
                    csvContent.Append(",");
                }
            }
        }

        XmlText csvText = xmlDoc.CreateTextNode(csvContent.ToString());
        dataElement.AppendChild(csvText);

        // Define the path to save the file in the Resources folder
        string filePath = Path.Combine(Application.dataPath, "Resources/Export", "EXTRACT_" + fileName + ".tmx");

        try
        {
            xmlDoc.Save(filePath);
            Debug.Log("TMX file saved to " + filePath);
        }
        catch (Exception ex)
        {
            Debug.LogError("Error saving TMX file: " + ex.Message);
        }
    }

}
