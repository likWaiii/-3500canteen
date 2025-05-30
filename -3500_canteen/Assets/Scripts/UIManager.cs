using UnityEngine;
using UnityEngine.UI;

public class PauseMenuButtonUI : MonoBehaviour
{
    [SerializeField]
    private Button pauseMenuButton;

    private void Awake()
    {
        pauseMenuButton.onClick.AddListener(() =>
        {
            //之前的方法弃用
            // GamePauseUI pauseUI = FindObjectOfType<GamePauseUI>();
            // if (pauseUI != null)
            // {
            //     KitchenGameManager.Instance.SetPausedExternally();
            //     pauseUI.Show();
            // }

            // 确保 KitchenGameManager 实例存在
            if (KitchenGameManager.Instance != null)
            {
                // 调用正确的、包含网络同步逻辑的暂停/恢复请求方法
                KitchenGameManager.Instance.TogglePauseGameRequest();
            }
            else
            {
                Debug.LogError("尝试切换暂停状态时未找到 KitchenGameManager 实例！");
            }
        });
    }
}
