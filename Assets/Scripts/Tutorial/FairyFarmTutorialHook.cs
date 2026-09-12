//==============================================================================
//  File   : FairyFarmTutorialHook.cs
//  Brief  : 妖精の畑(FairyParsonalTestScene)での初回チュートリアル
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/9/12
//------------------------------------------------------------------------------
//  スロット0を使う想定(初回は3鉢とも空のため確定で使える)。
//  種植え自体は既存の鉢タップ→キーワード3つ選択の通常フローをそのまま使ってもらい、
//  「植え終わった(FairySaveBridge.IsPlanted)」をポーリングして次に進める。
//  加速アイテムも同様に「使われた(所持数が減った)」をポーリングして検知し、
//  今回は特別に残り時間を丸ごと0にして(999999秒ぶん短縮)、その場で誕生させる。
//==============================================================================
using System.Collections;
using UnityEngine;

public class FairyFarmTutorialHook : MonoBehaviour {
    [Header("ハイライトする鉢(スロット0・全景でのタップ対象)")]
    [SerializeField] private RectTransform pot0Rect;

    [Header("スロット0の「時間を短縮させる」ボタン(鉢を開いた時のもの)")]
    [SerializeField] private RectTransform reduceButton0Rect;

    [Header("鉢のアップ画面の開閉制御")]
    [SerializeField] private FairyPotFocus potFocus;

    private const int Slot = 0;
    private const string GodName = "神様";
    private const string SeedItemId = "FairySeed_01";
    private const string ReduceItemId = "TimeReduceItem_01";

    void Start() {
        var tm = TutorialManager.Instance;
        if (tm == null || tm.Completed) return;
        StartCoroutine(Routine(tm));
    }

    private IEnumerator Routine(TutorialManager tm) {
        if (tm.Stage == TutorialStage.None || (int)tm.Stage >= (int)TutorialStage.FairyBorn) {
            if (tm.Overlay != null) tm.Overlay.Hide();
            yield break;
        }

        if (tm.Stage == TutorialStage.TownIntroShown) {
            yield return tm.Overlay.PlayMessage(GodName, "ここが妖精が生まれる「妖精の畑」です！");

            if (ConsumableBridge.GetCount(SeedItemId) <= 0) ConsumableBridge.Add(SeedItemId, 1);
            yield return tm.Overlay.PlayMessage(GodName, "今回は種をあげますので、一回植えてみましょう！");

            tm.AdvanceTo(TutorialStage.FarmIntroShown);
        }

        if (tm.Stage == TutorialStage.FarmIntroShown) {
            if (pot0Rect != null) tm.Overlay.ShowHighlight(pot0Rect);

            // 鉢が実際に開かれるまで待つ。開いたらすぐにハイライトを消す
            // (消さずに待ち続けると、全景の鉢の位置に開けた穴のままキーワード選択画面の上に
            //  暗幕が乗ってしまい、穴の外にあるキーワードがタップできなくなるため)
            yield return new WaitUntil(() =>
                (potFocus != null && potFocus.FocusedSlot == Slot) || FairySaveBridge.IsPlanted(Slot));
            tm.Overlay.Hide();

            // 実際にキーワードを3つ選んで植え終わる、までは普段どおりのUIに任せる
            yield return new WaitUntil(() => FairySaveBridge.IsPlanted(Slot));

            tm.AdvanceTo(TutorialStage.SeedPlanted);
        }

        if (tm.Stage == TutorialStage.SeedPlanted) {
            yield return tm.Overlay.PlayMessage(GodName, "いいですね！かわいい子が生まれそうです");
            yield return tm.Overlay.PlayMessage(GodName, "種を植えると生まれるまで少し時間がかかります。");

            int before = ConsumableBridge.GetCount(ReduceItemId);
            if (before <= 0) { ConsumableBridge.Add(ReduceItemId, 1); before = ConsumableBridge.GetCount(ReduceItemId); }
            yield return tm.Overlay.PlayMessage(GodName, "今回は特別に加速アイテムをあげますので、使ってみてください！");

            // 鉢が閉じていたら開き直す(短縮ボタンは鉢を開いている間だけ表示される)
            if (potFocus != null && potFocus.FocusedSlot != Slot) potFocus.Open(Slot);

            if (reduceButton0Rect != null) tm.Overlay.ShowHighlight(reduceButton0Rect);

            // 実際にアイテムが使われた(所持数が減った)のを検知する
            yield return new WaitUntil(() =>
                FairySaveBridge.IsReadyToHatch(Slot) || ConsumableBridge.GetCount(ReduceItemId) < before);

            // 今回だけ特別に、丸ごと生まれさせる(通常の短縮量に関わらず即誕生させる演出)
            FairySaveBridge.ReduceSeconds(Slot, 999999f);

            tm.Overlay.Hide();
            tm.AdvanceTo(TutorialStage.TimeReduceUsed);
        }

        if (tm.Stage == TutorialStage.TimeReduceUsed) {
            // 誕生自体は既存の FairyBirthWatcher が自動で検知してポップアップを出す
            yield return new WaitUntil(() => FairySaveBridge.FindUnnamed() != null);

            yield return tm.Overlay.PlayMessage(GodName, "妖精が生まれましたので、一回タッチしてみましょう！");

            tm.Overlay.Hide();
            tm.AdvanceTo(TutorialStage.FairyBorn);
            // ここから先は誕生ポップアップ(BirthPopup)をタップして着せ替え画面へ、という通常フローに任せる
        }
    }
}
