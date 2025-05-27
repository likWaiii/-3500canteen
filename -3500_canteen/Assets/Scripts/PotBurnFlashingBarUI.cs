using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotBurnFlashingBarUI : MonoBehaviour
{
    private const string IS_FLASHING = "IsFlashing";

    [SerializeField]
    private PotCounter potCounter;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        potCounter.OnProgressChanged += PotCounter_OnProgressChanged;

        animator.SetBool(IS_FLASHING, false);
    }

    private void PotCounter_OnProgressChanged(
        object sender,
        IHasProgress.OnProgressChangedEventArgs e
    )
    {
        float burnShowProgressAmount = .5f;
        bool show = potCounter.IsBoiled() && e.progressNormalized >= burnShowProgressAmount;

        animator.SetBool(IS_FLASHING, show);
    }
}
