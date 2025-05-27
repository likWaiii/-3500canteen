using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotCounterVisual : MonoBehaviour
{
    [SerializeField]
    private PotCounter potCounter;

    [SerializeField]
    private GameObject potOnGameObject;

    [SerializeField]
    private GameObject particlesOnGameObject;

    private void Start()
    {
        potCounter.OnStateChanged += PotCounter_OnStateChanged;
    }

    private void PotCounter_OnStateChanged(object sender, PotCounter.OnStateChangedEventArgs e)
    {
        bool showVisual =
            e.state == PotCounter.State.Boiling || e.state == PotCounter.State.Boiled;
        potOnGameObject.SetActive(showVisual); //Á£×Ó
        particlesOnGameObject.SetActive(showVisual); //ÑÌÎí
    }
}
