//==============================================================================
//  File   : TalkPrompt.cs
//  Brief  : すれ違った2人の間に出す「！」ポップアップ(World Space Canvas想定)
//
//  Name   : Ryoto Kikuchi
//
//  TalkManagerがシーンに1つだけ持ち、すれ違いのたびに位置だけ動かして使い回す。
//  タップ検知そのものはInspectorで割り当てたBoxボタン(TapButton)に任せ、
//  ここでは表示/非表示だけを担当する。
//==============================================================================
using UnityEngine;
using UnityEngine.UI;

public class TalkPrompt : MonoBehaviour {
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Button tapButton;

    /// <summary>タップ検知用のボタン(TalkManagerがonClickを購読する)</summary>
    public Button TapButton => tapButton;

    void Awake() {
        Hide();
    }

    public void Show() {
        if (canvasGroup == null) return;
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
    }

    public void Hide() {
        if (canvasGroup == null) return;
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }
}
