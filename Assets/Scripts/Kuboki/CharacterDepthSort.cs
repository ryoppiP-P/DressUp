//==============================================================================
//  File   : CharacterDepthSort.cs
//  Brief  : 街でキャラ同士がすれ違った時に、服のレイヤーがちらつかないようにする
//
//  Author : Ryoto Kikuchi
//------------------------------------------------------------------------------
//  どのキャラも服/髪/顔のレイヤーは同じ Sorting Order(0〜50)を使っているので、
//  2人が重なると「同じ順位の絵同士」の前後が毎フレーム入れ替わってちらつく。
//  服/髪/顔のレイヤーだけを子オブジェクト(Visual)にまとめて SortingGroup を付け、
//  「キャラ1体 = 1枚の絵」として扱い、Y座標が低い(画面の手前にいる)キャラほど前に描く。
//  キャラ内部のレイヤーの前後関係は SortingGroup の中でこれまで通り保たれる。
//
//  報酬ポップアップや吹き出しは Visual の外(ルート直下)に置き、
//  Sorting Order を十分大きくして、どのキャラより常に手前に出す。
//==============================================================================
using UnityEngine;
using UnityEngine.Rendering;

public class CharacterDepthSort : MonoBehaviour {
    [Tooltip("服/髪/顔のレイヤーをまとめた子オブジェクトの SortingGroup")]
    [SerializeField] private SortingGroup visualGroup;

    [Tooltip("建物(Sorting Order 10)より常に手前に出すための基準値。街の奥(Yが大きい)でも順位がマイナスにならない大きさにする")]
    [SerializeField] private int baseOrder = 3000;

    [Tooltip("Y座標1あたりの順位の刻み。大きいほど近いキャラ同士も細かく区別できる")]
    [SerializeField] private float orderPerUnit = 100f;

    private int _tieBreak;

    void Awake() {
        if (visualGroup == null) visualGroup = GetComponentInChildren<SortingGroup>(true);
        // 完全に同じ位置で重なっても順位が同点にならないよう、キャラごとに固定の小さなずれを付ける
        _tieBreak = Mathf.Abs(GetInstanceID()) % 16;
    }

    // 移動(TownWander の Update)が終わった後に、その位置で順位を決める
    void LateUpdate() {
        if (visualGroup == null) return;
        visualGroup.sortingOrder = baseOrder + Mathf.RoundToInt(-transform.position.y * orderPerUnit) + _tieBreak;
    }
}
