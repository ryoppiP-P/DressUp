//==============================================================================
//  File   : HelpPanel.cs
//  Brief  : ヘルプ画面(チュートリアル一覧をアコーディオン表示)
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/8/2
//------------------------------------------------------------------------------
//  topics の文言はダミー。実際のチュートリアル文言ができ次第、
//  Inspector 上でタイトル/詳細を差し替えるだけで反映される。
//==============================================================================
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HelpPanel : MonoBehaviour {
    // ヘルプ1項目分のデータ(タイトル + 詳細)
    [Serializable]
    public class HelpTopic {
        public string title;
        [TextArea] public string detail;
    }

    [Header("パネル本体(開閉対象)")]
    [SerializeField] private GameObject panelRoot;

    [Header("戻り先")]
    [SerializeField] private MenuPanel menuPanel;

    [Header("一覧")]
    [SerializeField] private HelpEntrySlot slotPrefab;
    [SerializeField] private Transform contentParent; // ScrollView の Content

    [Header("戻るボタン")]
    [SerializeField] private Button backButton;

    // チュートリアル一覧(ダミー文言。後で正式なテキストに差し替える前提)
    [Header("チュートリアル一覧(ダミー文言)")]
    [SerializeField]
    private List<HelpTopic> topics = new() {
        new HelpTopic { title = "基本操作", detail = "画面をタップ/スワイプして操作します。(ダミー)" },
        new HelpTopic { title = "着せ替え", detail = "アイテムをタップするとキャラクターに装備されます。(ダミー)" },
        new HelpTopic { title = "ミッション", detail = "デイリー/ウィークリー/チャレンジのミッションを達成すると報酬がもらえます。(ダミー)" },
        new HelpTopic { title = "ガチャ", detail = "ナットやハニーを使ってアイテムを入手できます。(ダミー)" },
        new HelpTopic { title = "ショップ", detail = "アイテムを購入できます。(ダミー)" },
    };

    private readonly List<HelpEntrySlot> _spawned = new();

    void Start() {
        if (backButton) backButton.onClick.AddListener(OnClickBack);
    }

    /// <summary>ヘルプ画面を開く</summary>
    public void Open() {
        if (panelRoot) panelRoot.SetActive(true);
        Rebuild();
    }

    public void Close() {
        if (panelRoot) panelRoot.SetActive(false);
    }

    private void Rebuild() {
        foreach (var slot in _spawned) Destroy(slot.gameObject);
        _spawned.Clear();

        foreach (var topic in topics) {
            var slot = Instantiate(slotPrefab, contentParent);
            slot.Setup(topic.title, topic.detail);
            _spawned.Add(slot);
        }
    }

    private void OnClickBack() {
        if (menuPanel) menuPanel.ShowMain();
    }
}
