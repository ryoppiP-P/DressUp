/*
* ファイル名 RouteTest.cs
* タイトル   ルート移動テスト用コントローラー
* 作成者   　久保木幹太
* 作成日   　6月2日
*/

using UnityEngine;

public class RouteTest : MonoBehaviour
{
    [Header("動かしたいキャラクター")]
    [SerializeField] private CharacterManager targetCharacter;

    [Header("向かわせたい建物")]
    [SerializeField] private Building targetBuilding;

    private void Start()
    {
        // どちらもインスペクターでセットされているか確認
        if (targetCharacter != null && targetBuilding != null)
        {
            // キャラクターに建物へのルート移動を開始させる！
            targetCharacter.StartRouteNavigation(targetBuilding);
            Debug.Log($"[Test] {targetCharacter.gameObject.name} に {targetBuilding.BuildingName} への移動命令を出しました。");
        }
        else
        {
            Debug.LogWarning("[Test] キャラクターまたは建物がインスペクターで設定されていません！");
        }
    }
}