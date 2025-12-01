using UnityEngine;

/// <summary>
/// 障害物を簡単に配置するためのテスト用スクリプト
/// エディタまたは実行時に使用可能
/// </summary>
public class ObstaclePlacer : MonoBehaviour
{
    [Header("配置設定")]
    [Tooltip("配置する障害物データ")]
    public ObstacleData obstacleData;

    [Tooltip("配置するグリッド座標")]
    public Vector3Int gridPosition = new Vector3Int(0, 0, 0);

    [Header("テスト用")]
    [Tooltip("実行時に自動配置するか")]
    public bool placeOnStart = false;

    void Start()
    {
        if (placeOnStart)
        {
            PlaceObstacle();
        }
    }

    /// <summary>
    /// 障害物を配置する（エディタまたは実行時）
    /// </summary>
    [ContextMenu("障害物を配置")]
    public void PlaceObstacle()
    {
        if (obstacleData == null)
        {
            Debug.LogError("ObstacleDataが設定されていません");
            return;
        }

        var mapManager = MapManager.Instance;
        if (mapManager == null)
        {
            Debug.LogError("MapManagerが見つかりません");
            return;
        }

        mapManager.PlaceObstacle(gridPosition, obstacleData);
        Debug.Log($"障害物を配置しました: {gridPosition}, データ: {obstacleData.displayName}");
    }

    /// <summary>
    /// 指定座標の障害物を削除する
    /// </summary>
    [ContextMenu("障害物を削除")]
    public void RemoveObstacle()
    {
        var mapManager = MapManager.Instance;
        if (mapManager == null)
        {
            Debug.LogError("MapManagerが見つかりません");
            return;
        }

        mapManager.RemoveObstacle(gridPosition);
        Debug.Log($"障害物を削除しました: {gridPosition}");
    }
}

