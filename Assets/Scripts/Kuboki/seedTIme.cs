using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SeedTime : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI seedTimeText;
    [SerializeField] private seedTimeSet seedTimeSet;

    [Header("畑の何番目の種スロットか(0-2)")]
    [SerializeField] private int slotIndex = 0;

    public int SlotIndex => slotIndex;

    // 成長に必要な合計時間
    public float GrowSeconds => seedTimeSet != null ? seedTimeSet.GetSeedTime() : 0f;

    // 植えているかどうか。状態はセーブデータ側が持っているので、
    // アプリを閉じても消えず、実時間で育ち続ける。
    public bool IsPlanted => FairySaveBridge.IsPlanted(slotIndex);

    // 育ちきったかどうか
    public bool IsReadyToHatch => FairySaveBridge.IsReadyToHatch(slotIndex);

    // 種を植える（キーワードを決めずに植える場合）
    public void PlantSeed()
    {
        PlantSeed(null, null);
    }

    // 願いを込めて種を植える（選んだキーワードと、そこから決まった性格を持たせる）
    public void PlantSeed(List<string> keywords, PersonalitySnapshot personality)
    {
        if (IsPlanted) return;

        if (seedTimeSet == null) return;

        FairySaveBridge.PlantSeed(slotIndex, seedTimeSet.GetSeedTime(), keywords, personality);
    }

    // 残り時間を計算して返す（植えた時刻からの実時間で計算する）
    public float GetRemainingTime()
    {
        return FairySaveBridge.GetRemainingSeconds(slotIndex);
    }

    // UIパネルが開いた時や、表示されている時だけこれを呼ぶ
    public void UpdateUI()
    {
        float remaining = GetRemainingTime();

        int hours = Mathf.FloorToInt(remaining / 3600f);
        int minutes = Mathf.FloorToInt((remaining % 3600f) / 60f);
        int seconds = Mathf.FloorToInt(remaining % 60f);

        if (seedTimeText != null)
        {
            seedTimeText.text = string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
        }
    }

    // 指定した秒数を短縮
    public void ReduceTime(float reduceSecond)
    {
        if (!IsPlanted) return;

        FairySaveBridge.ReduceSeconds(slotIndex, reduceSecond);

        UpdateUI();
    }
}
