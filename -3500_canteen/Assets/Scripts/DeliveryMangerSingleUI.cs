// using System.Collections;
// using System.Collections.Generic;
// using TMPro;
// using UnityEngine;
// using UnityEngine.UI;

// public class DeliveryManagerSingleUI : MonoBehaviour
// {
//     [SerializeField]
//     private TextMeshProUGUI recipeNameText;

//     [SerializeField]
//     private Transform iconContainer;

//     [SerializeField]
//     private Transform iconTemplate;

//     [SerializeField]
//     private Slider timeSlider;

//     [Header("顾客表情图像")]
//     [SerializeField]
//     private Image customerImage; // calm 表情

//     [SerializeField]
//     private Image exitEffectImage; // happy/angry 表情

//     private WaitingRecipe currentWaitingRecipe;
//     private CanvasGroup customerCanvasGroup;
//     private CanvasGroup exitEffectCanvasGroup;
//     private Vector2 originalPosition;
//     private Vector2 enterOffset = new Vector2(0, -80);
//     private Vector2 exitOffset = new Vector2(0, -100);
//     private bool isAnimating = false;

//     private void Awake()
//     {
//         iconTemplate.gameObject.SetActive(false);
//         customerCanvasGroup = customerImage.GetComponent<CanvasGroup>();
//         exitEffectCanvasGroup = exitEffectImage.GetComponent<CanvasGroup>();
//         originalPosition = customerImage.rectTransform.anchoredPosition;

//         customerImage.enabled = false;
//         exitEffectImage.enabled = false;
//     }

//     public void SetWaitingRecipe(WaitingRecipe waitingRecipe)
//     {
//         if (currentWaitingRecipe == waitingRecipe)
//             return;
//         currentWaitingRecipe = waitingRecipe;
//         RecipeSO recipeSO = currentWaitingRecipe.recipeSO;

//         int totalValue = 0;
//         foreach (KitchenObjectOS kitchenObject in recipeSO.kitchenObjectsOSList)
//         {
//             totalValue += kitchenObject.value;
//         }
//         recipeNameText.text = "价值：" + totalValue;

//         foreach (Transform child in iconContainer)
//         {
//             if (child == iconTemplate)
//                 continue;
//             Destroy(child.gameObject);
//         }

//         Dictionary<KitchenObjectOS, int> remainingSubmission = new Dictionary<KitchenObjectOS, int>(
//             waitingRecipe.submittedDict,
//             new KitchenObjectOSComparer()
//         );

//         foreach (KitchenObjectOS kitchenObjectOS in recipeSO.kitchenObjectsOSList)
//         {
//             Transform iconTransform = Instantiate(iconTemplate, iconContainer);
//             iconTransform.gameObject.SetActive(true);
//             Image iconImage = iconTransform.GetComponent<Image>();
//             iconImage.sprite = kitchenObjectOS.sprite;

//             if (
//                 remainingSubmission.ContainsKey(kitchenObjectOS)
//                 && remainingSubmission[kitchenObjectOS] > 0
//             )
//             {
//                 iconImage.color = Color.gray;
//                 remainingSubmission[kitchenObjectOS]--;
//             }
//             else
//             {
//                 iconImage.color = Color.white;
//             }
//         }

//         // 播放 calm 滑入动画
//         StartCoroutine(PlayEnterAnimation(recipeSO.calmSprite));
//     }

//     private IEnumerator PlayEnterAnimation(Sprite calmSprite)
//     {
//         Debug.Log("EnterAnimation 开始");
//         customerImage.sprite = calmSprite;
//         customerImage.enabled = true;
//         customerCanvasGroup.alpha = 0f;

//         Vector2 startPos = originalPosition + enterOffset;
//         Vector2 endPos = originalPosition;
//         float duration = 0.5f;
//         float elapsed = 0f;

//         customerImage.rectTransform.anchoredPosition = startPos;

//         while (elapsed < duration)
//         {
//             elapsed += Time.deltaTime;
//             float t = elapsed / duration;
//             customerImage.rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
//             customerCanvasGroup.alpha = t;
//             yield return null;
//         }

