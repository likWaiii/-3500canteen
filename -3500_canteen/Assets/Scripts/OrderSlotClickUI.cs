using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrderSlotClickUI: MonoBehaviour
{
    [SerializeField] private int slotIndex;

    public void OnClick()
    {
        var held = Player.Instance.GetKitchenObject()?.GetKitchenObjectOS();
        if (held == null)
        {
            Debug.Log("❌ 没有拿着菜");
            return;
        }

        DeliveryManager.Instance.TrySubmitItemToSlot(slotIndex, held);

        // 清除并销毁
        Player.Instance.GetKitchenObject().DestroySelf();
        Player.Instance.ClearKitchenObject();

    }
}