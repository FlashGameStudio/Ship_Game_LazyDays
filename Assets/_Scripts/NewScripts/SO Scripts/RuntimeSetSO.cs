using System.Collections.Generic;
using UnityEngine;

public abstract class RuntimeSetSO<T> : ScriptableObject
{
    public List<T> Items = new List<T>();

    public virtual void AddItem(T item)
    {
        if (Items.Contains(item)) return;
        Items.Add(item);
    }
    public virtual void RemoveItem(T item)
    {
        Items.Remove(item);
    }
}
