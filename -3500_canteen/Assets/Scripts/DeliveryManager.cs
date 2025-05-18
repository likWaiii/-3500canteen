// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class DeliveryManager : MonoBehaviour
// {
//     public event EventHandler OnRecipeSpawned;
//     public event EventHandler OnRecipeComplete;
//     public event EventHandler OnRecipeSucess;
//     public event EventHandler OnRecipeFailed;

//     public static DeliveryManager Instance { get; private set; }

//     [SerializeField]
//     private RecipeListSO recipeListSO;

//     private List<WaitingRecipe> waitingRecipeList;
//     private float spawnRecipeTimer;
//     private float spawnRecipeTimerMax = 4f;
//     private int waitingRecipeMax = 4;
//     private int successfulRecipesAmount;
//     private int totalEarnedValue = 0;

//     private void Awake()
//     {
//         Instance = this;

//         // 初始化为固定 4 槽位，初始为 null
//         waitingRecipeList = new List<WaitingRecipe>(new WaitingRecipe[4]);
//     }

//     private bool HasEmptySlot()
//     {
//         foreach (var recipe in waitingRecipeList)
//         {
//             if (recipe == null)
//                 return true;
//         }
//         return false;
//     }

//     public void TrySubmitItemToSlot(int slotIndex, KitchenObjectOS item)
//     {
//         if (slotIndex < 0 || slotIndex >= waitingRecipeList.Count)
//             return;

//         WaitingRecipe wr = waitingRecipeList[slotIndex];
//         if (wr == null || wr.isCompleted)
//             return;

//         RecipeSO recipeSO = wr.recipeSO;

//         // ✅ 统计该菜在订单中总共需要几份
//         int requiredCount = 0;
//         foreach (var obj in recipeSO.kitchenObjectsOSList)
//         {
//             if (obj.objectName == item.objectName)
//                 requiredCount++;
//         }

//         if (requiredCount == 0)
//         {
//             Debug.Log($"❌ 提交失败：订单 {slotIndex} 不需要 {item.objectName}");
//             OnRecipeFailed?.Invoke(this, EventArgs.Empty);
//             return;
//         }

//         // ✅ 获取当前已经提交的数量
//         int submittedCount = 0;
//         wr.submittedDict.TryGetValue(item, out submittedCount);

//         if (submittedCount >= requiredCount)
//         {
//             Debug.Log($"⚠️ 已提交足够数量的 {item.objectName}，不能再交");
//             return;
//         }

//         // ✅ 提交成功
//         if (wr.submittedDict.ContainsKey(item))
//         {
//             wr.submittedDict[item]++;
//         }
//         else
//         {
//             wr.submittedDict[item] = 1;
//         }

//         Debug.Log($"✅ 成功提交 {item.objectName} 给订单槽 {slotIndex}");

//         // ✅ 判断是否所有菜都已交完
//         bool allSubmitted = true;

//         Dictionary<KitchenObjectOS, int> requiredDict = new Dictionary<KitchenObjectOS, int>(
//             new KitchenObjectOSComparer()
//         );

//         foreach (var obj in recipeSO.kitchenObjectsOSList)
//         {
//             if (requiredDict.ContainsKey(obj))
//                 requiredDict[obj]++;
//             else
//                 requiredDict[obj] = 1;
//         }

//         foreach (var kvp in requiredDict)
//         {
//             int already = 0;
//             wr.submittedDict.TryGetValue(kvp.Key, out already);
//             if (already < kvp.Value)
//             {
//                 allSubmitted = false;
//                 break;
//             }
//         }

//         if (allSubmitted)
//         {
//             wr.isCompleted = true;
//             StartCoroutine(DelayRemoveCompletedOrder(slotIndex, 2f));
//             successfulRecipesAmount++;

//             // ✅ 加分逻辑（加在这里）
//             int recipeValue = 0;
//             foreach (var obj in wr.recipeSO.kitchenObjectsOSList)
//             {
//                 recipeValue += obj.value;
//             }
//             totalEarnedValue += recipeValue;

