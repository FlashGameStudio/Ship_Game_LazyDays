using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MovementSystem : MonoBehaviour
{
    private BFSResult movementRange = new BFSResult();
    public List<Vector3Int> currentPath = new List<Vector3Int>();
    private List<Vector3Int> originalPath = new List<Vector3Int>();

    public void HideRange(HexGrid hexGrid)
    {
        if (movementRange.visitedNodesDict == null)
            return;

        foreach (Vector3Int hexPosition in movementRange.GetRangePositions())
        {
            hexGrid.GetTileAt(hexPosition).DisableHighlight();
        }
        movementRange = new BFSResult();
    }

    public void ShowRange(Unit selectedUnit, HexGrid hexGrid)
    {
        CalculateRange(selectedUnit, hexGrid);

        Vector3Int unitPos = hexGrid.GetClosestHex(selectedUnit.transform.position);

        foreach (Vector3Int hexPosition in movementRange.GetRangePositions())
        {
            if (unitPos == hexPosition)
                continue;
            hexGrid.GetTileAt(hexPosition).EnableHighlight();
        }
    }

    public void PrepareRangeForNPC(Unit playerUnit, HexGrid hexGrid)
    {
       CalculateRangeForNPC(playerUnit, hexGrid);

    }

    public void CalculateRange(Unit selectedUnit, HexGrid hexGrid)
    {
        movementRange = GraphSearch.BFSGetRange(hexGrid, hexGrid.GetClosestHex(selectedUnit.transform.position), selectedUnit.MovementPoints);
    }

    public void CalculateRangeNew(Vector3 position, HexGrid hexGrid)
    {
        movementRange = GraphSearch.BFSGetRange(hexGrid, hexGrid.GetClosestHex(position), 100);
    }

    public void CalculateRangeForNPC(Unit playerUnit, HexGrid hexGrid)
    {
        movementRange = GraphSearch.BFSGetRangeForNPC(hexGrid, hexGrid.GetClosestHex(playerUnit.transform.position));
    }

    public void CalculateRangeForEnemyPC(NPC enemyPlayer, HexGrid hexGrid)
    {
        movementRange = GraphSearch.BFSGetRangeForNPC(hexGrid, hexGrid.GetClosestHex(enemyPlayer.transform.position));
    }

    public void ShowPath(Vector3Int selectedHexPosition, HexGrid hexGrid)
    {
        if (movementRange.GetRangePositions().Contains(selectedHexPosition))
        {
            foreach (Vector3Int hexPosition in currentPath)
            {
                //hexGrid.GetTileAt(hexPosition).DisableHighlight();
                hexGrid.GetTileAt(hexPosition).ResetHighlight();
            }
            currentPath = movementRange.GetPathTo(selectedHexPosition);
            foreach (Vector3Int hexPosition in currentPath)
            {
                //hexGrid.GetTileAt(hexPosition).EnableHighlight();
                hexGrid.GetTileAt(hexPosition).HighlightPath();
            }
            originalPath = currentPath;
        }
    }

    public List<Vector3Int> HighlightAndShowPath(Vector3Int selectedHexPosition, HexGrid hexGrid)
    {
        if (movementRange.GetRangePositions().Contains(selectedHexPosition))
        {
            foreach (Vector3Int hexPosition in currentPath)
            {
                hexGrid.GetTileAt(hexPosition).DisableHighlight();
                hexGrid.GetTileAt(hexPosition).ResetHighlight();
            }
            // this part finds path to the target
            currentPath = movementRange.GetPathTo(selectedHexPosition);
            foreach (Vector3Int hexPosition in currentPath)
            {
                hexGrid.GetTileAt(hexPosition).EnableHighlight();
                hexGrid.GetTileAt(hexPosition).HighlightPath();
            }
            originalPath = currentPath;
        }
        return currentPath;
    }

    /// <summary>
    /// Exists to move ship itself
    /// </summary>
    /// <param name="selectedUnit"></param>
    /// <param name="hexGrid"></param>
    public void MoveUnit(Unit selectedUnit, HexGrid hexGrid)
    {
        //Debug.Log("Moving unit " + selectedUnit.name);
        selectedUnit.MoveThroughPath(currentPath.Select(pos => hexGrid.GetTileAt(pos).transform.position).ToList());

    }

    /// <summary>
    /// New Method taking Vector3 path
    /// </summary>
    /// <param name="captain"></param>
    /// <param name="hexGrid"></param>
    /// <param name="player"></param>
    /// <param name="currentPath2"></param>
    public void MoveCaptainFromNPC(Unit captain, HexGrid hexGrid, string player, List<Vector3> currentPath2)
    {       
        captain.MoveCaptainBackThroughPath(currentPath2, 0f);

    }
    public bool IsHexInRange(Vector3Int hexPosition)
    {
        return movementRange.IsHexPositionInRange(hexPosition);
    }
}
