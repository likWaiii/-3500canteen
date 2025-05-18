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
    //新增进度条和顾客状态图
    [SerializeField] private Slider timeSlider;
    [SerializeField] private Image customerImage;
    private WaitingRecipe currentWaitingRecipe;

    private void Awake()
    {
        iconTemplate.gameObject.SetActive(false);
    }

    public void SetWaitingRecipe(WaitingRecipe waitingRecipe)
    {
        currentWaitingRecipe = waitingRecipe;
        RecipeSO recipeSO = currentWaitingRecipe.recipeSO;

        // 计算总价值
        int totalValue = 0;
        foreach (KitchenObjectOS kitchenObject in recipeSO.kitchenObjectsOSList)
        {
            totalValue += kitchenObject.value;
        }
        recipeNameText.text = "价值：" + totalValue;

        // 清除旧图标
        foreach (Transform child in iconContainer)
        {
            if (child == iconTemplate) continue;
            Destroy(child.gameObject);
        }

        // ✅ 按对象引用统计提交数量
        Dictionary<KitchenObjectOS, int> remainingSubmission = new Dictionary<KitchenObjectOS, int>(waitingRecipe.submittedDict, new KitchenObjectOSComparer());

        // ✅ 从左到右生成图标
        foreach (KitchenObjectOS kitchenObjectOS in recipeSO.kitchenObjectsOSList)
        {
            Transform iconTransform = Instantiate(iconTemplate, iconContainer);
            iconTransform.gameObject.SetActive(true);

            Image iconImage = iconTransform.GetComponent<Image>();
            iconImage.sprite = kitchenObjectOS.sprite;

            // ✅ 判断是否还有未消耗的提交数量
            if (remainingSubmission.ContainsKey(kitchenObjectOS) && remainingSubmission[kitchenObjectOS] > 0)
            {
                iconImage.color = Color.gray;
                remainingSubmission[kitchenObjectOS]--; // 消耗一次
            }
            else
            {
                iconImage.color = Color.white;
            }
        }

        // 初始表情
        customerImage.sprite = recipeSO.calmSprite;
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
        customerImage.sprite = null;
        currentWaitingRecipe = null;
    }

    private void Update()
    {
        if (currentWaitingRecipe == null) return;

        // ✅ 更新时间进度条
        float remaining = currentWaitingRecipe.remainingTime;
        float max = currentWaitingRecipe.recipeSO.maxTime;
        timeSlider.value = remaining / max;

        // ✅ 如果订单完成或失败，显示不同表情
        if (currentWaitingRecipe.isCompleted)
        {
            customerImage.sprite = currentWaitingRecipe.recipeSO.happySprite;
        }
        else if (remaining <= 0f)
        {
            customerImage.sprite = currentWaitingRecipe.recipeSO.angrySprite;
        }
    }


}
