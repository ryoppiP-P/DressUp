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

    // キャラの反転の影響をリワードポップアップが受けないように。
    void LateUpdate() {
        var parent = transform.parent;
        if (parent == null) return;

        float sign = parent.lossyScale.x < 0 ? -1f : 1f;
        Vector3 s = transform.localScale;
        s.x = Mathf.Abs(s.x) * sign;
        transform.localScale = s;
    }
}
