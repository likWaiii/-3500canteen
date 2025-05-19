using UnityEngine;

public interface IKitchenObjectParent
{
    void ClearKitchenObject();
    bool HasKitchenObject();
    KitchenObject GetKitchenObject();
    void SetKitchenObject(KitchenObject kitchenObject);
    Transform GetKitchenObjectFollowTransform();
}
