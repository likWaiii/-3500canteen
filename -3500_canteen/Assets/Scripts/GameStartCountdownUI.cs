using System.Collections;
using TMPro;
using UnityEngine;

public class GameStartCountdownUI : MonoBehaviour
{
    private const string NUMBER_POPUP = "NumberPOPup";

    [SerializeField]
    private TextMeshProUGUI countdownText;
    private Animator animator;
    private int previousCountDownNumber = -1;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        Hide();
        enabled = false;
    }

    public void Initialize()
    {
        StartCoroutine(WaitForKitchenGameManagerAndStart());
    }

    private IEnumerator WaitForKitchenGameManagerAndStart()
    {
        yield return new WaitUntil(() => KitchenGameManager.Instance != null);
        KitchenGameManager.Instance.OnStateChangedLocal += KitchenManager_OnStateChanged;
        // KitchenGameManager.Instance.OnStateChanged += KitchenManager_OnStateChanged;
        enabled = true;
        if (KitchenGameManager.Instance.IsCountToStartActive())
        {
            Show();
        }
    }

    private void OnDestroy()
    {
        if (KitchenGameManager.Instance != null)
        {
            KitchenGameManager.Instance.OnStateChangedLocal -= KitchenManager_OnStateChanged;
            // KitchenGameManager.Instance.OnStateChanged -= KitchenManager_OnStateChanged;
        }
    }

    private void KitchenManager_OnStateChanged(object sender, System.EventArgs e)
    {
        if (KitchenGameManager.Instance.IsCountToStartActive())
        {
            Show();
        }
        else
        {
            Hide();
        }
    }

    private void Update()
    {
        if (KitchenGameManager.Instance == null)
            return;
        int countdownNumber = Mathf.CeilToInt(
            KitchenGameManager.Instance.GetCountdownToStartTimer()
        );
        countdownText.text = countdownNumber.ToString();
        if (previousCountDownNumber != countdownNumber)
        {
            previousCountDownNumber = countdownNumber;
            if (animator != null)
            {
                animator.SetTrigger(NUMBER_POPUP);
            }
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayCountDownSound();
            }
        }
    }

    private void Hide() => gameObject.SetActive(false);

    private void Show() => gameObject.SetActive(true);
}
