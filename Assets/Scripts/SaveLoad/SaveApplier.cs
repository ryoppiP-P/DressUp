//==============================================================================
//  File   : SaveApplier.cs
//  Brief  : SaveManager のデータを各システムに適用する
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/6/18
//------------------------------------------------------------------------------
//  ゲーム起動時に SaveManager のデータを読んで、Audio / Screen 等に反映する。
//  全てのロード->適用をここで行う。
//  設定変更時の保存（Save系）を集約。
//==============================================================================
using UnityEngine;

public static class SaveApplier {
    //--------------------------------------------------------------------------
    // 全設定を一括適用
    //--------------------------------------------------------------------------
    public static void ApplyAll() {
        if (SaveManager.Instance == null) return;

        //ApplyAudio();
        //ApplyScreen();
    }

    ////==========================================================================
    //// Audio - 適用
    ////==========================================================================
    //public static void ApplyAudio() {
    //    if (AudioManager.Instance == null) return;
    //    if (SaveManager.Instance == null) return;

    //    var s = SaveManager.Instance.Current.settings;
    //    AudioManager.Instance.SetMasterVolume(s.masterVolume);
    //    AudioManager.Instance.SetBGMVolume(s.bgmVolume);
    //    AudioManager.Instance.SetSEVolume(s.seVolume);
    //}

    ////==========================================================================
    //// Audio - 保存
    ////==========================================================================

    //public static void SaveAudio() {
    //    if (AudioManager.Instance == null) return;
    //    var s = SaveManager.Instance.Current.settings;
    //    s.masterVolume = AudioManager.Instance.GetMasterVolume();
    //    s.bgmVolume = AudioManager.Instance.GetBGMVolume();
    //    s.seVolume = AudioManager.Instance.GetSEVolume();
    //    SaveManager.Instance.SaveAuto();
    //}

    ////==========================================================================
    //// Screen - 適用
    ////==========================================================================
    //public static void ApplyScreen() {
    //    if (SaveManager.Instance == null) return;

    //    int index = SaveManager.Instance.Current.settings.screenMode;
    //    FullScreenMode mode = index switch {
    //        0 => FullScreenMode.ExclusiveFullScreen,
    //        1 => FullScreenMode.FullScreenWindow,
    //        2 => FullScreenMode.Windowed,
    //        _ => FullScreenMode.FullScreenWindow,
    //    };

    //    Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, mode);
    //}

    ////==========================================================================
    //// Screen - 保存
    ////==========================================================================
    //public static void SaveScreen(int dropdownValue) {
    //    SaveManager.Instance.Current.settings.screenMode = dropdownValue;
    //    SaveManager.Instance.SaveAuto();
    //}
}
