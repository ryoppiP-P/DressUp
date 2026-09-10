//==============================================================================
//  File   : SoundLibrary.cs
//  Brief  : BGM / SE の enum 定義とクリップ管理を束ねる ScriptableObject
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/3/22 (2026/9/10 DressUp用にenum入れ替え・整理)
//------------------------------------------------------------------------------
//  Project で右クリック → Create → Audio → Sound Library で生成。
//  クリップを追加する時は enum に値を追加 → Inspector で紐付け登録。
//==============================================================================
using UnityEngine;
using System.Collections.Generic;

//------------------------------------------------------------------------------
// サウンドID(enum)
//  実際に用意したクリップに合わせて自由に増減・改名してよい。
//  ここに足したら SoundLibrary アセットの Inspector で AudioClip を紐付ける。
//------------------------------------------------------------------------------

public enum BGMType {
    None = 0,
    Title,
    Town,
    DressUp,     // 着せ替え画面(KisekaeScene)
    FairyFarm,   // 妖精の畑
}

public enum SEType {
    None = 0,
    Tap,          // ボタン全般
    Decide,       // 決定
    Cancel,       // 戻る / キャンセル
    Gacha,        // ガチャを引く
    GachaResult,  // ガチャ結果が出る
    Purchase,     // ショップ購入
    CoinGet,      // 通貨獲得
    OutfitApply,  // コーデ適用OK
    FairyBorn,    // 妖精誕生
    Error,        // お金が足りない等のNG
}

//------------------------------------------------------------------------------
// クリップライブラリ本体
//------------------------------------------------------------------------------

[CreateAssetMenu(fileName = "SoundLibrary", menuName = "Audio/Sound Library")]
public class SoundLibrary : ScriptableObject {
    [System.Serializable]
    private class BGMEntry {
        public BGMType type;
        public AudioClip clip;
    }

    [System.Serializable]
    private class SEEntry {
        public SEType type;
        public AudioClip clip;
    }

    [SerializeField] private List<BGMEntry> bgmEntries = new();
    [SerializeField] private List<SEEntry> seEntries = new();

    private Dictionary<BGMType, AudioClip> bgmDict;
    private Dictionary<SEType, AudioClip> seDict;

    public AudioClip GetBGM(BGMType type) {
        BuildDictIfNeeded();
        return bgmDict.TryGetValue(type, out var clip) ? clip : null;
    }

    public AudioClip GetSE(SEType type) {
        BuildDictIfNeeded();
        return seDict.TryGetValue(type, out var clip) ? clip : null;
    }

    private void BuildDictIfNeeded() {
        if (bgmDict != null && seDict != null) return;

        bgmDict = new Dictionary<BGMType, AudioClip>();
        seDict = new Dictionary<SEType, AudioClip>();

        foreach (var e in bgmEntries)
            if (e.clip != null && !bgmDict.ContainsKey(e.type))
                bgmDict[e.type] = e.clip;

        foreach (var e in seEntries)
            if (e.clip != null && !seDict.ContainsKey(e.type))
                seDict[e.type] = e.clip;
    }
}
