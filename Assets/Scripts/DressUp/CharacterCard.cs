//==============================================================================
//  File   : CharacterCard.cs
//  Brief  : キャラ選択画面のカード1枚分
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/8/18
//------------------------------------------------------------------------------
//  中身の絵は OutfitCapture で焼いたサムネ(今着ているコーデ)を貼るだけ。
//  誰を表しているかは CharacterSelectList が Show() で渡す。
//==============================================================================
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterCard : MonoBehaviour {
    [SerializeField] private Button button;
    [SerializeField] private Image thumbImage;   // 今着ているコーデ
    [SerializeField] private TMP_Text nameText;

    private string _characterId;
    private Action<string> _onClicked;

    void Awake() {
        if (button != null) button.onClick.AddListener(OnClicked);
    }

    void OnDestroy() {
        if (button != null) button.onClick.RemoveListener(OnClicked);
    }

    public void Show(string characterId, string displayName, Sprite thumbnail, Action<string> onClicked) {
        _characterId = characterId;
        _onClicked = onClicked;

        if (nameText != null) nameText.text = displayName;

        if (thumbImage != null) {
            thumbImage.sprite = thumbnail;
            thumbImage.enabled = thumbnail != null;
        }
    }

    private void OnClicked() {
        if (string.IsNullOrEmpty(_characterId)) return;
        if (_onClicked != null) _onClicked(_characterId);
    }
}
