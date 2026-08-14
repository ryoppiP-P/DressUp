//==============================================================================
//  File   : TownCharacterPlacement.cs
//  Brief  : 街に出るキャラクターの立ち位置を決める
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/8/13
//------------------------------------------------------------------------------
//  前回の居場所が記録されていればそこから再開し、
//  初めて街に出るキャラは WayPoint のどれかの近くにランダムで置く。
//  (道の上に置きたいので、適当な座標ではなく WayPoint を基準にしている)
//==============================================================================
using UnityEngine;

public static class TownCharacterPlacement {
    // 同じ WayPoint に複数体が重ならないよう、少しだけばらけさせる幅(仮)
    private const float ScatterX = 0.6f;
    private const float ScatterY = 0.4f;

    /// <summary>前回の場所があればそこへ、無ければランダムな場所へ置く</summary>
    public static void Place(Character character) {
        if (character == null) return;

        string id = character.CharacterId;
        if (string.IsNullOrEmpty(id)) return;

        Vector3 saved;
        if (TownSaveBridge.TryGetPosition(id, out saved)) {
            saved.z = character.transform.position.z;
            character.transform.position = saved;
            return;
        }

        character.transform.position = RandomSpot(character.transform.position.z);
    }

    /// <summary>WayPoint のどれかを選んで、その周りに少しばらして座標を返す</summary>
    public static Vector3 RandomSpot(float z = 0f) {
        var points = Object.FindObjectsByType<WayPoint>(FindObjectsSortMode.None);
        if (points == null || points.Length == 0) return new Vector3(0f, 0f, z);

        var point = points[Random.Range(0, points.Length)];

        return new Vector3(
            point.Position.x + Random.Range(-ScatterX, ScatterX),
            point.Position.y + Random.Range(-ScatterY, ScatterY),
            z);
    }
}
