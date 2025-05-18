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
    private int totalEarnedValue = 0;
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

        // ✅ 统计该菜在订单中总共需要几份
        int requiredCount = 0;
        foreach (var obj in recipeSO.kitchenObjectsOSList)
        {
            if (obj.objectName == item.objectName) requiredCount++;
        }

        if (requiredCount == 0)
        {
            Debug.Log($"❌ 提交失败：订单 {slotIndex} 不需要 {item.objectName}");
            OnRecipeFailed?.Invoke(this, EventArgs.Empty);
            return;
        }

        // ✅ 获取当前已经提交的数量
        int submittedCount = 0;
        wr.submittedDict.TryGetValue(item, out submittedCount);

        if (submittedCount >= requiredCount)
        {
            Debug.Log($"⚠️ 已提交足够数量的 {item.objectName}，不能再交");
            return;
        }

        // ✅ 提交成功
        if (wr.submittedDict.ContainsKey(item))
        {
            wr.submittedDict[item]++;
        }
        else
        {
            wr.submittedDict[item] = 1;
        }

        Debug.Log($"✅ 成功提交 {item.objectName} 给订单槽 {slotIndex}");

        // ✅ 判断是否所有菜都已交完
        bool allSubmitted = true;

        Dictionary<KitchenObjectOS, int> requiredDict = new Dictionary<KitchenObjectOS, int>(new KitchenObjectOSComparer());

        foreach (var obj in recipeSO.kitchenObjectsOSList)
        {
            if (requiredDict.ContainsKey(obj))
                requiredDict[obj]++;
            else
                requiredDict[obj] = 1;
        }

        foreach (var kvp in requiredDict)
        {
            int already = 0;
            wr.submittedDict.TryGetValue(kvp.Key, out already);
            if (already < kvp.Value)
            {
                allSubmitted = false;
                break;
            }
        }

        if (allSubmitted)
        {
            wr.isCompleted = true;
            StartCoroutine(DelayRemoveCompletedOrder(slotIndex, 2f));
            successfulRecipesAmount++;

            // ✅ 加分逻辑（加在这里）
            int recipeValue = 0;
            foreach (var obj in wr.recipeSO.kitchenObjectsOSList)
            {
                recipeValue += obj.value;
            }
            totalEarnedValue += recipeValue;

            Debug.Log($"🎉 订单 {slotIndex} 完成！");
            OnRecipeComplete?.Invoke(this, EventArgs.Empty);
            OnRecipeSucess?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            Debug.Log($"🔄 订单 {slotIndex} 进度更新");
            OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
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
    public int GetTotalEarnedValue()
    {
        return totalEarnedValue;
    }
    private IEnumerator DelayRemoveCompletedOrder(int slotIndex, float delay)
    {
        yield return new WaitForSeconds(delay);
        waitingRecipeList[slotIndex] = null;
        OnRecipeSpawned?.Invoke(this, EventArgs.Empty); // 强制刷新 UI 显示空槽
    }
}