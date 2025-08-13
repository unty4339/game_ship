using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// 2つのTilemap（床と壁）を統合管理するマネージャクラス
/// 通行可否や遮蔽判定 タイルの配置と除去 ワールドとセルの座標変換
/// マップ更新バージョンとイベント通知を提供
/// </summary>
public class MapManager : MonoBehaviour
{
    /// <summary>シングルトン参照</summary>
    public static MapManager Instance { get; private set; }

    [Header("Tilemaps")]
    [SerializeField] private Tilemap floorMap;  // 床用
    [SerializeField] private Tilemap wallMap;   // 壁や遮蔽用

    [Header("Options")]
    [Tooltip("通行可能とみなすのに床タイルが必要かどうか falseなら床が無くても通行可")]
    [SerializeField] private bool requireFloorForPassable = true;

    /// <summary>
    /// マップの更新バージョン
    /// 壁や床の構造が変化するたびに加算される
    /// LoSキャッシュの無効化などに使用
    /// </summary>
    public int MapVersion { get; private set; } = 0;

    /// <summary>
    /// マップが変更されたときに発火するイベント
    /// タイルの配置や除去 オプション変更時に通知
    /// </summary>
    public event Action OnMapChanged;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple MapManager instances detected Destroying this one");
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// 指定セルが通行可能かを返す
    /// デフォルトでは壁が無く かつ床が必要な場合は床が存在する場合のみtrue
    /// </summary>
    public bool IsPassable(Vector3Int cell)
    {
        bool hasWall = HasWall(cell);
        if (hasWall) return false;

        if (!requireFloorForPassable) return true;
        return HasFloor(cell);
    }

    /// <summary>
    /// 指定セルが遮蔽セルかを返す
    /// </summary>
    public bool IsBlocked(Vector3Int cell) => HasWall(cell);

    /// <summary>
    /// 床タイルが存在するかを返す
    /// </summary>
    public bool HasFloor(Vector3Int cell) => floorMap != null && floorMap.HasTile(cell);

    /// <summary>
    /// 壁タイルが存在するかを返す
    /// </summary>
    public bool HasWall(Vector3Int cell) => wallMap != null && wallMap.HasTile(cell);

    /// <summary>
    /// ワールド座標からセル座標に変換
    /// 床と壁が同一グリッド前提
    /// </summary>
    public Vector3Int WorldToCell(Vector3 worldPos)
    {
        var tm = floorMap != null ? floorMap : wallMap;
        return tm.WorldToCell(worldPos);
    }

    /// <summary>
    /// セルの中心ワールド座標を返す
    /// </summary>
    public Vector3 CellToWorldCenter(Vector3Int cell)
    {
        var tm = floorMap != null ? floorMap : wallMap;
        return tm.GetCellCenterWorld(cell);
    }

    /// <summary>
    /// 指定セルに壁タイルを配置する
    /// 配置後にMapVersionを更新しイベント通知
    /// </summary>
    public void PlaceWall(Vector3Int cell, TileBase tile)
    {
        if (wallMap == null) return;
        wallMap.SetTile(cell, tile);
        Touch();
    }

    /// <summary>
    /// 指定セルの壁タイルを除去する
    /// 除去後にMapVersionを更新しイベント通知
    /// </summary>
    public void RemoveWall(Vector3Int cell)
    {
        if (wallMap == null) return;
        wallMap.SetTile(cell, null);
        Touch();
    }

    /// <summary>
    /// 指定セルに床タイルを配置する
    /// 配置後にMapVersionを更新しイベント通知
    /// </summary>
    public void PlaceFloor(Vector3Int cell, TileBase tile)
    {
        if (floorMap == null) return;
        floorMap.SetTile(cell, tile);
        Touch();
    }

    /// <summary>
    /// 指定セルの床タイルを除去する
    /// 除去後にMapVersionを更新しイベント通知
    /// </summary>
    public void RemoveFloor(Vector3Int cell)
    {
        if (floorMap == null) return;
        floorMap.SetTile(cell, null);
        Touch();
    }

    /// <summary>
    /// 指定セルの床と壁を両方除去する
    /// 除去後にMapVersionを更新しイベント通知
    /// </summary>
    public void ClearCell(Vector3Int cell)
    {
        if (floorMap != null) floorMap.SetTile(cell, null);
        if (wallMap != null) wallMap.SetTile(cell, null);
        Touch();
    }

