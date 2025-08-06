using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class MessagePopupManager : MonoBehaviour
{
    [SerializeField] private MessagePopup messagePopupPrefab;
    [SerializeField] private Transform messageParent;
    [SerializeField] private Transform floatUpTarget;
    [SerializeField] private int maxMessages = 3;
    [SerializeField] private float disapearTime = 3f;
    [SerializeField] private float floatUpTimeDelay = 0.5f;
    [SerializeField] private float distanceBetweenPopups = 5;
    [SerializeField] private string[] messages;

    private List<MessagePopup> popupClones = new List<MessagePopup>();

    public void ShowPopup()
    {

        string message = messages[Random.Range(0, messages.Length)];
        MessagePopup popupClone = Instantiate(messagePopupPrefab, transform.position, Quaternion.identity, messageParent);

        InitPopupData(popupClone);
        popupClone.Show(message);
        popupClones.Add(popupClone);

        AddOffset();

        popupClone.destroyEvent += OnPopupDestroyed;

        
        if (popupClones.Count > maxMessages)
        {
            MessagePopup firstMessage = popupClones[0];
            firstMessage.destroyEvent -= OnPopupDestroyed;
            popupClones.Remove(firstMessage);
            StartCoroutine(firstMessage.Disapear(0));

        }
    }
    private void AddOffset()
    {
        if (popupClones.Count < 2) return;

        MessagePopup newPopup = popupClones[popupClones.Count - 1];
        RectTransform newRect = newPopup.GetComponent<RectTransform>();
        RectTransform parentRect = messageParent.GetComponent<RectTransform>();

        float popupHeight = newRect.rect.height;
        float verticalSpacing = popupHeight + 10f;

        // Find the lowest popup y position (the one with smallest anchoredPosition.y)
        float lowestY = 0f;
        foreach (var popup in popupClones)
        {
            RectTransform rt = popup.GetComponent<RectTransform>();
            if (rt.anchoredPosition.y < lowestY)
            {
                lowestY = rt.anchoredPosition.y;
            }
        }

        // Calculate the new popup's start y position (below the lowest)
        float finalY = lowestY - verticalSpacing;

        // Clamp to bottom limit
        float bottomLimitY = -parentRect.rect.height / 2f + popupHeight / 2f;

        if (finalY < bottomLimitY)
        {
            finalY = bottomLimitY;
        }

        newRect.anchoredPosition = new Vector2(0f, finalY);
    }

    private void OnPopupDestroyed(MessagePopup obj)
    {
        popupClones.Remove(obj);
        obj.destroyEvent -= OnPopupDestroyed;
    }
    private void InitPopupData(MessagePopup popup)
    {
        PopupData popupData = new PopupData
        {
            FloatUpTime = floatUpTimeDelay,
            TargetPoint = floatUpTarget,
            DisapearTime = disapearTime
        };
        popup.Init(popupData);
    }
}
