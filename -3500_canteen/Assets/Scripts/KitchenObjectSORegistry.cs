using System.Collections.Generic;
using UnityEngine;

public class KitchenObjectSORegistry : MonoBehaviour
{
    public static KitchenObjectSORegistry Instance { get; private set; }

    [SerializeField]
    private List<KitchenObjectOS> kitchenObjectSOList;

    private Dictionary<string, KitchenObjectOS> kitchenObjectSODict;

    private void Awake()
    {
        Instance = this;

        kitchenObjectSODict = new Dictionary<string, KitchenObjectOS>();
        foreach (var kitchenObjectSO in kitchenObjectSOList)
        {
            kitchenObjectSODict[kitchenObjectSO.objectName] = kitchenObjectSO;
        }
    }

    public KitchenObjectOS GetKitchenObjectSOByName(string objectName)
    {
        if (kitchenObjectSODict.TryGetValue(objectName, out KitchenObjectOS kitchenObjectSO))
        {
            return kitchenObjectSO;
        }

        Debug.LogError($"找不到名为 {objectName} 的KitchenObjectSO!");
        return null;
    }
}
