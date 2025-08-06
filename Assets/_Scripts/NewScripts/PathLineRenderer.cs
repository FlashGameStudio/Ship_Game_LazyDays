using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PathLineRenderer : MonoBehaviour
{
    [SerializeField] private Material lineMaterial;
    [SerializeField] private Color lineColor = Color.cyan;
    [SerializeField] private float lineWidth = 0.2f;

    private Dictionary<string, GameObject> heroPathLines = new();

    private void OnEnable()
    {
        DrawPathEvent.Instance += OnDrawPath;
        HeroUnit.ReachToBaseEvent += OnHeroReachedBase;
        HeroRecalledPathUpdateEvent.Instance += OnHeroRecalledDrawPath;
    }
    private void OnDisable()
    {
        DrawPathEvent.Instance -= OnDrawPath;
        HeroUnit.ReachToBaseEvent -= OnHeroReachedBase;
        HeroRecalledPathUpdateEvent.Instance -= OnHeroRecalledDrawPath;
    }

    public void DrawPath(HeroData heroData)
    {
        if (CannotDrawPath(heroData)) return;

        ClearOldPath(heroData.HeroID);

        GameObject lineObj;
        LineRenderer lineRenderer;
        CreateLineObject(heroData.HeroID, out lineObj, out lineRenderer);

        // Add player position at start
        var fullPath = heroData.CurrentPath.ToList();
        Vector3Int playerPos = heroData.hexGrid.GetClosestHex(heroData.ShipPosition);
        fullPath.Insert(0, playerPos);

        lineRenderer.positionCount = fullPath.Count;
        for (int i = 0; i < fullPath.Count; i++)
        {
            var pos = heroData.hexGrid.GetTileAt(fullPath[i]).transform.position + Vector3.up * 1.2f;
            lineRenderer.SetPosition(i, pos);
        }
        heroPathLines[heroData.HeroID] = lineObj;
    }
    private static bool CannotDrawPath(HeroData heroData)
    {
        return heroData.CurrentPath == null || heroData.CurrentPath.Count == 0 || string.IsNullOrEmpty(heroData.HeroID);
    }

    public void OnHeroRecalledDrawPath(HeroData heroData)
    {
        if (CannotDrawPath(heroData)) return;

        ClearOldPath(heroData.HeroID);

        GameObject lineObj;
        LineRenderer lineRenderer;

        CreateLineObject(heroData.HeroID, out lineObj, out lineRenderer);

        var fullPath = heroData.LineRendereRecallPath.ToList();

        lineRenderer.positionCount = fullPath.Count;
        for (int i = 0; i < fullPath.Count; i++)
        {
            var point = heroData.hexGrid.GetTileAt(fullPath[i]).transform.position + Vector3.up * 1.2f;
            lineRenderer.SetPosition(i, point);
        }
        heroPathLines[heroData.HeroID] = lineObj;
    }

    private void ClearOldPath(string heroID)
    {
        if (heroPathLines.TryGetValue(heroID, out GameObject existing))
        {
            Destroy(existing);
            heroPathLines.Remove(heroID);
        }
    }

    private void CreateLineObject(string heroID, out GameObject lineObj, out LineRenderer lineRenderer)
    {
        lineObj = new GameObject($"PathLine_{heroID}");
        lineObj.transform.SetParent(this.transform);
        lineRenderer = lineObj.AddComponent<LineRenderer>();
        lineRenderer.material = lineMaterial;
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.useWorldSpace = true;
    }

    private void OnHeroReachedBase(HeroUnit hero)
    {
        string heroID = hero.HeroId;

        if (!string.IsNullOrEmpty(heroID) && heroPathLines.TryGetValue(heroID, out GameObject pathState))
        {
            Destroy(pathState);
            heroPathLines.Remove(heroID);
        }
    }
    private void OnDrawPath(HeroData heroData)
    {
        if (!string.IsNullOrEmpty(heroData.HeroID))
        {
            if (heroData.HasRecalled)
            {
                OnHeroRecalledDrawPath(heroData);
            }
            else
            {
                DrawPath(heroData);
            }
        }
    }
}
public struct PathState
{
    public GameObject LineObj;
    public Vector3 StartPoint;
}
