using UnityEngine;
using UnityEngine.UI;

public class ItemButton : MonoBehaviour {
    [SerializeField] private Image iconImage;
    [SerializeField] private Button button;

    public void Setup(DressUpItem item, Character character) {
        iconImage.sprite = item.icon;
        button.onClick.AddListener(() => character.Equip(item));
    }
}
