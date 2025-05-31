using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrinksCounterSound : MonoBehaviour
{
    [SerializeField]
    private DrinksCounter drinksCounter;

    private AudioSource audioSource;
    private float warningSoundTimer;
    private bool playWarningSound;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        drinksCounter.OnStateChanged += DrinksCounter_OnStateChanged;
        drinksCounter.OnProgressChanged += DrinksCounter_OnProgressChanged;
    }

    private void DrinksCounter_OnProgressChanged(
        object sender,
        IHasProgress.OnProgressChangedEventArgs e
    )
    {
        float overflowShowProgressAmount = 1f;
        playWarningSound = drinksCounter.IsOverflow() && e.progressNormalized >= overflowShowProgressAmount;
    }

    private void DrinksCounter_OnStateChanged(object sender, DrinksCounter.OnStateChangedEventArgs e)
    {
        bool playSound =
            e.state == DrinksCounter.State.Pouring || e.state == DrinksCounter.State.Overflow;
        if (playSound)
        {
            audioSource.Play();
        }
        else
        {
            audioSource.Pause();
        }
    }

    private void Update()
    {
        if (playWarningSound)
        {
            warningSoundTimer -= Time.deltaTime;
            if (warningSoundTimer <= 0)
            {
                float warningSoundTimerMax = .2f;
                warningSoundTimer = warningSoundTimerMax;

                SoundManager.Instance.PlayWarningSound(drinksCounter.transform.position);
            }
        }
    }
}
