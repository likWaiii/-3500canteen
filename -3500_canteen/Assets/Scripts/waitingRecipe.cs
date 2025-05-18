using System;
using System.Collections.Generic;
using UnityEngine;

public class WaitingRecipe
{
    public RecipeSO recipeSO;
    public float remainingTime;
    public bool isCompleted;
    public HashSet<KitchenObjectOS> submittedSet = new HashSet<KitchenObjectOS>();
    public WaitingRecipe(RecipeSO recipeSO)
    {
        this.recipeSO = recipeSO;
        this.remainingTime = recipeSO.maxTime;
        this.isCompleted = false;
    }
}
