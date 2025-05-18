using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class RecipeSO : ScriptableObject
{

    public List<KitchenObjectOS> kitchenObjectsOSList;
    public string recipeName;

    // ✅ 新增：每张订单的最大时间（比如 30 秒）
    public float maxTime = 30f;

    // ✅ 新增：顾客状态图
    public Sprite calmSprite;   // 等待状态
    public Sprite happySprite;  // 完成状态
    public Sprite angrySprite;  // 超时失败状态
}
