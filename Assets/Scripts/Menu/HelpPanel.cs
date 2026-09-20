//==============================================================================
//  File   : HelpPanel.cs
//  Brief  : ヘルプ画面(チュートリアル一覧をアコーディオン表示)
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/8/2
//------------------------------------------------------------------------------
//  文言は DefaultTopics(このファイル内)が正。Inspector の topics に項目を入れると
//  そちらを優先する(空のままならコードの文言を使う)。
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

    // Inspector で項目を入れた時だけそちらを使う(空なら下の DefaultTopics)。
    // 文言は普段はここ(コード)を直す。ゲームの仕様が変わったら合わせて更新すること。
    [Header("ヘルプの項目(空ならコードの既定文言を使う)")]
    [SerializeField]
    private List<HelpTopic> topics = new();

    private static readonly HelpTopic[] DefaultTopics = {
        new HelpTopic {
            title = "はじめに",
            detail = "ここは妖精たちの村。妖精の畑で種を植えて妖精を育て、街づくりや着せ替えで自分だけの村を作ろう。\n"
                   + "画面下のバーから、街づくり・着せ替え・妖精の畑・ミッション・ガチャ・ショップへ移動できるよ。右下の葉っぱのボタンで、バーをしまったり出したりできるよ。",
        },
        new HelpTopic {
            title = "街のようす",
            detail = "妖精たちは公園・図書館・カフェをめぐって、街を歩き回っているよ。画面をドラッグすると街を動かせて、2本指(PCはホイール)で拡大・縮小できるよ。\n"
                   + "頭上にマークが出ている妖精をタップすると、どんぐりやはちみつがもらえるよ。\n"
                   + "妖精どうしがすれ違うと「！」が出るので、タップすると会話が始まって仲良くなれるよ。妖精をドラッグして好きな場所に置くこともできて、ほかの妖精の近くに置くと会話に誘えるよ。",
        },
        new HelpTopic {
            title = "妖精の畑",
            detail = "種を植える鉢は3つあるよ。鉢をタップして、育ってほしい性格のキーワードを3つ選び、「はい」を押すと種を植えられるよ。\n"
                   + "植えてから1時間で妖精が生まれるよ。「時短の実」を使うと、育つ時間を縮められるよ。\n"
                   + "生まれたら名前をつけよう。街に出せるのは、育てている種もふくめて5体までだよ。種はショップで買えるよ。",
        },
        new HelpTopic {
            title = "着せ替え",
            detail = "まず着せ替えたい子を選ぶよ。服やアクセサリーをタップすると着て、もう一度タップすると脱げるよ。「フィルター」で絞り込み、「並べ替え」で順番を変えられるよ。\n"
                   + "「コーデを適用」を押すと、今の服が保存されるよ。適用しないで戻るときは確認が出るよ。\n"
                   + "「コーデを保存」でお気に入りのコーデをとっておけて、「リセット」で服やアクセサリーをすべて外せるよ。",
        },
        new HelpTopic {
            title = "街づくり",
            detail = "街づくりのボタンを押すと、街を飾る画面になるよ。\n"
                   + "「配置する」で持っている装飾を選んで街をタップすると置けて、「削除する」で片づけられるよ。「終了する」で元の画面に戻るよ。\n"
                   + "装飾は「街装飾ガチャ」で手に入るよ。",
        },
        new HelpTopic {
            title = "ミッション",
            detail = "ミッションは「デイリー」「ウィークリー」「チャレンジ」の3種類。デイリーは毎日、ウィークリーは毎週リセットされて、チャレンジは達成するたびに目標が上がっていくよ。\n"
                   + "達成したら「受け取る」でごほうびがもらえて、「一括受け取り」でまとめて受け取れるよ。まだ達成していないミッションの「クリア」を押すと、そのために行く画面へ移動できるものもあるよ。",
        },
        new HelpTopic {
            title = "ガチャ",
            detail = "はちみつを使って、街装飾ガチャと洋服ガチャを引けるよ。1回ガチャと10回ガチャがあるよ。\n"
                   + "出やすさは N が78%・R が20%・SR が2%。洋服ガチャの10回ガチャは、R以上が1個かならず入るよ。\n"
                   + "ガチャで手に入れたアイテムは、メニューの「アイテム一覧」で見られるよ。",
        },
        new HelpTopic {
            title = "ショップ",
            detail = "どんぐりやはちみつを使って、アイテムを買えるよ。妖精の種や時短の実などが並んでいるよ。\n"
                   + "お金が足りないときは買えないので、ミッションや街の妖精からもらって貯めよう。",
        },
        new HelpTopic {
            title = "どんぐりとはちみつ",
            detail = "お金は「どんぐり」と「はちみつ」の2種類だよ。\n"
                   + "ミッションのごほうびや、街の妖精の頭上のマークをタップするともらえるよ。ガチャははちみつで引いて、ショップの品物はどんぐりやはちみつで買うよ。",
        },
        new HelpTopic {
            title = "メニューと設定",
            detail = "右上の三本線のボタンでメニューが開くよ。\n"
                   + "「環境設定」で音量の調整とデータの削除、「アイテム一覧」で持っているアイテムの確認、「ヘルプ」でこの画面が見られるよ。\n"
                   + "データを削除すると元に戻せないので気をつけてね。",
        },
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

        var source = (topics != null && topics.Count > 0) ? (IEnumerable<HelpTopic>)topics : DefaultTopics;
        foreach (var topic in source) {
            var slot = Instantiate(slotPrefab, contentParent);
            slot.Setup(topic.title, topic.detail);
            _spawned.Add(slot);
        }
    }

    private void OnClickBack() {
        if (menuPanel) menuPanel.ShowMain();
    }
}
