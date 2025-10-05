using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 右クリックで選択中ユニットへ移動命令を送るコントローラ
/// 複数選択時はフォーメーション風に周辺セルへ分散配置
/// </summary>
public class CommandController : MonoBehaviour
{
    [Header("Formation")]
    public int ringCount = 3;          // 外周の層数
    public int ringStep = 6;           // 各層に置く候補数
    public bool avoidBlocked = true;   // 壁セルは避ける

    [Header("Raycast")]
    public LayerMask unitMask = ~0;     // Unitsレイヤーなどを指定
    public float rayMaxDist = 100f;     // 画面奥行き十分な距離

    /// <summary>
    /// ユニット上での右クリック時に発火するイベント
    /// 外部で別処理を行いたい場合に購読
    /// </summary>
    public event System.Action<UnitCore> OnRightClickUnit;

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
            IssueMoveCommand();
    }

    void IssueMoveCommand()
    {
        // UI上は無視
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        // 追加 ユニットに当たっているかを優先判定
        var cam = Camera.main;
        var mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);

        var hit = Physics2D.Raycast(mouseWorld, Vector2.zero, rayMaxDist, unitMask);
        if (hit.collider != null)
        {
            var core = hit.collider.GetComponentInParent<UnitCore>();
            if (core != null)
            {
                // フックを呼んで移動はしない
                OnRightClickUnit?.Invoke(core);
                return;
            }
        }

        var mm = MapManager.Instance;
        if (mm == null) return;

        var world = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var centerCell = mm.WorldToCell(world);

        var sels = SelectionManager.Instance?.Current;
        if (sels == null || sels.Count == 0) return;

        // ローカル予約
        var reserved = new HashSet<Vector3Int>();

        foreach (var s in sels)
        {
            if (s == null) continue;
            var path = s.GetComponent<UnitPathAgent>();
            var grid = s.GetComponent<GridAgent>();
            if (path == null || grid == null) continue;

            var myCell = grid.Cell;
            var goal = FindBestGoalForUnit(centerCell, myCell, mm, reserved, s.GetComponent<UnitCore>());

            reserved.Add(goal);
            path.SetDestination(goal);
        }
    }

    /// <summary>
    /// 単一ユニットに最適な目標セルを選ぶ
    /// クリック中心から半径を広げ候補を列挙し
    /// 通行可 未予約 未占有の中から ユニット現在地に最も近いセルを返す
    /// </summary>
    Vector3Int FindBestGoalForUnit(
        Vector3Int center,
        Vector3Int myCell,
        MapManager mm,
        HashSet<Vector3Int> reserved,
        UnitCore self)
    {
        // まず中心が有効なら即採用
        if (IsCellFree(center, mm, reserved, self))
            return center;

        // 半径を広げながら候補収集
        const int maxRadius = 6; // 必要なら調整
        Vector3Int best = center;
        int bestScore = int.MaxValue;

        for (int r = 1; r <= maxRadius; r++)
        {
            // 外周だけ走査
            for (int dx = -r; dx <= r; dx++)
            {
                int x = center.x + dx;
                int y1 = center.y + r;
                int y2 = center.y - r;

                var c1 = new Vector3Int(x, y1, 0);
                var c2 = new Vector3Int(x, y2, 0);

                TryUpdateBest(c1);
                TryUpdateBest(c2);
            }
            for (int dy = -r + 1; dy <= r - 1; dy++)
            {
                int y = center.y + dy;
                int x1 = center.x + r;
                int x2 = center.x - r;

                var c1 = new Vector3Int(x1, y, 0);
                var c2 = new Vector3Int(x2, y, 0);

                TryUpdateBest(c1);
                TryUpdateBest(c2);
            }

            if (bestScore != int.MaxValue) break; // 最初に見つかった最適距離で確定
        }

        return (bestScore == int.MaxValue) ? center : best;

        void TryUpdateBest(Vector3Int c)
        {
            if (!IsCellFree(c, mm, reserved, self)) return;
            int score = UnitDirectory.SqrCellDistance(myCell, c); // 現在地からの距離指標
            if (score < bestScore) { bestScore = score; best = c; }
        }
    }

    /// <summary>
    /// そのセルが通行可 未予約 未占有かを判定
    /// selfが占有しているセルは占有扱いから除外
    /// </summary>
    bool IsCellFree(Vector3Int cell, MapManager mm, HashSet<Vector3Int> reserved, UnitCore self)
    {
        if (!mm.IsInsideBounds(cell)) return false;
        if (!mm.IsPassable(cell)) return false;
        if (reserved != null && reserved.Contains(cell)) return false;

        // 占有確認
        var dir = UnitDirectory.Instance;
        if (dir != null && dir.IsCellOccupied(cell))
        {
            foreach (var u in dir.GetOccupants(cell))
            {
                if (u == null) continue;
                if (u == self) return true; // 自分自身なら許可
                return false;
            }
        }
        return true;
    }

    List<Vector3Int> BuildGoalCells(Vector3Int center, int count)
    {
        var list = new List<Vector3Int>(count + 8);
        list.Add(center);

        int placed = 1;
        for (int r = 1; placed < count && r <= ringCount; r++)
        {
            for (int k = 0; k < ringStep; k++)
            {
                float t = (k / (float)ringStep) * Mathf.PI * 2f;
                int dx = Mathf.RoundToInt(Mathf.Cos(t) * r);
                int dy = Mathf.RoundToInt(Mathf.Sin(t) * r);
                var c = new Vector3Int(center.x + dx, center.y + dy, 0);
                if (!list.Contains(c)) { list.Add(c); placed++; if (placed >= count) break; }
            }
        }
        return list;
    }

    Vector3Int FindAssignable(List<Vector3Int> goals, int startIndex, MapManager mm, HashSet<Vector3Int> reserved)
    {
        // まず候補リスト内で未予約かつ通行可を探す
        for (int offset = 0; offset < goals.Count; offset++)
        {
            int idx = (startIndex + offset) % goals.Count;
            var g = goals[idx];
            if (!mm.IsInsideBounds(g)) continue;
            if (!mm.IsPassable(g)) continue;
            if (reserved != null && reserved.Contains(g)) continue;
            return g;
        }

        // すべて埋まっているか壁なら周囲を小さく広げて探索
        var origin = goals[Mathf.Clamp(startIndex, 0, goals.Count - 1)];
        for (int r = 1; r <= 4; r++) // 小さな半径で十分
        {
            for (int dx = -r; dx <= r; dx++)
            for (int dy = -r; dy <= r; dy++)
            {
                if (Mathf.Abs(dx) != r && Mathf.Abs(dy) != r) continue; // 外周のみ
                var c = new Vector3Int(origin.x + dx, origin.y + dy, 0);
                if (!mm.IsInsideBounds(c)) continue;
                if (!mm.IsPassable(c)) continue;
                if (reserved != null && reserved.Contains(c)) continue;
                return c;
            }
        }
        // どうしても無ければ元の候補を返す
        return origin;
    }

}
