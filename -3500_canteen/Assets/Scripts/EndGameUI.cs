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

//lyl修改前
//using TMPro;
//using UnityEngine;

//public class EndGameUI : MonoBehaviour
//{
//    public static EndGameUI Instance { get; private set; }

//    [SerializeField]
//    private CanvasGroup panel; // 整个结束面板

//    [SerializeField]
//    private TextMeshProUGUI resultText;

//    private void Awake()
//    {
//        if (Instance != null && Instance != this)
//        {
//            Destroy(gameObject);
//            return;
//        }
//        Instance = this;
//        // 移除 DontDestroyOnLoad(gameObject);
//        panel.alpha = 0f;
//        panel.interactable = panel.blocksRaycasts = false;
//    }

//    /// <summary>
//    /// 显示胜负结果
//    /// </summary>
//    public void ShowResult(bool isWinner)
//    {
//        panel.alpha = 1f;
//        panel.interactable = panel.blocksRaycasts = true;
//        resultText.text = isWinner ? "You Win! 😊" : "You Lose 😢";
//    }
//}


using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EndGameUI : MonoBehaviour
{
    public static EndGameUI Instance { get; private set; }

    [SerializeField] private CanvasGroup panel;           // UI面板
    [SerializeField] private Image backgroundImage;       // 显示胜/负背景图
    [SerializeField] private Sprite winSprite;            // 胜利图片
    [SerializeField] private Sprite loseSprite;           // 失败图片
    [SerializeField] private Button returnButton;         // 返回按钮

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        panel.alpha = 0f;
        panel.interactable = panel.blocksRaycasts = false;

        returnButton.onClick.AddListener(() =>
        {
            Loader.Load(Loader.Scene.GameScene);
        });
    }

    /// <summary>
    /// 显示结果背景图（胜或负）
    /// </summary>
    public void ShowResult(bool isWinner)
    {
        panel.alpha = 1f;
        panel.interactable = panel.blocksRaycasts = true;

        backgroundImage.sprite = isWinner ? winSprite : loseSprite;
    }

    private void OnReturnToLoading()
    {
        SceneManager.LoadScene("Loading"); // 替换成你的 loading 场景名
    }
}

