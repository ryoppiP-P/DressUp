// ’Ê‰Ý•\Ž¦UI
using UnityEngine;
using TMPro;

public class CurrencyView : MonoBehaviour {
    [SerializeField] private CurrencyType type;
    [SerializeField] private TMP_Text label;

    void OnEnable() {
        if (SaveManager.Instance != null)
            SaveManager.Instance.OnCurrencyChanged += Refresh;
        Refresh();
    }

    void OnDisable() {
        if (SaveManager.Instance != null)
            SaveManager.Instance.OnCurrencyChanged -= Refresh;
    }

    void Refresh() {
        if (SaveManager.Instance == null || label == null) return;
        label.text = SaveManager.Instance.GetCurrency(type).ToString();
    }
}
