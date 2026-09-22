using UnityEngine;
using UnityEngine.UI;

public class FilterPanel : MonoBehaviour {
    [SerializeField] private GameObject filterPanel;
    [SerializeField] private CanvasGroup dressupCanvasGroup;

    [SerializeField] private Button filterButton; // フィルターボタン
    [SerializeField] private Button closeButton;  // 閉じるボタン

    void Start() {
        filterButton.onClick.AddListener(Open);
        closeButton.onClick.AddListener(Close);
        filterPanel.SetActive(false); // 起動時は閉じておく
    }

    void Open() {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySE(SEType.Tap);
        filterPanel.SetActive(true);
        dressupCanvasGroup.interactable = false;
        dressupCanvasGroup.blocksRaycasts = false;
    }

    void Close() {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySE(SEType.Click);
        filterPanel.SetActive(false);
        dressupCanvasGroup.interactable = true;
        dressupCanvasGroup.blocksRaycasts = true;
    }
}
