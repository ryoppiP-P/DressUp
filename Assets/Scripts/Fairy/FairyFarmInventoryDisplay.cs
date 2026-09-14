//==============================================================================
//  File   : FairyFarmInventoryDisplay.cs
//  Brief  : 妖精の畑で、種と時短アイテムの所持数を常時表示する
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/9/12
//------------------------------------------------------------------------------
//  ConsumableBridge.GetCount を定期的にポーリングして表示するだけ(既存の
//  ショップ/使用処理には一切手を加えない)。
//==============================================================================
using UnityEngine;
using TMPro;

public class FairyFarmInventoryDisplay : MonoBehaviour {
    [SerializeField] private string seedItemId = "FairySeed_01";
    [SerializeField] private string reduceItemId = "TimeReduceItem_01";
    [SerializeField] private TMP_Text seedCountText;
    [SerializeField] private TMP_Text reduceCountText;
    [SerializeField] private float refreshInterval = 0.5f;

    private float _timer;

    void OnEnable() {
        _timer = 0f;
        Refresh();
    }

    void Update() {
        _timer += Time.deltaTime;
        if (_timer < refreshInterval) return;
        _timer = 0f;
        Refresh();
    }

    private void Refresh() {
        if (seedCountText != null) seedCountText.text = ConsumableBridge.GetCount(seedItemId).ToString();
        if (reduceCountText != null) reduceCountText.text = ConsumableBridge.GetCount(reduceItemId).ToString();
    }
}
