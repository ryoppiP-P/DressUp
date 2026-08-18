//==============================================================================
//  File   : CharacterSelectList.cs
//  Brief  : 「どの子を着せ替える？」今いるコだけカードで並べる
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/8/18
//------------------------------------------------------------------------------
//  並べる対象は FairySaveBridge.GetNamedFairies()。
//  畑で生まれて名前を付け終わったコ = 街に出ているコと同じ顔ぶれになる。
//
//  カードの絵は、画面外に置いた撮影用キャラにそのコの装備を着せて
//  OutfitCapture で1枚ずつ焼いている(コーデ保存のサムネと同じやり方)。
//  押すと CharacterSelection.SelectedId に入れて着せ替え画面へ。
//  受け取るのは DressUpSceneBootstrap なので、そちら側は変更なしで動く。
//==============================================================================
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterSelectList : MonoBehaviour {
    [Header("カードを並べる先(ScrollRect の Content)")]
    [SerializeField] private RectTransform content;

    [Header("カードのプレハブ")]
    [SerializeField] private CharacterCard cardPrefab;

    [Header("サムネ撮影(画面外に置いた撮影用キャラと、その専用カメラ)")]
    [SerializeField] private Character previewCharacter;
    [SerializeField] private OutfitCapture capture;

    [Header("1体もいない時に出すもの")]
    [SerializeField] private GameObject emptyMessage;

    [Header("進む先(着せ替え画面)")]
    [SerializeField] private string dressUpSceneName = "KisekaeScene";

    [Header("並べ方")]
    [SerializeField] private int columns = 2;
    [SerializeField] private Vector2 cardSize = new Vector2(430f, 430f);
    [SerializeField] private Vector2 spacing = new Vector2(60f, 70f);
    [SerializeField] private float topPadding = 40f;
    [SerializeField] private float bottomPadding = 80f;

    private readonly List<CharacterCard> _cards = new List<CharacterCard>();

    void Start() {
        Rebuild();
    }

    /// <summary>今いるコを数え直してカードを並べ直す</summary>
    public void Rebuild() {
        ClearCards();

        var fairies = FairySaveBridge.GetNamedFairies();

        if (emptyMessage != null) emptyMessage.SetActive(fairies.Count == 0);
        if (fairies.Count == 0) {
            ResizeContent(0);
            return;
        }

        // 撮影用キャラは普段見えないところに置いてあるので、撮る間だけ起こす
        bool capturing = PrepareCapture();

        for (int i = 0; i < fairies.Count; i++) {
            var entry = fairies[i];
            if (entry == null || string.IsNullOrEmpty(entry.characterId)) continue;

            var card = Instantiate(cardPrefab, content);
            card.name = "Card_" + entry.characterId;
            Place(card.GetComponent<RectTransform>(), _cards.Count, fairies.Count);

            card.Show(entry.characterId,
                      ResolveName(entry.characterId),
                      capturing ? CaptureThumbnail(entry.characterId) : null,
                      Select);

            _cards.Add(card);
        }

        FinishCapture();
        ResizeContent(_cards.Count);
    }

    //--------------------------------------------------------------------------
    // 並べる
    //--------------------------------------------------------------------------

    // 最後の行が半端な数になったら、その行だけ中央に寄せる
    private void Place(RectTransform rect, int index, int total) {
        if (rect == null) return;

        int column = index % columns;
        int row = index / columns;

        int countInRow = Mathf.Min(columns, total - row * columns);
        float rowWidth = countInRow * cardSize.x + (countInRow - 1) * spacing.x;

        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.sizeDelta = cardSize;

        float x = -rowWidth * 0.5f + cardSize.x * 0.5f + column * (cardSize.x + spacing.x);
        float y = -(topPadding + row * (cardSize.y + spacing.y));
        rect.anchoredPosition = new Vector2(x, y);
    }

    private void ResizeContent(int count) {
        if (content == null) return;

        int rows = count <= 0 ? 0 : Mathf.CeilToInt(count / (float)columns);
        float height = topPadding + bottomPadding;
        if (rows > 0) height += rows * cardSize.y + (rows - 1) * spacing.y;

        content.sizeDelta = new Vector2(content.sizeDelta.x, height);
        content.anchoredPosition = new Vector2(content.anchoredPosition.x, 0f);
    }

    private void ClearCards() {
        foreach (var card in _cards) {
            if (card == null) continue;

            // Destroy はフレーム終わりまで効かないので、先に消しておかないと
            // 並べ直した瞬間だけ古いカードが重なって見える
            card.gameObject.SetActive(false);
            Destroy(card.gameObject);
        }
        _cards.Clear();
    }

    //--------------------------------------------------------------------------
    // サムネ撮影
    //--------------------------------------------------------------------------

    private bool PrepareCapture() {
        if (previewCharacter == null || capture == null) return false;

        previewCharacter.gameObject.SetActive(true);
        capture.gameObject.SetActive(true);   // Camera.Render() は非アクティブだと効かない
        return true;
    }

    private void FinishCapture() {
        if (previewCharacter != null) previewCharacter.gameObject.SetActive(false);
        if (capture != null) capture.gameObject.SetActive(false);
    }

    // そのコの装備を撮影用キャラに着せて1枚撮る
    private Sprite CaptureThumbnail(string characterId) {
        previewCharacter.SetCharacterId(characterId);
        previewCharacter.ReloadForId();
        return capture.Capture();
    }

    private string ResolveName(string characterId) {
        if (SaveManager.Instance == null) return "";

        string saved = SaveManager.Instance.GetCharacterName(characterId);
        return string.IsNullOrEmpty(saved) ? "なまえ" : saved;
    }

    //--------------------------------------------------------------------------
    // 選ぶ
    //--------------------------------------------------------------------------

    private void Select(string characterId) {
        CharacterSelection.SelectedId = characterId;
        SceneManager.LoadScene(dressUpSceneName);
    }
}
