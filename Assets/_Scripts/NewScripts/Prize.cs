using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Prize : MonoBehaviour
{
    [SerializeField] private TMP_Text prizeText;
    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void UpdateText(string name)
    {
        prizeText.SetText(name);
    }

    public void MoveTo(RectTransform target)
    {
        StartCoroutine(MoveToTarget(target));
    }

    private IEnumerator MoveToTarget(RectTransform target)
    {
        float duration = 0.3f;
        float elapsed = 0f;

        Vector3 startPos = rectTransform.position;
        Vector3 endPos = target.localPosition;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            rectTransform.localPosition = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        // Re-parent after the move is done
        rectTransform.SetParent(target, worldPositionStays: false);
        rectTransform.localPosition = Vector3.zero;
        rectTransform.localRotation = Quaternion.identity;
        rectTransform.localScale = Vector3.one;
    }

}
