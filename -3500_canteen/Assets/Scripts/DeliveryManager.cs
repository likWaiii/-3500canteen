// using System;
// using System.Collections;
// using System.Collections.Generic;
// using Unity.Netcode; // 导入 Netcode
// using UnityEngine;

// public struct RecipeDto : INetworkSerializable
// {
//     public int recipeIndex;
//     public int slotIndex;
//     public float remainingTime;

//     public void NetworkSerialize<T>(BufferSerializer<T> serializer)
//         where T : IReaderWriter
//     {
//         serializer.SerializeValue(ref recipeIndex);
//         serializer.SerializeValue(ref slotIndex);
//         serializer.SerializeValue(ref remainingTime);
//     }
// }

// public class DeliveryManager : NetworkBehaviour
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
//         waitingRecipeList = new List<WaitingRecipe>(new WaitingRecipe[waitingRecipeMax]);
//         // 给定时器一个初始值，避免一开始就一直 < 0
//         spawnRecipeTimer = spawnRecipeTimerMax;
//         Debug.Log(
//             "[DeliveryManager] Awake 初始化完成，waitingRecipeList.Count=" + waitingRecipeList.Count
//         );
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

//     public override void OnNetworkSpawn()
//     {
//         Debug.Log($"[DeliveryManager] OnNetworkSpawn. IsServer={IsServer}, IsClient={IsClient}");
//     }

//     // // Host 会生成订单，所有客户端都能看到
//     // [ServerRpc(RequireOwnership = false)]
//     // public void SpawnRecipeServerRpc()
//     // {
//     //     spawnRecipeTimer = spawnRecipeTimerMax;

//     //     if (KitchenGameManager.Instance.IsGamePlaying() && HasEmptySlot())
//     //     {
//     //         RecipeSO waitingRecipeSO = recipeListSO.recipeSOList[
//     //             UnityEngine.Random.Range(0, recipeListSO.recipeSOList.Count)
//     //         ];
//     //         WaitingRecipe newWaitingRecipe = new WaitingRecipe(waitingRecipeSO);

//     //         List<int> emptyIndices = new List<int>();
//     //         for (int i = 0; i < waitingRecipeList.Count; i++)
//     //         {
//     //             if (waitingRecipeList[i] == null)
//     //             {
//     //                 emptyIndices.Add(i);
//     //             }
//     //         }

//     //         if (emptyIndices.Count > 0)
//     //         {
//     //             int randomIndex = emptyIndices[UnityEngine.Random.Range(0, emptyIndices.Count)];
//     //             waitingRecipeList[randomIndex] = newWaitingRecipe;
//     //             // OnRecipeSpawned?.Invoke(this, EventArgs.Empty);

//     //             // 通知所有客户端刷新
//     //             UpdateRecipeListClientRpc();
//     //             OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
//     //         }
//     //     }

//     //     SpawnRecipeClientRpc(
//     //         new RecipeDto
//     //         {
//     //             recipeIndex = recipeIndex,
//     //             slotIndex = slotIndex,
//     //             remainingTime = newWaitingRecipe.remainingTime,
//     //         }
//     //     );
//     //     OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
//     // }
//     [ServerRpc(RequireOwnership = false)]
//     public void SpawnRecipeServerRpc()
//     {
//         spawnRecipeTimer = spawnRecipeTimerMax;

//         if (KitchenGameManager.Instance.IsGamePlaying() && HasEmptySlot())
//         {
//             // 随机选择一个食谱
//             int recipeIndex = UnityEngine.Random.Range(0, recipeListSO.recipeSOList.Count);
//             RecipeSO waitingRecipeSO = recipeListSO.recipeSOList[recipeIndex];
//             WaitingRecipe newWaitingRecipe = new WaitingRecipe(waitingRecipeSO);

//             // 找到所有空槽的索引
//             List<int> emptyIndices = new List<int>();
//             for (int i = 0; i < waitingRecipeList.Count; i++)
//             {
//                 if (waitingRecipeList[i] == null)
//                 {
//                     emptyIndices.Add(i);
//                 }
//             }

