using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrinksCounterVisual : MonoBehaviour
{
    [SerializeField]
    private DrinksCounter drinksCounter;

    [SerializeField]
    private GameObject particlesOnGameObject;

    private void Start()
    {
        drinksCounter.OnStateChanged += DrinksCounter_OnStateChanged;
    }

    private void DrinksCounter_OnStateChanged(object sender, DrinksCounter.OnStateChangedEventArgs e)
    {
        bool showVisual =
            e.state == DrinksCounter.State.Overflow;
        particlesOnGameObject.transform.position = drinksCounter.getCup().transform.position; // …Ë÷√—ÃŒÌŒª÷√
        particlesOnGameObject.SetActive(showVisual); // —ÃŒÌ  
    }
}
