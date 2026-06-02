/*
* ファイル名　Building.cs
* タイトル　　建物
* 作成者　　　久保木幹太
* 作成日     6月2日
* 更新日     6月2日
*/

using UnityEngine;
using System.Collections.Generic;

public class Building : MonoBehaviour
{
    // 建物の種類を定義するEnum(仮置き)
    public enum BuildingType
    {
        None,
        House,      // 住宅
        Shop,       // 店
        Office,     // 会社
        School,     // 学校
        Park        // 公園
    }

    // テーマを定義するEnum(仮置き)
    public enum ThemeType
    {
        Default,
        Modern,
        Classic,
        Cyberpunk
    }

    // 今は使わないけど、今後マスとかで座標を管理するときに使う
    [System.Serializable]
    public struct GridPosition
    {
        public int x;
        public int y;
    }

    [System.Serializable]
    public struct ParameterChange
    {
        public string targetParamName; // 変動させたい特徴の名前
        public int changeValue;        // 変動値
    }

    [Header("建物識別・カテゴリ設定")]
    [SerializeField] private string buildingID = "BL_001";
    [SerializeField] private string buildingName = "BL_NONE";
    [SerializeField] private BuildingType buildingType = BuildingType.None;
    [SerializeField] private ThemeType themeID = ThemeType.Default; // まだ使わない

    [Header("キャラクター特徴パラメーターの変動設定")]
    [Tooltip("CharacterManagerに設定した特徴の名前と一致させて")]
    [SerializeField] private List<ParameterChange> statusChange = new List<ParameterChange>();

    [Header("建物の外観・内観設定")]
    [SerializeField] private SpriteRenderer spOut = null;
    [SerializeField] private SpriteRenderer spIn = null;

    [Header("配置・グラフィック設定")]
    [SerializeField] private GridPosition gridPos; // マス間隔の配置（今後実装予定の仮置き）
    [SerializeField] private string outID = "EXT_001";  // 外観設定ID
    [SerializeField] private string inID = "INT_001";   // 内部表示ID

    [Header("trueなら内部テクスチャ、falseなら外観テクスチャを表示")]
    [SerializeField] private bool showInteriorTexture = false;

    [Header("進入・依存関係設定")]
    [SerializeField] private bool isEnter = true;  // bool型で建物に入れるかを判断
    [SerializeField] private bool isAccessible = false; // 依存先の建物が解放されているか
    [Tooltip("この建物が解放されていないと、自身が進入不可になる他の建物リスト")]
    [SerializeField] private List<Building> requiredBuildings = new List<Building>();

    [Header("キャラクターのタグ")]
    [SerializeField] private string targetTag = "NONE";

    [Header("この建物に向かうためのルート")]
    [SerializeField] private List<Vector3> walkRoute = new List<Vector3>();

    // NavMeshAgentから行先として設定する際に参照するプロパティ
    public string BuildingID => buildingID;
    public string BuildingName => buildingName;
    public BuildingType CurrentBuildingType => buildingType;

    // 建物の世界座標
    public Vector3 DestinationPosition => transform.position;

    private void Start()
    {
        SetInteriorView(showInteriorTexture);
    }

    // この建物に進入可能かどうかを判定する
    public bool CanEnter()
    {
        if (!isEnter)
            return false;

        // 依存関係にある建物がすべて 入れる状態 になっているかチェック
        foreach (Building required in requiredBuildings) // 解放されているかのチェック
        {
            if (required != null && !required.IsAccessibleStatus())
            {
                return false;
            }
        }

        return true;
    }

    public void SetAccessible(bool isAccess)
    {
        isAccessible = isAccess;
    }

    // 進入許可状態を返す
    public bool IsAccessibleStatus()
    {
        return isAccessible;
    }

    /// 進入可能状態を切り替える
    public void SetAccessibility(bool status)
    {
        isEnter = status;
    }

    /// テクスチャ切り替えのフラグを変更する
    public void SetInteriorView(bool showInterior)
    {
        showInteriorTexture = showInterior;

        if (spOut != null)
        {
            // 内部
            spOut.gameObject.SetActive(!showInteriorTexture);
        }

        if (spIn != null)
        {
            // 外部
            spIn.gameObject.SetActive(showInteriorTexture);
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        SetInteriorView(showInteriorTexture);
    }

    // インスペクター上でルートを線でつなげて分かりやすくするデバッグ画面
    private void OnDrawGizmosSelected()
    {
        if (walkRoute == null || walkRoute.Count == 0) return;

        Gizmos.color = Color.cyan;

        // リストの一番最後
        int lastIndex = walkRoute.Count - 1;
        Gizmos.DrawSphere(walkRoute[lastIndex], 0.15f);

        Vector3 previousPoint = walkRoute[lastIndex];

        // 最後のポイントに球を表示
        Gizmos.DrawSphere(previousPoint, 0.15f);

        for (int i = lastIndex; i > 0; i--)
        {
            // 新しい要素から古い要素へ向かってシアンの線を引く
            Gizmos.DrawLine(walkRoute[i], walkRoute[i - 1]);
            Gizmos.DrawSphere(walkRoute[i - 1], 0.15f);
        }

        // 最後は建物本体に向けて線でつなぐ
        Gizmos.color = Color.red;
        Gizmos.DrawLine(walkRoute[0], transform.position);
        Gizmos.DrawSphere(transform.position, 0.25f);
    }
#endif

    // トリガー判定
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!CanEnter())
            return;

        if (collision.CompareTag(targetTag))
        {
            CharacterManager character = collision.GetComponent<CharacterManager>();

            if (character != null)
            {
                OnCharacterEnter(character);
            }
        }
    }

    // キャラクターが建物に入った時の処理
    private void OnCharacterEnter(CharacterManager character)
    {
        if (character == null) return;

        foreach (var status in statusChange)
        {
            character.AddData(status.targetParamName, status.changeValue);
        }
    }

    // この建物に向かうためのルート
    public List<Vector3> GetMoveRoute()
    {
        // 新しく追加した末尾から古い方へ逆順に辿るリストを作成する
        List<Vector3> fullRoute = new List<Vector3>();

        // リストの末尾から順番に新しいリストに詰め込んでいく
        for (int i = walkRoute.Count - 1; i >= 0; i--)
        {
            fullRoute.Add(walkRoute[i]);
        }

        // 最後に最終目的地としての建物位置を末尾に追加
        fullRoute.Add(transform.position);
        return fullRoute;
    }
}