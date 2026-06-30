//==============================================================================
//  File   : Character.cs
//  Brief  : キャラクター管理（着せ替え・アニメーション）
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/6/21
//------------------------------------------------------------------------------
//==============================================================================
using UnityEngine;
using System.Collections.Generic;

public class Character : MonoBehaviour {
    [System.Serializable]
    public class Layer {
        public CategoryType category;
        public SpriteRenderer renderer;
        [HideInInspector] public DressUpItem item;
    }

    [Header("Layers")]
    [SerializeField] private List<Layer> layers;

    [Header("Animation")]
    [SerializeField] private bool animate = true;       // false=フリーズ（着せ替え画面用）
    [SerializeField] private float frameRate = 8f;
    [SerializeField] private CharaState freezeState = CharaState.Idle; // 止めるときの状態

    [SerializeField] private ItemDatabase itemDatabase;

    [Header("Identity")]
    [SerializeField] private string characterId = "chara_01"; // セーブのキー（不変）
    [SerializeField] private string displayName = "みみ";      // 表示用（変更可）
    public string CharacterId => characterId;
    public string DisplayName => displayName;

    [Header("Default")]
    [SerializeField] private DressUpItem defaultBody; // 常に着る素体

    private CharaState _state = CharaState.Idle;
    private float _timer;
    private int _frame;

    void Start() {
        if (SaveManager.Instance != null && itemDatabase != null)
            DressUpSaveBridge.LoadIntoState(characterId, itemDatabase);

        EnsureBody();

        if (SaveManager.Instance != null)
            ApplyState(SaveManager.Instance.GetEquipState(characterId));
    }

    private void EnsureBody() {
        if (defaultBody == null || SaveManager.Instance == null) return;
        var state = SaveManager.Instance.GetEquipState(characterId);
        if (!state.equipped.ContainsKey(CategoryType.Body))
            state.Set(CategoryType.Body, defaultBody);
    }

    //--------------------------------------------------------------------------
    // 装備の反映
    //--------------------------------------------------------------------------

    // 保存状態を各レイヤーの item に割り当て、見た目も更新
    public void ApplyState(EquipState state) {
        // 一旦全レイヤーの item をクリア
        foreach (var layer in layers) layer.item = null;

        foreach (var pair in state.equipped) {
            var layer = layers.Find(l => l.category == pair.Key);
            if (layer != null) layer.item = pair.Value;
        }
        RefreshSprites();
    }

    public void Equip(DressUpItem item) {
        // 1. 本体を着る
        SetItem(item.category, item);

        // 2. 競合カテゴリを脱がす
        foreach (var conflict in GetConflicts(item.category))
            SetItem(conflict, null);

        // 3. 保存状態を更新
        if (SaveManager.Instance != null) {
            var state = SaveManager.Instance.GetEquipState(characterId);
            state.Set(item.category, item);
            foreach (var conflict in GetConflicts(item.category))
                state.Clear(conflict);
        }

        RefreshSprites();
        DressUpSaveBridge.SaveEquipped(characterId);
    }

    public void UnequipAll() {
        foreach (var layer in layers) {
            if (layer.category == CategoryType.Body) continue; // Body は脱がない
            layer.item = null;
        }

        if (SaveManager.Instance != null) {
            var state = SaveManager.Instance.GetEquipState(characterId);
            // Body 以外を全部消す
            var keys = new List<CategoryType>(state.equipped.Keys);
            foreach (var k in keys)
                if (k != CategoryType.Body) state.Clear(k);
        }
        RefreshSprites();
        DressUpSaveBridge.SaveEquipped(characterId);
    }

    //--------------------------------------------------------------------------
    // アニメーション
    //--------------------------------------------------------------------------

    public void SetState(CharaState newState) {
        if (_state == newState) return;
        _state = newState;
        _frame = 0;
        _timer = 0f;
        RefreshSprites();
    }

    void Update() {
        if (!animate) return; // フリーズ中は進めない

        _timer += Time.deltaTime;
        float interval = 1f / Mathf.Max(1f, frameRate);
        if (_timer < interval) return;
        _timer -= interval;
        _frame++;
        RefreshSprites();
    }

    //--------------------------------------------------------------------------
    // 内部処理
    //--------------------------------------------------------------------------

    // 現在の item / state / frame から全レイヤーのスプライトを決める
    private void RefreshSprites() {
        // フリーズ中は freezeState の0フレーム目で固定
        CharaState state = animate ? _state : freezeState;
        int frameIndex = animate ? _frame : 0;

        foreach (var layer in layers) {
            if (layer.item == null) {
                layer.renderer.sprite = null;
                continue;
            }
            var frames = layer.item.GetFrames(state);
            if (frames != null && frames.Length > 0)
                layer.renderer.sprite = frames[frameIndex % frames.Length];
            else
                layer.renderer.sprite = layer.item.previewSprite;
        }
    }

    private void SetItem(CategoryType category, DressUpItem item) {
        var layer = layers.Find(l => l.category == category);
        if (layer != null) layer.item = item;
    }

    private IEnumerable<CategoryType> GetConflicts(CategoryType category) {
        switch (category) {
            case CategoryType.Dress:
                return new[] { CategoryType.Tops, CategoryType.Bottoms };
            case CategoryType.Tops:
            case CategoryType.Bottoms:
                return new[] { CategoryType.Dress };
            default:
                return System.Array.Empty<CategoryType>();
        }
    }
}