//             if (emptyIndices.Count > 0)
//             {
//                 // 随机选择一个空槽
//                 int slotIndex = emptyIndices[UnityEngine.Random.Range(0, emptyIndices.Count)];
//                 waitingRecipeList[slotIndex] = newWaitingRecipe;

//                 // 通知所有客户端更新
//                 SpawnRecipeClientRpc(
//                     new RecipeDto
//                     {
//                         recipeIndex = recipeIndex,
//                         slotIndex = slotIndex,
//                         remainingTime = newWaitingRecipe.remainingTime,
//                     }
//                 );

//                 // 触发事件
//                 OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
//             }
//         }
//     }

//     // 客户端会接收更新并刷新 UI
//     [ClientRpc]
//     private void UpdateRecipeListClientRpc()
//     {
//         OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
//     }

//     // 提交食材的操作由 Host 和 Client 都能调用
//     public void TrySubmitItemToSlot(int slotIndex, KitchenObjectOS item)
//     {
//         if (slotIndex < 0 || slotIndex >= waitingRecipeList.Count)
//             return;

//         WaitingRecipe wr = waitingRecipeList[slotIndex];
//         if (wr == null || wr.isCompleted)
//             return;

//         RecipeSO recipeSO = wr.recipeSO;

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

//         int submittedCount = 0;
//         wr.submittedDict.TryGetValue(item, out submittedCount);

//         if (submittedCount >= requiredCount)
//         {
//             Debug.Log($"⚠️ 已提交足够数量的 {item.objectName}，不能再交");
//             return;
//         }

//         if (wr.submittedDict.ContainsKey(item))
//         {
//             wr.submittedDict[item]++;
//         }
//         else
//         {
//             wr.submittedDict[item] = 1;
//         }

//         Debug.Log($"✅ 成功提交 {item.objectName} 给订单槽 {slotIndex}");

//         // 提交后立即同步到所有客户端
//         UpdateRecipeListClientRpc();

//         // 检查订单是否完成
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

//     private IEnumerator DelayRemoveCompletedOrder(int slotIndex, float delay)
//     {
//         yield return new WaitForSeconds(delay);
//         waitingRecipeList[slotIndex] = null;
//         UpdateRecipeListClientRpc(); // 强制刷新 UI 显示空槽
//     }

//     private void Update()
//     {
//         if (!IsServer)
//             return;

//         spawnRecipeTimer -= Time.deltaTime;
//         // Debug.Log($"[DeliveryManager] Timer: {spawnRecipeTimer:F2}s 剩余");
//         if (spawnRecipeTimer <= 0f)
//         {
//             Debug.Log("[DeliveryManager] Timer 到 0，尝试生成订单");
//             spawnRecipeTimer = spawnRecipeTimerMax;
//             SpawnRecipeOnServer();
//         }
//     }

//     /// <summary>
//     /// 真正的生成订单逻辑，只在服务端运行，
//     /// 并通过 ClientRpc 推给所有客户端
//     /// </summary>
//     private void SpawnRecipeOnServer()
//     {
//         bool playing = KitchenGameManager.Instance?.IsGamePlaying() ?? false;
//         bool hasSlot = HasEmptySlot();
//         Debug.Log(
//             $"[DeliveryManager] SpawnRecipeOnServer(), IsGamePlaying={playing}, HasEmptySlot={hasSlot}"
//         );

//         if (!playing || !hasSlot)
//         {
//             Debug.Log("[DeliveryManager] 跳过生成：游戏状态 or 空槽 条件不满足");
//             return;
//         }

//         int recipeIndex = UnityEngine.Random.Range(0, recipeListSO.recipeSOList.Count);
//         var so = recipeListSO.recipeSOList[recipeIndex];
//         WaitingRecipe newWaiting = new WaitingRecipe(so);

