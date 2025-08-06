using System.Collections.Generic;
using UnityEngine;

public class OpenPrizes : MonoBehaviour
{
    [SerializeField] Prize prizePrefab;
    [SerializeField] RectTransform prizeParent; // The HorizontalLayoutGroup
    [SerializeField] Canvas prizeCanvas;        // Canvas to spawn prize under
    [SerializeField] float spawnYOffset = 100f; // Adjustable Y offset
    private List<Prize> prizes = new List<Prize>();

    public void OpenPrize(string prizeName)
    {
        // 1. Instantiate under the canvas
        Prize newPrize = Instantiate(prizePrefab, prizeCanvas.transform);
        newPrize.UpdateText(prizeName);
        prizes.Add(newPrize);

        // 2. Position it at center + Y offset
        RectTransform prizeRect = newPrize.GetComponent<RectTransform>();
        prizeRect.anchoredPosition = new Vector2(0f, spawnYOffset);

        // 3. Start movement to layout group
        newPrize.MoveTo(prizeParent);
    }
    public void CloseChest()
    {
        if (prizes == null || prizes.Count == 0) return;
        foreach (var prize in prizes)
        {
            Destroy(prize.gameObject);
        }
        prizes.Clear();

    }
}
