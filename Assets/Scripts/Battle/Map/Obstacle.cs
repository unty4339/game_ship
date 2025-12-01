using UnityEngine;

/// <summary>
/// シーン上に配置される障害物の実体クラス
/// グリッド座標を保持し、ダメージを受けて破壊可能
/// </summary>
public class Obstacle : MonoBehaviour
{
    [Header("データ参照")]
    [Tooltip("参照する障害物データ")]
    [SerializeField] private ObstacleData data;

    [Header("状態")]
    [Tooltip("現在の耐久値")]
    [SerializeField] private int currentHP;

    /// <summary>
    /// 自身のグリッド座標
    /// </summary>
    public Vector3Int GridPosition { get; private set; }

    /// <summary>
    /// 参照するデータ
    /// </summary>
    public ObstacleData Data => data;

    /// <summary>
    /// 現在の耐久値
    /// </summary>
    public int CurrentHP => currentHP;

    /// <summary>
    /// 最大耐久値
    /// </summary>
    public int MaxHP => data != null ? data.maxHP : 0;

    /// <summary>
    /// 初期化（MapManagerから呼ばれる）
    /// </summary>
    public void Initialize(Vector3Int gridPosition, ObstacleData obstacleData)
    {
        GridPosition = gridPosition;
        data = obstacleData;

        if (data != null)
        {
            currentHP = data.maxHP;

            // スプライトを設定
            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && data.defaultSprite != null)
            {
                spriteRenderer.sprite = data.defaultSprite;
            }
        }
    }

    /// <summary>
    /// ダメージを受け、HPが0になったら破壊・消滅させる
    /// </summary>
    /// <param name="amount">受けるダメージ量</param>
    public void TakeDamage(int amount)
    {
        if (data == null) return;

        currentHP = Mathf.Max(0, currentHP - amount);

        // HPが0になったら破壊
        if (currentHP <= 0)
        {
            DestroyObstacle();
        }
    }

    /// <summary>
    /// 障害物を破壊し、MapManagerから削除する
    /// </summary>
    private void DestroyObstacle()
    {
        var mm = MapManager.Instance;
        if (mm != null)
        {
            mm.RemoveObstacle(GridPosition);
        }
        else
        {
            // MapManagerが無い場合は直接破壊
            Destroy(gameObject);
        }
    }

    void OnValidate()
    {
        // エディタ上でデータが設定されている場合、スプライトを更新
        if (data != null && data.defaultSprite != null)
        {
            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && spriteRenderer.sprite != data.defaultSprite)
            {
                spriteRenderer.sprite = data.defaultSprite;
            }
        }
    }
}

