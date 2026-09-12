//==============================================================================
//  File   : TutorialManager.cs
//  Brief  : 初回チュートリアルの進行状態を持つ常駐シングルトン
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/9/12
//------------------------------------------------------------------------------
//  各シーンの Hook(TownTutorialHook / FairyFarmTutorialHook / KisekaeTutorialHook)が
//  自分の Start() で Instance.Stage / Completed を見て、
//  「もう終わってるので何もしない」「ここから再開する」を判断する。
//  段階が進むたびに AdvanceTo() で即セーブするので、強制終了しても
//  最後に進んだ段階から再開できる(=チェックポイント)。
//==============================================================================
using UnityEngine;

public class TutorialManager : MonoBehaviour {
    public static TutorialManager Instance { get; private set; }

    [SerializeField] private TutorialOverlay overlay;

    public TutorialOverlay Overlay => overlay;
    public TutorialStage Stage { get; private set; } = TutorialStage.None;
    public bool Completed { get; private set; } = false;

    void Awake() {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    private void Load() {
        var data = SaveManager.Instance != null && SaveManager.Instance.Current != null
            ? SaveManager.Instance.Current.tutorialData : null;
        if (data == null) return;

        Completed = data.completed;
        Stage = (TutorialStage)data.stage;
    }

    private void Save() {
        if (SaveManager.Instance == null || SaveManager.Instance.Current == null) return;

        var data = SaveManager.Instance.Current.tutorialData;
        data.completed = Completed;
        data.stage = (int)Stage;
        SaveManager.Instance.SaveAuto();
    }

    /// <summary>チュートリアルの段階を進める(=チェックポイント。即セーブする)</summary>
    public void AdvanceTo(TutorialStage next) {
        if (Stage == next) return;
        Stage = next;
        Save();
    }

    /// <summary>チュートリアル全体を完了扱いにする(以後、二度と出さない)</summary>
    public void Complete() {
        if (Completed) return;
        Completed = true;
        Stage = TutorialStage.Completed;
        Save();
        if (overlay != null) overlay.Hide();
    }
}
