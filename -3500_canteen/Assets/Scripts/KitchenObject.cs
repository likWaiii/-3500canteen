using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KitchenObject : MonoBehaviour
{
    [SerializeField]
    private KitchenObjectOS kitchenObjectOS;

    private IKithenObjectParent kitchenObjectParent;

    public KitchenObjectOS GetKitchenObjectOS()
    {
        return kitchenObjectOS;
    }

    public void SetKitchenObjectParent(IKithenObjectParent kitchenObjectParent)
    {
        if (this.kitchenObjectParent != null)
        {
            this.kitchenObjectParent.ClearKitchenObject();
        }

        this.kitchenObjectParent = kitchenObjectParent;

        if (kitchenObjectParent.HasKitchenObject())
        {
            Debug.LogError("Counter already has a KitchenObject!!");
        }

        kitchenObjectParent.SetKitchenObject(this);

        transform.parent = kitchenObjectParent.GetKitchenObjectFollowTransform();
        transform.localPosition = Vector3.zero;
    }

    public IKithenObjectParent GetKitchenObjectParent()
    {
        return kitchenObjectParent;
    }

    public void DestroySelf()
    {
        kitchenObjectParent.ClearKitchenObject();

        Destroy(gameObject);
    }

    public bool TryGetPlate(out PlateKitchenObject plateKitchenObject)
    {
        if (this is PlateKitchenObject)
        {
            plateKitchenObject = this as PlateKitchenObject;
            return true;
        }
        else
        {
            plateKitchenObject = null;
            return false;
        }
    }

    public static KitchenObject SpwanKitchenObject(
        KitchenObjectOS kitchenObjectOS,
        IKithenObjectParent kithenObjectParent
    )
    {
        Transform kitchenObjectTransform = Instantiate(kitchenObjectOS.prefab);

        KitchenObject kitchenObject = kitchenObjectTransform.GetComponent<KitchenObject>();

        kitchenObject.SetKitchenObjectParent(kithenObjectParent);

        return kitchenObject;
    }
}
// using Unity.Netcode;
// using UnityEngine;

// public class KitchenObject : NetworkBehaviour
// {
//     [SerializeField]
//     private KitchenObjectOS kitchenObjectOS;

//     // 1. NetworkVariable：记录父对象的 NetworkObjectId
//     public NetworkVariable<ulong> ParentId = new NetworkVariable<ulong>(
//         0ul,
//         NetworkVariableReadPermission.Everyone,
//         NetworkVariableWritePermission.Everyone
//     );

//     private IKitchenObjectParent parent;

//     public override void OnNetworkSpawn()
//     {
//         ParentId.OnValueChanged += OnParentChanged;

//         // 如果本机刚生成，就主动设置一次
//         if (IsOwner && ParentId.Value != 0)
//             OnParentChanged(0, ParentId.Value);
//     }

//     // 本地调用：直接更新场景里自己，并同步给全网
//     public void SetParentLocal(IKitchenObjectParent newParent)
//     {
//         // 更新本地场景
//         ApplyParent(newParent);

//         // 写入网络变量，广播给别人
//         ParentId.Value = newParent.NetworkObjectId;
//     }

//     private void OnParentChanged(ulong oldId, ulong newId)
//     {
//         if (newId == 0)
//             return;
//         if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(newId, out var netObj))
//         {
//             var parentComp = netObj.GetComponent<IKitchenObjectParent>();
//             ApplyParent(parentComp);
//         }
//     }

//     private void ApplyParent(IKitchenObjectParent newParent)
//     {
//         if (parent != null)
//             parent.ClearKitchenObject();
//         parent = newParent;
//         parent.SetKitchenObject(this);
//         transform.SetParent(parent.GetKitchenObjectFollowTransform());
//         transform.localPosition = Vector3.zero;
//     }

//     // 2. 任何一端都可发起生成请求
//     [ServerRpc(RequireOwnership = false)]
//     public void SpawnObjectServerRpc(ulong parentId)
//     {
//         // Host（Server）会跑这里：Instantiate + Spawn + 同步 ParentId
//         var prefab = kitchenObjectOS.prefab;
//         var instance = Instantiate(prefab);
//         var netObj = instance.GetComponent<NetworkObject>();
//         netObj.Spawn();
//         var kitchenObj = instance.GetComponent<KitchenObject>();
//         kitchenObj.ParentId.Value = parentId;
//     }
// }