//         List<int> empty = new List<int>();
//         for (int i = 0; i < waitingRecipeList.Count; i++)
//             if (waitingRecipeList[i] == null)
//                 empty.Add(i);

//         Debug.Log("[DeliveryManager] 空槽索引：" + string.Join(",", empty));

//         if (empty.Count == 0)
//         {
//             Debug.Log("[DeliveryManager] 没有空槽可用");
//             return;
//         }

//         int slot = empty[UnityEngine.Random.Range(0, empty.Count)];
//         waitingRecipeList[slot] = newWaiting;
//         Debug.Log($"[DeliveryManager] 在槽 {slot} 生成了新订单，RecipeIndex={recipeIndex}");

//         OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
//         Debug.Log("[DeliveryManager] 触发 OnRecipeSpawned 事件");

//         SpawnRecipeClientRpc(
//             new RecipeDto
//             {
//                 recipeIndex = recipeIndex,
//                 slotIndex = slot,
//                 remainingTime = newWaiting.remainingTime,
//             }
//         );
//         Debug.Log("[DeliveryManager] 已调用 SpawnRecipeClientRpc 同步客户端");
//     }

//     [ClientRpc]
//     private void SpawnRecipeClientRpc(RecipeDto dto)
//     {
//         Debug.Log(
//             $"[DeliveryManager] ClientRpc 收到配方：recipeIndex={dto.recipeIndex}, slotIndex={dto.slotIndex}, remainingTime={dto.remainingTime:F2}"
//         );
//         var so = recipeListSO.recipeSOList[dto.recipeIndex];
//         waitingRecipeList[dto.slotIndex] = new WaitingRecipe(so)
//         {
//             remainingTime = dto.remainingTime,
//         };
//         OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
//         Debug.Log("[DeliveryManager] 客户端已触发 OnRecipeSpawned，UI 应刷新");
//     }

//     /// <summary>
//     /// 给旧的 DeliveryCounter/DeliveryManagerUI 调用，获取当前所有订单槽的状态
//     /// </summary>
//     public List<WaitingRecipe> GetWaitingRecipeList()
//     {
//         return waitingRecipeList;
//     }

//     /// <summary>
//     /// 给旧的 ScoreUI 调用，获取当前完成的订单数
//     /// </summary>
//     public int GetSuccessfulRecipesAmount()
//     {
//         return successfulRecipesAmount;
//     }

//     /// <summary>
//     /// 给旧的 ScoreUI 调用，获取当前总收益
//     /// </summary>
//     public int GetTotalEarnedValue()
//     {
//         return totalEarnedValue;
//     }

//     /// <summary>
//     /// 如果你还需要 DeliverRecipe（Plate 交付）逻辑，原样拷贝并在提交后同步：
//     /// </summary>
//     public void DeliverRecipe(PlateKitchenObject plateKitchenObject)
//     {
//         for (int i = 0; i < waitingRecipeList.Count; i++)
//         {
//             WaitingRecipe wr = waitingRecipeList[i];
//             if (wr == null || wr.isCompleted)
//                 continue;

//             RecipeSO waitingRecipeSO = wr.recipeSO;
//             var plateList = plateKitchenObject.GetKitchenObjectOSList();
//             if (waitingRecipeSO.kitchenObjectsOSList.Count != plateList.Count)
//                 continue;

//             bool matches = true;
//             foreach (var recipeObj in waitingRecipeSO.kitchenObjectsOSList)
//             {
//                 if (!plateList.Contains(recipeObj))
//                 {
//                     matches = false;
//                     break;
//                 }
//             }

//             if (matches)
//             {
//                 wr.isCompleted = true;
//                 waitingRecipeList[i] = null;
//                 successfulRecipesAmount++;
//                 // 同步状态给客户端
//                 UpdateRecipeListClientRpc();
//                 OnRecipeComplete?.Invoke(this, EventArgs.Empty);
//                 OnRecipeSucess?.Invoke(this, EventArgs.Empty);
//                 return;
//             }
//         }

