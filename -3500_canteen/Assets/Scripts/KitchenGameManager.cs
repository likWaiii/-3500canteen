using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KitchenGameManager : MonoBehaviour
{
    public static KitchenGameManager Instance { get; private set; }

    public event EventHandler OnStateChanged;
    public event EventHandler OnGamePaused;
    public event EventHandler OnGameUnpaused;

    private enum State
    {
        WaitingToStart,
        CountdownToStart,
        GamePlaying,
        GameOver,
    }

    private void Start()
    {
        GameInput.Instance.OnPauseActions += GameInput_OnPauseActions;
        GameInput.Instance.OnInteractActions += GameInput_OnInteractActions;

        if (state == State.WaitingToStart)
        {
            state = State.CountdownToStart;
            OnStateChanged?.Invoke(this, new EventArgs());
        }
    }

    private void GameInput_OnInteractActions(object sender, EventArgs e)
    {
        if (state == State.WaitingToStart)
        {
            state = State.CountdownToStart;
            OnStateChanged?.Invoke(this, new EventArgs());
        }
    }

    private void GameInput_OnPauseActions(object sender, EventArgs e)
    {
        TogglePauseGame();
    }

    private State state;
    private float countdownToStartTimer = 3f;
    private float gamePlayingTimer;
    private float gamePlayingTimerMax = 180f;
    private bool isGamePaused = false;

    private void Awake()
    {
        Instance = this;
        state = State.WaitingToStart;
    }

    private void Update()
    {
        switch (state)
        {
            case State.WaitingToStart:
                break;

            case State.CountdownToStart:
                countdownToStartTimer -= Time.deltaTime;
                if (countdownToStartTimer < 0f)
                {
                    state = State.GamePlaying;
                    gamePlayingTimer = gamePlayingTimerMax;
                    OnStateChanged?.Invoke(this, EventArgs.Empty);
                }

                break;

            case State.GamePlaying:
                gamePlayingTimer -= Time.deltaTime;
                if (gamePlayingTimer < 0f)
                {
                    state = State.GameOver;
                    OnStateChanged?.Invoke(this, EventArgs.Empty);
                }

                break;

            case State.GameOver:
                break;
        }
    }

    public bool IsGamePlaying()
    {
        return state == State.GamePlaying;
    }

    public bool IsCountToStartActive()
    {
        return state == State.CountdownToStart;
    }

    public float GetCountdownToStartTimer()
    {
        return countdownToStartTimer;
    }

    public bool IsGameOver()
    {
        return state == State.GameOver;
    }

    public float GetGamePlayingTimerNormalized()
    {
        return 1 - (gamePlayingTimer / gamePlayingTimerMax);
    }

    public void TogglePauseGame()
    {
        isGamePaused = !isGamePaused;

        if (isGamePaused)
        {
            Time.timeScale = 0f;

            OnGamePaused?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            Time.timeScale = 1f;

            OnGameUnpaused?.Invoke(this, EventArgs.Empty);
        }
    }
}
// using System;
// using Unity.Netcode;
// using UnityEngine;

// public class KitchenGameManager : NetworkBehaviour
// {
//     public static KitchenGameManager Instance { get; private set; }

//     public event EventHandler OnStateChanged;
//     public event EventHandler OnGamePaused;
//     public event EventHandler OnGameUnpaused;

//     [SerializeField]
//     private float gamePlayingTimerMax = 180f;

//     private NetworkVariable<State> state = new NetworkVariable<State>(State.WaitingToStart);
//     private NetworkVariable<float> countdownToStartTimer = new NetworkVariable<float>(3f);
//     private NetworkVariable<float> gamePlayingTimer = new NetworkVariable<float>();
//     private bool isGamePaused = false;

//     // 分别跟踪每个玩家的得分
//     private NetworkVariable<int> player1Score = new NetworkVariable<int>(0);
//     private NetworkVariable<int> player2Score = new NetworkVariable<int>(0);

//     private enum State
//     {
//         WaitingToStart,
//         CountdownToStart,
//         GamePlaying,
//         GameOver,
//     }

//     private void Awake()
//     {
//         Instance = this;
//     }

//     public override void OnNetworkSpawn()
//     {
//         if (IsServer)
//         {
//             // 初始化网络变量
//             state.Value = State.WaitingToStart;
//             countdownToStartTimer.Value = 3f;
//             gamePlayingTimer.Value = gamePlayingTimerMax;

//             // 监听玩家连接
//             NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
//         }

//         // 订阅网络变量变化事件
//         state.OnValueChanged += OnStateValueChanged;
//     }

//     private void OnClientConnectedCallback(ulong clientId)
//     {
//         // 当两个玩家都连接上时，开始游戏倒计时
//         if (NetworkManager.Singleton.ConnectedClientsIds.Count >= 2)
//         {
//             state.Value = State.CountdownToStart;
//         }
//     }

//     private void OnStateValueChanged(State previousValue, State newValue)
//     {
//         OnStateChanged?.Invoke(this, EventArgs.Empty);
//     }

//     private void Update()
//     {
//         if (!IsServer)
//             return;

//         switch (state.Value)
//         {
//             case State.WaitingToStart:
//                 // 等待玩家连接
//                 break;

//             case State.CountdownToStart:
//                 countdownToStartTimer.Value -= Time.deltaTime;
//                 if (countdownToStartTimer.Value < 0f)
//                 {
//                     state.Value = State.GamePlaying;
//                     gamePlayingTimer.Value = gamePlayingTimerMax;
//                 }
//                 break;

//             case State.GamePlaying:
//                 gamePlayingTimer.Value -= Time.deltaTime;
//                 if (gamePlayingTimer.Value < 0f)
//                 {
//                     state.Value = State.GameOver;
//                 }
//                 break;

//             case State.GameOver:
//                 // 游戏结束逻辑
//                 break;
//         }
//     }

//     public bool IsGamePlaying()
//     {
//         return state.Value == State.GamePlaying;
//     }

//     public bool IsGameOver()
//     {
//         return state.Value == State.GameOver;
//     }

//     public bool IsCountdownToStartActive()
//     {
//         return state.Value == State.CountdownToStart;
//     }

//     public float GetCountdownToStartTimer()
//     {
//         return countdownToStartTimer.Value;
//     }

//     public float GetGamePlayingTimerNormalized()
//     {
//         return 1 - (gamePlayingTimer.Value / gamePlayingTimerMax);
//     }

//     [ServerRpc(RequireOwnership = false)]
//     public void AddScoreServerRpc(ulong playerId, int scoreToAdd)
//     {
//         if (playerId == NetworkManager.Singleton.ConnectedClientsIds[0])
//         {
//             player1Score.Value += scoreToAdd;
//         }
//         else
//         {
//             player2Score.Value += scoreToAdd;
//         }
//     }

//     public int GetPlayerScore(bool isPlayer1)
//     {
//         return isPlayer1 ? player1Score.Value : player2Score.Value;
//     }

//     public void TogglePause()
//     {
//         isGamePaused = !isGamePaused;
//         if (isGamePaused)
//         {
//             Time.timeScale = 0f;
//             OnGamePaused?.Invoke(this, EventArgs.Empty);
//         }
//         else
//         {
//             Time.timeScale = 1f;
//             OnGameUnpaused?.Invoke(this, EventArgs.Empty);
//         }
//     }
// }
