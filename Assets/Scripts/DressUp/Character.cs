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
    [SerializeField] private DressUpItem defaultEyes;  // 生まれたての目
    [SerializeField] private DressUpItem defaultMouth; // 生まれたての口

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
        if (SaveManager.Instance == null) return;
        var state = SaveManager.Instance.GetEquipState(characterId);

        // 生まれたての妖精は素体と顔だけの状態から始まる。
        // 既に着ているものがある場合は上書きしない。
        EnsureDefaultItem(state, CategoryType.Body, defaultBody);
        EnsureDefaultItem(state, CategoryType.FaceEyes, defaultEyes);
        EnsureDefaultItem(state, CategoryType.FaceMouth, defaultMouth);
    }

    private void EnsureDefaultItem(EquipState state, CategoryType category, DressUpItem item) {
        if (item == null) return;
        if (state.Count(category) == 0)
            state.Set(category, item);
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
            if (pair.Value == null) continue;
            AssignToLayers(pair.Key, pair.Value);
        }
        RefreshSprites();
    }

    // そのカテゴリのレイヤーへ、着ている順に前から詰めていく。
    // アクセサリはレイヤーが4枚あるので、4つまで同時に出せる。
    // (レイヤーが足りない分は表示されないだけで、着用データからは消えない)
    private void AssignToLayers(CategoryType category, List<DressUpItem> items) {
        var slots = layers.FindAll(l => l.category == category);

        for (int i = 0; i < slots.Count; i++)
            slots[i].item = i < items.Count ? items[i] : null;
    }

    // 今そのカテゴリで着ているものを、レイヤーの並び順どおりに集める
    private List<DressUpItem> CollectWorn(CategoryType category) {
        var result = new List<DressUpItem>();
        foreach (var layer in layers) {
            if (layer.category != category) continue;
            if (layer.item != null) result.Add(layer.item);
        }
        return result;
    }

    public void Equip(DressUpItem item) {
        if (item == null) return;

        // 1. 本体を着る(プレビューのみ。実際に保存されるのはApplyOutfit()が呼ばれた時)
        if (CategoryMap.IsMultiEquip(item.category)) {
            // アクセサリは重ね着けできる。上限(4個)を超えたら一番古いものが外れる。
            var worn = CollectWorn(item.category);
            if (!worn.Contains(item)) {
                worn.Add(item);

                int max = CategoryMap.GetMaxEquip(item.category);
                while (worn.Count > max) worn.RemoveAt(0);
            }
            AssignToLayers(item.category, worn);
        } else {
            SetItem(item.category, item);
        }

        // 2. 競合カテゴリを脱がす
        foreach (var conflict in GetConflicts(item.category))
            ClearCategory(conflict);

        RefreshSprites();
    }

    // 今その見た目になっているか(プレビュー基準。保存状態ではなく、今キャラが着ているもの)
    public bool IsWearing(DressUpItem item) {
        if (item == null) return false;

        return layers.Exists(l => l.category == item.category && l.item == item);
    }

    // そのカテゴリを丸ごと脱ぐ(プレビューのみ)。素体と顔は無いと成立しないので脱がせない
    public void Unequip(CategoryType category) {
        if (IsAlwaysOn(category)) return;

        ClearCategory(category);
        RefreshSprites();
    }

    // アイテム1つだけ脱ぐ(アクセサリを重ね着けしている時に、その1個だけ外す)
    public void Unequip(DressUpItem item) {
        if (item == null || IsAlwaysOn(item.category)) return;

        if (!CategoryMap.IsMultiEquip(item.category)) {
            Unequip(item.category);
            return;
        }

        var worn = CollectWorn(item.category);
        if (!worn.Remove(item)) return;

        AssignToLayers(item.category, worn);
        RefreshSprites();
    }

    // 着ていれば脱ぐ、着ていなければ着る(アイテムをタップした時用)
    public void Toggle(DressUpItem item) {
        if (item == null) return;

        if (IsWearing(item)) Unequip(item);
        else Equip(item);
    }

    // 素体と顔は外させない
    private static bool IsAlwaysOn(CategoryType category) {
        return category == CategoryType.Body
            || category == CategoryType.FaceEyes
            || category == CategoryType.FaceMouth;
    }

    // そのカテゴリのレイヤーを全部空にする
    private void ClearCategory(CategoryType category) {
        foreach (var layer in layers)
            if (layer.category == category) layer.item = null;
    }

    public void UnequipAll() {
        // プレビューのみ。保存はApplyOutfit()が呼ばれた時。Body・目・口はリセット対象外
        foreach (var layer in layers) {
            if (layer.category == CategoryType.Body) continue;
            if (layer.category == CategoryType.FaceEyes || layer.category == CategoryType.FaceMouth) continue;
            layer.item = null;
        }

        RefreshSprites();
    }

    // 現在プレビュー中の見た目をそのまま辞書として取得する(保存状態とは限らない)。
    // アクセサリは複数着けられるので、カテゴリごとにリストで返す。
    public Dictionary<CategoryType, List<DressUpItem>> GetVisualSnapshot() {
        var dict = new Dictionary<CategoryType, List<DressUpItem>>();
        foreach (var layer in layers) {
            if (layer.item == null) continue;

            if (!dict.TryGetValue(layer.category, out var list)) {
                list = new List<DressUpItem>();
                dict[layer.category] = list;
            }
            if (!list.Contains(layer.item)) list.Add(layer.item);
        }
        return dict;
    }

    // 「コーデを適用」ボタンから呼ばれる。プレビュー中の見た目を実際の保存状態に反映する
    public void ApplyOutfit() {
        if (SaveManager.Instance == null) return;

        var state = SaveManager.Instance.GetEquipState(characterId);
        state.ClearAll();
        foreach (var pair in GetVisualSnapshot())
            foreach (var item in pair.Value)
                state.Add(pair.Key, item);

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


    // そのカテゴリをこの1つだけにする(同カテゴリの余りレイヤーは空にする)
    private void SetItem(CategoryType category, DressUpItem item) {
        bool assigned = false;
        foreach (var layer in layers) {
            if (layer.category != category) continue;

            layer.item = assigned ? null : item;
            assigned = true;
        }
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
