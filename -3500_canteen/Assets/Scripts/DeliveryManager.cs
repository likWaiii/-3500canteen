using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DeliveryManager : MonoBehaviour
{

    public event EventHandler OnRecipeSpawned;
    public event EventHandler OnRecipeComplete;
    public event EventHandler OnRecipeSucess;
    public event EventHandler OnRecipeFailed;

    public static DeliveryManager Instance { get; private set; }

    [SerializeField] private RecipeListSO recipeListSO;

    private List<WaitingRecipe> waitingRecipeList;
    private float spawnRecipeTimer;
    private float spawnRecipeTimerMax = 4f;
    private int waitingRecipeMax = 4;
    private int successfulRecipesAmount;

    private void Awake()
    {
        Instance = this;

        // 初始化为固定 4 槽位，初始为 null
        waitingRecipeList = new List<WaitingRecipe>(new WaitingRecipe[4]);
    }

    private bool HasEmptySlot()
    {
        foreach (var recipe in waitingRecipeList)
        {
            if (recipe == null) return true;
        }
        return false;
    }

    public void TrySubmitItemToSlot(int slotIndex, KitchenObjectOS item)
    {
        if (slotIndex < 0 || slotIndex >= waitingRecipeList.Count) return;

        WaitingRecipe wr = waitingRecipeList[slotIndex];
        if (wr == null || wr.isCompleted) return;

        RecipeSO recipeSO = wr.recipeSO;

        if (!recipeSO.kitchenObjectsOSList.Contains(item))
        {
            Debug.Log($"❌ 提交失败：订单 {slotIndex} 不需要 {item.name}");
            OnRecipeFailed?.Invoke(this, EventArgs.Empty);
            return;
        }

        if (wr.submittedSet.Contains(item))
        {
            Debug.Log($"⚠️ 已经交过 {item.name}，不能重复提交");
            return;
        }

        wr.submittedSet.Add(item);
        Debug.Log($"✅ 成功提交 {item.name} 给订单槽 {slotIndex}");

        // 判断是否所有菜都交完
        bool allSubmitted = true;
        foreach (KitchenObjectOS required in recipeSO.kitchenObjectsOSList)
        {
            if (!wr.submittedSet.Contains(required))
            {
                allSubmitted = false;
                break;
            }
        }

        if (allSubmitted)
        {
            wr.isCompleted = true;
            waitingRecipeList[slotIndex] = null;
            successfulRecipesAmount++;

            OnRecipeComplete?.Invoke(this, EventArgs.Empty);
            OnRecipeSucess?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            // 只提交了其中一个菜，不触发完成事件，但可以手动刷新 UI
            OnRecipeSpawned?.Invoke(this, EventArgs.Empty); // 或定义一个 OnRecipeProgressUpdated
        }
    }

    private void Update()
    {
        spawnRecipeTimer -= Time.deltaTime;

        if (spawnRecipeTimer < 0f)
        {
            spawnRecipeTimer = spawnRecipeTimerMax;

            if (KitchenGameManager.Instance.IsGamePlaying() && HasEmptySlot())
            {
                RecipeSO waitingRecipeSO = recipeListSO.recipeSOList[
                    UnityEngine.Random.Range(0, recipeListSO.recipeSOList.Count)
                ];
                WaitingRecipe newWaitingRecipe = new WaitingRecipe(waitingRecipeSO);

                // ✅ 随机选择一个空槽位插入订单
                List<int> emptyIndices = new List<int>();
                for (int i = 0; i < waitingRecipeList.Count; i++)
                {
                    if (waitingRecipeList[i] == null)
                    {
                        emptyIndices.Add(i);
                    }
                }

                if (emptyIndices.Count > 0)
                {
                    int randomIndex = emptyIndices[UnityEngine.Random.Range(0, emptyIndices.Count)];
                    waitingRecipeList[randomIndex] = newWaitingRecipe;
                    OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        // ✅ 倒计时逻辑
        for (int i = 0; i < waitingRecipeList.Count; i++)
        {
            WaitingRecipe wr = waitingRecipeList[i];
            if (wr == null || wr.isCompleted) continue;

            wr.remainingTime -= Time.deltaTime;

            if (wr.remainingTime <= 0f)
            {
                // 超时失败
                OnRecipeFailed?.Invoke(this, EventArgs.Empty);
                waitingRecipeList[i] = null;
            }
        }
    }

    public void DeliverRecipe(PlateKitchenObject plateKitchenObject)
    {
        for (int i = 0; i < waitingRecipeList.Count; i++)
        {
            WaitingRecipe wr = waitingRecipeList[i];
            if (wr == null || wr.isCompleted) continue;

            RecipeSO waitingRecipeSO = wr.recipeSO;

            if (waitingRecipeSO.kitchenObjectsOSList.Count == plateKitchenObject.GetKitchenObjectOSList().Count)
            {
                bool plateContentMatchesRecipe = true;

                foreach (KitchenObjectOS recipeKitchenObjectSO in waitingRecipeSO.kitchenObjectsOSList)
                {
                    bool ingredientsFound = false;

                    foreach (KitchenObjectOS plateKitchenObjectSO in plateKitchenObject.GetKitchenObjectOSList())
                    {
                        if (plateKitchenObjectSO == recipeKitchenObjectSO)
                        {
                            ingredientsFound = true;
                            break;
                        }
                    }

                    if (!ingredientsFound)
                    {
                        plateContentMatchesRecipe = false;
                        break;
                    }
                }

                if (plateContentMatchesRecipe)
                {
                    successfulRecipesAmount++;
                    wr.isCompleted = true;
                    waitingRecipeList[i] = null;

                    OnRecipeComplete?.Invoke(this, EventArgs.Empty);
                    OnRecipeSucess?.Invoke(this, EventArgs.Empty);
                    return;
                }
            }
        }

        // 所有订单都不匹配，视为失败交付
        OnRecipeFailed?.Invoke(this, EventArgs.Empty);
    }

    public List<WaitingRecipe> GetWaitingRecipeList()
    {
        return waitingRecipeList;
    }

    public int GetSuccessfulRecipesAmount()
    {
        return successfulRecipesAmount;
    }
}