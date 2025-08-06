using System.Collections;
using TMPro;
using UnityEngine;
using System;
using System.Drawing.Text;
public class MessagePopup : MonoBehaviour
{
    const string FADE_OUT = "FadeOut";
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Animator animator;
    [SerializeField] private float floatUpSpeed = 10f;

    private PopupData popupData;
    public Action<MessagePopup> destroyEvent;

    public void Init(PopupData newPopupData)
    {
        popupData = newPopupData;
    }
    public void Show(string info)
    {
        messageText.SetText(info);
        StartCoroutine(FloatUp());
        StartCoroutine(Disapear(popupData.DisapearTime));
    }
    public IEnumerator Disapear(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        animator.SetTrigger(FADE_OUT);
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        Destroy(this.gameObject);
    }
    void OnDestroy()
    {
        destroyEvent?.Invoke(this);
    }
    IEnumerator FloatUp()
    {
        yield return new WaitForSeconds(popupData.FloatUpTime);

        if (popupData.TargetPoint == null)
            yield break;

        Vector3 startPos = transform.localPosition;
        Vector3 endPos = popupData.TargetPoint.localPosition;
        float elapsed = 0f;
        float duration = Vector3.Distance(startPos, endPos) / floatUpSpeed;

        while (elapsed < duration)
        {
            transform.localPosition = Vector3.Lerp(startPos, endPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = endPos; // Snap to final
    }

}
public struct PopupData
{
    public float DisapearTime;
    public float FloatUpTime;
    public Transform TargetPoint;
}
