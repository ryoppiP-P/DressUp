//==============================================================================
//  File   : TalkData.cs
//  Brief  : すれ違い会話1本分のデータ(ScriptableObject)
//
//  personality は既存の PersonalityAxis(6軸パラメータ)を再利用する。
//  各キャラの中で一番値が高い軸を「今の性格タイプ」として扱い、
//  話しかけた側の性格タイプに一致するTalkDataが優先的に選ばれる。
//
//  lines は偶数番目=話しかけた側、奇数番目=返した側のセリフとして交互に使われる。
//  {partner}=相手の表示名、{item}=相手が今着ている服の名前、
//  {place}=話しかけた側が向かっている建物名 に置き換えられる。
//==============================================================================
using UnityEngine;

[CreateAssetMenu(menuName = "Talk/TalkData")]
public class TalkData : ScriptableObject {
    [Header("この会話が選ばれやすい性格タイプ")]
    public PersonalityAxis personality;

    [Header("話題")]
    public TalkTopic topic;

    [Header("セリフ(偶数=話しかけた側/奇数=返した側)")]
    [TextArea] public string[] lines;
}
