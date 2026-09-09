//==============================================================================
//  File   : SceneFadeIn.cs
//  Brief  : シーン開始時、真っ黒な画面から徐々に透明にしてフェードインする
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/9/8
//------------------------------------------------------------------------------
//  タイトル画面の頭に「ゲームスタート → フェードイン → タイトル」を作るために追加。
//  CanvasGroup の alpha を 1→0 にするだけなので、他のシーンに置いても使い回せる。
//==============================================================================
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class SceneFadeIn : MonoBehaviour {
    [Header("フェードにかける秒数")]
    [SerializeField] private float duration = 1f;

    private CanvasGroup _group;

    void Awake() {
        _group = GetComponent<CanvasGroup>();
        _group.alpha = 1f;          // 最初は真っ黒
        _group.blocksRaycasts = true; // フェード中はタップさせない
        _group.interactable = false;
    }

    void Start() {
        StartCoroutine(FadeRoutine());
    }

    private IEnumerator FadeRoutine() {
        float t = 0f;
        while (t < duration) {
            t += Time.deltaTime;
            _group.alpha = 1f - Mathf.Clamp01(t / duration);
            yield return null;
        }

        _group.alpha = 0f;
        _group.blocksRaycasts = false;
        gameObject.SetActive(false); // フェード後は完全に消して、以後は何も邪魔しない
    }
}
