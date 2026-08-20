using UnityEngine;
using UnityEngine.UI;

public class ItemButton : MonoBehaviour {
    [SerializeField] private Image iconImage;
    [SerializeField] private Image rarityImage;
    [SerializeField] private RarityIconTable rarityTable;
    [SerializeField] private Button button;
    [SerializeField] private Image selectImage;  // 着用中に出す枠(Resources/DressUp/ItemSelect)

    // onChanged: 着脱した後に一覧を並べ直してもらうための呼び出し
    public void Setup(DressUpItem item, Character character, System.Action onChanged = null) {
        if (item == null) {
            Debug.LogWarning("[ItemButton] item が null（items リストに空要素があるかも）", this);
            return;
        }
        if (iconImage == null) {
            Debug.LogError("[ItemButton] iconImage が未割り当て。プレハブの Inspector を確認", this);
            return;
        }

        iconImage.sprite = item.icon;

        // 今そのアイテムを着ていたら枠を出す
        if (selectImage != null)
            selectImage.enabled = character != null && character.IsWearing(item);
        if (button != null) {
            button.onClick.RemoveAllListeners(); // 二重登録防止
            button.onClick.AddListener(() => {
                // 着ているものをもう一度押したら脱ぐ
                character.Toggle(item);
                if (onChanged != null) onChanged();
            });
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
