using System;
using System.Collections.Generic;
using UnityEngine;

public class WaitingRecipe
{
    public RecipeSO recipeSO;
    public float remainingTime;
    public bool isCompleted;
    public Dictionary<KitchenObjectOS, int> submittedDict;
    public WaitingRecipe(RecipeSO recipeSO)
    {
        this.recipeSO = recipeSO;
        this.remainingTime = recipeSO.maxTime;
        this.isCompleted = false;
        this.submittedDict = new Dictionary<KitchenObjectOS, int>(new KitchenObjectOSComparer());
    }
}
