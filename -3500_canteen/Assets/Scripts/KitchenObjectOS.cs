using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu()]
public class KitchenObjectOS : ScriptableObject
{
    public Transform prefab;
    public Sprite sprite;
    public string objectName;
    public int value;// 新增字段：价值
}