//         customerCanvasGroup.alpha = 1f;
//         customerImage.rectTransform.anchoredPosition = endPos;
//     }

//     public void ClearUI()
//     {
//         recipeNameText.text = "";
//         foreach (Transform child in iconContainer)
//         {
//             if (child == iconTemplate)
//                 continue;
//             Destroy(child.gameObject);
//         }

//         timeSlider.value = 0f;
//         customerImage.enabled = false;
//         customerImage.sprite = null;
//         exitEffectImage.enabled = false;
//         currentWaitingRecipe = null;

//         customerCanvasGroup.alpha = 1f;
//         customerImage.rectTransform.anchoredPosition = originalPosition;
//         exitEffectCanvasGroup.alpha = 1f;
//         isAnimating = false;
//     }

//     private void Update()
//     {
//         if (currentWaitingRecipe == null || isAnimating)
//             return;

//         float remaining = currentWaitingRecipe.remainingTime;
//         float max = currentWaitingRecipe.recipeSO.maxTime;
//         timeSlider.value = remaining / max;

//         if (currentWaitingRecipe.isCompleted)
//         {
//             StartCoroutine(PlayExitAnimation(currentWaitingRecipe.recipeSO.happySprite));
//             isAnimating = true;
//         }
//         else if (remaining <= 0f)
//         {
//             StartCoroutine(PlayExitAnimation(currentWaitingRecipe.recipeSO.angrySprite));
//             isAnimating = true;
//         }
//     }

//     private IEnumerator PlayExitAnimation(Sprite exitSprite)
//     {
//         customerImage.enabled = false;

//         exitEffectImage.sprite = exitSprite;
//         exitEffectImage.enabled = true;
//         exitEffectCanvasGroup.alpha = 1f;

//         Vector2 startPos = originalPosition;
//         Vector2 endPos = originalPosition + exitOffset;
//         exitEffectImage.rectTransform.anchoredPosition = startPos;

//         float duration = 1f;
//         float elapsed = 0f;

//         while (elapsed < duration)
//         {
//             elapsed += Time.deltaTime;
//             float t = elapsed / duration;
//             exitEffectImage.rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
//             exitEffectCanvasGroup.alpha = 1f - t;
//             yield return null;
//         }

