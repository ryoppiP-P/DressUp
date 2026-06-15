using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class OutfitSlots : MonoBehaviour {
    [SerializeField] private Character character;
    [SerializeField] private Button saveButton;
    [SerializeField] private List<OutfitSlot> slots; // 事前配置したスロット
    [SerializeField] private OutfitCapture capture;

    void Start() {
        saveButton.onClick.AddListener(SaveCurrent);
    }

    void SaveCurrent() {
        if (PlayerEquipManager.Instance == null) return;

        var slot = slots.Find(s => s.IsEmpty);
        if (slot == null) { Debug.Log("空きスロットがない"); return; }

        var outfit = new SavedOutfit();
        outfit.Capture(PlayerEquipManager.Instance.State);

        Sprite thumb = capture.Capture(); // ← OutfitCapture を参照
        slot.Show(outfit, thumb, Load);
    }

    void Load(SavedOutfit outfit) {
        character.UnequipAll();
        foreach (var pair in outfit.items)
            character.Equip(pair.Value);
    }
}
