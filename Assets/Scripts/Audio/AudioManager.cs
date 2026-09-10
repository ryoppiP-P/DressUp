//==============================================================================
//  File   : AudioManager.cs
//  Brief  : BGM / SE 再生の窓口。DontDestroyOnLoad で常駐する。
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/3/22 (2026/9/10 DressUp用に移植: AudioMixer非依存・2Dのみ)
//------------------------------------------------------------------------------
//  ・立体音響(3D)は使わないので全 AudioSource は spatialBlend = 0。
//  ・音量は AudioMixer ではなく AudioSource.volume / AudioListener.volume で調整。
//      Master … AudioListener.volume
//      BGM    … _bgmVol01 を bgmSource.volume に反映
//      SE     … _seVol01 を再生時に乗算
//  ・設定値は SaveApplier / EnvironmentSettingsPanel から Set***Volume(0-100) で入る。
//    起動順に依存しないよう、Awake でも SaveManager から直接読む。
//==============================================================================
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour {
    public static AudioManager Instance { get; private set; }

    [Header("クリップ一覧")]
    [SerializeField] private SoundLibrary library;

    [Header("BGM")]
    [SerializeField] private float fadeTime = 1f;

    [Header("SE")]
    [SerializeField] private int seSourceCount = 5;

    // 0-1 の音量倍率(設定から入ってくる)
    private float _bgmVol01 = 1f;
    private float _seVol01 = 1f;

    private AudioSource _bgmSource;
    private readonly List<AudioSource> _seSources = new();
    private int _seIndex = 0;

    private Coroutine _fade;
    private BGMType _currentBGM = BGMType.None;

    //==========================================================================
    // 初期化
    //==========================================================================

    private void Awake() {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SetupSources();

        // セーブ済みの音量を反映(SaveApplier の呼び出し順に依存しないよう自分でも読む)
        if (SaveManager.Instance != null && SaveManager.Instance.Current != null) {
            var s = SaveManager.Instance.Current.settings;
            SetMasterVolume(s.masterVolume);
            SetBGMVolume(s.bgmVolume);
            SetSEVolume(s.seVolume);
        }
    }

    private void SetupSources() {
        _bgmSource = gameObject.AddComponent<AudioSource>();
        _bgmSource.loop = true;
        _bgmSource.playOnAwake = false;
        _bgmSource.spatialBlend = 0f;   // 2Dのみ
        _bgmSource.volume = _bgmVol01;

        for (int i = 0; i < seSourceCount; i++) {
            var se = gameObject.AddComponent<AudioSource>();
            se.loop = false;
            se.playOnAwake = false;
            se.spatialBlend = 0f;
            se.volume = _seVol01;
            _seSources.Add(se);
        }
    }

    //==========================================================================
    // BGM
    //==========================================================================

    public void PlayBGM(BGMType type) {
        if (type == BGMType.None) { StopBGM(); return; }
        if (_currentBGM == type && _bgmSource.isPlaying) return;

        var clip = library != null ? library.GetBGM(type) : null;
        if (clip == null) {
            Debug.LogWarning($"[AudioManager] BGM not found: {type}");
            return;
        }

        _currentBGM = type;
        if (_fade != null) StopCoroutine(_fade);
        _fade = StartCoroutine(CrossFade(clip));
    }

    public void StopBGM() {
        _currentBGM = BGMType.None;
        if (_fade != null) { StopCoroutine(_fade); _fade = null; }
        _bgmSource.Stop();
    }

    public void FadeOutBGM() {
        _currentBGM = BGMType.None;
        if (_fade != null) StopCoroutine(_fade);
        _fade = StartCoroutine(FadeOut());
    }

    //==========================================================================
    // SE
    //==========================================================================

    public void PlaySE(SEType type) => PlaySE(type, 1f);

    public void PlaySE(SEType type, float volumeScale) {
        if (type == SEType.None) return;
        var clip = library != null ? library.GetSE(type) : null;
        if (clip == null) {
            Debug.LogWarning($"[AudioManager] SE not found: {type}");
            return;
        }

        var src = _seSources[_seIndex];
        src.clip = clip;
        src.volume = Mathf.Clamp01(volumeScale) * _seVol01;
        src.Play();

        _seIndex = (_seIndex + 1) % _seSources.Count;
    }

    // 多重再生してほしいSE
    public void PlaySEOneShot(SEType type, float volumeScale = 1f) {
        if (type == SEType.None) return;
        var clip = library != null ? library.GetSE(type) : null;
        if (clip == null) {
            Debug.LogWarning($"[AudioManager] SE not found: {type}");
            return;
        }
        _seSources[0].PlayOneShot(clip, Mathf.Clamp01(volumeScale) * _seVol01);
    }

    //==========================================================================
    // ループ SE
    //==========================================================================

    public AudioSource PlayLoopSE(SEType type) {
        if (type == SEType.None) return null;
        var clip = library != null ? library.GetSE(type) : null;
        if (clip == null) return null;

        var src = gameObject.AddComponent<AudioSource>();
        src.clip = clip;
        src.loop = true;
        src.spatialBlend = 0f;
        src.volume = _seVol01;
        src.Play();

        return src;
    }

    public void StopLoopSE(AudioSource source) {
        if (source == null) return;
        source.Stop();
        Destroy(source);
    }

    //==========================================================================
    // 音量設定(0 ~ 100 の値で受け取る)
    //==========================================================================

    public void SetMasterVolume(float value0to100) {
        AudioListener.volume = Mathf.Clamp01(value0to100 / 100f);
    }

    public void SetBGMVolume(float value0to100) {
        _bgmVol01 = Mathf.Clamp01(value0to100 / 100f);
        // フェード中はコルーチンが volume を握っているので触らない(フェード先が _bgmVol01 なので自然に追従)
        if (_fade == null && _bgmSource != null) _bgmSource.volume = _bgmVol01;
    }

    public void SetSEVolume(float value0to100) {
        _seVol01 = Mathf.Clamp01(value0to100 / 100f);
        // 鳴っている(ループ含む)SEにも即反映
        foreach (var s in _seSources) if (s != null) s.volume = _seVol01;
    }

    public float GetMasterVolume() => AudioListener.volume * 100f;
    public float GetBGMVolume() => _bgmVol01 * 100f;
    public float GetSEVolume() => _seVol01 * 100f;

    //==========================================================================
    // フェード
    //==========================================================================

    private IEnumerator CrossFade(AudioClip next) {
        float half = Mathf.Max(0.01f, fadeTime / 2f);

        float start = _bgmSource.volume;
        float t = 0f;
        while (t < half) {
            t += Time.unscaledDeltaTime;
            _bgmSource.volume = Mathf.Lerp(start, 0f, t / half);
            yield return null;
        }

        _bgmSource.Stop();
        _bgmSource.clip = next;
        _bgmSource.Play();

        t = 0f;
        while (t < half) {
            t += Time.unscaledDeltaTime;
            _bgmSource.volume = Mathf.Lerp(0f, _bgmVol01, t / half);
            yield return null;
        }

        _bgmSource.volume = _bgmVol01;
        _fade = null;
    }

    private IEnumerator FadeOut() {
        float dur = Mathf.Max(0.01f, fadeTime);

        float start = _bgmSource.volume;
        float t = 0f;
        while (t < dur) {
            t += Time.unscaledDeltaTime;
            _bgmSource.volume = Mathf.Lerp(start, 0f, t / dur);
            yield return null;
        }

        _bgmSource.Stop();
        _bgmSource.volume = _bgmVol01;
        _fade = null;
    }

    //==========================================================================
    // 全停止
    //==========================================================================

    public void StopAll() {
        if (_fade != null) { StopCoroutine(_fade); _fade = null; }

        _currentBGM = BGMType.None;
        if (_bgmSource != null) _bgmSource.Stop();

        foreach (var s in _seSources) if (s != null) s.Stop();

        // PlayLoopSE で動的追加した AudioSource を掃除
        foreach (var s in GetComponents<AudioSource>()) {
            if (s != _bgmSource && !_seSources.Contains(s)) {
                s.Stop();
                Destroy(s);
            }
        }
    }
}
