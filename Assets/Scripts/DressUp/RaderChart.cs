using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class RadarChart : Graphic {
    [Header("形状")]
    [SerializeField] private float radius = 100f;      // 値100のときの半径
    [SerializeField] private float startAngle = 90f;   // 1軸目の角度（90=真上）
    [SerializeField] private bool clockwise = true;    // 時計回りに並べる

    [Header("値（0-1で6軸）")]
    [SerializeField, Range(0f, 1f)] private float[] values = new float[6];

    // 外から 0-100 の値を6つ渡す
    public void SetValues(int[] v) {
        if (v == null) return;
        if (values == null || values.Length != 6) values = new float[6];
        for (int i = 0; i < 6; i++)
            values[i] = (i < v.Length) ? Mathf.Clamp01(v[i] / 100f) : 0f;
        SetVerticesDirty(); // 再描画要求
    }

    protected override void OnPopulateMesh(VertexHelper vh) {
        vh.Clear();
        if (values == null || values.Length < 6) return;

        // 中心（インデックス0）
        var center = UIVertex.simpleVert;
        center.color = color;
        center.position = Vector3.zero;
        vh.AddVert(center);

        // 6方向の外周頂点（インデックス1..6）
        for (int i = 0; i < 6; i++) {
            float dir = clockwise ? -1f : 1f;
            float angle = Mathf.Deg2Rad * (startAngle + dir * 60f * i);
            float r = radius * values[i];

            var vert = UIVertex.simpleVert;
            vert.color = color;
            vert.position = new Vector3(Mathf.Cos(angle) * r, Mathf.Sin(angle) * r, 0f);
            vh.AddVert(vert);
        }

        // 中心と隣り合う外周2点で三角形を6枚
        for (int i = 0; i < 6; i++) {
            int cur = i + 1;
            int next = (i + 1) % 6 + 1;
            vh.AddTriangle(0, cur, next);
        }
    }

#if UNITY_EDITOR
    // Inspectorで値をいじったらエディタ上でも即反映
    protected override void OnValidate() {
        base.OnValidate();
        SetVerticesDirty();
    }
#endif
}
