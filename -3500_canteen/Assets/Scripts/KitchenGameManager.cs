using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class KitchenGameManager : NetworkBehaviour
{
    public static KitchenGameManager Instance { get; private set; }

    public event EventHandler OnStateChangedLocal;
    public event EventHandler OnGamePaused;
    public event EventHandler OnGameUnpaused;
    public event EventHandler<bool> OnCanTogglePauseChanged;

    [SerializeField]
    private GameObject deliveryManagerPrefab;

    private NetworkVariable<State> state = new NetworkVariable<State>(State.WaitingToStart);
    private NetworkVariable<float> countdownToStartTimerNet = new NetworkVariable<float>(3f);

    [SerializeField]
    private float gamePlayingTimerMax = 180f;
    private NetworkVariable<float> gamePlayingTimerNet = new NetworkVariable<float>();
    private NetworkVariable<bool> isGamePausedNet = new NetworkVariable<bool>(false);
    private NetworkVariable<ulong> pausingClientIdNet = new NetworkVariable<ulong>(ulong.MaxValue);

    private bool localCanTogglePause = false;

    private enum State
    {
        WaitingToStart,
        CountdownToStart,
        GamePlaying,
        GameOver,
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            SpawnDeliveryManager();
        }
        state.OnValueChanged += OnStateChanged_NetworkVariable;
        isGamePausedNet.OnValueChanged += IsGamePausedNet_OnValueChanged;
        pausingClientIdNet.OnValueChanged += PausingClientIdNet_OnValueChanged;

        UpdateLocalCanTogglePause();
        Time.timeScale = isGamePausedNet.Value ? 0f : 1f;
        Debug.Log(
            $"[KitchenGameManager] OnNetworkSpawn - 客户端 {NetworkManager.Singleton.LocalClientId}: Time.timeScale 设置为 {Time.timeScale}"
        );
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        state.OnValueChanged -= OnStateChanged_NetworkVariable;
        isGamePausedNet.OnValueChanged -= IsGamePausedNet_OnValueChanged;
        pausingClientIdNet.OnValueChanged -= PausingClientIdNet_OnValueChanged;
    }

    private void OnStateChanged_NetworkVariable(State oldState, State newState)
    {
        OnStateChangedLocal?.Invoke(this, EventArgs.Empty);
        UpdateLocalCanTogglePause();
        Debug.Log(
            $"[KitchenGameManager] 状态变更为 {newState} 于客户端 {NetworkManager.Singleton.LocalClientId}"
        );

        if (newState == State.GameOver)
        {
            // 当状态变更为 GameOver 时，调用 GameOver 方法来显示结束UI等
            // 对于超时结束，通常认为是没有玩家达成胜利条件，所以传入 false
            GameOver(false); // <-- 调用 GameOver 方法
        }
    }

    private void IsGamePausedNet_OnValueChanged(bool oldPaused, bool newPaused)
    {
        if (newPaused)
        {
            Time.timeScale = 0f;
            OnGamePaused?.Invoke(this, EventArgs.Empty);
            Debug.Log(
                $"[KitchenGameManager] 游戏暂停于客户端 {NetworkManager.Singleton.LocalClientId}"
            );
        }
        else
        {
            Time.timeScale = 1f;
            OnGameUnpaused?.Invoke(this, EventArgs.Empty);
            Debug.Log(
                $"[KitchenGameManager] 游戏解除暂停于客户端 {NetworkManager.Singleton.LocalClientId}"
            );
        }
        UpdateLocalCanTogglePause();
    }

    private void PausingClientIdNet_OnValueChanged(ulong oldClientId, ulong newClientId)
    {
        UpdateLocalCanTogglePause();
        Debug.Log(
            $"[KitchenGameManager] 暂停客户端变更为 {newClientId} 于客户端 {NetworkManager.Singleton.LocalClientId}"
        );
    }

    private void UpdateLocalCanTogglePause()
    {
        bool canToggle = false;
        if (state.Value == State.GamePlaying || state.Value == State.CountdownToStart)
        {
            if (!isGamePausedNet.Value)
            {
                canToggle = true;
            }
            else
            {
                canToggle = (NetworkManager.Singleton.LocalClientId == pausingClientIdNet.Value);
            }
        }

        if (canToggle != localCanTogglePause)
        {
            localCanTogglePause = canToggle;
            OnCanTogglePauseChanged?.Invoke(this, localCanTogglePause);
            Debug.Log(
                $"[KitchenGameManager] 客户端 {NetworkManager.Singleton.LocalClientId} 可否操作暂停: {localCanTogglePause}"
            );
        }
    }

    private void SpawnDeliveryManager()
    {
        if (deliveryManagerPrefab == null)
        {
            Debug.LogError("DeliveryManager 预制件未分配！");
            return;
        }
        GameObject deliveryManagerGO = Instantiate(deliveryManagerPrefab);
        NetworkObject deliveryManagerNetworkObject =
            deliveryManagerGO.GetComponent<NetworkObject>();
        if (deliveryManagerNetworkObject != null)
        {
            deliveryManagerNetworkObject.Spawn();
            Debug.Log("[KitchenGameManager] 服务器上生成了 DeliveryManager");
        }
        else
        {
            Debug.LogError("DeliveryManager 预制件缺少 NetworkObject 组件！");
        }
    }

    private void Start()
    {
        GameInput.Instance.OnPauseActions += GameInput_OnPauseActions;
        GameInput.Instance.OnInteractActions += GameInput_OnInteractActions;
        Debug.Log("[KitchenGameManager] Start - 已订阅输入事件");
    }

    private void GameInput_OnInteractActions(object sender, EventArgs e)
    {
        // 未修改此部分，保持原样
    }

    private void GameInput_OnPauseActions(object sender, EventArgs e)
    {
        TogglePauseGameRequest();
        Debug.Log(
            $"[KitchenGameManager] 客户端 {NetworkManager.Singleton.LocalClientId} 触发暂停操作"
        );
    }

    private void Update()
    {
        if (!IsServer)
            return;
        if (isGamePausedNet.Value)
        {
            Debug.Log("[KitchenGameManager] 服务器: 游戏已暂停，跳过更新");
            return;
        }

        switch (state.Value)
        {
            case State.WaitingToStart:
                break;
            case State.CountdownToStart:
                countdownToStartTimerNet.Value -= Time.deltaTime;
                if (countdownToStartTimerNet.Value < 0f)
                {
                    state.Value = State.GamePlaying;
                    gamePlayingTimerNet.Value = gamePlayingTimerMax;
                    Debug.Log("[KitchenGameManager] 服务器: 倒计时结束，开始游戏");
                }
                break;
            case State.GamePlaying:
                gamePlayingTimerNet.Value -= Time.deltaTime;
                if (gamePlayingTimerNet.Value < 0f)
                {
                    state.Value = State.GameOver;
                    Debug.Log("[KitchenGameManager] 服务器: 游戏时间结束，游戏结束");
                }
                break;
            case State.GameOver:
                break;
        }
    }

    public void StartGameCountdown()
    {
        if (!IsServer)
        {
            Debug.LogWarning("只有服务器可以启动倒计时！");
            return;
        }
        if (state.Value == State.WaitingToStart)
        {
            countdownToStartTimerNet.Value = 3f;
            state.Value = State.CountdownToStart;
            Debug.Log("[KitchenGameManager] 服务器: 开始倒计时");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void TriggerGameOverServerRpc()
    {
        if (state.Value != State.GameOver)
        {
            state.Value = State.GameOver;
            Debug.Log("[KitchenGameManager] 服务器通过 RPC 将状态设置为 GameOver");
        }
    }

    public void GameOver(bool playerWon)
    {
        Debug.Log(
            $"[KitchenGameManager] 本地客户端 {NetworkManager.Singleton.LocalClientId} 调用 GameOver。玩家胜利: {playerWon}"
        );

        Time.timeScale = 0f;

        if (EndGameUI.Instance != null)
        {
            EndGameUI.Instance.ShowResult(playerWon);
            Debug.Log("[KitchenGameManager] 已调用 EndGameUI.ShowResult");
        }
        else
        {
            Debug.LogError("[KitchenGameManager] 尝试显示结果时 EndGameUI.Instance 为 NULL！");
        }
    }

    public bool IsGamePlaying() => state.Value == State.GamePlaying;

    public bool IsCountToStartActive() => state.Value == State.CountdownToStart;

    public float GetCountdownToStartTimer() => countdownToStartTimerNet.Value;

    public bool IsGameOver() => state.Value == State.GameOver;

    public float GetGamePlayingTimerNormalized() =>
        1 - (gamePlayingTimerNet.Value / gamePlayingTimerMax);

    public bool IsGamePaused() => isGamePausedNet.Value;

    public ulong GetPausingClientId() => pausingClientIdNet.Value;

    public void TogglePauseGameRequest()
    {
        bool currentlyPaused = isGamePausedNet.Value;
        ulong localClientId = NetworkManager.Singleton.LocalClientId;

        if (!currentlyPaused)
        {
            RequestPauseServerRpc();
            Debug.Log($"[KitchenGameManager] 客户端 {localClientId} 请求暂停");
        }
        else
        {
            if (localClientId == pausingClientIdNet.Value)
            {
                RequestUnpauseServerRpc();
                Debug.Log($"[KitchenGameManager] 客户端 {localClientId} 请求解除暂停");
            }
            else
            {
                Debug.LogWarning(
                    $"[KitchenGameManager] 客户端 {localClientId} 尝试解除暂停，但游戏由客户端 {pausingClientIdNet.Value} 暂停"
                );
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestPauseServerRpc(ServerRpcParams rpcParams = default)
    {
        if (
            (state.Value == State.GamePlaying || state.Value == State.CountdownToStart)
            && !isGamePausedNet.Value
        )
        {
            isGamePausedNet.Value = true;
            pausingClientIdNet.Value = rpcParams.Receive.SenderClientId;
            Debug.Log(
                $"[KitchenGameManager] 服务器: 游戏已被客户端 {pausingClientIdNet.Value} 暂停"
            );
        }
        else
        {
            Debug.Log(
                $"[KitchenGameManager] 服务器: 客户端 {rpcParams.Receive.SenderClientId} 的暂停请求被忽略。状态: {state.Value}, 已暂停: {isGamePausedNet.Value}"
            );
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestUnpauseServerRpc(ServerRpcParams rpcParams = default)
    {
        if (isGamePausedNet.Value && rpcParams.Receive.SenderClientId == pausingClientIdNet.Value)
        {
            isGamePausedNet.Value = false;
            pausingClientIdNet.Value = ulong.MaxValue;
            Debug.Log(
                $"[KitchenGameManager] 服务器: 游戏已被客户端 {rpcParams.Receive.SenderClientId} 解除暂停"
            );
        }
        else
        {
            Debug.Log(
                $"[KitchenGameManager] 服务器: 客户端 {rpcParams.Receive.SenderClientId} 的解除暂停请求被忽略。已暂停: {isGamePausedNet.Value}, 暂停者: {pausingClientIdNet.Value}"
            );
        }
    }

    public void TogglePauseGame()
    {
        if (!IsServer)
            return;
        isGamePausedNet.Value = !isGamePausedNet.Value;
        Debug.Log($"[KitchenGameManager] 服务器: 暂停状态切换为 {isGamePausedNet.Value}");
    }

    public void SetPausedExternally()
    {
        if (!IsServer)
            return;
        isGamePausedNet.Value = true;
        Debug.Log("[KitchenGameManager] 服务器: 游戏被外部暂停");
    }
}
