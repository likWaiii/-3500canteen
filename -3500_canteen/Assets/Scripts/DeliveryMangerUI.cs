// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class DeliveryManagerUI : MonoBehaviour
// {
//     [SerializeField]
//     private List<DeliveryManagerSingleUI> slotUIs;

//     private void Start()
//     {
//         // 启动时等待 DeliveryManager 单例初始化完毕
//         StartCoroutine(SetupWhenReady());
//     }

//     private IEnumerator SetupWhenReady()
//     {
//         // 等待直到 DeliveryManager.Instance 被赋值
//         yield return new WaitUntil(() => DeliveryManager.Instance != null);

//         // 订阅事件
//         DeliveryManager.Instance.OnRecipeSpawned += OnRecipeChanged;
//         DeliveryManager.Instance.OnRecipeSpawned += HandleRecipeChanged;
//         DeliveryManager.Instance.OnRecipeComplete += HandleRecipeChanged;
//         DeliveryManager.Instance.OnRecipeFailed += HandleRecipeChanged;

//         // 首次刷新 UI
//         UpdateVisual();
//     }

//     private void OnRecipeChanged(object sender, EventArgs e)
//     {
//         UpdateVisual();
//     }

//     private void OnDisable()
//     {
//         // 取消订阅，防止内存泄漏
//         if (DeliveryManager.Instance == null)
//             return;
//         DeliveryManager.Instance.OnRecipeSpawned -= OnRecipeChanged;
//         DeliveryManager.Instance.OnRecipeSpawned -= HandleRecipeChanged;
//         DeliveryManager.Instance.OnRecipeComplete -= HandleRecipeChanged;
//         DeliveryManager.Instance.OnRecipeFailed -= HandleRecipeChanged;
//     }

//     private void HandleRecipeChanged(object sender, EventArgs e)
//     {
//         UpdateVisual();
//     }

//     private void UpdateVisual()
//     {
//         var dm = DeliveryManager.Instance;
//         if (dm == null)
//             return;

//         var waitingRecipes = dm.GetWaitingRecipeList();
//         if (waitingRecipes == null)
//             return;

//         for (int i = 0; i < slotUIs.Count; i++)
//         {
//             var ui = slotUIs[i];
//             if (ui == null)
//                 continue;

//             if (
//                 i < waitingRecipes.Count
//                 && waitingRecipes[i] != null
//                 && !waitingRecipes[i].isCompleted
//             )
//             {
//                 ui.SetWaitingRecipe(waitingRecipes[i]);
//             }
//             else
//             {
//                 ui.ClearUI(); // 清理已完成的订单
//             }
//         }
//     }
// }
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeliveryManagerUI : MonoBehaviour
{
    [SerializeField]
    private List<DeliveryManagerSingleUI> slotUIs;

    private void Start()
    {
        // 启动时等待 DeliveryManager 单例初始化完毕
        StartCoroutine(SetupWhenReady());
    }

    private IEnumerator SetupWhenReady()
    {
        // 等待直到 DeliveryManager.Instance 被赋值
        yield return new WaitUntil(() => DeliveryManager.Instance != null);

        // 订阅事件
        DeliveryManager.Instance.OnRecipeSpawned += OnAnyRecipeChanged;
        DeliveryManager.Instance.OnRecipeComplete += OnAnyRecipeChanged;
        DeliveryManager.Instance.OnRecipeFailed += OnAnyRecipeChanged;

        // 首次刷新 UI
        UpdateVisual();
    }

    private void OnAnyRecipeChanged(object sender, EventArgs e)
    {
        UpdateVisual();
    }

    private void OnDisable()
    {
        // 取消订阅，防止内存泄漏
        if (DeliveryManager.Instance == null)
            return;
        DeliveryManager.Instance.OnRecipeSpawned -= OnAnyRecipeChanged;
        DeliveryManager.Instance.OnRecipeComplete -= OnAnyRecipeChanged;
        DeliveryManager.Instance.OnRecipeFailed -= OnAnyRecipeChanged;
    }

    private void UpdateVisual()
    {
        var dm = DeliveryManager.Instance;
        if (dm == null)
            return;

        var waitingRecipes = dm.GetWaitingRecipeList();
        if (waitingRecipes == null)
            return;

        for (int i = 0; i < slotUIs.Count; i++)
        {
            var ui = slotUIs[i];
            if (ui == null)
                continue;

            if (
                i < waitingRecipes.Count
                && waitingRecipes[i] != null
                && !waitingRecipes[i].isCompleted
            )
            {
                ui.SetWaitingRecipe(waitingRecipes[i]);
            }
            else
            {
                ui.ClearUI(); // 清理已完成的订单
            }
        }
    }
}
