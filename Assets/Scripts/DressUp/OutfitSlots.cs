using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class OutfitSlots : MonoBehaviour {
    [SerializeField] private Character character;
    [SerializeField] private Button saveButton;
    [SerializeField] private List<OutfitSlot> slots;
    [SerializeField] private OutfitCapture capture;
    [SerializeField] private ItemDatabase itemDatabase;

    void Start() {
        saveButton.onClick.AddListener(SaveCurrent);
        RestoreFromSave();
    }

    // 起動時：保存済みコーデをスロットへ復元
    void RestoreFromSave() {
        var outfits = DressUpSaveBridge.LoadSavedOutfits(itemDatabase);
        for (int i = 0; i < slots.Count; i++) {
            if (i < outfits.Count) {
                int dataIndex = i; // この slot が savedOutfits の何番目か
                var thumb = BuildThumbnail(outfits[i]); // 再撮影でサムネ生成
                slots[i].Show(outfits[i], thumb, Load, () => DeleteOutfit(dataIndex));
            }
            else {
                slots[i].Clear();
            }
        }
    }

    void SaveCurrent() {
        if (SaveManager.Instance == null) return;
        int index = slots.FindIndex(s => s.IsEmpty);
        if (index < 0) return;

        var outfit = new SavedOutfit();
        outfit.Capture(SaveManager.Instance.GetEquipState(character.CharacterId));

        DressUpSaveBridge.AddSavedOutfit(outfit);
        RestoreFromSave();
    }

    void Load(SavedOutfit outfit) {
        character.UnequipAll();
        foreach (var pair in outfit.items)
            character.Equip(pair.Value);
    }

    // 保存コーデから一時的にキャラを着せ替えて撮影し、元に戻す
    Sprite BuildThumbnail(SavedOutfit outfit) {
        if (capture == null || SaveManager.Instance == null) return null;

        // 現在装備を退避
        var backup = new SavedOutfit();
        backup.Capture(SaveManager.Instance.GetEquipState(character.CharacterId));

        // コーデを着せて撮影
        character.UnequipAll();
        foreach (var pair in outfit.items)
            character.Equip(pair.Value);
        Sprite thumb = capture.Capture();

        // 元に戻す
        character.UnequipAll();
        foreach (var pair in backup.items)
            character.Equip(pair.Value);

        return thumb;
    }

    void DeleteOutfit(int dataIndex) {
        DressUpSaveBridge.RemoveSavedOutfit(dataIndex);
        RestoreFromSave(); // 削除後に詰め直して再表示
    }
}
