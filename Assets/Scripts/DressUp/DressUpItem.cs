// 入手順は、セーブデータ作り次第導入
using UnityEngine;
using System.Collections.Generic;

public enum ItemColor {
    Red,         // レッド
    Orange,      // オレンジ
    Yellow,      // イエロー
    YellowGreen, // 黄緑
    Green,       // グリーン
    Blue,        // ブルー
    Purple,      // パープル
    Pink,        // ピンク
    Brown,       // ブラウン
    Beige,       // ベージュ
    Gray,        // グレー
    White,       // ホワイト
    Black,       // ブラック
    Gold,        // ゴールド
    Silver,      // シルバー
    Colorful,    // カラフル
    Clear,       // クリア
}

[CreateAssetMenu(menuName = "DressUp/Item")]
public class DressUpItem : GameItem {
    public CategoryType category; // 着用カテゴリ(ヘア/トップ等)
    public Sprite previewSprite;  // キャラのレイヤーに差し込むスプライト

    public int releaseYear = 2026;          // リリース年（絞り込み・並べ替え用）
    public ItemColor[] colors;       // このアイテムが持つ色（複数可）

    [System.Serializable]
    public class StateAnim {
        public CharaState state;
        public Sprite[] frames;
    }

    public List<StateAnim> animations; // 状態ごとのコマ

    public Sprite[] GetFrames(CharaState state) {
        var a = animations.Find(x => x.state == state);
        return (a != null) ? a.frames : null;
    }
}