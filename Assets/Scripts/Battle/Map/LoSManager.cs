using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// タイルベースのLoS判定を提供するマネージャ
/// MapManagerと連携しセル単位で遮蔽を評価
/// 結果キャッシュにより再計算を削減
///
/// 使い方例
/// セル座標で判定する場合
///   bool visible = LoSManager.Instance.CanSeeCells(allyCell, enemyCell)
///
/// ワールド座標で判定する場合
///   bool visible = LoSManager.Instance.CanSeeWorld(allyWorldPos, enemyWorldPos)
///
/// 視野角を考慮する場合（fromForwardNorm2は正規化済み2D方向ベクトル）
///   bool visible = LoSManager.Instance.CanSeeWorld(allyWorldPos, enemyWorldPos, allyForward)
///
/// MapManager.OnMapChangedは自動で購読しており地形更新時にキャッシュを破棄
/// 距離や視野角のしきい値はSetMaxRangeCellsやSetFovCosで設定可能
/// </summary>

public class LoSManager : MonoBehaviour
{
    /// <summary>シングルトン参照</summary>
    public static LoSManager Instance { get; private set; }

    [Header("Broad phase options")]
    [Tooltip("セル距離の最大有効範囲 負値で無制限")]
    [SerializeField] private float maxRangeCells = -1f;

    [Tooltip("視野角の下限cos値 例 60度なら0.5 無効化は負の大きい値")]
    [SerializeField] private float fovCos = -2f;

    [Tooltip("終点セルが遮蔽セルでも可とみなすならfalse 通常はtrue")]
    [SerializeField] private bool includeEndCell = true;

    /// <summary>
    /// キャッシュキー FromセルとToセルの組
    /// </summary>
    private struct CacheKey : IEquatable<CacheKey>
    {
        public readonly Vector3Int A;
        public readonly Vector3Int B;

        public CacheKey(Vector3Int a, Vector3Int b)
        {
            A = a;
            B = b;
        }

        public bool Equals(CacheKey other) => A.Equals(other.A) && B.Equals(other.B);
        public override bool Equals(object obj) => obj is CacheKey k && Equals(k);
        public override int GetHashCode() => unchecked(A.GetHashCode() * 486187739 + B.GetHashCode());
    }

    /// <summary>LoS結果キャッシュ trueで可視 falseで遮蔽</summary>
    private readonly Dictionary<CacheKey, bool> _cache = new();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple LoSManager instances detected Destroying this one");
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void OnEnable()
    {
        if (MapManager.Instance != null)
            MapManager.Instance.OnMapChanged += InvalidateAll;
    }

    void OnDisable()
    {
        if (MapManager.Instance != null)
            MapManager.Instance.OnMapChanged -= InvalidateAll;
    }

    /// <summary>
    /// 全キャッシュ無効化
    /// 地形更新時に呼ばれる
    /// </summary>
    public void InvalidateAll()
    {
        _cache.Clear();
    }

    /// <summary>
    /// セル同士のLoSを返す
    /// キャッシュを参照し未計算なら判定して保存
    /// </summary>
    public bool CanSeeCells(Vector3Int fromCell, Vector3Int toCell, Vector2? fromForwardNorm2 = null)
    {
        var key = new CacheKey(fromCell, toCell);
        if (_cache.TryGetValue(key, out var hit))
            return hit;

        if (!BroadPhasePass(fromCell, toCell, fromForwardNorm2))
        {
            _cache[key] = false;
            return false;
        }

        bool ok = TileLineOfSight(fromCell, toCell, includeEndCell);
        _cache[key] = ok;
        return ok;
    }

    /// <summary>
    /// ワールド座標同士のLoSを返す
    /// MapManagerのグリッドに投影して評価
    /// </summary>
    public bool CanSeeWorld(Vector3 fromWorld, Vector3 toWorld, Vector2? fromForwardNorm2 = null)
    {
        var mm = MapManager.Instance;
        var a = mm.WorldToCell(fromWorld);
        var b = mm.WorldToCell(toWorld);
        return CanSeeCells(a, b, fromForwardNorm2);
    }

