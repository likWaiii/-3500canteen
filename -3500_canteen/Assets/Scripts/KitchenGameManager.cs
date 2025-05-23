// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class KitchenGameManager : MonoBehaviour
// {
//     public static KitchenGameManager Instance { get; private set; }

//     public event EventHandler OnStateChanged;
//     public event EventHandler OnGamePaused;
//     public event EventHandler OnGameUnpaused;

//     private enum State
//     {
//         WaitingToStart,
//         CountdownToStart,
//         GamePlaying,
//         GameOver,
//     }

//     private void Start()
//     {
//         GameInput.Instance.OnPauseActions += GameInput_OnPauseActions;
//         GameInput.Instance.OnInteractActions += GameInput_OnInteractActions;

//         if (state == State.WaitingToStart)
//         {
//             state = State.CountdownToStart;
//             OnStateChanged?.Invoke(this, new EventArgs());
//         }
//     }

//     private void GameInput_OnInteractActions(object sender, EventArgs e)
//     {
//         if (state == State.WaitingToStart)
//         {
//             state = State.CountdownToStart;
//             OnStateChanged?.Invoke(this, new EventArgs());
//         }
//     }

//     private void GameInput_OnPauseActions(object sender, EventArgs e)
//     {
//         TogglePauseGame();
//     }

//     private State state;
//     private float countdownToStartTimer = 3f;
//     private float gamePlayingTimer;
//     private float gamePlayingTimerMax = 180f;
//     private bool isGamePaused = false;

//     private void Awake()
//     {
//         Instance = this;
//         state = State.WaitingToStart;
//     }

//     private void Update()
//     {
//         switch (state)
//         {
//             case State.WaitingToStart:
//                 break;

//             case State.CountdownToStart:
//                 countdownToStartTimer -= Time.deltaTime;
//                 if (countdownToStartTimer < 0f)
//                 {
//                     state = State.GamePlaying;
//                     gamePlayingTimer = gamePlayingTimerMax;
//                     OnStateChanged?.Invoke(this, EventArgs.Empty);
//                 }

//                 break;

//             case State.GamePlaying:
//                 gamePlayingTimer -= Time.deltaTime;
//                 if (gamePlayingTimer < 0f)
//                 {
//                     state = State.GameOver;
//                     OnStateChanged?.Invoke(this, EventArgs.Empty);
//                 }

//                 break;

//             case State.GameOver:
//                 break;
//         }
//     }

//     /// <summary>
//     /// 游戏结束时由外部脚本（DeliveryManager、Timer 等）调用
//     /// </summary>
//     public void GameOver(bool playerWon)
//     {
//         Debug.Log("调用 GameOver，胜利？" + playerWon);

//         // 暂停游戏（可选）
//         Time.timeScale = 0f;

//         // 显示结束面板
//         if (EndGameUI.Instance != null)
//         {
//             EndGameUI.Instance.ShowResult(playerWon);
//         }
//         else
//         {
//             Debug.LogError("找不到 EndGameUI.Instance，请确认场景中已挂载 EndGameUI 脚本！");
//         }
//     }

//     public bool IsGamePlaying()
//     {
//         return state == State.GamePlaying;
//     }

//     public bool IsCountToStartActive()
//     {
//         return state == State.CountdownToStart;
//     }

//     public float GetCountdownToStartTimer()
//     {
//         return countdownToStartTimer;
//     }

//     public bool IsGameOver()
//     {
//         return state == State.GameOver;
//     }

//     public float GetGamePlayingTimerNormalized()
//     {
//         return 1 - (gamePlayingTimer / gamePlayingTimerMax);
//     }

//     public void TogglePauseGame()
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
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class KitchenGameManager : NetworkBehaviour
{
    public static KitchenGameManager Instance { get; private set; }

    public event EventHandler OnStateChanged;
    public event EventHandler OnGamePaused;
    public event EventHandler OnGameUnpaused;

    [SerializeField]
    private GameObject deliveryManagerPrefab; // 在 Inspector 中分配 DeliveryManager 预制件

    private enum State
    {
        WaitingToStart,
        CountdownToStart,
        GamePlaying,
        GameOver,
    }

    private State state;
    private float countdownToStartTimer = 3f;
    private float gamePlayingTimer;
    private float gamePlayingTimerMax = 180f;
    private bool isGamePaused = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        state = State.WaitingToStart;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            SpawnDeliveryManager();
        }
    }

    private void SpawnDeliveryManager()
    {
        if (deliveryManagerPrefab == null)
        {
            Debug.LogError("KitchenGameManager 中未分配 DeliveryManager 预制件！");
            return;
        }

        GameObject deliveryManagerGO = Instantiate(deliveryManagerPrefab);
        NetworkObject deliveryManagerNetworkObject =
            deliveryManagerGO.GetComponent<NetworkObject>();

        if (deliveryManagerNetworkObject != null)
        {
            deliveryManagerNetworkObject.Spawn();
            Debug.Log("[KitchenGameManager] DeliveryManager 预制件已在服务器上生成。");
        }
        else
        {
            Debug.LogError("[KitchenGameManager] DeliveryManager 预制件没有 NetworkObject 组件！");
        }
    }

    private void Start()
    {
        GameInput.Instance.OnPauseActions += GameInput_OnPauseActions;
        GameInput.Instance.OnInteractActions += GameInput_OnInteractActions;

        if (state == State.WaitingToStart)
        {
            state = State.CountdownToStart;
            OnStateChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private void GameInput_OnInteractActions(object sender, EventArgs e)
    {
        if (state == State.WaitingToStart)
        {
            state = State.CountdownToStart;
            OnStateChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private void GameInput_OnPauseActions(object sender, EventArgs e)
    {
        TogglePauseGame();
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

    public void GameOver(bool playerWon)
    {
        Debug.Log("调用 GameOver，胜利？" + playerWon);
        Time.timeScale = 0f;

        if (EndGameUI.Instance != null)
        {
            EndGameUI.Instance.ShowResult(playerWon);
        }
        else
        {
            Debug.LogError("找不到 EndGameUI.Instance，请确认场景中已挂载 EndGameUI 脚本！");
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
