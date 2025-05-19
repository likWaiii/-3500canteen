using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// public class ScoreUI : MonoBehaviour
// {
//     [SerializeField]
//     private TextMeshProUGUI scoreText;

//     private void Start()
//     {
//         // 监听订单成功事件
//         DeliveryManager.Instance.OnRecipeSucess += OnRecipeSucess;
//         RefreshScore();
//     }

//     private void OnDestroy()
//     {
//         DeliveryManager.Instance.OnRecipeSucess -= OnRecipeSucess;
//     }

//     private void OnRecipeSucess(object sender, System.EventArgs e)
//     {
//         RefreshScore();
//     }

//     private void RefreshScore()
//     {
//         int value = DeliveryManager.Instance.GetTotalEarnedValue();
//         scoreText.text = $"总得分：{value}";
//     }
// }
public class ScoreUI : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI scoreText;

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => DeliveryManager.Instance != null);

        Debug.Log(
            $"ScoreUI ({GetInstanceID()}) 订阅 DeliveryManager ({DeliveryManager.Instance.GetInstanceID()}) 的 OnRecipeSucess 事件"
        );
        DeliveryManager.Instance.OnRecipeSucess += OnRecipeSucess;
        DeliveryManager.Instance.OnScoreUpdated += OnScoreUpdated;
        RefreshScore();
    }

    private void OnDestroy()
    {
        if (DeliveryManager.Instance != null)
        {
            Debug.Log("取消订阅 OnRecipeSucess 事件");
            DeliveryManager.Instance.OnRecipeSucess -= OnRecipeSucess;
            DeliveryManager.Instance.OnScoreUpdated -= OnScoreUpdated;
        }
    }

    private void OnRecipeSucess(object sender, System.EventArgs e)
    {
        Debug.Log("OnRecipeSucess 事件触发，刷新得分");
        RefreshScore();
    }

    private void OnScoreUpdated(object sender, int newTotalValue)
    {
        Debug.Log("OnScoreUpdated 事件触发，刷新得分");
        RefreshScore();
        // scoreText.text = $"总得分：{newTotalValue}";
    }

    private void RefreshScore()
    {
        if (DeliveryManager.Instance == null)
        {
            Debug.LogError("DeliveryManager.Instance 为 null");
            return;
        }

        int value = DeliveryManager.Instance.GetTotalEarnedValue();
        Debug.Log($"当前总得分：{value}");
        if (scoreText != null)
        {
            scoreText.text = $"总得分：{value}";
        }
        else
        {
            Debug.LogError("scoreText 未赋值");
        }
    }
}
