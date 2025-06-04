using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndGameUI : MonoBehaviour
{
    public static EndGameUI Instance { get; private set; }

    [SerializeField]
    private CanvasGroup panel; // UI面板

    [SerializeField]
    private Image backgroundImage; // 显示胜/负背景图

    [SerializeField]
    private Sprite winSprite; // 胜利图片

    [SerializeField]
    private Sprite loseSprite; // 失败图片

    [SerializeField]
    private Button returnButton; // 返回按钮

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

        // 修改这里的监听器，调用一个新的方法来处理返回逻辑
        returnButton.onClick.AddListener(HandleReturnButtonClick);

        // 注意：你原来的代码这里是 Loader.Load(Loader.Scene.GameScene);
        // 如果从游戏结束返回是应该去主菜单，这里应该加载主菜单场景。
        // 下面的 HandleReturnButtonClick 方法会加载主菜单场景。
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

    /// <summary>
    /// 处理返回按钮点击事件
    /// </summary>
    private void HandleReturnButtonClick()
    {
        Debug.Log("[EndGameUI] 返回按钮被点击。");

        // 1. 关闭 NetworkManager 会话
        // 在加载新场景（主菜单）之前，必须先关闭当前的 NetworkManager 会话
        // 只需要检查 NetworkManager.Singleton 是否存在即可，Shutdown() 方法本身是安全的
        if (NetworkManager.Singleton != null)
        {
            Debug.Log("[EndGameUI] 正在关闭 NetworkManager...");
            NetworkManager.Singleton.Shutdown(); // 调用 Shutdown 来停止网络会话
            Debug.Log("[EndGameUI] NetworkManager 已关闭。");
        }
        else
        {
            Debug.LogWarning("[EndGameUI] NetworkManager.Singleton 不存在，无需关闭。");
        }

        SceneManager.LoadScene("MainMenuScene"); 
    }

    private void OnReturnToLoading()
    {
        Debug.LogWarning(
            "[EndGameUI] OnReturnToLoading 方法被调用，但请确认它包含了 NetworkManager Shutdown 逻辑和场景加载。"
        );
        if (NetworkManager.Singleton != null) {
            NetworkManager.Singleton.Shutdown();
        }
        SceneManager.LoadScene("MainMenuScene"); // 替换场景名
    }
}
