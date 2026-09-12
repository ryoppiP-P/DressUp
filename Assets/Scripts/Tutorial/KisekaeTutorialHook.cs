//==============================================================================
//  File   : KisekaeTutorialHook.cs
//  Brief  : 着せ替え画面(KisekaeScene)での初回チュートリアル案内
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/9/12
//------------------------------------------------------------------------------
//  誕生直後の命名フロー中(FairyBirthFlow.IsNamingFlow)だけ、2行の案内を出す。
//  実際の着せ替え・名前入力・街へ戻る、は既存の NewFairyNamingPanel の
//  フローにそのまま任せる(このセッションで「名づけを必ずやらせる」対応済み)。
//  完了判定は TownScene 側の TownTutorialHook が「街に戻ってきた」ことで行う。
//==============================================================================
using System.Collections;
using UnityEngine;

public class KisekaeTutorialHook : MonoBehaviour {
    private const string GodName = "神様";

    void Start() {
        var tm = TutorialManager.Instance;
        if (tm == null || tm.Completed) return;

        if (tm.Stage != TutorialStage.FairyBorn || !FairyBirthFlow.IsNamingFlow) {
            if (tm.Overlay != null) tm.Overlay.Hide();
            return;
        }

        StartCoroutine(Routine(tm));
    }

    private IEnumerator Routine(TutorialManager tm) {
        yield return tm.Overlay.PlayMessage(GodName, "ここでは、生まれた妖精を着せ替えすることができます");
        yield return tm.Overlay.PlayMessage(GodName, "一回、自分だけの妖精を作ってみてください！");
        tm.Overlay.Hide();
    }
}
