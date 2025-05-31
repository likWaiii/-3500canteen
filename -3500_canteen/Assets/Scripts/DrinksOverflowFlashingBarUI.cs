using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrinksOverflowFlashingBarUI : MonoBehaviour
{
    private const string IS_FLASHING = "IsFlashing";

    [SerializeField]
    private DrinksCounter drinksCounter;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        drinksCounter.OnProgressChanged += DrinksCounter_OnProgressChanged;

        animator.SetBool(IS_FLASHING, false);
    }

    private void DrinksCounter_OnProgressChanged(
        object sender,
        IHasProgress.OnProgressChangedEventArgs e
    )
    {
        float overflowShowProgressAmount = 1f;
        bool show = drinksCounter.IsPouring() && e.progressNormalized >= overflowShowProgressAmount;

        animator.SetBool(IS_FLASHING, show);
    }
}
