//==============================================================================
//  File   : CurrencyDisplay.cs
//  Brief  : 通貨(どんぐり/はちみつ)の所持数を表示するテキストを実際の値に追従させる
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/9/12
//------------------------------------------------------------------------------
//  SaveManager.OnCurrencyChanged を購読して増減のたびに再表示する。
//==============================================================================
using UnityEngine;
using TMPro;

public class CurrencyDisplay : MonoBehaviour {
    [SerializeField] private CurrencyType currencyType;
    [SerializeField] private TMP_Text amountText;

    void OnEnable() {
        Refresh();
        if (SaveManager.Instance != null) SaveManager.Instance.OnCurrencyChanged += Refresh;
    }

    void OnDisable() {
        if (SaveManager.Instance != null) SaveManager.Instance.OnCurrencyChanged -= Refresh;
    }

    private void Refresh() {
        if (amountText == null || SaveManager.Instance == null) return;
        amountText.text = SaveManager.Instance.GetCurrency(currencyType).ToString();
    }
}
