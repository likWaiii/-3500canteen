using UnityEngine;
using UnityEngine.UI;

public class PauseMenuButtonUI : MonoBehaviour
{
    [SerializeField] private Button pauseMenuButton;

    private void Awake()
    {
        pauseMenuButton.onClick.AddListener(() =>
        {
            GamePauseUI pauseUI = FindObjectOfType<GamePauseUI>();
            if (pauseUI != null)
            {
                KitchenGameManager.Instance.SetPausedExternally();
                pauseUI.Show();
            }
        });
    }
}
