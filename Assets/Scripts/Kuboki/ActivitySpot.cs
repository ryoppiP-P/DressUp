//==============================================================================
//  File   : ActivitySpot.cs
//  Brief  : 建物の中の「ここで、このモーションを、この秒数だけやる」場所
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/10/11
//------------------------------------------------------------------------------
//  椅子や座布団など、キャラが立ち寄って座る場所に置く。WayPoint と同じ
//  GameObject に付け、通路の WayPoint と manualNeighbors でつないでおく
//  (経路探索はこの場所の WayPoint をゴールにして歩いてくる)。
//
//  建物(Building.activitySpots)に登録すると、その建物に来たキャラが
//  order の小さい順に回る。同じ order の場所が複数あれば、空いている所から1つ選ぶ
//  (例: 1階の椅子4つ = order 0、2階の座布団 = order 1)。
//==============================================================================
using System.Collections.Generic;
using UnityEngine;

public class ActivitySpot : MonoBehaviour {
    [System.Serializable]
    public struct Step {
        public CharaState state;   // 再生するモーション
        public float seconds;      // 何秒続けるか
    }

    public enum Facing { Keep, Left, Right }

    [Header("建物の中で回る順番(同じ番号の中から空いている1つを選ぶ)")]
    [SerializeField] private int order = 0;

    [Header("ここでやること(上から順に再生する)")]
    [SerializeField] private List<Step> steps = new List<Step> {
        new Step { state = CharaState.Sit_Start, seconds = 2.2f },   // 座る
        new Step { state = CharaState.Sit, seconds = 15f },          // 座っている
        new Step { state = CharaState.Sit_End, seconds = 2.2f },     // 立ち上がる
    };

    [Header("座っている間の向き(Keep = 歩いてきた向きのまま)")]
    [SerializeField] private Facing facing = Facing.Keep;

    [Header("座った時の位置の微調整(ワールド座標。モーションの絵に合わせて調整する)")]
    [SerializeField] private Vector2 seatOffset = Vector2.zero;

    public int Order => order;
    public IReadOnlyList<Step> Steps => steps;
    public Facing FaceTo => facing;
    public Vector2 SeatOffset => seatOffset;
    public Vector3 Position => transform.position;

    /// <summary>今この場所を使っている(向かっている)キャラ。空いていれば null</summary>
    public CharacterManager Occupant { get; private set; }

    public bool IsFree => Occupant == null;

    public bool TryReserve(CharacterManager who) {
        if (Occupant != null && Occupant != who) return false;
        Occupant = who;
        return true;
    }

    public void Release(CharacterManager who) {
        if (Occupant == who) Occupant = null;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos() {
        Gizmos.color = order == 0 ? new Color(1f, 0.3f, 0.3f) : new Color(1f, 0.6f, 0.1f);
        Gizmos.DrawWireSphere(transform.position, 0.4f);
    }
#endif
}
