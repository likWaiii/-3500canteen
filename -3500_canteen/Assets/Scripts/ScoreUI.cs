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
//         Debug.Log("[ScoreUI] �ȴ� DeliveryManager ��ʼ��...");
//         yield return new WaitUntil(() => DeliveryManager.Instance != null);

//         Debug.Log("[ScoreUI] ���� OnScoreUpdated �¼�");
//         DeliveryManager.Instance.OnScoreUpdated += OnScoreUpdated;
//         RefreshScore();
//     }

//     private void OnDestroy()
//     {
//         if (DeliveryManager.Instance != null)
//         {
//             Debug.Log("[ScoreUI] ȡ������ OnScoreUpdated �¼�");
//             DeliveryManager.Instance.OnScoreUpdated -= OnScoreUpdated;
//         }
//     }

//     private void OnScoreUpdated(object sender, int newScore)
//     {
//         Debug.Log($"[ScoreUI] �յ��������£�{newScore}");
//         RefreshScore();
//     }

//     private void RefreshScore()
//     {
//         if (scoreText == null)
//         {
//             Debug.LogError("[ScoreUI] scoreText δ��ֵ");
//             return;
//         }

//         // ��ȡ������ҷ���
//         ulong localClientId = NetworkManager.Singleton.LocalClientId;
//         int value = DeliveryManager.Instance.GetPlayerScore(localClientId);
//         Debug.Log($"[ScoreUI] ˢ�·�����{value}");
//         scoreText.text = $"�ܵ÷֣�{value}";
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
            Debug.Log("[ScoreUI] �ȴ� DeliveryManager ��ʼ��...");
            yield return null;
        }
        DeliveryManager.Instance.OnScoreUpdated -= OnScoreUpdated;
        DeliveryManager.Instance.OnScoreUpdated += OnScoreUpdated;
        Debug.Log("[ScoreUI] �ɹ����� OnScoreUpdated �¼�");
        RefreshScore();
    }

    private void OnDestroy()
    {
        if (DeliveryManager.Instance != null)
        {
            DeliveryManager.Instance.OnScoreUpdated -= OnScoreUpdated;
            Debug.Log("[ScoreUI] ��ȡ������ OnScoreUpdated �¼�");
        }
    }

    private void OnScoreUpdated(object sender, int newScore)
    {
        Debug.Log($"[ScoreUI] �յ��������£�{newScore}");
        RefreshScore();
    }

    private void RefreshScore()
    {
        if (scoreText == null)
        {
            Debug.LogError("[ScoreUI] scoreText δ��ֵ");
            return;
        }
        ulong localClientId = NetworkManager.Singleton.LocalClientId;
        int value = DeliveryManager.Instance.GetPlayerScore(localClientId);
        Debug.Log($"[ScoreUI] ˢ�·�����{value}");
        scoreText.text = $"�ܵ÷֣�{value}";
    }
}
