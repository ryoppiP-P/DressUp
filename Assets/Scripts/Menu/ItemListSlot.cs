//==============================================================================
//  File   : ItemListSlot.cs
//  Brief  : アイテム一覧画面の1マス分の表示(見せるだけ・装備操作はしない)
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/8/2
//------------------------------------------------------------------------------
//==============================================================================
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemListSlot : MonoBehaviour {
    [Header("表示")]
    [SerializeField] private Image iconImage;

    [Header("個数表示(右下のバッジ。種・時短の実など個数管理のアイテムだけ出す)")]
    [SerializeField] private GameObject countBadge;
    [SerializeField] private TMP_Text countText;

    /// <summary>アイテムのアイコンを表示する</summary>
    public void Setup(GameItem item) {
        if (item == null) return;
        if (iconImage == null) {
            Debug.LogWarning("[ItemListSlot] iconImage が未割り当てです。プレハブの Inspector を確認してください", this);
            return;
        }

        iconImage.sprite = item.icon;
        iconImage.enabled = item.icon != null;

        // 服やアクセのように「持っているか」だけのアイテムはConsumableBridgeに記録が無いので
        // GetCountは0になり、バッジは自然に出ない(種・時短の実だけ個数が表示される)
        int count = ConsumableBridge.GetCount(item);
        if (countBadge != null) countBadge.SetActive(count > 0);
        if (countText != null && count > 0) countText.text = "x" + count;
    }
}
