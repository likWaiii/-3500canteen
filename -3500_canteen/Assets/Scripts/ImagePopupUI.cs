using UnityEngine;
using UnityEngine.UI;

public class ImagePopupUI : MonoBehaviour
{
    [SerializeField] private GameObject popupPanel;
    [SerializeField] private Button triggerButton;
    [SerializeField] private Button backButton;

    private void Awake()
    {
        popupPanel.SetActive(false); // Ĭ�����ص���

        triggerButton.onClick.AddListener(() =>
        {
            popupPanel.SetActive(true); // ��ʾ����
        });

        backButton.onClick.AddListener(() =>
        {
            popupPanel.SetActive(false); // ���ص���
        });
    }
}
