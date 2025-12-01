using UnityEngine;

/// <summary>
/// 障害物のデータ定義（ScriptableObject）
/// 土嚢や岩などの障害物の基本パラメータを定義
/// </summary>
[CreateAssetMenu(fileName = "New Obstacle Data", menuName = "Battle/Obstacle Data")]
public class ObstacleData : ScriptableObject
{
    [Header("基本情報")]
    [Tooltip("表示名")]
    public string displayName = "障害物";

    [Tooltip("基本スプライト")]
    public Sprite defaultSprite;

    [Header("耐久値")]
    [Tooltip("最大耐久値")]
    public int maxHP = 100;

    [Header("通行設定")]
    [Tooltip("通行可能か（falseなら完全な壁扱い）")]
    public bool isWalkable = true;

    [Tooltip("通行コスト倍率（例: 1.0=通常, 2.0=2倍の時間がかかる）")]
    [Range(0.1f, 10f)]
    public float walkCostMultiplier = 1.0f;

    [Header("遮蔽効果")]
    [Tooltip("遮蔽率 (0.0f ~ 1.0f)")]
    [Range(0f, 1f)]
    public float coverEffectiveness = 0.5f;

    [Header("配置設定")]
    [Tooltip("上に留まれるか（射撃位置として使えるか）")]
    public bool canStandOn = false;

    [Header("プレハブ")]
    [Tooltip("障害物のプレハブ（Obstacleコンポーネントがアタッチされている必要があります）")]
    public GameObject obstaclePrefab;
}
