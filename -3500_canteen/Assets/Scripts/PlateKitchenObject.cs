using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlateKitchenObject : KitchenObject
{
    public event EventHandler<OnIngredientAddedEventArgs> OnIngredientAdded;
    
    [SerializeField] private PlateRecipeSO[] plateRecipeList;
    public class OnIngredientAddedEventArgs : EventArgs
    {
        public KitchenObjectOS kitchenObjectOS;
    }

    [SerializeField]
    private List<KitchenObjectOS> validkitchenObjectSOList;

    private List<KitchenObjectOS> kitchenObjectSOList;

    private void Awake()
    {
        kitchenObjectSOList = new List<KitchenObjectOS>();
    }

    private void Update()
    {
        foreach (PlateRecipeSO plateRecipeSO in plateRecipeList)
        {
            bool isRecipeComplete = true;

            foreach (KitchenObjectOS kitchenObjectOS in plateRecipeSO.input)
            {
                if (!kitchenObjectSOList.Contains(kitchenObjectOS))
                {
                    isRecipeComplete = false;
                    break;
                }
            }
            if (!isRecipeComplete)
            {
                continue;
            }
            if (kitchenObjectSOList.Count != plateRecipeSO.input.Length)
            {
                //The ingredient count does not match the recipe input count
                continue;
            }
            //Recipe is complete
            IKithenObjectParent kitchenObjectParent = GetKitchenObjectParent();
            if (kitchenObjectParent != null && kitchenObjectParent.HasKitchenObject())
            {
                KitchenObject kitchenObject = kitchenObjectParent.GetKitchenObject();
                while (kitchenObject != null)
                {
                    //Destroy the plate
                    Debug.Log("Plate is complete, destroying the plate KitchenObject: " + kitchenObject.name);
                    kitchenObject.DestroySelf();
                    kitchenObject = kitchenObjectParent.GetKitchenObject();
                }
                //Spawn the output KitchenObject
                KitchenObjectOS outputKitchenObjectOS = plateRecipeSO.output;
                Debug.Log("Spawning output KitchenObject: " + outputKitchenObjectOS.name);
                KitchenObject outputKitchenObject = KitchenObject.SpwanKitchenObject(
                    outputKitchenObjectOS,
                    kitchenObjectParent
                );
            }
        }
    }

    public bool TryAddIngredient(KitchenObjectOS kitchenObjectOS)
    {
        if (!validkitchenObjectSOList.Contains(kitchenObjectOS))
        {
            return false;
        }
        if (kitchenObjectSOList.Contains(kitchenObjectOS))
        {
            //Already has this type
            return false;
        }
        else
        {
            kitchenObjectSOList.Add(kitchenObjectOS);

            OnIngredientAdded?.Invoke(
                this,
                new OnIngredientAddedEventArgs { kitchenObjectOS = kitchenObjectOS }
            );

            return true;
        }
    }

    public List<KitchenObjectOS> GetKitchenObjectOSList()
    {
        return kitchenObjectSOList;
    }
}
