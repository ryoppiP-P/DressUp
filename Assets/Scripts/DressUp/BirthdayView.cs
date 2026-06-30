using TMPro;
using UnityEngine;
using static Character;

public class BirthdayView : MonoBehaviour {
    [SerializeField] private Character character;
    [SerializeField] private TMP_Text birthText; // 生年月日＋「生まれ」を出す

    void Start() {
        EnsureBirthDate(); // 未設定なら今日を誕生日として記録
        Refresh();
    }

    // まだ誕生日が無ければ、今日（リアルタイム）を記録
    private void EnsureBirthDate() {
        if (SaveManager.Instance == null || character == null) return;

        var data = SaveManager.Instance.Current.dressUp.GetOrCreate(character.CharacterId);
        // year が 0 なら未設定とみなす
        if (data.birthDate == null || data.birthDate.year == 0) {
            var now = System.DateTime.Now;
            data.birthDate = new BirthDate(now.Year, now.Month, now.Day);
            SaveManager.Instance.SaveAuto();
        }
    }

    public void Refresh() {
        if (SaveManager.Instance == null || character == null) return;

        var data = SaveManager.Instance.Current.dressUp.GetOrCreate(character.CharacterId);
        var b = data.birthDate;

        if (birthText != null)
            birthText.text = $"{b.year}/{b.month}/{b.day}\n生まれ";
    }
}
