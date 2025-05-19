using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class TestingNetworkUI : MonoBehaviour
{
    [SerializeField]
    private Button StartHostButton;

    [SerializeField]
    private Button StartClientButton;

    private void Awake()
    {
        StartHostButton.onClick.AddListener(() =>
        {
            Debug.Log("Starting Host");
            NetworkManager.Singleton.StartHost();
            Hide();
        });

        StartClientButton.onClick.AddListener(() =>
        {
            Debug.Log("Starting Client");
            NetworkManager.Singleton.StartClient();
            Hide();
        });
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
