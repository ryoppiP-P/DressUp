using UnityEngine;

[CreateAssetMenu(menuName = "DressUp/Item")]
public class DressUpItem : ScriptableObject {
    public string itemName;
    public CategoryType category;
    public Sprite icon;           // グリッドに表示するアイコン
    public Sprite previewSprite;  // キャラのレイヤーに差し込むスプライト
}
