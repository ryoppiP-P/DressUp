//==============================================================================
//  File   : RewardPopupTap.cs
//  Brief  : キャラの頭上に出る報酬ポップアップのタップ受け
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/8/21
//------------------------------------------------------------------------------
//  以前は OnMouseDown で拾っていたが、Input System 環境では飛んでこないことがあり
//  タッチでも反応しないので、EventSystem 経由(IPointerClickHandler)に変えた。
//  これが効くには、カメラ側に Physics2DRaycaster が付いている必要がある
//  (TownScene の Main Camera に付けてある)。
//==============================================================================
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider2D))]
public class RewardPopupTap : MonoBehaviour, IPointerClickHandler {
    [SerializeField] private CharacterReward owner;

    public void OnPointerClick(PointerEventData eventData) {
        if (owner != null) owner.OnPopupTapped();
    }
}