//         exitEffectImage.enabled = false;
//         ClearUI();
//     }
// }
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeliveryManagerSingleUI : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI recipeNameText;

    [SerializeField]
    private Transform iconContainer;

    [SerializeField]
    private Transform iconTemplate;

    [SerializeField]
    private Slider timeSlider;

    [Header("顾客表情图像")]
    [SerializeField]
    private Image customerImage; // calm 表情

    [SerializeField]
    private Image exitEffectImage; // happy/angry 表情

    private WaitingRecipe currentWaitingRecipe;
    private CanvasGroup customerCanvasGroup;
    private CanvasGroup exitEffectCanvasGroup;
    private Vector2 originalPosition;
    private Vector2 enterOffset = new Vector2(0, -80);
    private Vector2 exitOffset = new Vector2(0, -100);
    private bool isAnimating = false;

    private void Awake()
    {
        iconTemplate.gameObject.SetActive(false);
        customerCanvasGroup = customerImage.GetComponent<CanvasGroup>();
        exitEffectCanvasGroup = exitEffectImage.GetComponent<CanvasGroup>();
        originalPosition = customerImage.rectTransform.anchoredPosition;

        customerImage.enabled = false;
        exitEffectImage.enabled = false;
    }

    /// <summary>
    /// 当新订单进来时调用：构建 UI 并播放入场动画
    /// </summary>
    public void SetWaitingRecipe(WaitingRecipe waitingRecipe)
    {
        // 判断是不是新订单
        bool isNew = currentWaitingRecipe != waitingRecipe;

        // 更新当前订单
        currentWaitingRecipe = waitingRecipe;

        // 重建图标并打灰度
        BuildSubmissionIcons();

        // 如果是新订单，才播放入场动画
        if (isNew)
        {
            StartCoroutine(PlayEnterAnimation(waitingRecipe.recipeSO.calmSprite));
        }
    }

    /// <summary>
    /// 只负责重建所有图标并根据已提交数量打灰，不触发动画
    /// </summary>
    public void RefreshSubmissionIcons()
    {
        if (currentWaitingRecipe == null)
            return;
        BuildSubmissionIcons();
    }

    private void BuildSubmissionIcons()
    {
        // 更新名称+价值
        RecipeSO recipeSO = currentWaitingRecipe.recipeSO;
        int totalValue = 0;
        foreach (var k in recipeSO.kitchenObjectsOSList)
            totalValue += k.value;
        recipeNameText.text = "价值：" + totalValue;

        // 清掉旧图标
        foreach (Transform child in iconContainer)
            if (child != iconTemplate)
                Destroy(child.gameObject);

        // 拷贝一份提交字典，用于灰度判断
        var remaining = new Dictionary<KitchenObjectOS, int>(
            currentWaitingRecipe.submittedDict,
            new KitchenObjectOSComparer()
        );

        // 重新生成图标，并根据 remaining 打灰
        foreach (var k in recipeSO.kitchenObjectsOSList)
        {
            var icon = Instantiate(iconTemplate, iconContainer);
            icon.gameObject.SetActive(true);
            var img = icon.GetComponent<Image>();
            img.sprite = k.sprite;

            if (remaining.TryGetValue(k, out int cnt) && cnt > 0)
            {
                img.color = Color.gray;
                remaining[k]--;
            }
            else
            {
                img.color = Color.white;
            }
        }
    }

    private IEnumerator PlayEnterAnimation(Sprite calmSprite)
    {
        Debug.Log("EnterAnimation 开始");
        customerImage.sprite = calmSprite;
        customerImage.enabled = true;
        customerCanvasGroup.alpha = 0f;

        Vector2 startPos = originalPosition + enterOffset;
        Vector2 endPos = originalPosition;
        float duration = 0.5f;
        float elapsed = 0f;

        customerImage.rectTransform.anchoredPosition = startPos;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            customerImage.rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            customerCanvasGroup.alpha = t;
            yield return null;
        }

        customerCanvasGroup.alpha = 1f;
        customerImage.rectTransform.anchoredPosition = endPos;
    }

    public void ClearUI()
    {
        recipeNameText.text = "";
        foreach (Transform child in iconContainer)
        {
            if (child == iconTemplate)
                continue;
            Destroy(child.gameObject);
        }

        timeSlider.value = 0f;
        customerImage.enabled = false;
        customerImage.sprite = null;
        exitEffectImage.enabled = false;
        currentWaitingRecipe = null;

        customerCanvasGroup.alpha = 1f;
        customerImage.rectTransform.anchoredPosition = originalPosition;
        exitEffectCanvasGroup.alpha = 1f;
        isAnimating = false;
    }

    private void Update()
    {
        if (currentWaitingRecipe == null || isAnimating)
            return;

        float remaining = currentWaitingRecipe.remainingTime;
        float max = currentWaitingRecipe.recipeSO.maxTime;
        timeSlider.value = remaining / max;

        if (currentWaitingRecipe.isCompleted)
        {
            StartCoroutine(PlayExitAnimation(currentWaitingRecipe.recipeSO.happySprite));
            isAnimating = true;
        }
        else if (remaining <= 0f)
        {
            StartCoroutine(PlayExitAnimation(currentWaitingRecipe.recipeSO.angrySprite));
            isAnimating = true;
        }
    }

    private IEnumerator PlayExitAnimation(Sprite exitSprite)
    {
        customerImage.enabled = false;

        exitEffectImage.sprite = exitSprite;
        exitEffectImage.enabled = true;
        exitEffectCanvasGroup.alpha = 1f;

        Vector2 startPos = originalPosition;
        Vector2 endPos = originalPosition + exitOffset;
        exitEffectImage.rectTransform.anchoredPosition = startPos;

        float duration = 1f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            exitEffectImage.rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            exitEffectCanvasGroup.alpha = 1f - t;
            yield return null;
        }

        exitEffectImage.enabled = false;
        ClearUI();
    }
}