    /// <summary>
    /// 粗判定を実施
    /// 範囲 角度などで早期に落とす
    /// </summary>
    private bool BroadPhasePass(Vector3Int a, Vector3Int b, Vector2? forwardOpt)
    {
        if (maxRangeCells >= 0f)
        {
            var dx = a.x - b.x;
            var dy = a.y - b.y;
            if ((dx * dx + dy * dy) > maxRangeCells * maxRangeCells) return false;
        }

        if (forwardOpt.HasValue && fovCos > -1.5f)
        {
            // セル中心ベクトルで視野角評価
            var mm = MapManager.Instance;
            var aw = mm.CellToWorldCenter(a);
            var bw = mm.CellToWorldCenter(b);
            var dir = (bw - aw);
            dir.z = 0f;
            var dv = new Vector2(dir.x, dir.y);
            if (dv.sqrMagnitude < 1e-6f) return true;

            var v = dv.normalized;
            var f = forwardOpt.Value.normalized;
            if (Vector2.Dot(f, v) < fovCos) return false;
        }

        return true;
    }

    /// <summary>
    /// タイルベースの本判定を実施
    /// スーパーカバー線で通過セルを列挙し壁セルの存在を確認
    /// </summary>
    private bool TileLineOfSight(Vector3Int fromCell, Vector3Int toCell, bool includeEnd)
    {
        var mm = MapManager.Instance;
        foreach (var cell in SupercoverLine(fromCell, toCell))
        {
            if (cell == fromCell) continue;
            if (!includeEnd && cell == toCell) continue;

            if (mm.IsBlocked(cell)) return false;
        }
        return true;
    }

    /// <summary>
    /// スーパーカバー線を列挙
    /// 斜め移動時は角跨ぎ対策のため隣接直交セルも通過扱い
    /// </summary>
    private IEnumerable<Vector3Int> SupercoverLine(Vector3Int a, Vector3Int b)
    {
        int x0 = a.x, y0 = a.y;
        int x1 = b.x, y1 = b.y;

        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;

        int x = x0, y = y0;
        int err = dx - dy;

        while (true)
        {
            yield return new Vector3Int(x, y, 0);
            if (x == x1 && y == y1) break;

            int e2 = err * 2;
            int px = x, py = y;

            if (e2 > -dy)
            {
                err -= dy;
                x += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                y += sy;
            }

            // 斜め更新時の角抜け対策
            if (x != px && y != py)
            {
                yield return new Vector3Int(x, py, 0);
                yield return new Vector3Int(px, y, 0);
            }
        }
    }

    /// <summary>
    /// 視野角しきい値を設定
    /// 例 60度ならMathf.Cos(60f * Mathf.Deg2Rad)を指定
    /// 無効化は負の大きい値を指定
    /// </summary>
    public void SetFovCos(float cosThreshold)
    {
        fovCos = cosThreshold;
        _cache.Clear();
    }

    /// <summary>
    /// 最大セル距離を設定
    /// 無効化は負値を指定
    /// </summary>
    public void SetMaxRangeCells(float range)
    {
        maxRangeCells = range;
        _cache.Clear();
    }

    /// <summary>
    /// 終点セルの遮蔽を可とするか設定
    /// trueなら終点も遮蔽対象 falseなら終点セルは無視
    /// </summary>
    public void SetIncludeEndCell(bool include)
    {
        includeEndCell = include;
        _cache.Clear();
    }

#if UNITY_EDITOR
    [Header("Debug draw")]
    [SerializeField] private bool debugDrawLast = false;
    private Vector3Int _dbgA, _dbgB;
    private bool _dbgHit;

    /// <summary>
    /// デバッグ用に最後に評価した線を記録
    /// </summary>
    private void RecordDebug(Vector3Int a, Vector3Int b, bool hit)
    {
        if (!debugDrawLast) return;
        _dbgA = a;
        _dbgB = b;
        _dbgHit = hit;
    }

    void OnDrawGizmos()
    {
        if (!debugDrawLast) return;
        var mm = MapManager.Instance;
        if (mm == null) return;

        Gizmos.color = _dbgHit ? Color.green : Color.red;

        Vector3 prev = mm.CellToWorldCenter(_dbgA);
        foreach (var c in SupercoverLine(_dbgA, _dbgB))
        {
            var center = mm.CellToWorldCenter(c);
            Gizmos.DrawLine(prev, center);
            prev = center;
        }
    }
#endif
}