//             Debug.Log($"🎉 订单 {slotIndex} 完成！");
//             OnRecipeComplete?.Invoke(this, EventArgs.Empty);
//             OnRecipeSucess?.Invoke(this, EventArgs.Empty);
//         }
//         else
//         {
//             Debug.Log($"🔄 订单 {slotIndex} 进度更新");
//             OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
//         }
//     }

//     private void Update()
//     {
//         spawnRecipeTimer -= Time.deltaTime;

//         if (spawnRecipeTimer < 0f)
//         {
//             spawnRecipeTimer = spawnRecipeTimerMax;

//             if (KitchenGameManager.Instance.IsGamePlaying() && HasEmptySlot())
//             {
//                 RecipeSO waitingRecipeSO = recipeListSO.recipeSOList[
//                     UnityEngine.Random.Range(0, recipeListSO.recipeSOList.Count)
//                 ];
//                 WaitingRecipe newWaitingRecipe = new WaitingRecipe(waitingRecipeSO);

//                 // ✅ 随机选择一个空槽位插入订单
//                 List<int> emptyIndices = new List<int>();
//                 for (int i = 0; i < waitingRecipeList.Count; i++)
//                 {
//                     if (waitingRecipeList[i] == null)
//                     {
//                         emptyIndices.Add(i);
//                     }
//                 }

//                 if (emptyIndices.Count > 0)
//                 {
//                     int randomIndex = emptyIndices[UnityEngine.Random.Range(0, emptyIndices.Count)];
//                     waitingRecipeList[randomIndex] = newWaitingRecipe;
//                     OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
//                 }
//             }
//         }

//         // ✅ 倒计时逻辑
//         for (int i = 0; i < waitingRecipeList.Count; i++)
//         {
//             WaitingRecipe wr = waitingRecipeList[i];
//             if (wr == null || wr.isCompleted)
//                 continue;

//             wr.remainingTime -= Time.deltaTime;

//             if (wr.remainingTime <= 0f)
//             {
//                 // 超时失败
//                 OnRecipeFailed?.Invoke(this, EventArgs.Empty);
//                 waitingRecipeList[i] = null;
//             }
//         }
//     }

//     public void DeliverRecipe(PlateKitchenObject plateKitchenObject)
//     {
//         for (int i = 0; i < waitingRecipeList.Count; i++)
//         {
//             WaitingRecipe wr = waitingRecipeList[i];
//             if (wr == null || wr.isCompleted)
//                 continue;

//             RecipeSO waitingRecipeSO = wr.recipeSO;

//             if (
//                 waitingRecipeSO.kitchenObjectsOSList.Count
//                 == plateKitchenObject.GetKitchenObjectOSList().Count
//             )
//             {
//                 bool plateContentMatchesRecipe = true;

//                 foreach (
//                     KitchenObjectOS recipeKitchenObjectSO in waitingRecipeSO.kitchenObjectsOSList
//                 )
//                 {
//                     bool ingredientsFound = false;

//                     foreach (
//                         KitchenObjectOS plateKitchenObjectSO in plateKitchenObject.GetKitchenObjectOSList()
//                     )
//                     {
//                         if (plateKitchenObjectSO == recipeKitchenObjectSO)
//                         {
//                             ingredientsFound = true;
//                             break;
//                         }
//                     }

//                     if (!ingredientsFound)
//                     {
//                         plateContentMatchesRecipe = false;
//                         break;
//                     }
//                 }

//                 if (plateContentMatchesRecipe)
//                 {
//                     successfulRecipesAmount++;
//                     wr.isCompleted = true;
//                     waitingRecipeList[i] = null;

//                     OnRecipeComplete?.Invoke(this, EventArgs.Empty);
//                     OnRecipeSucess?.Invoke(this, EventArgs.Empty);
//                     return;
//                 }
//             }
//         }

//         // 所有订单都不匹配，视为失败交付
//         OnRecipeFailed?.Invoke(this, EventArgs.Empty);
//     }

//     public List<WaitingRecipe> GetWaitingRecipeList()
//     {
//         return waitingRecipeList;
//     }

//     public int GetSuccessfulRecipesAmount()
//     {
//         return successfulRecipesAmount;
//     }

