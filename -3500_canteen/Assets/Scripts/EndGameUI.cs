// using TMPro;
// using UnityEngine;

// public class EndGameUI : MonoBehaviour
// {
//     public static EndGameUI Instance { get; private set; }

//     [SerializeField]
//     private CanvasGroup panel; // 整个结束面板

//     [SerializeField]
//     private TextMeshProUGUI resultText;

//     private void Awake()
//     {
//         if (Instance != null && Instance != this)
//         {
//             Destroy(gameObject);
//             return;
//         }
//         Instance = this;
//         DontDestroyOnLoad(gameObject);
//         panel.alpha = 0f;
//         panel.interactable = panel.blocksRaycasts = false;
//     }

//     /// <summary>
//     /// 显示胜负结果
//     /// </summary>
//     public void ShowResult(bool isWinner)
//     {
//         panel.alpha = 1f;
//         panel.interactable = panel.blocksRaycasts = true;
//         resultText.text = isWinner ? "You Win! 😊" : "You Lose 😢";
//     }
// }
using TMPro;
using UnityEngine;

public class EndGameUI : MonoBehaviour
{
    public static EndGameUI Instance { get; private set; }

    [SerializeField]
    private CanvasGroup panel; // 整个结束面板

    [SerializeField]
    private TextMeshProUGUI resultText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // 移除 DontDestroyOnLoad(gameObject);
        panel.alpha = 0f;
        panel.interactable = panel.blocksRaycasts = false;
    }

    /// <summary>
    /// 显示胜负结果
    /// </summary>
    public void ShowResult(bool isWinner)
    {
        panel.alpha = 1f;
        panel.interactable = panel.blocksRaycasts = true;
        resultText.text = isWinner ? "You Win! 😊" : "You Lose 😢";
    }
}
