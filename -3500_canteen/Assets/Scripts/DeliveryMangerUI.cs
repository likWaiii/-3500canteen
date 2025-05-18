using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class DeliveryManagerUI : MonoBehaviour
{
    [SerializeField] private List<DeliveryManagerSingleUI> slotUIs;

    private void Start()
    {
        DeliveryManager.Instance.OnRecipeSpawned += OnRecipeChanged;
        DeliveryManager.Instance.OnRecipeComplete += OnRecipeChanged;
        DeliveryManager.Instance.OnRecipeFailed += OnRecipeChanged;

        UpdateVisual();
    }
    private void OnRecipeChanged(object sender, EventArgs e)
    {
        UpdateVisual();
    }
    private void UpdateVisual()
    {
        List<WaitingRecipe> waitingRecipes = DeliveryManager.Instance.GetWaitingRecipeList();

        for (int i = 0; i < slotUIs.Count; i++)
        {
            if (i < waitingRecipes.Count && waitingRecipes[i] != null)
            {
                slotUIs[i].SetWaitingRecipe(waitingRecipes[i]);
            }
            else
            {
                slotUIs[i].ClearUI();  // 没有订单的槽位清空内容
            }
        }
    }
}
