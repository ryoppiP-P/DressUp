//==============================================================================
//  File   : ItemListSlot.cs
//  Brief  : アイテム一覧画面の1マス分の表示(見せるだけ・装備操作はしない)
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/8/2
//------------------------------------------------------------------------------
//==============================================================================
using UnityEngine;
using UnityEngine.UI;

public class ItemListSlot : MonoBehaviour {
    [Header("表示")]
    [SerializeField] private Image iconImage;

    /// <summary>アイテムのアイコンを表示する</summary>
    public void Setup(GameItem item) {
        if (item == null) return;
        if (iconImage == null) {
            Debug.LogWarning("[ItemListSlot] iconImage が未割り当てです。プレハブの Inspector を確認してください", this);
            return;
        }

        iconImage.sprite = item.icon;
        iconImage.enabled = item.icon != null;
    }
}
