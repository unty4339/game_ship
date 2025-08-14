using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// タイルグリッド上のA*パスファインダ
/// MapManagerと連携し通行可否とグリッド境界を参照
/// 4方向と8方向の移動を選択可能 斜め移動の角抜け防止に対応
/// ヒューリスティックはマンハッタン ユークリッド オクタイルから選択
///
/// 使い方例
///   var pf = GetComponent<AStarPathfinder>()
///   List<Vector3Int> cells = pf.FindPath(startCell, goalCell)
///   if (cells != null) unit.MoveAlongPath(cells)
///
/// ワールド座標を経由したい場合
///   var mm = MapManager.Instance
///   var start = mm.WorldToCell(unit.transform.position)
///   var goal = mm.WorldToCell(targetWorld)
///   var path = pf.FindPath(start, goal)
///
/// 8方向移動や角抜け禁止を切り替える場合
///   pf.allowDiagonal = true
///   pf.preventCornerCut = true
///
/// 結果をワールド座標列で得たい場合
///   var worldPath = pf.ToWorldCenters(path)
/// </summary>
public class AStarPathfinder : MonoBehaviour
{
    public enum HeuristicType { Manhattan, Euclidean, Octile }

    [Header("Search options")]
    [Tooltip("斜め移動を許可するか")]
    public bool allowDiagonal = true;

    [Tooltip("斜め移動で角抜けを禁止するか 斜め先の二つの直交隣接のどちらかが壁なら禁止")]
    public bool preventCornerCut = true;

    [Tooltip("ヒューリスティックの種類")]
    public HeuristicType heuristic = HeuristicType.Octile;

    [Tooltip("最大探索ノード数 負値で無制限")]
    public int maxSearchNodes = -1;

    [Tooltip("同コストのときゴールに近い方を優先するタイブレーク係数")]
    public float tieBreakerWeight = 0.001f;

    const float SQRT2 = 1.41421356237f;

    struct Node
    {
        public Vector3Int pos;   // セル座標
        public float g;          // 開始からの実コスト
        public float f;          // g + h
        public int parentIndex;  // 親ノードのインデックス ヒープ外の配列参照用
    }

    // ヒープに入れるレコード
    struct OpenRec : IComparable<OpenRec>
    {
        public int index;    // nodes配列上のインデックス
        public float f;      // 合計コスト
        public float h;      // ヒューリスティックでタイブレーク用

        public int CompareTo(OpenRec other)
        {
            int c = f.CompareTo(other.f);
            if (c != 0) return c;
            // fが同じならhが小さい方優先
            return h.CompareTo(other.h);
        }
    }

    // 最小ヒープ
    class MinHeap
    {
        List<OpenRec> _a = new List<OpenRec>();

        public int Count => _a.Count;

        public void Clear() => _a.Clear();

        public void Push(OpenRec x)
        {
            _a.Add(x);
            SiftUp(_a.Count - 1);
        }

        public OpenRec Pop()
        {
            var root = _a[0];
            int last = _a.Count - 1;
            _a[0] = _a[last];
            _a.RemoveAt(last);
            if (_a.Count > 0) SiftDown(0);
            return root;
        }

        void SiftUp(int i)
        {
            while (i > 0)
            {
                int p = (i - 1) >> 1;
                if (_a[p].CompareTo(_a[i]) <= 0) break;
                (_a[p], _a[i]) = (_a[i], _a[p]);
                i = p;
            }
        }

        void SiftDown(int i)
        {
            int n = _a.Count;
            while (true)
            {
                int l = i * 2 + 1;
                if (l >= n) break;
                int r = l + 1;
                int m = (r < n && _a[r].CompareTo(_a[l]) < 0) ? r : l;
                if (_a[i].CompareTo(_a[m]) <= 0) break;
                (_a[i], _a[m]) = (_a[m], _a[i]);
                i = m;
            }
        }
    }

    MinHeap _open = new MinHeap();

