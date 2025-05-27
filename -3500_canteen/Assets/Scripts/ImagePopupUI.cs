using UnityEngine;
using UnityEngine.UI;

public class ImagePopupUI : MonoBehaviour
{
    [SerializeField] private GameObject popupPanel;
    [SerializeField] private Button triggerButton;
    [SerializeField] private Button backButton;

    private void Awake()
    {
        popupPanel.SetActive(false); // Ä¬ÈÏÒþ²Øµ¯´°

        triggerButton.onClick.AddListener(() =>
        {
            popupPanel.SetActive(true); // ÏÔÊ¾µ¯´°
        });

        backButton.onClick.AddListener(() =>
        {
            popupPanel.SetActive(false); // Òþ²Øµ¯´°
        });
    }
}
