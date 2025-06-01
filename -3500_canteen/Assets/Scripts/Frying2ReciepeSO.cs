using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class Frying2ReciepeSO : ScriptableObject
{
    public KitchenObjectOS input;
    public KitchenObjectOS middle;
    public KitchenObjectOS output;

    public float fryingTimerMax;
}
