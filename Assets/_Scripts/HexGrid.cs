using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct Vector3S
{
    public float x;
    public float y;
    public float z;

    public Vector3S(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public override bool Equals(object obj)
    {
        if (!(obj is Vector3S))
        {
            return false;
        }

        var s = (Vector3S)obj;
        return x == s.x &&
               y == s.y &&
               z == s.z;
    }

    public override int GetHashCode()
    {
        var hashCode = 373119288;
        hashCode = hashCode * -1521134295 + x.GetHashCode();
        hashCode = hashCode * -1521134295 + y.GetHashCode();
        hashCode = hashCode * -1521134295 + z.GetHashCode();
        return hashCode;
    }

    public Vector3 ToVector3()
    {
        return new Vector3(x, y, z);
    }

    public static bool operator ==(Vector3S a, Vector3S b)
    {
        return a.x == b.x && a.y == b.y && a.z == b.z;
    }

    public static bool operator !=(Vector3S a, Vector3S b)
    {
        return a.x != b.x && a.y != b.y && a.z != b.z;
    }

    public static implicit operator Vector3(Vector3S x)
    {
        return new Vector3(x.x, x.y, x.z);
    }

    public static implicit operator Vector3S(Vector3 x)
    {
        return new Vector3S(x.x, x.y, x.z);
    }
}

public class HexGrid : MonoBehaviour
{
    public const string TILE_FOLDER = "Tiles_Prefabs";
    Dictionary<Vector3Int, Hex> hexTileDict = new Dictionary<Vector3Int, Hex>();
    Dictionary<Vector3Int, List<Vector3Int>> hexTileNeighboursDict = new Dictionary<Vector3Int, List<Vector3Int>>();

    public List<Transform> waterTiles = new List<Transform>();
    //public List<HexTile> map = new List<HexTile>();
    private void Start()
    {
        //CreateHex();
        LoadMap();

        foreach (Hex hex in FindObjectsOfType<Hex>())
        {
            hexTileDict[hex.HexCoords] = hex;
        }
    }

    public void LoadMap()
    {
        GameObject ground = GameObject.Find("Ground");
        GameObject hexGrass = (GameObject)Resources.Load($"{TILE_FOLDER}\\HexGrass");
        GameObject hexWater = (GameObject)Resources.Load($"{TILE_FOLDER}\\HexWater");
        GameObject hexForest = (GameObject)Resources.Load($"{TILE_FOLDER}\\HexForest");
        GameObject hexMountain = (GameObject)Resources.Load($"{TILE_FOLDER}\\HexMountain");
        GameObject hexWaterRocks = (GameObject)Resources.Load($"{TILE_FOLDER}\\HexWaterRocks");
        GameObject hexForestRocks = (GameObject)Resources.Load($"{TILE_FOLDER}\\HexForestRocks");
        
        string loadedText = string.Empty;

        List<GameManager.HexTile> loadedMap = GameManager.Instance.LoadTileMap(); //Newtonsoft.Json.JsonConvert.DeserializeObject<List<HexTile>>(jsonMapFile.ToString());

        foreach (var tile in loadedMap)
        {
            switch (tile.TileName)
            {
                case "HexGrass":
                    InstantiateMapTile(hexGrass, tile.TileName, tile.TileCoords.ToVector3(), ground.transform);
                    //GameObject hexGrassGO = Instantiate(hexGrass, tile.TileCoords.ToVector3(), Quaternion.Euler(0, 0, 0));
                    //hexGrassGO.gameObject.name = tile.TileName;
                    //hexGrassGO.gameObject.transform.SetParent(ground.transform);
                    break;
                case "HexWater":
                    GameObject waterTile = InstantiateMapTile(hexWater, tile.TileName, tile.TileCoords.ToVector3(), ground.transform);
                    waterTiles.Add(waterTile.transform);
                    break;
                case "HexForest":
                    InstantiateMapTile(hexForest, tile.TileName, tile.TileCoords.ToVector3(), ground.transform);
                    break;
                case "HexMountain":
                    InstantiateMapTile(hexMountain, tile.TileName, tile.TileCoords.ToVector3(), ground.transform);
                    break;
                case "HexWaterRocks":
                    InstantiateMapTile(hexWaterRocks, tile.TileName, tile.TileCoords.ToVector3(), ground.transform);
                    break;
                case "HexForestRocks":
                    InstantiateMapTile(hexForestRocks, tile.TileName, tile.TileCoords.ToVector3(), ground.transform);
                    break;
                    
                default:
                    GameObject defaultGO = Instantiate(hexGrass, tile.TileCoords.ToVector3(), Quaternion.Euler(0, 0, 0));
                    defaultGO.gameObject.name = tile.TileName;
                    defaultGO.gameObject.transform.SetParent(ground.transform);
                    break;
            } 
        }
    }

    public GameObject InstantiateMapTile(GameObject tile, string tileName, Vector3 vector, Transform parent)
    {
        GameObject tileGO = Instantiate(tile, vector, Quaternion.Euler(0, 0, 0));
        tileGO.gameObject.name = tileName;
        tileGO.gameObject.transform.SetParent(parent);
        return tileGO;
    }

    public void CreateHex()
    {
        GameObject ground = GameObject.Find("Ground");

        GameObject hexGrass = (GameObject)Resources.Load($"{TILE_FOLDER}\\HexGrass");
        GameObject hexGrassGO = Instantiate(hexGrass, new Vector3(1f, 0f, 1.73f), Quaternion.Euler(0, 0, 0));
        hexGrassGO.gameObject.name = "HexGrass";
        hexGrassGO.gameObject.transform.SetParent(ground.transform);
        //tempNPCGO.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
        //tempNPCGO.GetComponent<NPC>().InitializeNPC(positionIndex, category, quality);

        GameObject hexWater = (GameObject)Resources.Load($"{TILE_FOLDER}\\HexWater");
        GameObject hexWaterGO = Instantiate(hexWater, new Vector3(3f, 0f, -1.73f), Quaternion.Euler(0, 0, 0));
        hexWaterGO.gameObject.name = "HexWater";
        hexWaterGO.gameObject.transform.SetParent(ground.transform);

        for (float z = -5.19f; z >= -24.22f; z -= 3.46f)
        {
            for (int i = 0; i < 20; i++)
            {
                hexWaterGO = Instantiate(hexWater, new Vector3(-15f + 2 * i, 0f, z), Quaternion.Euler(0, 0, 0));
                hexWaterGO.gameObject.name = $"HexWater";
                hexWaterGO.gameObject.transform.SetParent(ground.transform);
                hexWaterGO = Instantiate(hexWater, new Vector3(-16f + 2 * i, 0f, z-1.73f), Quaternion.Euler(0, 0, 0));
                hexWaterGO.gameObject.name = $"HexWater";
                hexWaterGO.gameObject.transform.SetParent(ground.transform);
            }
        }
        for (float z = 8.65f; z <= 27.68f; z += 3.46f)
        {
            for (int i = 0; i < 20; i++)
            {
                hexWaterGO = Instantiate(hexWater, new Vector3(-15f + 2 * i, 0f, z), Quaternion.Euler(0, 0, 0));
                hexWaterGO.gameObject.name = $"HexWater";
                hexWaterGO.gameObject.transform.SetParent(ground.transform);
                hexWaterGO = Instantiate(hexWater, new Vector3(-16f + 2 * i, 0f, z - 1.73f), Quaternion.Euler(0, 0, 0));
                hexWaterGO.gameObject.name = $"HexWater";
                hexWaterGO.gameObject.transform.SetParent(ground.transform);
            }
        }
    }

    public Hex GetTileAt(Vector3Int hexCoordinates)
    {
        Hex result = null;
        hexTileDict.TryGetValue(hexCoordinates, out result);
        return result;
    }

    public List<Vector3Int> GetNeighboursFor(Vector3Int hexCoordinates)
    {
        if (hexTileDict.ContainsKey(hexCoordinates) == false)
            return new List<Vector3Int>();

        if (hexTileNeighboursDict.ContainsKey(hexCoordinates))
            return hexTileNeighboursDict[hexCoordinates];

        hexTileNeighboursDict.Add(hexCoordinates, new List<Vector3Int>());

        foreach (Vector3Int direction in Direction.GetDirectionList(hexCoordinates.z))
        {
            if (hexTileDict.ContainsKey(hexCoordinates + direction))
            {
                hexTileNeighboursDict[hexCoordinates].Add(hexCoordinates + direction);
            }
        }
        return hexTileNeighboursDict[hexCoordinates];
    }

    public Vector3Int GetClosestHex(Vector3 worldposition)
    {
        worldposition.y = 0;
        return HexCoordinates.ConvertPositionToOffset(worldposition);
    }

    public static Vector3Int GetClosestHexCoords(Vector3 worldposition)
    {
        worldposition.y = 0;
        return HexCoordinates.ConvertPositionToOffset(worldposition);
    }
}

public static class Direction
{
    public static List<Vector3Int> directionsOffsetOdd = new List<Vector3Int>
    {
        new Vector3Int(-1,0,1), //N1
        new Vector3Int(0,0,1), //N2
        new Vector3Int(1,0,0), //E
        new Vector3Int(0,0,-1), //S2
        new Vector3Int(-1,0,-1), //S1
        new Vector3Int(-1,0,0), //W
    };

    public static List<Vector3Int> directionsOffsetEven = new List<Vector3Int>
    {
        new Vector3Int(0,0,1), //N1
        new Vector3Int(1,0,1), //N2
        new Vector3Int(1,0,0), //E
        new Vector3Int(1,0,-1), //S2
        new Vector3Int(0,0,-1), //S1
        new Vector3Int(-1,0,0), //W
    };

    public static List<Vector3Int> GetDirectionList(int z)
        => z % 2 == 0 ? directionsOffsetEven : directionsOffsetOdd;
}
