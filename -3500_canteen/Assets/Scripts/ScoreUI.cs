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

    private void Start()
    {
        StartCoroutine(SubscribeToScoreEvents());
    }

    private IEnumerator SubscribeToScoreEvents()
    {
        while (DeliveryManager.Instance == null)
        {
            Debug.Log("[ScoreUI] 等待 DeliveryManager 初始化...");
            yield return null;
        }
        DeliveryManager.Instance.OnScoreUpdated -= OnScoreUpdated;
        DeliveryManager.Instance.OnScoreUpdated += OnScoreUpdated;
        Debug.Log("[ScoreUI] 成功订阅 OnScoreUpdated 事件");
        RefreshScore();
    }

    private void OnDestroy()
    {
        if (DeliveryManager.Instance != null)
        {
            DeliveryManager.Instance.OnScoreUpdated -= OnScoreUpdated;
            Debug.Log("[ScoreUI] 已取消订阅 OnScoreUpdated 事件");
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
        ulong localClientId = NetworkManager.Singleton.LocalClientId;
        int value = DeliveryManager.Instance.GetPlayerScore(localClientId);
        Debug.Log($"[ScoreUI] 刷新分数：{value}");
        scoreText.text = $"总得分：{value}";
    }
}
