//==============================================================================
//  File   : EnvironmentSettingsPanel.cs
//  Brief  : 環境設定画面(音量設定・データ削除)
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/8/2
//------------------------------------------------------------------------------
//  音量はスライダー操作のたびに SaveManager へ反映してオートセーブする。
//  データ削除は DeleteConfirmDialog を経由してから実行し、完了後はタイトルへ戻る。
//==============================================================================
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnvironmentSettingsPanel : MonoBehaviour {
    [Header("パネル本体(開閉対象)")]
    [SerializeField] private GameObject panelRoot;

    [Header("戻り先")]
    [SerializeField] private MenuPanel menuPanel; // 戻るボタンでメニュートップへ

    [Header("音量スライダー(0-100)")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private TMP_Text masterVolumeText;
    [SerializeField] private Slider bgmVolumeSlider;
    [SerializeField] private TMP_Text bgmVolumeText;
    [SerializeField] private Slider seVolumeSlider;
    [SerializeField] private TMP_Text seVolumeText;

    [Header("データ削除")]
    [SerializeField] private Button deleteDataButton;
    [SerializeField] private DeleteConfirmDialog confirmDialog;

    [Header("戻るボタン")]
    [SerializeField] private Button backButton;

    // データ削除後に戻るシーン名
    private const string TitleSceneName = "TitleScene";

    void Start() {
        if (masterVolumeSlider) masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        if (bgmVolumeSlider) bgmVolumeSlider.onValueChanged.AddListener(OnBgmVolumeChanged);
        if (seVolumeSlider) seVolumeSlider.onValueChanged.AddListener(OnSeVolumeChanged);
        if (deleteDataButton) deleteDataButton.onClick.AddListener(OnClickDeleteData);
        if (backButton) backButton.onClick.AddListener(OnClickBack);
    }

    /// <summary>環境設定画面を開く。開いた瞬間に保存済みの値をスライダーへ反映する</summary>
    public void Open() {
        if (panelRoot) panelRoot.SetActive(true);
        RefreshFromSave();
    }

    public void Close() {
        if (panelRoot) panelRoot.SetActive(false);
    }

    //--------------------------------------------------------------
    // 保存値 → UI 反映
    //--------------------------------------------------------------
    private void RefreshFromSave() {
        if (SaveManager.Instance == null) return;
        var s = SaveManager.Instance.Current.settings;

        // SetValueWithoutNotify で反映時の onValueChanged 発火(=不要な再セーブ)を防ぐ
        if (masterVolumeSlider) masterVolumeSlider.SetValueWithoutNotify(s.masterVolume);
        if (bgmVolumeSlider) bgmVolumeSlider.SetValueWithoutNotify(s.bgmVolume);
        if (seVolumeSlider) seVolumeSlider.SetValueWithoutNotify(s.seVolume);

        UpdateVolumeText(masterVolumeText, s.masterVolume);
        UpdateVolumeText(bgmVolumeText, s.bgmVolume);
        UpdateVolumeText(seVolumeText, s.seVolume);
    }

    private void UpdateVolumeText(TMP_Text text, float value) {
        if (text) text.text = Mathf.RoundToInt(value).ToString();
    }

    //--------------------------------------------------------------
    // スライダー操作 → 保存 + 即時反映
    //--------------------------------------------------------------
    private void OnMasterVolumeChanged(float value) {
        if (SaveManager.Instance == null) return;
        SaveManager.Instance.Current.settings.masterVolume = value;
        UpdateVolumeText(masterVolumeText, value);
        SaveApplier.ApplyAudio(); // マスター音量は AudioListener.volume に即反映
        SaveManager.Instance.SaveAuto();
    }

    private void OnBgmVolumeChanged(float value) {
        if (SaveManager.Instance == null) return;
        SaveManager.Instance.Current.settings.bgmVolume = value;
        UpdateVolumeText(bgmVolumeText, value);
        SaveManager.Instance.SaveAuto();
        // TODO: BGM 用の AudioMixer / AudioSource が用意でき次第、ここで音量を反映する
    }

    private void OnSeVolumeChanged(float value) {
        if (SaveManager.Instance == null) return;
        SaveManager.Instance.Current.settings.seVolume = value;
        UpdateVolumeText(seVolumeText, value);
        SaveManager.Instance.SaveAuto();
        // TODO: SE 用の AudioMixer / AudioSource が用意でき次第、ここで音量を反映する
    }

    //--------------------------------------------------------------
    // データ削除
    //--------------------------------------------------------------
    private void OnClickDeleteData() {
        if (confirmDialog == null) return;
        confirmDialog.Open(DeleteSaveAndReturnToTitle);
    }

    // 確認ダイアログで「はい」が押された後の実処理
    private void DeleteSaveAndReturnToTitle() {
        if (SaveManager.Instance != null) SaveManager.Instance.DeleteSave();
        UnityEngine.SceneManagement.SceneManager.LoadScene(TitleSceneName);
    }

    private void OnClickBack() {
        if (menuPanel) menuPanel.ShowMain();
    }
}
