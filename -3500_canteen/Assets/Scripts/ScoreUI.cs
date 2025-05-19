// // using System.Collections;
// // using System.Collections.Generic;
// // using TMPro;
// // using UnityEngine;

// // public class ScoreUI : MonoBehaviour
// // {
// //     [SerializeField]
// //     private TextMeshProUGUI scoreText;

// //     private IEnumerator Start()
// //     {
// //         yield return new WaitUntil(() => DeliveryManager.Instance != null);

// //         Debug.Log(
// //             $"ScoreUI ({GetInstanceID()}) 订阅 DeliveryManager ({DeliveryManager.Instance.GetInstanceID()}) 的 OnRecipeSucess 事件"
// //         );
// //         DeliveryManager.Instance.OnRecipeSucess += OnRecipeSucess;
// //         DeliveryManager.Instance.OnScoreUpdated += OnScoreUpdated;
// //         RefreshScore();
// //     }

// //     private void OnDestroy()
// //     {
// //         if (DeliveryManager.Instance != null)
// //         {
// //             Debug.Log("取消订阅 OnRecipeSucess 事件");
// //             DeliveryManager.Instance.OnRecipeSucess -= OnRecipeSucess;
// //             DeliveryManager.Instance.OnScoreUpdated -= OnScoreUpdated;
// //         }
// //     }

// //     private void OnRecipeSucess(object sender, System.EventArgs e)
// //     {
// //         Debug.Log("OnRecipeSucess 事件触发，刷新得分");
// //         RefreshScore();
// //     }

// //     private void OnScoreUpdated(object sender, int newTotalValue)
// //     {
// //         Debug.Log("OnScoreUpdated 事件触发，刷新得分");
// //         RefreshScore();
// //         // scoreText.text = $"总得分：{newTotalValue}";
// //     }

// //     private void RefreshScore()
// //     {
// //         if (DeliveryManager.Instance == null)
// //         {
// //             Debug.LogError("DeliveryManager.Instance 为 null");
// //             return;
// //         }

// //         int value = DeliveryManager.Instance.GetTotalEarnedValue();
// //         Debug.Log($"当前总得分：{value}");
// //         if (scoreText != null)
// //         {
// //             scoreText.text = $"总得分：{value}";
// //         }
// //         else
// //         {
// //             Debug.LogError("scoreText 未赋值");
// //         }
// //     }
// // }
// using System.Collections;
// using System.Collections.Generic;
// using TMPro;
// using Unity.Netcode;
// using UnityEngine;

// public class ScoreUI : MonoBehaviour
// {
//     [SerializeField]
//     private TextMeshProUGUI scoreText;

//     private IEnumerator Start()
//     {
//         Debug.Log("[ScoreUI] 等待 DeliveryManager 初始化...");
//         yield return new WaitUntil(() => DeliveryManager.Instance != null);

//         Debug.Log("[ScoreUI] 订阅 OnScoreUpdated 事件");
//         DeliveryManager.Instance.OnScoreUpdated += OnScoreUpdated;
//         RefreshScore();
//     }

//     private void OnDestroy()
//     {
//         if (DeliveryManager.Instance != null)
//         {
//             Debug.Log("[ScoreUI] 取消订阅 OnScoreUpdated 事件");
//             DeliveryManager.Instance.OnScoreUpdated -= OnScoreUpdated;
//         }
//     }

//     private void OnScoreUpdated(object sender, int newScore)
//     {
//         Debug.Log($"[ScoreUI] 收到分数更新：{newScore}");
//         RefreshScore();
//     }

//     private void RefreshScore()
//     {
//         if (scoreText == null)
//         {
//             Debug.LogError("[ScoreUI] scoreText 未赋值");
//             return;
//         }

//         // 获取本地玩家分数
//         ulong localClientId = NetworkManager.Singleton.LocalClientId;
//         int value = DeliveryManager.Instance.GetPlayerScore(localClientId);
//         Debug.Log($"[ScoreUI] 刷新分数：{value}");
//         scoreText.text = $"总得分：{value}";
//     }
// }
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class ScoreUI : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI scoreText;

    private IEnumerator Start()
    {
        Debug.Log("[ScoreUI] 等待 DeliveryManager 初始化...");
        yield return new WaitUntil(() => DeliveryManager.Instance != null);

        Debug.Log("[ScoreUI] 订阅 OnScoreUpdated 事件");
        DeliveryManager.Instance.OnScoreUpdated += OnScoreUpdated;
        RefreshScore();
    }

    private void OnDestroy()
    {
        if (DeliveryManager.Instance != null)
        {
            Debug.Log("[ScoreUI] 取消订阅 OnScoreUpdated 事件");
            DeliveryManager.Instance.OnScoreUpdated -= OnScoreUpdated;
        }
    }

    private void OnScoreUpdated(object sender, int newScore)
    {
        Debug.Log($"[ScoreUI] 收到分数更新：{newScore}");
        RefreshScore();
    }

    private void RefreshScore()
    {
        if (scoreText == null)
        {
            Debug.LogError("[ScoreUI] scoreText 未赋值");
            return;
        }

        // 获取本地玩家分数
        ulong localClientId = NetworkManager.Singleton.LocalClientId;
        int value = DeliveryManager.Instance.GetPlayerScore(localClientId);
        Debug.Log($"[ScoreUI] 刷新分数：{value}");
        scoreText.text = $"总得分：{value}";
    }
}
