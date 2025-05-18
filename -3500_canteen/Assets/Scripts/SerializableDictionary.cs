using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SerializableDictionary<TKey, TValue>
    : Dictionary<TKey, TValue>,
        ISerializationCallbackReceiver
{
    [SerializeField]
    private List<TKey> keys = new List<TKey>();

    [SerializeField]
    private List<TValue> values = new List<TValue>();

    public void OnBeforeSerialize()
    {
        keys.Clear();
        values.Clear();

        foreach (var kvp in this)
        {
            keys.Add(kvp.Key);
            values.Add(kvp.Value);
        }
    }

    public void OnAfterDeserialize()
    {
        this.Clear();

        if (keys.Count != values.Count)
        {
            throw new Exception(
                $"SerializableDictionary: Key count ({keys.Count}) does not match value count ({values.Count})."
            );
        }

        for (int i = 0; i < keys.Count; i++)
        {
            this[keys[i]] = values[i];
        }
    }
}
