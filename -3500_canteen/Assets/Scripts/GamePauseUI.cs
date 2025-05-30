// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;

// public class GamePauseUI : MonoBehaviour
// {
//     [SerializeField]
//     private Button resumeButton;

//     [SerializeField]
//     private Button mainMenuButton;

//     [SerializeField]
//     private Button optionButton;

//     private void Start()
//     {
//         KitchenGameManager.Instance.OnGamePaused += KitchenGameManager_OnGamePaused;
//         KitchenGameManager.Instance.OnGameUnpaused += KitchenGameManager_OnGameUnpaused;

//         Hide();
//     }

//     private void Awake()
//     {
//         resumeButton.onClick.AddListener(() =>
//         {
//             KitchenGameManager.Instance.TogglePauseGame();
//         });

//         mainMenuButton.onClick.AddListener(() =>
//         {
//             Loader.Load(Loader.Scene.MainMenuScene);
//         });

//         optionButton.onClick.AddListener(() =>
//         {
//             Hide();
//             OptionsUI.Instance.Show(Show);
//         });
//     }

//     private void KitchenGameManager_OnGameUnpaused(object sender, System.EventArgs e)
//     {
//         Hide();
//     }

//     private void KitchenGameManager_OnGamePaused(object sender, System.EventArgs e)
//     {
//         Show();
//     }

//     public void Show()
//     {
//         gameObject.SetActive(true);
//     }

//     private void Hide()
//     {
//         gameObject.SetActive(false);
//     }
// }
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GamePauseUI : MonoBehaviour
{
    [SerializeField]
    private Button resumeButton;

    [SerializeField]
    private Button mainMenuButton;

    [SerializeField]
    private Button optionButton;

    private void Start()
    {
        KitchenGameManager.Instance.OnGamePaused += KitchenGameManager_OnGamePaused;
        KitchenGameManager.Instance.OnGameUnpaused += KitchenGameManager_OnGameUnpaused;
        KitchenGameManager.Instance.OnCanTogglePauseChanged +=
            KitchenGameManager_OnCanTogglePauseChanged;

        Hide();
        UpdateResumeButtonInteractability();
        Debug.Log("[GamePauseUI] Start - 已订阅事件");
    }

    private void OnDestroy()
    {
        if (KitchenGameManager.Instance != null)
        {
            KitchenGameManager.Instance.OnGamePaused -= KitchenGameManager_OnGamePaused;
            KitchenGameManager.Instance.OnGameUnpaused -= KitchenGameManager_OnGameUnpaused;
            KitchenGameManager.Instance.OnCanTogglePauseChanged -=
                KitchenGameManager_OnCanTogglePauseChanged;
            Debug.Log("[GamePauseUI] OnDestroy - 已取消订阅事件");
        }
    }

    private void Awake()
    {
        resumeButton.onClick.AddListener(() =>
        {
            KitchenGameManager.Instance.TogglePauseGameRequest();
            Debug.Log("[GamePauseUI] 点击了恢复按钮");
        });

        mainMenuButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.Shutdown();
            Loader.Load(Loader.Scene.MainMenuScene);
            Debug.Log("[GamePauseUI] 点击了主菜单按钮，关闭网络并加载主菜单");
        });

        optionButton.onClick.AddListener(() =>
        {
            Hide();
            OptionsUI.Instance.Show(Show);
            Debug.Log("[GamePauseUI] 点击了选项按钮，显示选项 UI");
        });
    }

    private void KitchenGameManager_OnGameUnpaused(object sender, System.EventArgs e)
    {
        Hide();
        Debug.Log("[GamePauseUI] 游戏解除暂停，隐藏暂停 UI");
    }

    private void KitchenGameManager_OnGamePaused(object sender, System.EventArgs e)
    {
        Show();
        Debug.Log("[GamePauseUI] 游戏暂停，显示暂停 UI");
    }

    private void KitchenGameManager_OnCanTogglePauseChanged(object sender, bool canToggle)
    {
        UpdateResumeButtonInteractability();
        Debug.Log($"[GamePauseUI] 可操作暂停状态变更为 {canToggle}");
    }

    private void UpdateResumeButtonInteractability()
    {
        if (KitchenGameManager.Instance == null)
        {
            resumeButton.interactable = false;
            Debug.Log("[GamePauseUI] KitchenGameManager.Instance 为 null，禁用恢复按钮");
            return;
        }

        bool isPaused = KitchenGameManager.Instance.IsGamePaused();
        bool canLocalClientUnpause =
            isPaused
            && (
                NetworkManager.Singleton.LocalClientId
                == KitchenGameManager.Instance.GetPausingClientId()
            );
        resumeButton.interactable = canLocalClientUnpause;
        Debug.Log(
            $"[GamePauseUI] 恢复按钮可交互性: {canLocalClientUnpause} (已暂停: {isPaused}, 本地客户端 ID: {NetworkManager.Singleton.LocalClientId}, 暂停者客户端 ID: {KitchenGameManager.Instance.GetPausingClientId()})"
        );
    }

    public void Show()
    {
        gameObject.SetActive(true);
        UpdateResumeButtonInteractability();
        resumeButton.Select();
        Debug.Log("[GamePauseUI] 显示暂停 UI");
    }

    private void Hide()
    {
        gameObject.SetActive(false);
        Debug.Log("[GamePauseUI] 隐藏暂停 UI");
    }
}