//     public int GetTotalEarnedValue()
//     {
//         return totalEarnedValue;
//     }

//     private IEnumerator DelayRemoveCompletedOrder(int slotIndex, float delay)
//     {
//         yield return new WaitForSeconds(delay);
//         waitingRecipeList[slotIndex] = null;
//         OnRecipeSpawned?.Invoke(this, EventArgs.Empty); // 强制刷新 UI 显示空槽
//     }
// }
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode; // 导入 Netcode
using UnityEngine;

public struct RecipeDto : INetworkSerializable
{
    public int recipeIndex;
    public int slotIndex;
    public float remainingTime;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer)
        where T : IReaderWriter
    {
        serializer.SerializeValue(ref recipeIndex);
        serializer.SerializeValue(ref slotIndex);
        serializer.SerializeValue(ref remainingTime);
    }
}

public class DeliveryManager : NetworkBehaviour
{
    public event EventHandler OnRecipeSpawned;
    public event EventHandler OnRecipeComplete;
    public event EventHandler OnRecipeSucess;
    public event EventHandler OnRecipeFailed;

    public static DeliveryManager Instance { get; private set; }

    [SerializeField]
    private RecipeListSO recipeListSO;

    private List<WaitingRecipe> waitingRecipeList;
    private float spawnRecipeTimer;
    private float spawnRecipeTimerMax = 4f;
    private int waitingRecipeMax = 4;
    private int successfulRecipesAmount;
    private int totalEarnedValue = 0;

    private void Awake()
    {
        Instance = this;
        waitingRecipeList = new List<WaitingRecipe>(new WaitingRecipe[waitingRecipeMax]);
        // 给定时器一个初始值，避免一开始就一直 < 0
        spawnRecipeTimer = spawnRecipeTimerMax;
        Debug.Log(
            "[DeliveryManager] Awake 初始化完成，waitingRecipeList.Count=" + waitingRecipeList.Count
        );
    }

    private bool HasEmptySlot()
    {
        foreach (var recipe in waitingRecipeList)
        {
            if (recipe == null)
                return true;
        }
        return false;
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log($"[DeliveryManager] OnNetworkSpawn. IsServer={IsServer}, IsClient={IsClient}");
    }

    // // Host 会生成订单，所有客户端都能看到
    // [ServerRpc(RequireOwnership = false)]
    // public void SpawnRecipeServerRpc()
    // {
    //     spawnRecipeTimer = spawnRecipeTimerMax;

    //     if (KitchenGameManager.Instance.IsGamePlaying() && HasEmptySlot())
    //     {
    //         RecipeSO waitingRecipeSO = recipeListSO.recipeSOList[
    //             UnityEngine.Random.Range(0, recipeListSO.recipeSOList.Count)
    //         ];
    //         WaitingRecipe newWaitingRecipe = new WaitingRecipe(waitingRecipeSO);

    //         List<int> emptyIndices = new List<int>();
    //         for (int i = 0; i < waitingRecipeList.Count; i++)
    //         {
    //             if (waitingRecipeList[i] == null)
    //             {
    //                 emptyIndices.Add(i);
    //             }
    //         }

    //         if (emptyIndices.Count > 0)
    //         {
    //             int randomIndex = emptyIndices[UnityEngine.Random.Range(0, emptyIndices.Count)];
    //             waitingRecipeList[randomIndex] = newWaitingRecipe;
    //             // OnRecipeSpawned?.Invoke(this, EventArgs.Empty);

    //             // 通知所有客户端刷新
    //             UpdateRecipeListClientRpc();
    //             OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
    //         }
    //     }