//         // 都不匹配则失败
//         OnRecipeFailed?.Invoke(this, EventArgs.Empty);
//     }
// }
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
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
    public event EventHandler<int> OnScoreUpdated;
    
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
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // 如果需要跨场景保留
        waitingRecipeList = new List<WaitingRecipe>(new WaitingRecipe[waitingRecipeMax]);
        spawnRecipeTimer = spawnRecipeTimerMax;
    }

    [ClientRpc]
    private void UpdateScoreClientRpc(int totalEarnedValue)
    {
        OnScoreUpdated?.Invoke(this, totalEarnedValue);
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

    [ClientRpc]
    private void UpdateRecipeStatusClientRpc(int slotIndex, bool isCompleted)
    {
        if (waitingRecipeList[slotIndex] != null)
        {
            waitingRecipeList[slotIndex].isCompleted = isCompleted;
            OnRecipeSpawned?.Invoke(this, EventArgs.Empty); // 刷新 UI
        }
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log(
            $"[DeliveryManager] OnNetworkSpawn for Instance {GetInstanceID()}. IsServer={IsServer}, IsClient={IsClient}"
        );

        if (Instance == null || Instance != this)
        {
            // 这可能发生在其他实例成为了单例，或者单例引用被重置
            Debug.LogWarning(
                $"DeliveryManager.Instance 在 OnNetworkSpawn 时不是此实例 ({GetInstanceID()})。当前 Instance ID: {(Instance != null ? Instance.GetInstanceID().ToString() : "null")}"
            );
            // 如果不是单例，可能需要销毁自己，或者什么都不做（如果它是客户端同步过来的非主要实例）
            // 但最好的做法是确保只有一个主要的 Server/Host 实例。
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnRecipeServerRpc()
    {
        spawnRecipeTimer = spawnRecipeTimerMax;

        if (KitchenGameManager.Instance.IsGamePlaying() && HasEmptySlot())
        {
            int recipeIndex = UnityEngine.Random.Range(0, recipeListSO.recipeSOList.Count);
            RecipeSO waitingRecipeSO = recipeListSO.recipeSOList[recipeIndex];
            WaitingRecipe newWaitingRecipe = new WaitingRecipe(waitingRecipeSO);

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
                int slotIndex = emptyIndices[UnityEngine.Random.Range(0, emptyIndices.Count)];
                waitingRecipeList[slotIndex] = newWaitingRecipe;

                SpawnRecipeClientRpc(
                    new RecipeDto
                    {
                        recipeIndex = recipeIndex,
                        slotIndex = slotIndex,
                        remainingTime = newWaitingRecipe.remainingTime,
                    }
                );

                OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    // private void OnDestroy()
    // {
    //     // 如果被销毁的实例是当前的单例，则清除引用
    //     if (Instance == this)
    //     {
    //         base.OnDestroy(); // 调用基类的 OnDestroy 方法
    //         Debug.Log($"DeliveryManager 单例引用被清除。销毁的实例 ID: {GetInstanceID()}");
    //     }
    //     // 在销毁时取消事件订阅，防止内存泄漏（尽管 ScoreUI 也在 OnDestroy 取消）
    //     // 这里的取消订阅是针对 DeliveryManager 内部可能的其他订阅者，ScoreUI 的取消订阅逻辑是在 ScoreUI 自己的 OnDestroy 里。
    //     // OnRecipeSucess = null; // 不建议直接设为null，这样会清除所有订阅者，应该用 -=
    // }

    [ClientRpc]
    private void UpdateRecipeListClientRpc()
    {
        OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
    }

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

        UpdateRecipeListClientRpc();

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
            UpdateRecipeStatusClientRpc(slotIndex, true);
            StartCoroutine(DelayRemoveCompletedOrder(slotIndex, 2f));
            successfulRecipesAmount++;

            int recipeValue = 0;
            foreach (var obj in wr.recipeSO.kitchenObjectsOSList)
            {
                recipeValue += obj.value;
            }
            totalEarnedValue += recipeValue;
            UpdateScoreClientRpc(totalEarnedValue);
            Debug.Log($"🎉 订单 {slotIndex} 完成！在 DeliveryManager 实例 {GetInstanceID()}");
            OnRecipeComplete?.Invoke(this, EventArgs.Empty);
            if (OnRecipeSucess == null)
            {
                Debug.Log($"DeliveryManager 实例 {GetInstanceID()}: OnRecipeSucess 事件未被订阅");
            }
            else
            {
                Debug.Log($"DeliveryManager 实例 {GetInstanceID()}: 触发 OnRecipeSucess 事件");
                OnRecipeSucess?.Invoke(this, EventArgs.Empty);
            }

            // 刷新 UI
            UpdateRecipeListClientRpc();
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
        UpdateRecipeListClientRpc();
    }

    private void Update()
    {
        if (!IsServer)
            return;

        spawnRecipeTimer -= Time.deltaTime;

        if (spawnRecipeTimer <= 0f)
        {
            spawnRecipeTimer = spawnRecipeTimerMax;
            SpawnRecipeOnServer();
        }

        // 更新所有订单的剩余时间
        for (int i = 0; i < waitingRecipeList.Count; i++)
        {
            if (waitingRecipeList[i] != null && !waitingRecipeList[i].isCompleted)
            {
                waitingRecipeList[i].remainingTime -= Time.deltaTime;

                if (waitingRecipeList[i].remainingTime <= 0f)
                {
                    waitingRecipeList[i].remainingTime = 0f;
                    OnRecipeFailed?.Invoke(this, EventArgs.Empty);
                    StartCoroutine(DelayRemoveCompletedOrder(i, 2f));
                }

                // 同步剩余时间到客户端
                UpdateRecipeTimeClientRpc(i, waitingRecipeList[i].remainingTime);
            }
        }
    }

    [ClientRpc]
    private void UpdateRecipeTimeClientRpc(int slotIndex, float remainingTime)
    {
        if (waitingRecipeList[slotIndex] != null)
        {
            waitingRecipeList[slotIndex].remainingTime = remainingTime;
        }
    }

    private void SpawnRecipeOnServer()
    {
        bool playing = KitchenGameManager.Instance?.IsGamePlaying() ?? false;
        bool hasSlot = HasEmptySlot();

        if (!playing || !hasSlot)
        {
            return;
        }

        int recipeIndex = UnityEngine.Random.Range(0, recipeListSO.recipeSOList.Count);
        var so = recipeListSO.recipeSOList[recipeIndex];
        WaitingRecipe newWaiting = new WaitingRecipe(so);

        List<int> empty = new List<int>();
        for (int i = 0; i < waitingRecipeList.Count; i++)
            if (waitingRecipeList[i] == null)
                empty.Add(i);

        if (empty.Count == 0)
        {
            return;
        }

        int slot = empty[UnityEngine.Random.Range(0, empty.Count)];
        waitingRecipeList[slot] = newWaiting;

        OnRecipeSpawned?.Invoke(this, EventArgs.Empty);

        SpawnRecipeClientRpc(
            new RecipeDto
            {
                recipeIndex = recipeIndex,
                slotIndex = slot,
                remainingTime = newWaiting.remainingTime,
            }
        );
    }

    [ClientRpc]
    private void SpawnRecipeClientRpc(RecipeDto dto)
    {
        var so = recipeListSO.recipeSOList[dto.recipeIndex];
        waitingRecipeList[dto.slotIndex] = new WaitingRecipe(so)
        {
            remainingTime = dto.remainingTime,
        };
        OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
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
                UpdateRecipeListClientRpc();
                OnRecipeComplete?.Invoke(this, EventArgs.Empty);
                OnRecipeSucess?.Invoke(this, EventArgs.Empty);
                return;
            }
        }

        OnRecipeFailed?.Invoke(this, EventArgs.Empty);
    }
}
