using UnityEngine;

/// <summary>
/// 情報表示可能なオブジェクトのインターフェース
/// 
/// このインターフェースを実装することで、HoverInfoUIManagerが
/// マウスオーバー時に情報ウィンドウを表示できます。
/// 
/// 実装例：
/// - UnitCore: ユニット情報を表示
/// - Obstacle: 障害物情報を表示
/// </summary>
public interface IInfoDisplayable
{
    /// <summary>
    /// 表示名を取得
    /// </summary>
    string GetDisplayName();

    /// <summary>
    /// 情報表示用のキー（どのPrefabを使うかを判定）
    /// 例: "Unit", "Obstacle"など
    /// </summary>
    string GetInfoKey();
}

