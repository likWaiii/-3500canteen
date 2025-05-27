using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotCounterSound : MonoBehaviour
{
    [SerializeField]
    private PotCounter potCounter;

    private AudioSource audioSource;
    private float warningSoundTimer;
    private bool playWarningSound;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        potCounter.OnStateChanged += PotCounter_OnStateChanged;
        potCounter.OnProgressChanged += PotCounter_OnProgressChanged;
    }

    private void PotCounter_OnProgressChanged(
        object sender,
        IHasProgress.OnProgressChangedEventArgs e
    )
    {
        float burnShowProgressAmount = .5f;
        playWarningSound = potCounter.IsBoiled() && e.progressNormalized >= burnShowProgressAmount;
    }

    private void PotCounter_OnStateChanged(object sender, PotCounter.OnStateChangedEventArgs e)
    {
        bool playSound =
            e.state == PotCounter.State.Boiling || e.state == PotCounter.State.Boiled;
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

                SoundManager.Instance.PlayWarningSound(potCounter.transform.position);
            }
        }
    }
}
