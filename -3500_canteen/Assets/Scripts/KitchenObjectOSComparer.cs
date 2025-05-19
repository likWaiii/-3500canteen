using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KitchenObjectOSComparer : IEqualityComparer<KitchenObjectOS>
{
    public bool Equals(KitchenObjectOS a, KitchenObjectOS b)
    {
        if (a == null || b == null)
            return false;
        return a.objectName == b.objectName;
    }

    public int GetHashCode(KitchenObjectOS obj)
    {
        return obj.objectName.GetHashCode();
    }
}
