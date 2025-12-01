using UnityEngine;

/// <summary>
/// 障害物情報ウィンドウのインターフェース
/// 
/// このインターフェースを実装することで、HoverInfoUIManagerから
/// 障害物情報を表示するPrefabウィンドウを作成できます。
/// </summary>
public interface IObstacleInfoWindow
{
    /// <summary>
    /// Obstacleへの参照を設定する
    /// HoverInfoUIManagerから呼び出され、Prefab側で障害物情報にアクセスできるようになる
    /// </summary>
    /// <param name="obstacle">対象のObstacle</param>
    void SetObstacle(Obstacle obstacle);

    /// <summary>
    /// 情報を更新する
    /// 必要に応じて呼び出され、最新の障害物情報を反映する
    /// </summary>
    void UpdateInfo();
}

