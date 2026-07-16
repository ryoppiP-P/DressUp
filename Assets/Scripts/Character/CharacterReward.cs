using UnityEngine;

public class CharacterReward : MonoBehaviour {
    [Header("ポップアップ")]
    [SerializeField] private GameObject popup;        // 頭上に出す収集アイコン（子オブジェクト）
    [SerializeField] private float refillSeconds = 2f; // 補充までの時間（テスト用2秒）

    [Header("報酬（ランダム範囲）")]
    [SerializeField] private int nutMin = 10;
    [SerializeField] private int nutMax = 20;
    [SerializeField] private int honeyMin = 0;
    [SerializeField] private int honeyMax = 2;

    private float _timer;
    private bool _ready; // ポップアップ表示中＝タップ可能

    void Start() {
        SetPopupActive(false);
        _timer = refillSeconds; // 起動後 refillSeconds で最初のポップアップ
    }

    void Update() {
        if (_ready) return; // 既に表示中なら補充カウントしない

        _timer -= Time.deltaTime;
        if (_timer <= 0f) {
            _ready = true;
            SetPopupActive(true);
        }
    }

    // ポップアップがタップされたら呼ぶ
    public void OnPopupTapped() {
        if (!_ready) return;

        int nut = Random.Range(nutMin, nutMax + 1);   // 上限含むので +1
        int honey = Random.Range(honeyMin, honeyMax + 1);

        if (SaveManager.Instance != null) {
            SaveManager.Instance.AddCurrency(CurrencyType.Nut, nut);
            SaveManager.Instance.AddCurrency(CurrencyType.Honey, honey);
        }
        Debug.Log($"[Reward] 木の実+{nut}, はちみつ+{honey}");

        // 消して再補充へ
        _ready = false;
        SetPopupActive(false);
        _timer = refillSeconds;
    }

    private void SetPopupActive(bool on) {
        if (popup != null) popup.SetActive(on);
    }
}
