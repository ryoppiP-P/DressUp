using TMPro;
using UnityEngine;

public class SeedTime : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI seedTimeText;
    [SerializeField] private seedTimeSet seedTimeSet;

    private float plantedTime = 0f;  // 植えられた時点のTotalGameTime
    private float seedTime = 0f; // 成長に必要な合計時間
    private bool isPlanted = false;

    public bool IsPlanted => isPlanted;

    public void PlantSeed()
    {
        if (isPlanted) return;

        if (seedTimeSet == null || TimeManager.Instance == null) return;

        // 成長に必要な時間を取得
        seedTime = seedTimeSet.GetSeedTime();

        // 植えた時点の全体時間を記憶しておく
        plantedTime = TimeManager.Instance.TotalGameTime;

        isPlanted = true;
    }

    // 残り時間を計算して返す
    public float GetRemainingTime()
    {
        if (!isPlanted) return 0f;

        // 植えてからの経過時間＝ 現在の全体時間 － 植えた時間
        float elapsedTime = TimeManager.Instance.TotalGameTime - plantedTime;

        // 残り時間＝ 必要時間 － 経過時間
        float remaining = seedTime - elapsedTime;

        // 0以下なら0にする
        return Mathf.Max(0f, remaining);
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
        if (!isPlanted) return;

        plantedTime -= reduceSecond;

        UpdateUI();
    }
}