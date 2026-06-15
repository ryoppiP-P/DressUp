using UnityEngine;
using UnityEngine.UI;

public class ItemButton : MonoBehaviour {
    [SerializeField] private Image iconImage;
    [SerializeField] private Image rarityImage;
    [SerializeField] private RarityIconTable rarityTable;
    [SerializeField] private Button button;

    public void Setup(DressUpItem item, Character character) {
        iconImage.sprite = item.icon;
        button.onClick.AddListener(() => character.Equip(item));

        // レアリティ画像を出す
        var sprite = rarityTable.GetIcon(item.rarity);
        if (sprite != null) {
            rarityImage.sprite = sprite;
            rarityImage.enabled = true;
        }
        else {
            rarityImage.enabled = false; // 対応画像が無ければ隠す
        }
    }
}
