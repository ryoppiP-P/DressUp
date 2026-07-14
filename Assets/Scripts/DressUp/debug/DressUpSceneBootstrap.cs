using UnityEngine;

public class DressUpSceneBootstrap : MonoBehaviour {
    [SerializeField] private Character character;   // シーンにいる唯一のキャラ
    [SerializeField] private string debugFallbackId = "charaID_0001"; // 直接起動時の保険

    void Start() {
        if (character == null) {
            Debug.LogError("[Bootstrap] character が未設定です");
            return;
        }

        // 選択シーン経由なら選んだID、そうでなければ保険のID
        string id = string.IsNullOrEmpty(CharacterSelection.SelectedId)
            ? debugFallbackId
            : CharacterSelection.SelectedId;

        character.SetCharacterId(id);   // 選んだIDをキャラに書き込む
        character.ReloadForId();        // その新しいIDでセーブを読み直す
        Debug.Log($"[Bootstrap] id '{id}' を設定して読み込み");

        DressUpTarget.Instance.SetTarget(character);
    }
}
