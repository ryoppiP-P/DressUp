//==============================================================================
//  File   : PoseAlignWindow.cs
//  Brief  : 切り抜き画像で作ったモーション(勉強など)のパーツ位置合わせ画面
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/11/02
//------------------------------------------------------------------------------
//  メニュー「DressUp / ポーズ位置合わせ」から開く。
//  ・モーション(Study 等)を選ぶと、そのモーションを持つ素体・服を重ねて表示する
//  ・左の一覧でパーツを選び、プレビューをドラッグ / 数値 / 矢印ボタンで位置と倍率を合わせる
//  ・「全体を動かす」をオンにすると、表示中の全パーツをまとめて動かす
//  ・薄い灰色は Idle の素体(大きさの目安)。コマのスライダーで全コマ確認できる
//  値は DressUpItem アセット(StateAnim.offset / scale と extras)に直接保存される。
//==============================================================================
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PoseAlignWindow : EditorWindow {
    private class Part {
        public DressUpItem item;
        public int animIndex;     // item.animations の何番目か
        public int extraIndex;    // -1 = 本体、0以上 = extras の番号
        public string label;
        public int order;        // 描画の重なり順
        public int groupOrder;   // 一覧での並び(持ち主のアイテムの重なり順)

        public DressUpItem.StateAnim Anim => item.animations[animIndex];
        public DressUpItem.ExtraPart Extra => extraIndex >= 0 ? Anim.extras[extraIndex] : null;
        public Sprite[] Frames => extraIndex >= 0 ? Extra.frames : Anim.frames;
        public Vector2 Offset {
            get { return extraIndex >= 0 ? Extra.offset : Anim.offset; }
            set { if (extraIndex >= 0) Extra.offset = value; else Anim.offset = value; }
        }
        public float Scale {
            get { return extraIndex >= 0 ? Extra.scale : Anim.scale; }
            set { if (extraIndex >= 0) Extra.scale = value; else Anim.scale = value; }
        }
    }

    private CharaState _state = CharaState.Study;
    private readonly List<Part> _parts = new List<Part>();
    private readonly HashSet<DressUpItem> _hidden = new HashSet<DressUpItem>();
    private int _selected = -1;
    private int _frame;
    private bool _play;
    private double _lastTick;
    private float _zoom = 1.2f;
    private bool _moveAll;
    private bool _showGhost = true;
    private Vector2 _listScroll;
    private Sprite _ghost;

    [MenuItem("DressUp/ポーズ位置合わせ")]
    private static void Open() {
        GetWindow<PoseAlignWindow>("ポーズ位置合わせ").Reload();
    }

    private void OnEnable() {
        Reload();
        EditorApplication.update += Tick;
        Undo.undoRedoPerformed += Repaint;
    }

    private void OnDisable() {
        EditorApplication.update -= Tick;
        Undo.undoRedoPerformed -= Repaint;
    }

    private void Tick() {
        if (!_play) return;
        if (EditorApplication.timeSinceStartup - _lastTick < 1.0 / 24.0) return;
        _lastTick = EditorApplication.timeSinceStartup;
        _frame++;
        Repaint();
    }

    // そのモーションを「位置指定」で持っている素体・服を全部集める
    private void Reload() {
        _parts.Clear();
        _ghost = null;
        foreach (var guid in AssetDatabase.FindAssets("t:DressUpItem")) {
            var item = AssetDatabase.LoadAssetAtPath<DressUpItem>(AssetDatabase.GUIDToAssetPath(guid));
            if (item == null || item.animations == null) continue;

            if (item.category == CategoryType.Body && _ghost == null) {
                var idle = item.GetFrames(CharaState.Idle);
                if (idle != null && idle.Length > 0) _ghost = idle[0];
            }

            for (int i = 0; i < item.animations.Count; i++) {
                var a = item.animations[i];
                if (a == null || a.state != _state || !a.usePlacement) continue;
                int g = CategoryOrder(item.category);
                _parts.Add(new Part { item = item, animIndex = i, extraIndex = -1, label = item.name, order = g, groupOrder = g });
                if (a.extras == null) continue;
                for (int e = 0; e < a.extras.Count; e++)
                    _parts.Add(new Part { item = item, animIndex = i, extraIndex = e, label = "  └ " + a.extras[e].name, order = a.extras[e].sortingOrder, groupOrder = g });
            }
        }
        // 一覧は「アイテム → その追加パーツ」の順に並べる(描画順は DrawPreview で別に並べ替える)
        _parts.Sort((x, y) => {
            if (x.groupOrder != y.groupOrder) return x.groupOrder.CompareTo(y.groupOrder);
            int n = string.CompareOrdinal(x.item.name, y.item.name);
            if (n != 0) return n;
            return x.extraIndex.CompareTo(y.extraIndex);
        });
        if (_selected >= _parts.Count) _selected = -1;
        ApplyOutfit();
    }

    // ワンピースとトップス+ボトムスは同時に着られないので、どちらか一方だけ表示する
    private bool _showDress = true;

    private void ApplyOutfit() {
        foreach (var p in _parts) {
            if (p.extraIndex >= 0) continue;
            var c = p.item.category;
            bool dressSide = c == CategoryType.Dress;
            bool topsSide = c == CategoryType.Tops || c == CategoryType.Bottoms || c == CategoryType.Outers;
            if (dressSide) { if (_showDress) _hidden.Remove(p.item); else _hidden.Add(p.item); }
            if (topsSide) { if (_showDress) _hidden.Add(p.item); else _hidden.Remove(p.item); }
        }
    }

    // Character のレイヤー並び(TownScene のプレビューキャラ)と同じ描画順
    private static int CategoryOrder(CategoryType c) {
        switch (c) {
            case CategoryType.BackGround: return 0;
            case CategoryType.HairBack: return 1;
            case CategoryType.Body: return 2;
            case CategoryType.FaceEyes:
            case CategoryType.FaceMouth: return 3;
            case CategoryType.HairFront:
            case CategoryType.HairAll: return 4;
            case CategoryType.Shoes: return 5;
            case CategoryType.Bottoms: return 6;
            case CategoryType.Tops:
            case CategoryType.Outers: return 7;
            case CategoryType.Dress: return 8;
            case CategoryType.HeadAccessory: return 9;
            case CategoryType.GlassesAccessory: return 13;
            case CategoryType.BodyAccessory: return 17;
            default: return 0;
        }
    }

    private void OnGUI() {
        using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar)) {
            EditorGUI.BeginChangeCheck();
            _state = (CharaState)EditorGUILayout.EnumPopup(_state, EditorStyles.toolbarDropDown, GUILayout.Width(120));
            if (EditorGUI.EndChangeCheck()) { _selected = -1; Reload(); }
            if (GUILayout.Button("再読み込み", EditorStyles.toolbarButton, GUILayout.Width(80))) Reload();
            _showGhost = GUILayout.Toggle(_showGhost, "Idleの素体を薄く表示", EditorStyles.toolbarButton);
            _moveAll = GUILayout.Toggle(_moveAll, "全体を動かす", EditorStyles.toolbarButton);
            if (GUILayout.Button(_showDress ? "服: ワンピース" : "服: トップス+ボトムス", EditorStyles.toolbarButton, GUILayout.Width(130))) {
                _showDress = !_showDress;
                ApplyOutfit();
            }
            GUILayout.FlexibleSpace();
            GUILayout.Label("拡大", GUILayout.Width(30));
            _zoom = GUILayout.HorizontalSlider(_zoom, 0.5f, 3f, GUILayout.Width(120));
            if (GUILayout.Button("保存", EditorStyles.toolbarButton, GUILayout.Width(50))) AssetDatabase.SaveAssets();
        }

        if (_parts.Count == 0) {
            EditorGUILayout.HelpBox(_state + " を「位置指定(usePlacement)」で持っているアイテムがありません。", MessageType.Info);
            return;
        }

        using (new EditorGUILayout.HorizontalScope()) {
            DrawList();
            using (new EditorGUILayout.VerticalScope()) {
                DrawFrameBar();
                DrawPreview();
            }
        }
    }

    //--------------------------------------------------------------------------
    // 左: パーツ一覧と数値
    //--------------------------------------------------------------------------

    private void DrawList() {
        using (new EditorGUILayout.VerticalScope(GUILayout.Width(280))) {
            EditorGUILayout.LabelField("パーツ(チェック = 表示)", EditorStyles.boldLabel);
            _listScroll = EditorGUILayout.BeginScrollView(_listScroll, GUILayout.Height(position.height * 0.55f));
            for (int i = 0; i < _parts.Count; i++) {
                var p = _parts[i];
                using (new EditorGUILayout.HorizontalScope()) {
                    if (p.extraIndex < 0) {
                        bool shown = !_hidden.Contains(p.item);
                        bool now = EditorGUILayout.Toggle(shown, GUILayout.Width(18));
                        if (now != shown) { if (now) _hidden.Remove(p.item); else _hidden.Add(p.item); }
                    } else {
                        GUILayout.Space(22);
                    }
                    var style = i == _selected ? EditorStyles.whiteBoldLabel : EditorStyles.label;
                    if (GUILayout.Button(p.label + "  [" + p.order + "]", style)) _selected = i;
                }
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();
            if (_selected < 0) {
                EditorGUILayout.HelpBox("パーツを選ぶと、位置と倍率を調整できます。\nプレビューをドラッグしても動かせます。", MessageType.None);
                return;
            }

            var sel = _parts[_selected];
            EditorGUILayout.LabelField(sel.label.Trim(), EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            Vector2 off = EditorGUILayout.Vector2Field("位置(px)", sel.Offset);
            float sc = EditorGUILayout.Slider("倍率", sel.Scale, 0.2f, 3f);
            if (EditorGUI.EndChangeCheck()) {
                Record(sel.item);
                sel.Offset = off;
                sel.Scale = sc;
                MarkDirty(sel.item);
            }

            if (sel.extraIndex >= 0) {
                EditorGUI.BeginChangeCheck();
                int order = EditorGUILayout.IntField("重なり順", sel.Extra.sortingOrder);
                if (EditorGUI.EndChangeCheck()) {
                    Record(sel.item);
                    sel.Extra.sortingOrder = order;
                    sel.order = order;
                    MarkDirty(sel.item);
                }
            }

            EditorGUILayout.LabelField("少しずつ動かす(Shiftで10px)");
            float step = Event.current.shift ? 10f : 1f;
            using (new EditorGUILayout.HorizontalScope()) {
                if (GUILayout.Button("←")) Nudge(new Vector2(-step, 0));
                if (GUILayout.Button("→")) Nudge(new Vector2(step, 0));
                if (GUILayout.Button("↑")) Nudge(new Vector2(0, step));
                if (GUILayout.Button("↓")) Nudge(new Vector2(0, -step));
            }
        }
    }

    private void Nudge(Vector2 delta) {
        if (_moveAll) { MoveAll(delta); return; }
        if (_selected < 0) return;
        var p = _parts[_selected];
        Record(p.item);
        p.Offset += delta;
        MarkDirty(p.item);
    }

    private void MoveAll(Vector2 delta) {
        var touched = new HashSet<DressUpItem>();
        foreach (var p in _parts) {
            if (_hidden.Contains(p.item)) continue;
            if (touched.Add(p.item)) Record(p.item);
            p.Offset += delta;
        }
        foreach (var it in touched) MarkDirty(it);
    }

    private static void Record(DressUpItem item) { Undo.RecordObject(item, "ポーズ位置合わせ"); }
    private static void MarkDirty(DressUpItem item) { EditorUtility.SetDirty(item); }

    //--------------------------------------------------------------------------
    // 右: コマ送りとプレビュー
    //--------------------------------------------------------------------------

    private int MaxFrames() {
        int m = 1;
        foreach (var p in _parts) if (p.Frames != null) m = Mathf.Max(m, p.Frames.Length);
        return m;
    }

    private void DrawFrameBar() {
        using (new EditorGUILayout.HorizontalScope()) {
            _play = GUILayout.Toggle(_play, _play ? "■ 停止" : "▶ 再生", "Button", GUILayout.Width(70));
            int max = MaxFrames();
            _frame = EditorGUILayout.IntSlider("コマ", ((_frame % max) + max) % max, 0, max - 1);
        }
    }

    private void DrawPreview() {
        float size = 500f * _zoom;
        Rect area = GUILayoutUtility.GetRect(size, size, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
        EditorGUI.DrawRect(area, new Color(0.86f, 0.86f, 0.86f));
        GUI.BeginClip(area);
        area = new Rect(0, 0, area.width, area.height);
        // 500x500 の枠
        Handles.color = new Color(0, 0, 0, 0.25f);
        Handles.DrawWireCube(area.center, area.size);

        if (_showGhost && _ghost != null) {
            var prev = GUI.color;
            GUI.color = new Color(1, 1, 1, 0.3f);
            DrawSprite(area, _ghost, Vector2.zero, 500f / _ghost.rect.height);
            GUI.color = prev;
        }

        Rect selRect = Rect.zero;
        var drawOrder = new List<int>();
        for (int i = 0; i < _parts.Count; i++) drawOrder.Add(i);
        drawOrder.Sort((a, b) => _parts[a].order != _parts[b].order ? _parts[a].order.CompareTo(_parts[b].order) : a.CompareTo(b));
        foreach (int i in drawOrder) {
            var p = _parts[i];
            if (_hidden.Contains(p.item) || p.Frames == null || p.Frames.Length == 0) continue;
            var sp = p.Frames[_frame % p.Frames.Length];
            Rect r = DrawSprite(area, sp, p.Offset, p.Scale);
            if (i == _selected) selRect = r;
        }
        if (selRect != Rect.zero) {
            Handles.color = new Color(1f, 0.3f, 0.2f, 0.9f);
            Handles.DrawWireCube(selRect.center, selRect.size);
        }

        // ドラッグで動かす
        var ev = Event.current;
        if (ev.type == EventType.MouseDrag && ev.button == 0 && area.Contains(ev.mousePosition)) {
            Vector2 delta = new Vector2(ev.delta.x, -ev.delta.y) / _zoom;
            if (_moveAll) MoveAll(delta);
            else if (_selected >= 0) {
                var p = _parts[_selected];
                Record(p.item);
                p.Offset += delta;
                MarkDirty(p.item);
            }
            ev.Use();
            Repaint();
        }
        GUI.EndClip();
    }

    // 500px画面上の中心位置 offset(原点=中央, y上向き)・倍率 scale で sprite を描く
    private Rect DrawSprite(Rect area, Sprite sp, Vector2 offset, float scale) {
        if (sp == null || sp.texture == null) return Rect.zero;
        float w = sp.rect.width * scale * _zoom;
        float h = sp.rect.height * scale * _zoom;
        float cx = area.x + (250f + offset.x) * _zoom;
        float cy = area.y + (250f - offset.y) * _zoom;
        // スプライトの中心(pivot)が offset に来る
        float px = sp.pivot.x / sp.rect.width, py = sp.pivot.y / sp.rect.height;
        var r = new Rect(cx - w * px, cy - h * (1f - py), w, h);
        // Sprite Atlas が有効だと sp.texture はまとめ画像(余白を詰めた状態)になるので、
        // 元の画像ファイルのテクスチャから sp.rect の範囲を描く
        var tex = SourceTexture(sp);
        if (tex == null) return r;
        var sr = sp.rect;
        var uv = new Rect(sr.x / tex.width, sr.y / tex.height, sr.width / tex.width, sr.height / tex.height);
        GUI.DrawTextureWithTexCoords(r, tex, uv, true);
        return r;
    }

    private readonly Dictionary<string, Texture2D> _texCache = new Dictionary<string, Texture2D>();

    private Texture2D SourceTexture(Sprite sp) {
        string path = AssetDatabase.GetAssetPath(sp);
        if (string.IsNullOrEmpty(path)) return sp.texture;
        if (!_texCache.TryGetValue(path, out var tex) || tex == null) {
            tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            _texCache[path] = tex;
        }
        return tex;
    }
}
