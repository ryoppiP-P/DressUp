//==============================================================================
//  File   : OutfitSaveButton.cs
//  Brief  : 「コーデを保存」ボタン。パネルの開閉に関係なく押せるようにする
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/8/21
//------------------------------------------------------------------------------
//  保存の中身は OutfitSlots が持っているが、あちらはコーデ保存パネルの上に乗って
//  いるため、パネルが閉じている間は Start が走らずリスナーを登録できない。
//  (そのせいで「一度もパネルを開かずに保存を押すと何も起きない」状態だった)
//
//  このスクリプトはパネルの外＝常にアクティブなボタン側に付けるので、
//  画面を開いた直後から押せる。
//==============================================================================
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class OutfitSaveButton : MonoBehaviour {
    [Header("保存の実処理を持っているスクリプト(パネル側)")]
    [SerializeField] private OutfitSlots outfitSlots;

    private Button _button;

    void Awake() {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(Save);
    }

    void OnDestroy() {
        if (_button != null) _button.onClick.RemoveListener(Save);
    }

    private void Save() {
        if (outfitSlots == null) {
            Debug.LogWarning("[OutfitSaveButton] outfitSlots が未設定です", this);
            return;
        }
        outfitSlots.SaveCurrent();
    }
}
