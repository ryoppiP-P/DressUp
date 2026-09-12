//==============================================================================
//  File   : TownTutorialHook.cs
//  Brief  : TownSceneでの初回チュートリアル(街の導入→畑ボタンのハイライト)
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/9/12
//------------------------------------------------------------------------------
//  TutorialManager.Stage を見て、
//   ・None            … 街の導入3行を見せてから畑ボタンをハイライト
//   ・TownIntroShown  … (再開時)畑ボタンのハイライトだけ出し直す
//   ・FairyBorn       … 誕生〜着せ替え・命名を終えて街に戻ってきた → チュートリアル完了
//   ・それ以外        … このシーンでやることは無い(畑や着せ替え側が進行中)
//  を判断する。
//==============================================================================
using System.Collections;
using UnityEngine;

public class TownTutorialHook : MonoBehaviour {
    [Header("ハイライトする「妖精の畑」ボタン")]
    [SerializeField] private RectTransform fairyFieldButtonRect;

    private const string GodName = "神様";

    void Start() {
        var tm = TutorialManager.Instance;
        if (tm == null || tm.Completed) return;
        StartCoroutine(Routine(tm));
    }

    private IEnumerator Routine(TutorialManager tm) {
        if (tm.Stage == TutorialStage.FairyBorn) {
            // 誕生→着せ替え→命名を終えて街に戻ってきた。今回はここでチュートリアル終了とする
            // (街づくり/配置モードは未実装のためスコープ外)
            tm.Complete();
            yield break;
        }

        if (tm.Stage != TutorialStage.None && tm.Stage != TutorialStage.TownIntroShown) {
            if (tm.Overlay != null) tm.Overlay.Hide();
            yield break;
        }

        if (tm.Stage == TutorialStage.None) {
            yield return tm.Overlay.PlayMessage(GodName, "ここは妖精たちの村。");
            yield return tm.Overlay.PlayMessage(GodName, "でも今は誰もいないようですね。。");
            yield return tm.Overlay.PlayMessage(GodName, "貴方が、妖精を植えてくれませんか？");
            tm.AdvanceTo(TutorialStage.TownIntroShown);
        }

        if (tm.Stage == TutorialStage.TownIntroShown) {
            if (fairyFieldButtonRect != null) tm.Overlay.ShowHighlight(fairyFieldButtonRect);
            // ここから先は実際に畑ボタンをタップしてもらうのを待つだけ(タップで畑シーンへ遷移する)
        }
    }
}
