using UnityEngine;
using UnityEngine.UI;

public class ItemButton : MonoBehaviour {
    [SerializeField] private Image iconImage;
    [SerializeField] private Image rarityImage;
    [SerializeField] private RarityIconTable rarityTable;
    [SerializeField] private Button button;

    public void Setup(DressUpItem item, Character character) {
        if (item == null) {
            Debug.LogWarning("[ItemButton] item が null（items リストに空要素があるかも）", this);
            return;
        }
        if (iconImage == null) {
            Debug.LogError("[ItemButton] iconImage が未割り当て。プレハブの Inspector を確認", this);
            return;
        }

        iconImage.sprite = item.icon;
        if (button != null) {
            button.onClick.RemoveAllListeners(); // 二重登録防止
            button.onClick.AddListener(() => character.Equip(item));
        }

        // レアリティ画像を出す
        Sprite sprite = (rarityTable != null) ? rarityTable.GetIcon(item.rarity) : null;
        if (rarityImage != null) {
            if (sprite != null) {
                rarityImage.sprite = sprite;
                rarityImage.enabled = true;
            } else {
                rarityImage.enabled = false;
            }
        }
    }
}
