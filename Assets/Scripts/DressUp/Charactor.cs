using UnityEngine;
using System.Collections.Generic;

public class Character : MonoBehaviour {
    [System.Serializable]
    public class Layer {
        public CategoryType category;
        public SpriteRenderer renderer;
    }

    [SerializeField] private List<Layer> layers;

    public void Equip(DressUpItem item) {
        // 1. まず本体を着る
        SetSprite(item.category, item.previewSprite);

        // 2. 競合するカテゴリを脱がす
        foreach (var conflict in GetConflicts(item.category))
            SetSprite(conflict, null);
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
}
