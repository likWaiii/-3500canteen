// using System.Collections;
// using System.Collections.Generic;
// using TMPro;
// using UnityEngine;
// using UnityEngine.UI;

// public class DeliveryResultUI : MonoBehaviour
// {
//     private const string POPUP = "Popup";

//     [SerializeField]
//     private Image backgroundImage;

//     [SerializeField]
//     private Image iconImage;

//     [SerializeField]
//     private TextMeshProUGUI messageText;

//     [SerializeField]
//     private Color successColor;

//     [SerializeField]
//     private Color failedColor;

//     [SerializeField]
//     private Sprite sucessSprite;

//     [SerializeField]
//     private Sprite failedSprite;

//     private Animator animator;

//     private void Awake()
//     {
//         animator = GetComponentInChildren<Animator>();
//     }

//     private void Start()
//     {
//         DeliveryManager.Instance.OnRecipeSuccess += DeliveryManger_OnRecipeSuccess;
//         DeliveryManager.Instance.OnRecipeFailed += DeliveryManger_OnRecipeFailed;

//         gameObject.SetActive(false);
//     }

//     private void DeliveryManger_OnRecipeFailed(object sender, System.EventArgs e)
//     {
//         gameObject.SetActive(true);

//         animator.SetTrigger(POPUP);
//         backgroundImage.color = failedColor;
//         iconImage.sprite = failedSprite;
//         messageText.text = "送达\n失败";
//     }

//     private void DeliveryManger_OnRecipeSuccess(object sender, System.EventArgs e)
//     {
//         gameObject.SetActive(true);

//         animator.SetTrigger(POPUP);
//         backgroundImage.color = successColor;
//         iconImage.sprite = sucessSprite;
//         messageText.text = "送达\n成功";
//     }
// }
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeliveryResultUI : MonoBehaviour
{
    private const string POPUP = "Popup";

    [SerializeField]
    private Image backgroundImage;

    [SerializeField]
    private Image iconImage;

    [SerializeField]
    private TextMeshProUGUI messageText;

    [SerializeField]
    private Color successColor;

    [SerializeField]
    private Color failedColor;

    [SerializeField]
    private Sprite sucessSprite;

    [SerializeField]
    private Sprite failedSprite;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        StartCoroutine(SubscribeToDeliveryManagerEvents());
        gameObject.SetActive(false);
    }

    private IEnumerator SubscribeToDeliveryManagerEvents()
    {
        while (DeliveryManager.Instance == null)
        {
            Debug.Log("[DeliveryResultUI] 等待 DeliveryManager 初始化...");
            yield return null;
        }
        DeliveryManager.Instance.OnRecipeSuccess -= DeliveryManger_OnRecipeSuccess;
        DeliveryManager.Instance.OnRecipeFailed -= DeliveryManger_OnRecipeFailed;
        DeliveryManager.Instance.OnRecipeSuccess += DeliveryManger_OnRecipeSuccess;
        DeliveryManager.Instance.OnRecipeFailed += DeliveryManger_OnRecipeFailed;
        Debug.Log("[DeliveryResultUI] 成功订阅 DeliveryManager 事件");
    }

    private void OnDestroy()
    {
        if (DeliveryManager.Instance != null)
        {
            DeliveryManager.Instance.OnRecipeSuccess -= DeliveryManger_OnRecipeSuccess;
            DeliveryManager.Instance.OnRecipeFailed -= DeliveryManger_OnRecipeFailed;
            Debug.Log("[DeliveryResultUI] 已取消订阅 DeliveryManager 事件");
        }
    }

    private void DeliveryManger_OnRecipeFailed(object sender, System.EventArgs e)
    {
        gameObject.SetActive(true);
        animator.SetTrigger(POPUP);
        backgroundImage.color = failedColor;
        iconImage.sprite = failedSprite;
        messageText.text = "送达\n失败";
    }

    private void DeliveryManger_OnRecipeSuccess(object sender, System.EventArgs e)
    {
        gameObject.SetActive(true);
        animator.SetTrigger(POPUP);
        backgroundImage.color = successColor;
        iconImage.sprite = sucessSprite;
        messageText.text = "送达\n成功";
    }
}

