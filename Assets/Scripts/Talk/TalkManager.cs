//==============================================================================
//  File   : TalkManager.cs
//  Brief  : すれ違い成立時に呼ばれ、性格・話題に合ったセリフを選んで
//           頭上の吹き出しに交互に表示するシーン常駐マネージャー
//
//  Name   : Ryoto Kikuchi
//
//  TownSceneに1つだけ配置する想定(CharacterManager.CheckPassByから呼ばれる)。
//------------------------------------------------------------------------------
//  会話の流れ(2026/9/3 更新):
//   1. すれ違い成立 → OfferConversation() が「！」ポップアップを2人の間に出す。
//      10秒以内にタップされなければ、会話なしで解散して歩き出す。
//   2. タップされたら RunConversation() で、カメラをその場所へズームしてから
//      セリフを交互に表示する。終わったらカメラを元の位置・ズームへ戻す。
//   3. カメラ・ポップアップはシーンに1つずつなので、会話(誘い中〜終了まで)は
//      町全体で同時に1組だけ。既に誰かが話している/誘われている間、
//      別のペアがすれ違ってもOfferConversationは0を返して何も起きない。
//==============================================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TalkManager : MonoBehaviour {
    public static TalkManager Instance { get; private set; }

    [Header("会話データベース")]
    [SerializeField] private TalkDatabase database;

    [Header("1行あたりの表示時間(仮)")]
    [SerializeField] private float secondsPerLine = 2.5f;

    [Header("{item}が見つからない時に使う代わりの言葉")]
    [SerializeField] private string fallbackItemWord = "私服";

    [Header("会話中の立ち位置(仮、重なり回避)")]
    [SerializeField] private float talkSeparationDistance = 1.6f;

    [Header("同じ相手と続けて誘い/会話しないための猶予(秒)")]
    [SerializeField] private float postTalkCooldown = 5f;

    [Header("「！」ポップアップ")]
    [SerializeField] private TalkPrompt prompt;
    [SerializeField] private float promptTimeout = 10f;   // これだけタップされなければ会話なしで解散
    [SerializeField] private float promptHeight = 1.2f;    // 2人の中間からどれだけ上に出すか

    [Header("会話中のカメラズーム")]
    [SerializeField] private CameraController cameraController;   // 未指定ならCameraController.Instanceを使う
    [SerializeField] private float talkZoomSize = 3f;
    [SerializeField] private float cameraMoveDuration = 0.5f;

    // 今会話中のキャラクター。会話が終わるまで別の相手とは話さない。
    private readonly HashSet<CharacterManager> _talking = new HashSet<CharacterManager>();

    // 誘い(ポップアップ表示)〜会話終了まで、町全体で他のペアを誘わないためのガード。
    // ポップアップ・カメラがシーンに1つずつしか無いため、同時に持てる会話は1組だけ。
    private bool _busy;

    // タップ待ち中の誘いの中身。タップ or タイムアウトで null に戻す。
    private class PendingOffer {
        public CharacterManager a;
        public CharacterManager b;
        public TalkData talk;
        public string destinationName;
        public Coroutine timeoutRoutine;
    }
    private PendingOffer _pending;

    void Awake() {
        Instance = this;

        // ポップアップのタップは毎回リスナーを付け外ししない(重複登録を避けるため一度だけ購読する)
        if (prompt != null && prompt.TapButton != null)
            prompt.TapButton.onClick.AddListener(OnPromptTapped);
    }

    void OnDisable() {
        // 途中で止まった場合に状態が残らないようにする
        _talking.Clear();
        _busy = false;
        _pending = null;
    }

    /// <summary>そのキャラクターが今誘われている/会話中かどうか</summary>
    public bool IsTalking(CharacterManager who) {
        return who != null && _talking.Contains(who);
    }

    /// <summary>
    /// 会話を誘う(「！」ポップアップを出す)。
    /// 戻り値は「タップ待ちに必要な一時停止秒数」(誘えなければ0)。
    /// 呼び出し側(CharacterManager.CheckPassBy)はこれで一時停止時間を確保する。
    /// タップされた後の実際の会話時間は、こちらから改めて SetPauseSeconds で延長する。
    /// </summary>
    public float OfferConversation(CharacterManager a, CharacterManager b) {
        if (database == null || a == null || b == null || prompt == null) return 0f;
        if (_busy) return 0f;                          // 町全体で同時に1組だけ
        if (IsTalking(a) || IsTalking(b)) return 0f;

        SpeechBubble bubbleA = a.GetComponentInChildren<SpeechBubble>();
        SpeechBubble bubbleB = b.GetComponentInChildren<SpeechBubble>();
        if (bubbleA == null || bubbleB == null) return 0f;

        PersonalityAxis personality = GetDominantAxis(a);
        string destinationName = GetDestinationName(a);
        bool hasPlace = !string.IsNullOrEmpty(destinationName);

        TalkData talk = database.PickRandom(personality, hasPlace);
        if (talk == null || talk.lines == null || talk.lines.Length == 0) return 0f;

        _talking.Add(a);
        _talking.Add(b);
        _busy = true;

        SeparateForTalk(a, b);
        FaceEachOther(a, b);
        SetIdle(a);
        SetIdle(b);

        _pending = new PendingOffer {
            a = a, b = b, talk = talk, destinationName = destinationName,
        };

        Vector3 mid = (a.transform.position + b.transform.position) * 0.5f + Vector3.up * promptHeight;
        prompt.transform.position = mid;
        prompt.Show();

        _pending.timeoutRoutine = StartCoroutine(PromptTimeoutRoutine());

        return promptTimeout;
    }

    private IEnumerator PromptTimeoutRoutine() {
        yield return new WaitForSeconds(promptTimeout);

        // タップ済みなら既に _pending は null になっている
        if (_pending == null) yield break;

        CancelOffer();
    }

    private void OnPromptTapped() {
        if (_pending == null) return;   // タップ待ちの誘いが無い時の空振りタップは無視

        PendingOffer offer = _pending;
        _pending = null;

        if (offer.timeoutRoutine != null) StopCoroutine(offer.timeoutRoutine);
        prompt.Hide();

        // 実際の会話にかかる時間(カメラ移動×2 + セリフ)ぶん、一時停止を延長しておく。
        // コルーチンのつなぎ目には多少のオーバーヘッドが乗るので、少し多めに見積もっておく
        // (見積もりが足りずに会話の途中で_pauseTimerが尽きてしまうと、
        //  ズームしたままキャラが目的地へ歩き出してしまう)。
        // 実際に動き出すタイミングは、この見積もりではなく会話終了時のClearPause()で確定させる。
        float talkDuration = cameraMoveDuration * 2f + offer.talk.lines.Length * secondsPerLine + 1f;
        offer.a.SetPauseSeconds(talkDuration);
        offer.b.SetPauseSeconds(talkDuration);

        StartCoroutine(RunConversation(offer));
    }

    /// <summary>タップされずタイムアウトした時: 会話なしで解散する</summary>
    private void CancelOffer() {
        PendingOffer offer = _pending;
        _pending = null;
        _busy = false;

        prompt.Hide();

        _talking.Remove(offer.a);
        _talking.Remove(offer.b);

        // まだ移動を続ける途中なら歩きアニメーションに戻す(会話は無かったことになる)
        RestoreWalking(offer.a);
        RestoreWalking(offer.b);

        // すぐ横にいるのでこのままだと即座にまた誘い直してしまう。少し間を空ける。
        offer.a.SetPassByCooldown(offer.b, postTalkCooldown);
    }

    private IEnumerator RunConversation(PendingOffer offer) {
        CameraController cam = cameraController != null ? cameraController : CameraController.Instance;

        Vector3 mid = (offer.a.transform.position + offer.b.transform.position) * 0.5f;
        if (cam != null) yield return cam.FocusOn(mid, talkZoomSize, cameraMoveDuration);

        SpeechBubble bubbleA = offer.a.GetComponentInChildren<SpeechBubble>();
        SpeechBubble bubbleB = offer.b.GetComponentInChildren<SpeechBubble>();

        yield return PlayLines(offer.talk, offer.a, offer.b, bubbleA, bubbleB, offer.destinationName);

        if (cam != null) yield return cam.Restore(cameraMoveDuration);

        // 会話が終わったので、また別の相手と話せるようにする。
        // 歩きアニメーションに戻すのもここ(カメラが戻り切った後)でやる。
        // PlayLines側の直後でやると、_pauseTimer はカメラが戻る秒数ぶんまだ残っているのに
        // 見た目だけ歩き出してしまい、「その場でWalkアニメだけ再生されて動かない」状態になっていた。
        _talking.Remove(offer.a);
        _talking.Remove(offer.b);

        // SetPauseSecondsの見積もりが多少ズレていても、
        // 「動けるようになる」と「歩きアニメーションに戻す」を必ず同じ瞬間に揃える。
        // これでカメラが戻り切った直後、間を置かずそれぞれの目的地へ歩き出す。
        offer.a.ClearPause();
        offer.b.ClearPause();

        RestoreWalking(offer.a);
        RestoreWalking(offer.b);

        // すぐ横にいるのでこのままだと即座にまた誘い直してしまう。少し間を空ける。
        offer.a.SetPassByCooldown(offer.b, postTalkCooldown);

        _busy = false;
    }

    private IEnumerator PlayLines(TalkData talk, CharacterManager a, CharacterManager b,
        SpeechBubble bubbleA, SpeechBubble bubbleB, string destinationName) {
        for (int i = 0; i < talk.lines.Length; i++) {
            bool isA = (i % 2 == 0);
            CharacterManager speaker = isA ? a : b;
            CharacterManager listener = isA ? b : a;
            SpeechBubble bubble = isA ? bubbleA : bubbleB;

            string text = ResolveLine(talk.lines[i], speaker, listener, destinationName);
            if (bubble != null) bubble.ShowLine(text, secondsPerLine);

            yield return new WaitForSeconds(secondsPerLine);
        }
    }

    // 会話中はIdleアニメーションで立ち止まらせる
    private void SetIdle(CharacterManager who) {
        var view = who.GetComponent<Character>();
        if (view != null) view.SetState(CharaState.Idle);
    }

    // 会話終了後、まだルート移動中なら歩きアニメーションに戻す
    private void RestoreWalking(CharacterManager who) {
        if (!who.IsFollowingRoute) return;
        var view = who.GetComponent<Character>();
        if (view != null) view.SetState(CharaState.Walk);
    }

    // {partner}/{item}/{place} を実際の値に置き換える
    private string ResolveLine(string line, CharacterManager speaker, CharacterManager listener, string destinationName) {
        string text = line;

        Character listenerView = listener.GetComponent<Character>();
        if (listenerView != null)
            text = text.Replace("{partner}", listenerView.DisplayName);

        text = text.Replace("{item}", GetEquippedItemName(listener));
        text = text.Replace("{place}", destinationName ?? "");

        return text;
    }

    // 相手が今着ている服の名前(Dress優先→Tops→Bottoms)。無ければ代替語。
    private string GetEquippedItemName(CharacterManager who) {
        if (SaveManager.Instance == null) return fallbackItemWord;

        string id = who.CharaId;
        if (string.IsNullOrEmpty(id)) return fallbackItemWord;

        var state = SaveManager.Instance.GetEquipState(id);
        if (state == null) return fallbackItemWord;

        CategoryType[] priority = { CategoryType.Dress, CategoryType.Tops, CategoryType.Bottoms };
        foreach (var category in priority) {
            var item = state.Get(category);
            if (item != null) return item.itemName;
        }

        return fallbackItemWord;
    }

    // 話しかけた側が向かっている建物の名前。取れなければ空文字。
    private string GetDestinationName(CharacterManager who) {
        var wander = who.GetComponent<TownWander>();
        return wander != null ? wander.CurrentDestinationName : "";
    }

    // すれ違った時点では重なっているので、真ん中を基準に一定距離だけ引き離す。
    // キャラは左右にしか向けない(スプライトに上下向きが無い)ので、
    // 斜め/縦にすれ違った場合でもX方向だけで横並びにする(Yは真ん中に揃える)。
    private void SeparateForTalk(CharacterManager a, CharacterManager b) {
        Vector3 posA = a.transform.position;
        Vector3 posB = b.transform.position;
        Vector3 mid = (posA + posB) * 0.5f;

        // 元々どちらが右寄りだったかで左右の割り振りだけ決める
        float dirX = (posB.x >= posA.x) ? 1f : -1f;
        Vector3 dir = new Vector3(dirX, 0f, 0f);

        a.transform.position = mid - dir * (talkSeparationDistance * 0.5f);
        b.transform.position = mid + dir * (talkSeparationDistance * 0.5f);
    }

    // 会話中はお互いが向き合うように、相手がいる方向へ顔を向ける
    private void FaceEachOther(CharacterManager a, CharacterManager b) {
        Character viewA = a.GetComponent<Character>();
        Character viewB = b.GetComponent<Character>();

        bool bIsRight = b.transform.position.x > a.transform.position.x;
        if (viewA != null) viewA.SetFacing(bIsRight);
        if (viewB != null) viewB.SetFacing(!bIsRight);
    }

    // dataListの中で一番値が高い軸を、そのキャラの「今の性格タイプ」として返す
    private PersonalityAxis GetDominantAxis(CharacterManager who) {
        PersonalityAxis best = PersonalityAxis.Mystery;
        int bestValue = -1;

        foreach (PersonalityAxis axis in System.Enum.GetValues(typeof(PersonalityAxis))) {
            int value = who.GetData(axis);
            if (value > bestValue) {
                bestValue = value;
                best = axis;
            }
        }

        return best;
    }
}
