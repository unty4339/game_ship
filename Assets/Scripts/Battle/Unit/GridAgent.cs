using UnityEngine;

/// <summary>
/// セル座標を管理しMapManagerと同期
/// ワールドとセルの相互変換と占有通知を担当
/// </summary>
public class GridAgent : MonoBehaviour
{
    public Vector3Int Cell { get; private set; }

    public void SnapToGrid()
    {
        var mm = MapManager.Instance;
        Cell = mm.WorldToCell(transform.position);
        transform.position = mm.CellToWorldCenter(Cell);
    }

    /// <summary>
    /// セルの占有を試みる
    /// 通行可でありUnitDirectoryの占有が無い場合のみ確定
    /// 確定時はUnitDirectoryへ変更通知を送る
    /// </summary>
    public bool TryReserve(Vector3Int nextCell)
    {
        Debug.Log($"TryReserve: {nextCell}");
        var mm = MapManager.Instance;
        if (!mm.IsInsideBounds(nextCell) || !mm.IsPassable(nextCell)) return false;

        // 追加 占有チェック
        var dir = UnitDirectory.Instance;
        if (dir != null)
        {
            foreach (var u in dir.GetOccupants(nextCell))
            {
                // 自分以外がいたら予約不可
                if (u != null && u.gameObject != this.gameObject)
                    return false;
            }
        }

        // ここまで来たら確定
        var old = Cell;
        Cell = nextCell;

        // 追加 インデックス更新
        if (dir != null)
        {
            dir.UpdateUnitCell(GetComponent<UnitCore>(), old, nextCell);
        }
        Debug.Log($"TryReserve: {nextCell} success");
        return true;
    }
}
