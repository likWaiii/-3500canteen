using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public event EventHandler OnRecipeSuccess;
    public event EventHandler OnRecipeFailed;
    public event EventHandler<int> OnScoreUpdated;

    public static DeliveryManager Instance { get; private set; }

    [SerializeField]
    private RecipeListSO recipeListSO;
    private Dictionary<ulong, int> playerScores = new Dictionary<ulong, int>();
    private List<WaitingRecipe> waitingRecipeList;
    private float spawnRecipeTimer;
    private float spawnRecipeTimerMax = 4f;
    private int waitingRecipeMax = 4;
    private int successfulRecipesAmount;
    private int totalEarnedValue = 600;

    [SerializeField]
    private int scoreThreshold = 0;
    private bool isGameEnded = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[DeliveryManager] 检测到重复实例，销毁当前实例 (Awake)");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        waitingRecipeList = new List<WaitingRecipe>(new WaitingRecipe[waitingRecipeMax]);
        spawnRecipeTimer = spawnRecipeTimerMax;
        Debug.Log("[DeliveryManager] 初始化完成 (Awake)");
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            Instance = this; // 服务器端确保 Instance 是权威实例
            Debug.Log("[DeliveryManager] OnNetworkSpawn 在服务器端设置 Instance");
        }
        Debug.Log($"[DeliveryManager] OnNetworkSpawn: IsServer={IsServer}, IsClient={IsClient}");
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
        Debug.Log($"[DeliveryManager] {gameObject.name} 被销毁。");
    }

    [ClientRpc]
    private void SubmitIngredientClientRpc(int slotIndex, string kitchenObjectName, int newCount)
    {
        Debug.Log(
            $"[DeliveryManager] 客户端收到 SubmitIngredientClientRpc：slotIndex={slotIndex}, kitchenObjectName={kitchenObjectName}, newCount={newCount}"
        );
        var kitchenObject = recipeListSO.GetKitchenObjectByName(kitchenObjectName);
        var wr = waitingRecipeList[slotIndex];
        if (wr == null)
        {
            Debug.LogWarning($"[DeliveryManager] 未找到订单槽 {slotIndex}");
            return;
        }
        wr.submittedDict[kitchenObject] = newCount;
        OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
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
        Debug.Log(
            $"[DeliveryManager] 客户端收到 UpdateRecipeStatusClientRpc：slotIndex={slotIndex}, isCompleted={isCompleted}"
        );
        if (waitingRecipeList[slotIndex] != null)
        {
            waitingRecipeList[slotIndex].isCompleted = isCompleted;
            OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnRecipeServerRpc()
    {
        if (isGameEnded)
        {
            Debug.Log("[DeliveryManager] 游戏已结束，不再生成新订单");
            return;
        }

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

                Debug.Log(
                    $"[DeliveryManager] 生成新订单：slotIndex={slotIndex}, recipeIndex={recipeIndex}"
                );
                OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    [ClientRpc]
    private void UpdateRecipeListClientRpc()
    {
        Debug.Log("[DeliveryManager] 客户端收到 UpdateRecipeListClientRpc");
        OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SubmitIngredientServerRpc(
        int slotIndex,
        string kitchenObjectName,
        ServerRpcParams rpcParams = default
    )
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        Debug.Log(
            $"[DeliveryManager] 收到提交请求：clientId={clientId}, slotIndex={slotIndex}, kitchenObjectName={kitchenObjectName}"
        );
        KitchenObjectOS itemOS = recipeListSO.GetKitchenObjectByName(kitchenObjectName);
        if (itemOS == null)
        {
            Debug.LogWarning($"[DeliveryManager] 未找到 KitchenObject：{kitchenObjectName}");
            return;
        }

        ProcessSubmitToSlot(slotIndex, itemOS, clientId);
    }

    private void ProcessSubmitToSlot(int slotIndex, KitchenObjectOS item, ulong clientId)
    {
        if (slotIndex < 0 || slotIndex >= waitingRecipeList.Count)
        {
            Debug.LogWarning($"[DeliveryManager] 无效的订单槽：{slotIndex}");
            return;
        }

        WaitingRecipe wr = waitingRecipeList[slotIndex];
        if (wr == null || wr.isCompleted)
        {
            Debug.LogWarning($"[DeliveryManager] 订单槽 {slotIndex} 为空或已完成");
            return;
        }

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

        int updatedCount = wr.submittedDict[item];
        SubmitIngredientClientRpc(slotIndex, item.objectName, updatedCount);

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
            successfulRecipesAmount++;

            int recipeValue = wr.recipeSO.kitchenObjectsOSList.Sum(o => o.value);
            totalEarnedValue += recipeValue;

            if (!playerScores.ContainsKey(clientId))
                playerScores[clientId] = 0;
            playerScores[clientId] += recipeValue;

            UpdateRecipeStatusClientRpc(slotIndex, true);

            var rpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { clientId } },
            };
            UpdatePlayerScoreClientRpc(playerScores[clientId], rpcParams);

            Debug.Log(
                $"[DeliveryManager] 订单完成：clientId={clientId}, 得分={playerScores[clientId]}"
            );

            Debug.Log(
                $"[DeliveryManager] 检查结束条件，clientId={clientId}, 当前分数 = {playerScores[clientId]}, 阈值 = {scoreThreshold}，isGameEnded = {isGameEnded}"
            );
            if (!isGameEnded && playerScores[clientId] >= scoreThreshold)
            {
                isGameEnded = true;
                Debug.Log(
                    $"[DeliveryManager] 游戏结束，玩家 {clientId} 达到分数阈值 {scoreThreshold}"
                );

                // 服务器设置游戏状态
                if (KitchenGameManager.Instance != null)
                {
                    KitchenGameManager.Instance.TriggerGameOverServerRpc();
                }
                else
                {
                    Debug.LogError(
                        "[DeliveryManager] 服务器上的 KitchenGameManager.Instance 为空!"
                    );
                }

                // 然后通知所有客户端谁赢了
                EndGameClientRpc(clientId);
                return;
            }

            StartCoroutine(DelayRemoveCompletedOrder(slotIndex, 2f));
        }
        else
        {
            UpdateRecipeListClientRpc();
        }
    }

    [ClientRpc]
    private void UpdatePlayerScoreClientRpc(int newScore, ClientRpcParams rpcParams = default)
    {
        Debug.Log($"[DeliveryManager] 客户端收到 UpdatePlayerScoreClientRpc：newScore={newScore}");
        ulong localClientId = NetworkManager.Singleton.LocalClientId;
        playerScores[localClientId] = newScore;
        OnScoreUpdated?.Invoke(this, newScore);
    }

    public int GetPlayerScore(ulong clientId)
    {
        return playerScores.TryGetValue(clientId, out var score) ? score : 0;
    }

    private IEnumerator DelayRemoveCompletedOrder(int slotIndex, float delay)
    {
        yield return new WaitForSeconds(delay);
        waitingRecipeList[slotIndex] = null;
        RemoveOrderSlotClientRpc(slotIndex);
    }

    [ClientRpc]
    private void RemoveOrderSlotClientRpc(int slotIndex)
    {
        Debug.Log($"[DeliveryManager] 客户端收到 RemoveOrderSlotClientRpc：slotIndex={slotIndex}");
        waitingRecipeList[slotIndex] = null;
        OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
    }

    private void Update()
    {
        if (!IsServer)
            return;

        if (isGameEnded)
        {
            Debug.Log("[DeliveryManager] 游戏已结束，不再更新订单");
            return;
        }

        spawnRecipeTimer -= Time.deltaTime;

        if (spawnRecipeTimer <= 0f)
        {
            spawnRecipeTimer = spawnRecipeTimerMax;
            SpawnRecipeOnServer();
        }

        for (int i = 0; i < waitingRecipeList.Count; i++)
        {
            if (waitingRecipeList[i] != null && !waitingRecipeList[i].isCompleted)
            {
                waitingRecipeList[i].remainingTime -= Time.deltaTime;

                if (
                    waitingRecipeList[i].remainingTime <= 0f
                    && waitingRecipeList[i].remainingTime > -Time.deltaTime
                )
                {
                    Debug.Log($"[DeliveryManager] 订单 {i} 超时！触发 OnRecipeFailed 事件。");
                    waitingRecipeList[i].remainingTime = 0f;
                    OnRecipeFailed?.Invoke(this, EventArgs.Empty); // 只触发一次
                    StartCoroutine(DelayRemoveCompletedOrder(i, 2f));
                }

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
            return;

        int recipeIndex = UnityEngine.Random.Range(0, recipeListSO.recipeSOList.Count);
        var so = recipeListSO.recipeSOList[recipeIndex];
        WaitingRecipe newWaiting = new WaitingRecipe(so);

        List<int> empty = new List<int>();
        for (int i = 0; i < waitingRecipeList.Count; i++)
            if (waitingRecipeList[i] == null)
                empty.Add(i);

        if (empty.Count == 0)
            return;

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

    public void DeliverRecipe(KitchenObject kitchenObject)
    {
        Debug.Log(
            $"[DeliveryManager] 尝试交付菜品：{kitchenObject.GetKitchenObjectOS().objectName}"
        );
        for (int i = 0; i < waitingRecipeList.Count; i++)
        {
            Debug.Log($"[DeliveryManager] 检查订单槽 {i}");
            WaitingRecipe wr = waitingRecipeList[i];
            if (wr == null || wr.isCompleted)
                continue;

            RecipeSO waitingRecipeSO = wr.recipeSO;

            bool matches = true;
            foreach (var recipeObj in waitingRecipeSO.kitchenObjectsOSList)
            {
                Debug.Log(
                    $"[DeliveryManager] 检查菜品：{recipeObj.objectName} (需要) vs {kitchenObject.GetKitchenObjectOS().objectName} (提交)"
                );
                if (!recipeObj.Equals(kitchenObject))
                {
                    Debug.Log(
                        $"[DeliveryManager] 菜品不匹配：{recipeObj.objectName} != {kitchenObject.GetKitchenObjectOS().objectName}"
                    );
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
                OnRecipeSuccess?.Invoke(this, EventArgs.Empty);
                return;
            }
        }

        OnRecipeFailed?.Invoke(this, EventArgs.Empty);
    }

    [ClientRpc]
    private void EndGameClientRpc(ulong winningClientId)
    {
        bool isLocalWinner = NetworkManager.Singleton.LocalClientId == winningClientId;
        Debug.Log(
            $"[DeliveryManager] 收到 EndGameClientRpc，winnerId={winningClientId}，本地 clientId={NetworkManager.Singleton.LocalClientId}"
        );
        KitchenGameManager.Instance.GameOver(isLocalWinner);
    }
}
