//==============================================================================
//  File   : TownTutorialHook.cs
//  Brief  : TownSceneでの初回チュートリアル(街の導入→畑ボタンのハイライト)
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/9/12
//------------------------------------------------------------------------------
//  TutorialManager.Stage を見て、
//   ・None                 … 街の導入3行を見せてから畑ボタンをハイライト
//   ・TownIntroShown       … (再開時)畑ボタンのハイライトだけ出し直す
//   ・FairyBorn            … 誕生〜着せ替え・命名を終えて街に戻ってきた → 街づくり導入3行→街クリボタンをハイライト
//   ・TownCreateIntroShown … (再開時)街クリボタンのハイライトだけ出し直す(実際に入るのを待つ)
//   ・TownCreateEntered    … 配置モードに入った(装飾を1つ置くのを待つ)
//   ・TownCreateDecorated  … 装飾を1つ置いた(編集画面を出るのを待って締めの3行→完了)
//   ・それ以外             … このシーンでやることは無い(畑や着せ替え側が進行中)
//  を判断する。
//==============================================================================
using System.Collections;
using UnityEngine;

public class TownTutorialHook : MonoBehaviour {
    [Header("ハイライトする「妖精の畑」ボタン")]
    [SerializeField] private RectTransform fairyFieldButtonRect;

    [Header("ハイライトする「街クリエイト」ボタン")]
    [SerializeField] private RectTransform townCreateButtonRect;

    private const string GodName = "神様";

    void Start() {
        var tm = TutorialManager.Instance;
        if (tm == null || tm.Completed) return;
        StartCoroutine(Routine(tm));
    }

    private IEnumerator Routine(TutorialManager tm) {
        bool handledHere =
            tm.Stage == TutorialStage.None ||
            tm.Stage == TutorialStage.TownIntroShown ||
            tm.Stage == TutorialStage.FairyBorn ||
            tm.Stage == TutorialStage.TownCreateIntroShown ||
            tm.Stage == TutorialStage.TownCreateEntered ||
            tm.Stage == TutorialStage.TownCreateDecorated;

        if (!handledHere) {
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
            yield break;
        }

        if (tm.Stage == TutorialStage.FairyBorn) {
            yield return tm.Overlay.PlayMessage(GodName, "妖精さんが街に来ました！");
            yield return tm.Overlay.PlayMessage(GodName, "でも、街に何もないからなんか寂しいですね、、、");
            yield return tm.Overlay.PlayMessage(GodName, "ちょっと街を飾ってみましょう");
            tm.AdvanceTo(TutorialStage.TownCreateIntroShown);
        }

        if (tm.Stage == TutorialStage.TownCreateIntroShown) {
            if (townCreateButtonRect != null) tm.Overlay.ShowHighlight(townCreateButtonRect);
            // 実際に街クリボタンをタップして配置モードに入ってもらうのを待つ
            yield return new WaitUntil(() => TownCreateController.IsEditScreenOpen);
            tm.Overlay.Hide();
            tm.AdvanceTo(TutorialStage.TownCreateEntered);
        }

        if (tm.Stage == TutorialStage.TownCreateEntered) {
            // 装飾を実際に1つ置いてもらうのを待つ(セーブ済みの配置数を直接見るので、
            // 強制終了しての再開でも「既に置いてあるか」を正しく判定できる)
            yield return new WaitUntil(HasAnyDecorationPlaced);
            tm.AdvanceTo(TutorialStage.TownCreateDecorated);
        }

        if (tm.Stage == TutorialStage.TownCreateDecorated) {
            // 配置モードを抜けて通常の街画面に戻ってから締めの台詞を見せる
            yield return new WaitUntil(() => !TownCreateController.IsEditScreenOpen);
            yield return tm.Overlay.PlayMessage(GodName, "綺麗になりましたね！");
            yield return tm.Overlay.PlayMessage(GodName, "これからは、貴方だけの街を作ってください");
            yield return tm.Overlay.PlayMessage(GodName, "そのほかにも、ミッションやショップとか色々あるから頑張って！");
            tm.Complete();
        }
    }

    private static bool HasAnyDecorationPlaced() {
        if (SaveManager.Instance == null || SaveManager.Instance.Current == null) return false;
        var list = SaveManager.Instance.Current.townCreateData.placedDecorations;
        return list != null && list.Count > 0;
    }
}
