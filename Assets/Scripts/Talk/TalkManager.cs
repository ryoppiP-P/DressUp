//==============================================================================
//  File   : TalkManager.cs
//  Brief  : すれ違い成立時に呼ばれ、性格・話題に合ったセリフを選んで
//           頭上の吹き出しに交互に表示するシーン常駐マネージャー
//
//  Name   : Ryoto Kikuchi
//
//  TownMoveTestシーンに1つだけ配置する想定(CharacterManager.CheckPassByから呼ばれる)。
//==============================================================================
using System.Collections;
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

    void Awake() {
        Instance = this;
    }

    // 会話を開始し、想定される合計表示時間(秒)を返す。開始できなければ0を返す。
    // (呼び出し側はこの戻り値を使って、すれ違いの一時停止時間を会話の長さまで延長する)
    public float TryStartConversation(CharacterManager a, CharacterManager b) {
        if (database == null || a == null || b == null) return 0f;

        SpeechBubble bubbleA = a.GetComponentInChildren<SpeechBubble>();
        SpeechBubble bubbleB = b.GetComponentInChildren<SpeechBubble>();
        if (bubbleA == null || bubbleB == null) return 0f;

        PersonalityAxis personality = GetDominantAxis(a);
        string destinationName = GetDestinationName(a);
        bool hasPlace = !string.IsNullOrEmpty(destinationName);

        TalkData talk = database.PickRandom(personality, hasPlace);
        if (talk == null || talk.lines == null || talk.lines.Length == 0) return 0f;

        SeparateForTalk(a, b);
        FaceEachOther(a, b);
        SetIdle(a);
        SetIdle(b);
        StartCoroutine(PlayLines(talk, a, b, bubbleA, bubbleB, destinationName));

        return talk.lines.Length * secondsPerLine;
    }

    private IEnumerator PlayLines(TalkData talk, CharacterManager a, CharacterManager b,
        SpeechBubble bubbleA, SpeechBubble bubbleB, string destinationName) {
        for (int i = 0; i < talk.lines.Length; i++) {
            bool isA = (i % 2 == 0);
            CharacterManager speaker = isA ? a : b;
            CharacterManager listener = isA ? b : a;
            SpeechBubble bubble = isA ? bubbleA : bubbleB;

            string text = ResolveLine(talk.lines[i], speaker, listener, destinationName);
            bubble.ShowLine(text, secondsPerLine);

            yield return new WaitForSeconds(secondsPerLine);
        }

        // 会話が終わったら、まだ移動を続ける途中なら歩きアニメーションに戻す
        RestoreWalking(a);
        RestoreWalking(b);
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
            if (state.equipped.TryGetValue(category, out DressUpItem item) && item != null)
                return item.itemName;
        }

        return fallbackItemWord;
    }

    // 話しかけた側が向かっている建物の名前。取れなければ空文字。
    private string GetDestinationName(CharacterManager who) {
        var wander = who.GetComponent<TownWander>();
        return wander != null ? wander.CurrentDestinationName : "";
    }

    // すれ違った時点では重なっているので、真ん中を基準に一定距離だけ引き離す
    private void SeparateForTalk(CharacterManager a, CharacterManager b) {
        Vector3 posA = a.transform.position;
        Vector3 posB = b.transform.position;
        Vector3 mid = (posA + posB) * 0.5f;

        Vector3 dir = posB - posA;
        dir.z = 0f;
        if (dir.sqrMagnitude < 0.0001f) dir = Vector3.right; // 完全に重なっている場合のフォールバック
        dir.Normalize();

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