    //     SpawnRecipeClientRpc(
    //         new RecipeDto
    //         {
    //             recipeIndex = recipeIndex,
    //             slotIndex = slotIndex,
    //             remainingTime = newWaitingRecipe.remainingTime,
    //         }
    //     );
    //     OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
    // }
    [ServerRpc(RequireOwnership = false)]
    public void SpawnRecipeServerRpc()
    {
        spawnRecipeTimer = spawnRecipeTimerMax;

        if (KitchenGameManager.Instance.IsGamePlaying() && HasEmptySlot())
        {
            // 随机选择一个食谱
            int recipeIndex = UnityEngine.Random.Range(0, recipeListSO.recipeSOList.Count);
            RecipeSO waitingRecipeSO = recipeListSO.recipeSOList[recipeIndex];
            WaitingRecipe newWaitingRecipe = new WaitingRecipe(waitingRecipeSO);

            // 找到所有空槽的索引
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
                // 随机选择一个空槽
                int slotIndex = emptyIndices[UnityEngine.Random.Range(0, emptyIndices.Count)];
                waitingRecipeList[slotIndex] = newWaitingRecipe;

                // 通知所有客户端更新
                SpawnRecipeClientRpc(
                    new RecipeDto
                    {
                        recipeIndex = recipeIndex,
                        slotIndex = slotIndex,
                        remainingTime = newWaitingRecipe.remainingTime,
                    }
                );

                // 触发事件
                OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    // 客户端会接收更新并刷新 UI
    [ClientRpc]
    private void UpdateRecipeListClientRpc()
    {
        OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
    }

    // 提交食材的操作由 Host 和 Client 都能调用
    public void TrySubmitItemToSlot(int slotIndex, KitchenObjectOS item)
    {
        if (slotIndex < 0 || slotIndex >= waitingRecipeList.Count)
            return;

        WaitingRecipe wr = waitingRecipeList[slotIndex];
        if (wr == null || wr.isCompleted)
            return;

        RecipeSO recipeSO = wr.recipeSO;

        int requiredCount = 0;
        foreach (var obj in recipeSO.kitchenObjectsOSList)
        {
            if (obj.objectName == item.objectName)
                requiredCount++;
        }

        if (requiredCount == 0)
        {
            Debug.Log($"❌ 提交失败：订单 {slotIndex} 不需要 {item.objectName}");
            OnRecipeFailed?.Invoke(this, EventArgs.Empty);
            return;
        }

        int submittedCount = 0;
        wr.submittedDict.TryGetValue(item, out submittedCount);

        if (submittedCount >= requiredCount)
        {
            Debug.Log($"⚠️ 已提交足够数量的 {item.objectName}，不能再交");
            return;
        }

        if (wr.submittedDict.ContainsKey(item))
        {
            wr.submittedDict[item]++;
        }
        else
        {
            wr.submittedDict[item] = 1;
        }

        Debug.Log($"✅ 成功提交 {item.objectName} 给订单槽 {slotIndex}");

        // 提交后立即同步到所有客户端
        UpdateRecipeListClientRpc();

        // 检查订单是否完成
        bool allSubmitted = true;
        Dictionary<KitchenObjectOS, int> requiredDict = new Dictionary<KitchenObjectOS, int>(
            new KitchenObjectOSComparer()
        );

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

    private IEnumerator DelayRemoveCompletedOrder(int slotIndex, float delay)
    {
        yield return new WaitForSeconds(delay);
        waitingRecipeList[slotIndex] = null;
        UpdateRecipeListClientRpc(); // 强制刷新 UI 显示空槽
    }

    private void Update()
    {
        if (!IsServer)
            return;

        spawnRecipeTimer -= Time.deltaTime;
        // Debug.Log($"[DeliveryManager] Timer: {spawnRecipeTimer:F2}s 剩余");
        if (spawnRecipeTimer <= 0f)
        {
            Debug.Log("[DeliveryManager] Timer 到 0，尝试生成订单");
            spawnRecipeTimer = spawnRecipeTimerMax;
            SpawnRecipeOnServer();
        }
    }

    /// <summary>
    /// 真正的生成订单逻辑，只在服务端运行，
    /// 并通过 ClientRpc 推给所有客户端
    /// </summary>
    private void SpawnRecipeOnServer()
    {
        bool playing = KitchenGameManager.Instance?.IsGamePlaying() ?? false;
        bool hasSlot = HasEmptySlot();
        Debug.Log(
            $"[DeliveryManager] SpawnRecipeOnServer(), IsGamePlaying={playing}, HasEmptySlot={hasSlot}"
        );

        if (!playing || !hasSlot)
        {
            Debug.Log("[DeliveryManager] 跳过生成：游戏状态 or 空槽 条件不满足");
            return;
        }

        int recipeIndex = UnityEngine.Random.Range(0, recipeListSO.recipeSOList.Count);
        var so = recipeListSO.recipeSOList[recipeIndex];
        WaitingRecipe newWaiting = new WaitingRecipe(so);

        List<int> empty = new List<int>();
        for (int i = 0; i < waitingRecipeList.Count; i++)
            if (waitingRecipeList[i] == null)
                empty.Add(i);

        Debug.Log("[DeliveryManager] 空槽索引：" + string.Join(",", empty));

        if (empty.Count == 0)
        {
            Debug.Log("[DeliveryManager] 没有空槽可用");
            return;
        }

        int slot = empty[UnityEngine.Random.Range(0, empty.Count)];
        waitingRecipeList[slot] = newWaiting;
        Debug.Log($"[DeliveryManager] 在槽 {slot} 生成了新订单，RecipeIndex={recipeIndex}");

        OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
        Debug.Log("[DeliveryManager] 触发 OnRecipeSpawned 事件");

        SpawnRecipeClientRpc(
            new RecipeDto
            {
                recipeIndex = recipeIndex,
                slotIndex = slot,
                remainingTime = newWaiting.remainingTime,
            }
        );
        Debug.Log("[DeliveryManager] 已调用 SpawnRecipeClientRpc 同步客户端");
    }

    [ClientRpc]
    private void SpawnRecipeClientRpc(RecipeDto dto)
    {
        Debug.Log(
            $"[DeliveryManager] ClientRpc 收到配方：recipeIndex={dto.recipeIndex}, slotIndex={dto.slotIndex}, remainingTime={dto.remainingTime:F2}"
        );
        var so = recipeListSO.recipeSOList[dto.recipeIndex];
        waitingRecipeList[dto.slotIndex] = new WaitingRecipe(so)
        {
            remainingTime = dto.remainingTime,
        };
        OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
        Debug.Log("[DeliveryManager] 客户端已触发 OnRecipeSpawned，UI 应刷新");
    }

    /// <summary>
    /// 给旧的 DeliveryCounter/DeliveryManagerUI 调用，获取当前所有订单槽的状态
    /// </summary>
    public List<WaitingRecipe> GetWaitingRecipeList()
    {
        return waitingRecipeList;
    }

    /// <summary>
    /// 给旧的 ScoreUI 调用，获取当前完成的订单数
    /// </summary>
    public int GetSuccessfulRecipesAmount()
    {
        return successfulRecipesAmount;
    }

    /// <summary>
    /// 给旧的 ScoreUI 调用，获取当前总收益
    /// </summary>
    public int GetTotalEarnedValue()
    {
        return totalEarnedValue;
    }

    /// <summary>
    /// 如果你还需要 DeliverRecipe（Plate 交付）逻辑，原样拷贝并在提交后同步：
    /// </summary>
    public void DeliverRecipe(PlateKitchenObject plateKitchenObject)
    {
        for (int i = 0; i < waitingRecipeList.Count; i++)
        {
            WaitingRecipe wr = waitingRecipeList[i];
            if (wr == null || wr.isCompleted)
                continue;

            RecipeSO waitingRecipeSO = wr.recipeSO;
            var plateList = plateKitchenObject.GetKitchenObjectOSList();
            if (waitingRecipeSO.kitchenObjectsOSList.Count != plateList.Count)
                continue;

            bool matches = true;
            foreach (var recipeObj in waitingRecipeSO.kitchenObjectsOSList)
            {
                if (!plateList.Contains(recipeObj))
                {
                    matches = false;
                    break;
                }
            }

            if (matches)
            {
                wr.isCompleted = true;
                waitingRecipeList[i] = null;
                successfulRecipesAmount++;
                // 同步状态给客户端
                UpdateRecipeListClientRpc();
                OnRecipeComplete?.Invoke(this, EventArgs.Empty);
                OnRecipeSucess?.Invoke(this, EventArgs.Empty);
                return;
            }
        }

        // 都不匹配则失败
        OnRecipeFailed?.Invoke(this, EventArgs.Empty);
    }
}
