using UnityEngine;
using System.Collections.Generic;

public class Character : MonoBehaviour {
    [System.Serializable]
    public class Layer {
        public CategoryType category;
        public SpriteRenderer renderer;
    }

    [SerializeField] private List<Layer> layers;

    void Start() {
        // 保存済みの装備があれば復元する
        if (PlayerEquipManager.Instance != null)
            ApplyState(PlayerEquipManager.Instance.State);
    }

    // 保存状態を見た目に反映する
    public void ApplyState(EquipState state) {
        foreach (var pair in state.equipped)
            SetSprite(pair.Key, pair.Value.previewSprite);
    }

    public void Equip(DressUpItem item) {
        // 1. まず本体を着る
        SetSprite(item.category, item.previewSprite);

        // 2. 競合するカテゴリを脱がす
        foreach (var conflict in GetConflicts(item.category))
            SetSprite(conflict, null);

        // 3. 装備状態を保存（マネージャがあれば）
        if (PlayerEquipManager.Instance != null) {
            var state = PlayerEquipManager.Instance.State;
            state.Set(item.category, item);
            foreach (var conflict in GetConflicts(item.category))
                state.Clear(conflict);
        }
    }

    private void SetSprite(CategoryType category, Sprite sprite) {
        var layer = layers.Find(l => l.category == category);
        if (layer != null)
            layer.renderer.sprite = sprite;
    }

    // 着替えルール：このカテゴリを着たら、どのカテゴリを脱ぐか
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

    // リセット用全脱ぎ
    public void UnequipAll() {
        // 全レイヤーのスプライトを消す
        foreach (var layer in layers)
            layer.renderer.sprite = null;

        // 保存状態も空にする
        if (PlayerEquipManager.Instance != null)
            PlayerEquipManager.Instance.State.ClearAll();
    }
}
