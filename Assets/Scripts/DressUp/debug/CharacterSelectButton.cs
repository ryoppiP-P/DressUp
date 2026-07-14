using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterSelectButton : MonoBehaviour {
    [SerializeField] private string characterId;      // このボタンが表すキャラID
    [SerializeField] private string dressUpSceneName = "DressUp";

    // ボタンの OnClick に登録
    public void OnClick() {
        CharacterSelection.SelectedId = characterId;
        SceneManager.LoadScene(dressUpSceneName);
    }
}
public static class CharacterSelection {
    public static string SelectedId; // 選んだキャラのID
}
