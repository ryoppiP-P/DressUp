//==============================================================================
//  File   : Character.Posed.cs
//  Brief  : 「勉強」など、パーツごとに大きさの違う切り抜き画像で作ったモーションの表示
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/11/02
//------------------------------------------------------------------------------
//  Idle/Walk はどのパーツも同じ 500x500 の画面で書き出してあるので重ねるだけで合う。
//  Study などは Spine→AE の書き出しの都合でパーツごとに画像の大きさが違うため、
//  DressUpItem.StateAnim の offset/scale(500px画面上の中心位置と倍率)で置く。
//  腕・右の靴・眉などの追加パーツは StateAnim.extras で一緒に描く。
//
//  そのモーションを持っていない服を着ている時は、キャラ全体を Idle で表示する
//  (一部の服だけ消えるのを避けるため)。目・口・素体は、同じ種類の別アイテムが
//  そのモーションを持っていれば借りて表示する。セットの一部(チョーカー等)は消すだけ。
//==============================================================================
using System.Collections.Generic;
using UnityEngine;

public partial class Character {
    // 追加パーツ用のレンダラー(レイヤーごとに必要な数だけ作る)
    private readonly Dictionary<Layer, List<SpriteRenderer>> _extraRenderers = new Dictionary<Layer, List<SpriteRenderer>>();

    private void RefreshSpritesPosed(CharaState state, int frameIndex) {
        bool usable = CanUseState(state);
        CharaState s = usable ? state : CharaState.Idle;

        foreach (var layer in layers) {
            if (layer.renderer == null) continue;
            HideExtras(layer);

            if (layer.item == null) {
                layer.renderer.sprite = null;
                continue;
            }

            var anim = ResolveAnim(layer.item, layer.category, s);
            if (anim == null) {
                if (s == CharaState.Idle) {
                    // Idle すら無いアイテム(古いデータ)は今まで通りプレビュー画像
                    SetFitted(layer.renderer, layer.item.previewSprite);
                } else {
                    layer.renderer.sprite = null;   // セットの一部などで、このモーションには無いもの
                }
                continue;
            }

            Sprite chosen = anim.frames[frameIndex % anim.frames.Length];
            if (anim.usePlacement) Place(layer.renderer, chosen, anim.offset, anim.scale);
            else SetFitted(layer.renderer, chosen);

            if (anim.extras == null) continue;
            for (int i = 0; i < anim.extras.Count; i++) {
                var ex = anim.extras[i];
                if (ex == null || ex.frames == null || ex.frames.Length == 0) continue;
                var r = GetExtraRenderer(layer, i);
                r.sortingOrder = ex.sortingOrder;
                Place(r, ex.frames[frameIndex % ex.frames.Length], ex.offset, ex.scale);
            }
        }
    }

    /// <summary>今着ているもの全部がこのモーションを表示できるか</summary>
    public bool CanUseState(CharaState state) {
        if (state == CharaState.Idle) return true;
        foreach (var layer in layers) {
            if (layer.item == null) continue;
            if (ResolveAnim(layer.item, layer.category, state) != null) continue;
            if (layer.item.isSetPart) continue;   // セットの一部はそのモーションでは消すだけ
            return false;
        }
        return true;
    }

    // そのアイテムのモーション。目・口・素体は、無ければ同じ種類の別アイテムから借りる
    private DressUpItem.StateAnim ResolveAnim(DressUpItem item, CategoryType category, CharaState state) {
        var own = item.GetAnim(state);
        if (own != null) return own;
        if (state == CharaState.Idle) return null;

        bool borrowable = category == CategoryType.Body || category == CategoryType.FaceEyes || category == CategoryType.FaceMouth;
        if (!borrowable || itemDatabase == null || itemDatabase.allItems == null) return null;

        foreach (var other in itemDatabase.allItems) {
            if (other == null || other == item || other.category != category) continue;
            var a = other.GetAnim(state);
            if (a != null) return a;
        }
        return null;
    }

    // 500x500 の画面で書き出した画像(Idle/Walk など)。今まで通り高さで合わせ、位置は原点
    private void SetFitted(SpriteRenderer sr, Sprite sprite) {
        sr.sprite = sprite;
        sr.transform.localPosition = Vector3.zero;
        FitScale(sr, sprite);
    }

    // 切り抜き画像を、500px画面上の中心位置(offset)と倍率(scale)で置く
    private void Place(SpriteRenderer sr, Sprite sprite, Vector2 offset, float scale) {
        sr.sprite = sprite;
        if (sprite == null) return;
        float k = referenceHeight / 500f;
        float ppu = sprite.pixelsPerUnit;
        sr.transform.localScale = new Vector3(scale * k, scale * k, 1f);
        sr.transform.localPosition = new Vector3(offset.x * k / ppu, offset.y * k / ppu, 0f);
    }

    private SpriteRenderer GetExtraRenderer(Layer layer, int index) {
        if (!_extraRenderers.TryGetValue(layer, out var list)) {
            list = new List<SpriteRenderer>();
            _extraRenderers[layer] = list;
        }
        while (list.Count <= index) {
            var go = new GameObject(layer.renderer.name + "_Extra" + list.Count);
            go.transform.SetParent(layer.renderer.transform.parent, false);
            var r = go.AddComponent<SpriteRenderer>();
            r.sortingLayerID = layer.renderer.sortingLayerID;
            r.sharedMaterial = layer.renderer.sharedMaterial;
            r.color = layer.renderer.color;
            r.maskInteraction = layer.renderer.maskInteraction;
            go.layer = layer.renderer.gameObject.layer;
            list.Add(r);
        }
        return list[index];
    }

    private void HideExtras(Layer layer) {
        if (!_extraRenderers.TryGetValue(layer, out var list)) return;
        foreach (var r in list) if (r != null) r.sprite = null;
    }
}
