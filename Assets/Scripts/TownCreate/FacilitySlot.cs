//==============================================================================
//  File   : FacilitySlot.cs
//  Brief  : 街クリエイトで「ここに施設を置ける」という予約スペースの仮表示
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/9/12
//------------------------------------------------------------------------------
//  配置モード・装飾データ本体はまだ無いので、今は該当タイルの色を変えて
//  名前ラベルを出すだけのプレースホルダー。IsoTownGridの子として置く想定。
//==============================================================================
using UnityEngine;
using TMPro;

public class FacilitySlot : MonoBehaviour {
    [Header("施設名(ラベル表示用)")]
    [SerializeField] private string facilityName;

    [Header("グリッド座標(左下を原点とする、タイル数)")]
    [SerializeField] private int originX;
    [SerializeField] private int originY;
    [SerializeField] private int sizeX = 3;
    [SerializeField] private int sizeY = 3;

    [Header("見た目")]
    [SerializeField] private Color highlightColor = new Color(0.9f, 0.8f, 0.55f, 1f);
    [SerializeField] private TMP_FontAsset font;

    private TextMeshPro _label;

    [ContextMenu("Apply")]
    public void Apply() {
        var grid = GetComponentInParent<IsoTownGrid>();
        if (grid == null) {
            Debug.LogWarning("[FacilitySlot] 親に IsoTownGrid が見つかりません: " + name);
            return;
        }

        Vector3 sum = Vector3.zero;
        int count = 0;
        for (int y = 0; y < sizeY; y++) {
            for (int x = 0; x < sizeX; x++) {
                var tile = grid.GetTile(originX + x, originY + y);
                if (tile == null) continue;
                tile.SetHighlight(highlightColor);
                sum += tile.transform.position;
                count++;
            }
        }
        if (count == 0) return;

        Vector3 center = sum / count;

        if (_label == null) {
            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(transform, false);
            _label = labelGo.AddComponent<TextMeshPro>();
            _label.alignment = TextAlignmentOptions.Center;
            _label.fontSize = 3f;
            _label.color = Color.black;
            // 親(IsoTownGrid)のScaleY=0.5を打ち消して、ラベルの縦横比を保つ
            _label.transform.localScale = new Vector3(1f, 2f, 1f);
            if (font != null) _label.font = font;
        }
        _label.text = facilityName;
        _label.transform.position = center + new Vector3(0f, 0f, -0.1f);
    }
}
