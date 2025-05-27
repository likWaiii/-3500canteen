using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class TestingNetworkUI : MonoBehaviour
{
    [SerializeField] private Button StartHostButton;
    [SerializeField] private Button StartClientButton;
    [SerializeField] private GameStartCountdownUI countdownUI;

    private void Awake()
    {
        StartHostButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();

            if (countdownUI != null)
            {
                countdownUI.gameObject.SetActive(true); // ���� UI GameObject
                countdownUI.Initialize(); // �ӳټ���״̬�仯
            }

            Hide();
        });

        StartClientButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();

            if (countdownUI != null)
            {
                countdownUI.gameObject.SetActive(true);
                countdownUI.Initialize();
            }

            Hide();
        });
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
