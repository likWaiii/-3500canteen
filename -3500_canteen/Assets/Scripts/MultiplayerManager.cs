using Unity.Netcode;
using UnityEngine;

public class MultiplayerManager : NetworkBehaviour
{
    [SerializeField]
    private Transform[] playerSpawnPoints;

    [SerializeField]
    private GameObject playerPrefab;

    [SerializeField]
    private Transform[] kitchenTransforms; // 两个厨房的位置

    public static MultiplayerManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

            // 主机玩家自动生成
            SpawnPlayerForClient(NetworkManager.Singleton.LocalClientId, 0);
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        // 为新连接的客户端生成玩家，使用第二个生成点
        if (clientId != NetworkManager.Singleton.LocalClientId)
        {
            SpawnPlayerForClient(clientId, 1);
        }
    }

    private void SpawnPlayerForClient(ulong clientId, int spawnPointIndex)
    {
        // 确保索引有效
        if (spawnPointIndex >= playerSpawnPoints.Length)
        {
            Debug.LogError($"无效的生成点索引: {spawnPointIndex}");
            return;
        }

        // 生成玩家并设置所有权
        GameObject playerInstance = Instantiate(
            playerPrefab,
            playerSpawnPoints[spawnPointIndex].position,
            playerSpawnPoints[spawnPointIndex].rotation
        );

        NetworkObject networkObject = playerInstance.GetComponent<NetworkObject>();
        networkObject.SpawnAsPlayerObject(clientId);
    }

    public Transform GetKitchenTransformForPlayer(ulong playerId)
    {
        // 基于玩家ID返回对应的厨房位置
        int kitchenIndex = playerId == NetworkManager.Singleton.ConnectedClientsIds[0] ? 0 : 1;
        if (kitchenIndex < kitchenTransforms.Length)
        {
            return kitchenTransforms[kitchenIndex];
        }

        Debug.LogError("没有足够的厨房位置!");
        return null;
    }
}
