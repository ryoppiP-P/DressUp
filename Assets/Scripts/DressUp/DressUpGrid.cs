using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class DressupGrid : MonoBehaviour {
    [SerializeField] private Character character;
    [SerializeField] private Transform gridContent;     // Grid Layout Group の Content
    [SerializeField] private ItemButton itemButtonPrefab;
    [SerializeField] private List<DressUpItem> items;   // 表示したいアイテムを並べる

    void Start() {
        foreach (var item in items) {
            var btn = Instantiate(itemButtonPrefab, gridContent);
            btn.Setup(item, character);
        }
    }
}
