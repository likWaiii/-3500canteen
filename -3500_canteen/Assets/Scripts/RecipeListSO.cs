using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//[CreateAssetMenu()]

public class RecipeListSO : ScriptableObject
{
    public List<RecipeSO> recipeSOList;

    /// <summary>
    /// 从所有菜谱里，按 objectName 查找第一个匹配的 KitchenObjectOS
    /// 返回 null 表示没找到
    /// </summary>
    public KitchenObjectOS GetKitchenObjectByName(string objectName)
    {
        // LINQ 版
        return recipeSOList
            .SelectMany(r => r.kitchenObjectsOSList)
            .FirstOrDefault(o => o.objectName == objectName);

        // 或者手写循环版（不需要引用 System.Linq）：
        // foreach (var recipe in recipeSOList)
        // {
        //     foreach (var obj in recipe.kitchenObjectsOSList)
        //     {
        //         if (obj.objectName == objectName)
        //             return obj;
        //     }
        // }
        // return null;
    }
}
