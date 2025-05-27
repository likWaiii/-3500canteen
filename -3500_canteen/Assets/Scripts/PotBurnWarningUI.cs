using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotBurnWarningUI : MonoBehaviour
{
    [SerializeField]
    private PotCounter potCounter;

    private void Start()
    {
        potCounter.OnProgressChanged += PotCounter_OnProgressChanged;

        Hide();
    }

    private void PotCounter_OnProgressChanged(
        object sender,
        IHasProgress.OnProgressChangedEventArgs e
    )
    {
        float burnShowProgressAmount = .5f;
        bool show = potCounter.IsBoiled() && e.progressNormalized >= burnShowProgressAmount;

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
