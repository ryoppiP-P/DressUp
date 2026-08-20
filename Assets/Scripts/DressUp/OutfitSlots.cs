// 保存されたコーデのリスト
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class OutfitSlots : MonoBehaviour {
    [SerializeField] private Button saveButton;
    [SerializeField] private List<OutfitSlot> slots;
    [SerializeField] private OutfitCapture capture;
    [SerializeField] private ItemDatabase itemDatabase;

    private Character character => DressUpTarget.Instance != null
        ? DressUpTarget.Instance.Current : null;

    void OnEnable() {
        if (DressUpTarget.Instance != null)
            DressUpTarget.Instance.OnTargetChanged += OnTargetChanged;
    }

    void OnDisable() {
        if (DressUpTarget.Instance != null)
            DressUpTarget.Instance.OnTargetChanged -= OnTargetChanged;
    }

    void Start() {
        saveButton.onClick.AddListener(SaveCurrent);
        if (character != null) RestoreFromSave();
    }

    void OnTargetChanged(Character _) {
        RestoreFromSave();
    }

    // 起動時：保存済みコーデをスロットへ反映
    void RestoreFromSave() {
        var outfits = DressUpSaveBridge.LoadSavedOutfits(itemDatabase);
        for (int i = 0; i < slots.Count; i++) {
            if (i < outfits.Count) {
                int dataIndex = i; // このslotがsavedOutfitsの何番目か
                var thumb = BuildThumbnail(outfits[i]); // 再撮影でサムネ生成
                slots[i].Show(outfits[i], thumb, Load, () => DeleteOutfit(dataIndex));
            }
            else {
                slots[i].Clear();
            }
        }
    }

    void SaveCurrent() {
        if (SaveManager.Instance == null || character == null) return;
        int index = slots.FindIndex(s => s.IsEmpty);
        if (index < 0) return;

        var outfit = new SavedOutfit();
        outfit.Capture(character.GetVisualSnapshot());

        DressUpSaveBridge.AddSavedOutfit(outfit);
        RestoreFromSave();
    }

    void Load(SavedOutfit outfit) {
        if (character == null) return;
        character.UnequipAll();
        foreach (var item in outfit.AllItems())
            character.Equip(item);
    }

    // 保存コーデを一時的にキャラへ着せ替えて撮影し、元に戻す
    Sprite BuildThumbnail(SavedOutfit outfit) {
        if (capture == null || SaveManager.Instance == null || character == null)
            return null;

        // 現在の装備(プレビュー中の見た目)を退避
        var backup = new SavedOutfit();
        backup.Capture(character.GetVisualSnapshot());

        // コーデを着せて撮影
        character.UnequipAll();
        foreach (var item in outfit.AllItems())
            character.Equip(item);
        Sprite thumb = capture.Capture();

        // 元に戻す
        character.UnequipAll();
        foreach (var item in backup.AllItems())
            character.Equip(item);

        return thumb;
    }

    void DeleteOutfit(int dataIndex) {
        DressUpSaveBridge.RemoveSavedOutfit(dataIndex);
        RestoreFromSave(); // 削除後に詰め直して再表示
    }
}
