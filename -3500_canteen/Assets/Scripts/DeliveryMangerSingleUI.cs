using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeliveryManagerSingleUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI recipeNameText;
    [SerializeField] private Transform iconContainer;
    [SerializeField] private Transform iconTemplate;
    [SerializeField] private Slider timeSlider;

    [Header("顾客表情图像")]
    [SerializeField] private Image customerImage; // calm 表情
    [SerializeField] private Image exitEffectImage; // happy/angry 表情

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

    public void SetWaitingRecipe(WaitingRecipe waitingRecipe)
    {
        if (currentWaitingRecipe == waitingRecipe) return;
        currentWaitingRecipe = waitingRecipe;
        RecipeSO recipeSO = currentWaitingRecipe.recipeSO;

        int totalValue = 0;
        foreach (KitchenObjectOS kitchenObject in recipeSO.kitchenObjectsOSList)
        {
            totalValue += kitchenObject.value;
        }
        recipeNameText.text = "价值：" + totalValue;

        foreach (Transform child in iconContainer)
        {
            if (child == iconTemplate) continue;
            Destroy(child.gameObject);
        }

        Dictionary<KitchenObjectOS, int> remainingSubmission = new Dictionary<KitchenObjectOS, int>(
            waitingRecipe.submittedDict, new KitchenObjectOSComparer()
        );

        foreach (KitchenObjectOS kitchenObjectOS in recipeSO.kitchenObjectsOSList)
        {
            Transform iconTransform = Instantiate(iconTemplate, iconContainer);
            iconTransform.gameObject.SetActive(true);
            Image iconImage = iconTransform.GetComponent<Image>();
            iconImage.sprite = kitchenObjectOS.sprite;

            if (remainingSubmission.ContainsKey(kitchenObjectOS) && remainingSubmission[kitchenObjectOS] > 0)
            {
                iconImage.color = Color.gray;
                remainingSubmission[kitchenObjectOS]--;
            }
            else
            {
                iconImage.color = Color.white;
            }
        }

        // 播放 calm 滑入动画
        StartCoroutine(PlayEnterAnimation(recipeSO.calmSprite));
    }

    private IEnumerator PlayEnterAnimation(Sprite calmSprite)
    {
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
            if (child == iconTemplate) continue;
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
        if (currentWaitingRecipe == null || isAnimating) return;

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