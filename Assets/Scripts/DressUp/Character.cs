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
    [SerializeField] private CharaState freezeState = CharaState.Idle; // 止めるときの状態
    [SerializeField] private CharaState initialState = CharaState.Idle; // 開始時の状態
    [SerializeField] private int freezeFrame = 0;       // フリーズ時に表示するフレーム番号
    [SerializeField] private float frameRate = 24f;

    [SerializeField] private ItemDatabase itemDatabase;

    [Header("Identity")]
    [SerializeField] private string characterId = "charaID_0001"; // セーブのキー（不変）
    [SerializeField] private string displayName = "みみ";      // 表示用（変更可）
    public string CharacterId => characterId;
    public string DisplayName => displayName;

    [Header("Scale")]
    [SerializeField] private float referenceHeight = 500f; // 素体の高さpxに合わせる

    public void SetCharacterId(string id) {
        characterId = id;
    }

    [Header("Default")]
    [SerializeField] private DressUpItem defaultBody; // 常に着る素体

    private CharaState _state = CharaState.Idle;
    private float _timer;
    private int _frame;

    private bool _facingRight = false;

    public CharaState CurrentState => _state;

    void Start() {
        _state = initialState;

        if (SaveManager.Instance != null && itemDatabase != null)
            DressUpSaveBridge.LoadIntoState(characterId, itemDatabase);

        EnsureBody();

        if (SaveManager.Instance != null)
            ApplyState(SaveManager.Instance.GetEquipState(characterId));

        LoadDisplayName();
    }

    private void EnsureBody() {
        if (defaultBody == null || SaveManager.Instance == null) return;
        var state = SaveManager.Instance.GetEquipState(characterId);
        if (!state.equipped.ContainsKey(CategoryType.Body))
            state.Set(CategoryType.Body, defaultBody);
    }

    // 画像サイズを500x500に揃える（素体の高さpxに合わせる）
    private void FitScale(SpriteRenderer sr, Sprite sprite) {
        if (sr == null || sprite == null) return;
        float spriteHeight = sprite.rect.height;
        if (spriteHeight <= 0f) return;
        float scale = referenceHeight / spriteHeight;
        sr.transform.localScale = new Vector3(scale, scale, 1f);
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

    /// <summary>コマ送りの再生/停止（着せ替え画面やパネル表示中に止める用）</summary>
    public void SetAnimate(bool on) {
        if (animate == on) return;
        animate = on;
        RefreshSprites();
    }

    /// <summary>フリーズ中に表示する状態を差し替える（着せ替え画面のポーズ変更用）</summary>
    public void SetFreezeState(CharaState state) {
        if (freezeState == state) return;
        freezeState = state;
        if (!animate) RefreshSprites();
    }

    /// <summary>向きの反転。ルートのスケールを反転させる（レイヤー側は FitScale が使うため触らない）</summary>
    public void SetFacing(bool right) {
        if (_facingRight == right) return;
        _facingRight = right;
        var s = transform.localScale;
        s.x = Mathf.Abs(s.x) * (right ? -1f : 1f);
        transform.localScale = s;
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
        // フリーズ中は freezeState の固定フレームで止める
        CharaState state = animate ? _state : freezeState;
        int frameIndex = animate ? _frame : freezeFrame;

        foreach (var layer in layers) {
            if (layer.renderer == null) continue;

            if (layer.item == null) {
                layer.renderer.sprite = null;
                continue;
            }

            Sprite chosen;
            var frames = layer.item.GetFrames(state);
            if (frames != null && frames.Length > 0) {
                chosen = frames[frameIndex % frames.Length];
            } else {
                // 指定状態のフレームが無い → Idle にフォールバック
                var idle = layer.item.GetFrames(CharaState.Idle);
                chosen = (idle != null && idle.Length > 0) ? idle[0] : layer.item.previewSprite;
            }

            layer.renderer.sprite = chosen;
            FitScale(layer.renderer, chosen);   // ← セット直後にスケール補正
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

    public void ReloadForId() {
        if (SaveManager.Instance != null && itemDatabase != null)
            DressUpSaveBridge.LoadIntoState(characterId, itemDatabase);

        EnsureBody();

        if (SaveManager.Instance != null)
            ApplyState(SaveManager.Instance.GetEquipState(characterId));

        LoadDisplayName();
    }

    public void SetDisplayName(string name) {
        displayName = name;
        if (SaveManager.Instance != null)
            SaveManager.Instance.SetCharacterName(characterId, name);
    }

    // セーブに名前があれば displayName に反映（無ければInspectorの初期名のまま）
    private void LoadDisplayName() {
        if (SaveManager.Instance == null) return;
        string saved = SaveManager.Instance.GetCharacterName(characterId);
        if (!string.IsNullOrEmpty(saved))
            displayName = saved;
    }
}