    /// <summary>
    /// A*で開始セルから目標セルへの経路を検索
    /// 見つからない場合はnullを返す
    /// </summary>
    public List<Vector3Int> FindPath(Vector3Int start, Vector3Int goal)
    {
        var mm = MapManager.Instance;
        if (mm == null) { Debug.LogError("MapManager not found"); return null; }
        if (!mm.IsInsideBounds(start) || !mm.IsInsideBounds(goal)) return null;
        if (!mm.IsPassable(start) || !mm.IsPassable(goal)) return null;

        _open.Clear();

        // ノード配列とインデックス検索用ディクショナリ
        var nodes = new List<Node>(256);
        var indexOf = new Dictionary<Vector3Int, int>(256);
        var closed = new HashSet<Vector3Int>();

        // 開始ノードを追加
        var startNode = new Node
        {
            pos = start,
            g = 0f,
            f = Heuristic(start, goal),
            parentIndex = -1
        };
        nodes.Add(startNode);
        indexOf[start] = 0;
        _open.Push(new OpenRec { index = 0, f = startNode.f, h = Heuristic(start, goal) });

        int expanded = 0;

        while (_open.Count > 0)
        {
            if (maxSearchNodes >= 0 && expanded >= maxSearchNodes) return null;

            var curRec = _open.Pop();
            var cur = nodes[curRec.index];

            if (closed.Contains(cur.pos)) continue;
            closed.Add(cur.pos);
            expanded++;

            if (cur.pos == goal)
                return Reconstruct(nodes, curRec.index);

            foreach (var nxt in GetNeighbors(cur.pos))
            {
                if (closed.Contains(nxt)) continue;
                if (!mm.IsInsideBounds(nxt) || !mm.IsPassable(nxt)) continue;
                if (preventCornerCut && IsDiagonalBlocked(cur.pos, nxt)) continue;

                float step = StepCost(cur.pos, nxt);
                float tentativeG = cur.g + step;

                int idx;
                if (!indexOf.TryGetValue(nxt, out idx))
                {
                    // 新規ノード
                    var n = new Node
                    {
                        pos = nxt,
                        g = tentativeG,
                        f = tentativeG + Heuristic(nxt, goal),
                        parentIndex = curRec.index
                    };
                    idx = nodes.Count;
                    nodes.Add(n);
                    indexOf[nxt] = idx;

                    float h = n.f - n.g;
                    // タイブレークでゴールに近い方を優先
                    float tiebreak = h * tieBreakerWeight;
                    _open.Push(new OpenRec { index = idx, f = n.f + tiebreak, h = h });
                }
                else
                {
                    // 既存ノードの改善
                    var n = nodes[idx];
                    if (tentativeG < n.g)
                    {
                        n.g = tentativeG;
                        n.f = tentativeG + Heuristic(n.pos, goal);
                        n.parentIndex = curRec.index;
                        nodes[idx] = n;

                        float h = n.f - n.g;
                        float tiebreak = h * tieBreakerWeight;
                        _open.Push(new OpenRec { index = idx, f = n.f + tiebreak, h = h });
                    }
                }
            }
        }

        return null;
    }

    /// <summary>
    /// 結果のセル列をワールド座標の中心点列に変換
    /// </summary>
    public List<Vector3> ToWorldCenters(List<Vector3Int> cells)
    {
        if (cells == null) return null;
        var mm = MapManager.Instance;
        var list = new List<Vector3>(cells.Count);
        foreach (var c in cells) list.Add(mm.CellToWorldCenter(c));
        return list;
    }

    IEnumerable<Vector3Int> GetNeighbors(Vector3Int cell)
    {
        var mm = MapManager.Instance;
        if (allowDiagonal)
        {
            // 8方向を手動で列挙
            for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                yield return new Vector3Int(cell.x + dx, cell.y + dy, 0);
            }
        }
        else
        {
            foreach (var n in mm.GetNeighbors4(cell))
                yield return n;
        }
    }

    bool IsDiagonalBlocked(Vector3Int a, Vector3Int b)
    {
        // 斜め移動時に角を塞ぐ二つの直交セルがどちらも壁なら禁止
        int dx = b.x - a.x;
        int dy = b.y - a.y;
        if (Mathf.Abs(dx) + Mathf.Abs(dy) != 2) return false;

        var mm = MapManager.Instance;
        var c1 = new Vector3Int(a.x + dx, a.y, 0);
        var c2 = new Vector3Int(a.x, a.y + dy, 0);

        // どちらかが通行可能なら通過許可
        if (mm.IsInsideBounds(c1) && mm.IsPassable(c1)) return false;
        if (mm.IsInsideBounds(c2) && mm.IsPassable(c2)) return false;
        return true;
    }

    float Heuristic(Vector3Int a, Vector3Int b)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);

        switch (heuristic)
        {
            case HeuristicType.Manhattan:
                return dx + dy;
            case HeuristicType.Euclidean:
                return Mathf.Sqrt(dx * dx + dy * dy);
            case HeuristicType.Octile:
            default:
                int dmin = Mathf.Min(dx, dy);
                int dmax = Mathf.Max(dx, dy);
                return dmin * SQRT2 + (dmax - dmin);
        }
    }

    float StepCost(Vector3Int a, Vector3Int b)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);
        if (dx + dy == 2) return SQRT2;
        return 1f;
    }

    List<Vector3Int> Reconstruct(List<Node> nodes, int goalIndex)
    {
        var path = new List<Vector3Int>();
        int i = goalIndex;
        while (i >= 0)
        {
            path.Add(nodes[i].pos);
            i = nodes[i].parentIndex;
        }
        path.Reverse();
        return path;
    }
}