    /// <summary>
    /// 上下左右4方向の隣接セルを返す
    /// </summary>
    public IEnumerable<Vector3Int> GetNeighbors4(Vector3Int cell)
    {
        yield return new Vector3Int(cell.x + 1, cell.y, cell.z);
        yield return new Vector3Int(cell.x - 1, cell.y, cell.z);
        yield return new Vector3Int(cell.x, cell.y + 1, cell.z);
        yield return new Vector3Int(cell.x, cell.y - 1, cell.z);
    }

    /// <summary>
    /// 斜めを含む8方向の隣接セルを返す
    /// </summary>
    public IEnumerable<Vector3Int> GetNeighbors8(Vector3Int cell)
    {
        for (int dx = -1; dx <= 1; dx++)
        for (int dy = -1; dy <= 1; dy++)
        {
            if (dx == 0 && dy == 0) continue;
            yield return new Vector3Int(cell.x + dx, cell.y + dy, cell.z);
        }
    }

    /// <summary>
    /// セルが床と壁Tilemapの合成範囲内に含まれるかを返す
    /// </summary>
    public bool IsInsideBounds(Vector3Int cell)
    {
        var b = GetCombinedBoundsInt();
        return cell.x >= b.xMin && cell.x < b.xMax &&
               cell.y >= b.yMin && cell.y < b.yMax;
    }

    /// <summary>
    /// 床と壁TilemapのBoundsIntを統合した領域を返す
    /// </summary>
    public BoundsInt GetCombinedBoundsInt()
    {
        bool hasFloor = floorMap != null;
        bool hasWall = wallMap != null;

        if (!hasFloor && !hasWall) return new BoundsInt(Vector3Int.zero, Vector3Int.zero);

        BoundsInt b = hasFloor ? floorMap.cellBounds : wallMap.cellBounds;
        if (hasWall)
        {
            var w = wallMap.cellBounds;
            int xMin = Math.Min(b.xMin, w.xMin);
            int yMin = Math.Min(b.yMin, w.yMin);
            int xMax = Math.Max(b.xMax, w.xMax);
            int yMax = Math.Max(b.yMax, w.yMax);
            b = new BoundsInt(new Vector3Int(xMin, yMin, 0),
                              new Vector3Int(xMax - xMin, yMax - yMin, 1));
        }
        return b;
    }

    /// <summary>
    /// 壁セルの集合を返す
    /// LoSのキャッシュキー作成などに利用可能
    /// </summary>
    public HashSet<Vector2Int> GetBlockedSet()
    {
        var set = new HashSet<Vector2Int>();
        if (wallMap == null) return set;

        foreach (var pos in EnumerateAllCells(wallMap))
        {
            if (wallMap.HasTile(pos))
                set.Add(new Vector2Int(pos.x, pos.y));
        }
        return set;
    }

    /// <summary>
    /// 指定Tilemapの全セルを走査する
    /// </summary>
    private IEnumerable<Vector3Int> EnumerateAllCells(Tilemap tm)
    {
        var b = tm.cellBounds;
        for (int y = b.yMin; y < b.yMax; y++)
        for (int x = b.xMin; x < b.xMax; x++)
            yield return new Vector3Int(x, y, 0);
    }

    /// <summary>
    /// MapVersionを加算し変更イベントを発火する
    /// </summary>
    private void Touch()
    {
        MapVersion++;
        OnMapChanged?.Invoke();
    }

    /// <summary>
    /// requireFloorForPassableの値を変更する
    /// 値が変わった場合はMapVersionを更新しイベント通知
    /// </summary>
    public void SetRequireFloorForPassable(bool requireFloor)
    {
        if (requireFloorForPassable == requireFloor) return;
        requireFloorForPassable = requireFloor;
        Touch();
    }

#if UNITY_EDITOR
    /// <summary>
    /// シーンビューでのデバッグ表示
    /// 壁セルを枠で描画
    /// </summary>
    void OnDrawGizmosSelected()
    {
        if (floorMap == null && wallMap == null) return;
        var b = GetCombinedBoundsInt();

        Gizmos.matrix = Matrix4x4.identity;
        for (int y = b.yMin; y < b.yMax; y++)
        for (int x = b.xMin; x < b.xMax; x++)
        {
            var cell = new Vector3Int(x, y, 0);
            if (IsBlocked(cell))
            {
                var center = CellToWorldCenter(cell);
                var size = floorMap.layoutGrid.cellSize;
                Gizmos.DrawWireCube(center, new Vector3(size.x, size.y, 0.01f));
            }
        }
    }
#endif
}
