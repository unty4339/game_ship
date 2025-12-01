using UnityEngine;

/// <summary>
/// ユニット情報ウィンドウのインターフェース
/// 
/// このインターフェースを実装することで、UnitInfoUIから呼び出されるPrefabウィンドウを作成できます。
/// Prefab側でこのインターフェースを実装し、SetUnitCore()でUnitCoreを受け取って情報を表示します。
/// </summary>
public interface IUnitInfoWindow
{
    /// <summary>
    /// UnitCoreへの参照を設定する
    /// UnitInfoUIから呼び出され、Prefab側でユニット情報にアクセスできるようになる
    /// </summary>
    /// <param name="unitCore">対象のUnitCore</param>
    void SetUnitCore(UnitCore unitCore);

    /// <summary>
    /// 情報を更新する
    /// 必要に応じて呼び出され、最新のユニット情報を反映する
    /// </summary>
    void UpdateInfo();
}

