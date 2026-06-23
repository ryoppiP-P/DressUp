using UnityEngine;
using UnityEngine.UI;

public class ItemButton : MonoBehaviour {
    [SerializeField] private Image iconImage;
    [SerializeField] private Image rarityImage;
    [SerializeField] private RarityIconTable rarityTable;
    [SerializeField] private Button button;

    public void Setup(DressUpItem item, Character character) {
        iconImage.sprite = item.icon;
        if (button != null) {
            button.onClick.RemoveAllListeners(); // “ñd“o˜^–h~
            button.onClick.AddListener(() => character.Equip(item));
        }

        // ƒŒƒAƒŠƒeƒB‰æ‘œ‚ğo‚·
        Sprite sprite = (rarityTable != null) ? rarityTable.GetIcon(item.rarity) : null;
        if (sprite != null) {
            rarityImage.sprite = sprite;
            rarityImage.enabled = true;
        }
        else {
            rarityImage.enabled = false; // ‘Î‰‰æ‘œ‚ª–³‚¯‚ê‚Î‰B‚·
        }
    }
}
