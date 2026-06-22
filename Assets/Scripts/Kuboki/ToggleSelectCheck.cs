/*
* ファイル名　ToggleSelectCheck.cs
* タイトル　　トグルの選択確認
* 作成者　　　久保木幹太
* 作成日　　　6月22日
* 更新日　　　6月22日
*/

using UnityEngine;
using UnityEngine.UI;

public class ToggleSelectCheck : MonoBehaviour
{
    [System.Serializable]
    public struct States
    {
        [Header("寂しがり")]public int Lonely;
        [Header("気まぐれ")]public int Whim;
        [Header("甘えん坊")]public int Spoiled;
        [Header("不思議")]public int Mystery;
        [Header("照れ屋")]public int Shy;
        [Header("面倒見がいい")]public int Mom;
    }

    private struct ToggleState
    {
        public bool fuwa;
        public bool kappatu;
        public bool ooraka;
        public bool sunao;
        public bool majime;
        public bool jikkuri;
        public bool cool;
        public bool sikkari;
    }

    [Header("現在のステータス（インスペクターで基準値を設定可能）")]
    public States currentStates;

    public Toggle[] toggles; // トグルの配列

    private ToggleState currentToggleState; // 今のフレームの状態
    private ToggleState prevToggleState;    // 1フレーム前の状態


    void Update()
    {
        // すべてのトグルの最新状態を正しく取得する
        CaptureCurrentToggleStates();

        // 1フレーム前と比較して、変化があったトグルのステータスを計算する
        CheckAndApplyStatusChanges();

        // 次のフレームのために、今の状態を過去データとして保存する
        prevToggleState = currentToggleState;
    }

    // 各トグルの名前を見て、純粋に true/false をセットする関数（上書き問題を解決）
    private void CaptureCurrentToggleStates()
    {
        foreach (Toggle t in toggles)
        {
            if (t == null) continue;

            bool isOn = t.isOn;
            if (t.gameObject.name == "ふわふわ↑") currentToggleState.fuwa = isOn;
            if (t.gameObject.name == "活発") currentToggleState.kappatu = isOn;
            if (t.gameObject.name == "おおらか") currentToggleState.ooraka = isOn;
            if (t.gameObject.name == "すなお") currentToggleState.sunao = isOn;
            if (t.gameObject.name == "真面目") currentToggleState.majime = isOn;
            if (t.gameObject.name == "じっくり") currentToggleState.jikkuri = isOn;
            if (t.gameObject.name == "クール") currentToggleState.cool = isOn;
            if (t.gameObject.name == "しっかり") currentToggleState.sikkari = isOn;
        }
    }

    private void CheckAndApplyStatusChanges()
    {
        // ふわふわ↑
        if (currentToggleState.fuwa != prevToggleState.fuwa)
        {
            int modifier = currentToggleState.fuwa ? 1 : -1;

            currentStates.Mystery += 10 * modifier;
            currentStates.Lonely -= 10 * modifier;
        }

        // 活発
        if (currentToggleState.kappatu != prevToggleState.kappatu)
        {
            int modifier = currentToggleState.kappatu ? 1 : -1;

            currentStates.Shy -= 10 * modifier;
            currentStates.Whim += 5 * modifier;
            currentStates.Mystery += 5 * modifier;
        }

        // おおらか
        if (currentToggleState.ooraka != prevToggleState.ooraka)
        {
            int modifier = currentToggleState.ooraka ? 1 : -1;

            currentStates.Mystery -= 10 * modifier;
            currentStates.Mom += 10 * modifier;
        }

        // すなお
        if (currentToggleState.sunao != prevToggleState.sunao)
        {
            int modifier = currentToggleState.sunao ? 1 : -1;

            currentStates.Shy -= 10 * modifier;
            currentStates.Spoiled += 10 * modifier;
        }

        // 真面目
        if (currentToggleState.majime != prevToggleState.majime)
        {
            int modifier = currentToggleState.majime ? 1 : -1;

            currentStates.Spoiled -= 5 * modifier;
            currentStates.Mom += 5 * modifier;
        }

        // じっくり
        if (currentToggleState.jikkuri != prevToggleState.jikkuri)
        {
            int modifier = currentToggleState.jikkuri ? 1 : -1;

            currentStates.Lonely += 10 * modifier;
            currentStates.Spoiled -= 10 * modifier;
        }

        // クール
        if (currentToggleState.cool != prevToggleState.cool)
        {
            int modifier = currentToggleState.cool ? 1 : -1;

            currentStates.Spoiled -= 5 * modifier;
            currentStates.Shy -= 5 * modifier;
            currentStates.Whim -= 8 * modifier;
            currentStates.Mom += 2 * modifier;
        }

        // しっかり
        if (currentToggleState.sikkari != prevToggleState.sikkari)
        {
            int modifier = currentToggleState.sikkari ? 1 : -1;

            currentStates.Whim -= 8 * modifier;
            currentStates.Mom += 8 * modifier;
        }
    }
}