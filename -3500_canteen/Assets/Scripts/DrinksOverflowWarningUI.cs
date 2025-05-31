using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrinksOverflowWarningUI : MonoBehaviour
{
    [SerializeField]
    private DrinksCounter drinksCounter;

    private void Start()
    {
        drinksCounter.OnProgressChanged += DrinksCounter_OnProgressChanged;

        Hide();
    }

    private void DrinksCounter_OnProgressChanged(
        object sender,
        IHasProgress.OnProgressChangedEventArgs e
    )
    {
        float overflowShowProgressAmount = 1f;
        bool show = drinksCounter.IsPouring() && e.progressNormalized >= overflowShowProgressAmount;

        if (show)
        {
            Show();
        }
        else
        {
            Hide();
        }
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
