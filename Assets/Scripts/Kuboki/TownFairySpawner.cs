//==============================================================================
//  File   : TownFairySpawner.cs
//  Brief  : セーブデータの名簿にいる妖精を街に並べる
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/8/13
//------------------------------------------------------------------------------
//  既存の Character_0001 / Character_0001 (1) はシーンに直接置かれたままなので
//  触らない。ここで出すのは「妖精の畑で生まれて名前が付いた妖精」だけ。
//  同じ characterId が既にシーンにいる場合は二重に出さない。
//==============================================================================
using System;
using System.Collections.Generic;
using UnityEngine;

public class TownFairySpawner : MonoBehaviour {
    [Header("街に出す妖精のプレハブ(TownWander等が付いた街用のもの)")]
    [SerializeField] private GameObject fairyPrefab;

    // 生成は TownPositionSaver の Start から呼ばれる。
    // (配置を TownWander が経路を引く前に済ませたいので、順番を固定したい)
    public void SpawnAll() {
        if (fairyPrefab == null) {
            Debug.LogWarning("[TownFairySpawner] fairyPrefab が未設定です");
            return;
        }
        if (SaveManager.Instance == null) return;

        var existing = CollectExistingIds();

        int spawned = 0;
        foreach (var entry in FairySaveBridge.GetNamedFairies()) {
            if (entry == null || string.IsNullOrEmpty(entry.characterId)) continue;
            if (existing.Contains(entry.characterId)) continue; // 既にいる

            Spawn(entry);

            existing.Add(entry.characterId);
            spawned++;
        }

        if (spawned > 0) Debug.Log($"[TownFairySpawner] 妖精を {spawned} 体 街に追加しました");
    }

    private void Spawn(FairyRosterEntry entry) {
        var instance = Instantiate(fairyPrefab, Vector3.zero, Quaternion.identity);
        instance.name = entry.characterId;

        // セーブから見た目(装備)と名前を読み込ませる
        var view = instance.GetComponent<Character>();
        if (view != null) {
            view.SetCharacterId(entry.characterId);
            view.ReloadForId();

            // 前回の居場所があればそこから、初めてならランダムな場所へ
            TownCharacterPlacement.Place(view);
        }

        // 畑で決まった性格を流し込む
        ApplyPersonality(instance.GetComponent<CharacterManager>(), entry.personality);
    }

    // 生まれた時の性格を CharacterManager の特徴リストとして持たせる
    private void ApplyPersonality(CharacterManager manager, PersonalitySnapshot personality) {
        if (manager == null || personality == null) return;

        manager.dataList.Clear();
        foreach (PersonalityAxis axis in Enum.GetValues(typeof(PersonalityAxis))) {
            manager.dataList.Add(new CharaData {
                AttributeType = axis,
                Parameter = personality.Get(axis),
            });
        }
    }

    // 今シーンにいるキャラの characterId を集める
    private HashSet<string> CollectExistingIds() {
        var ids = new HashSet<string>();
        foreach (var character in FindObjectsByType<Character>(FindObjectsSortMode.None)) {
            if (character != null && !string.IsNullOrEmpty(character.CharacterId))
                ids.Add(character.CharacterId);
        }
        return ids;
    }
}
