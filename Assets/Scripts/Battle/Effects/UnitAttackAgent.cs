using UnityEngine;

/// <summary>
/// 攻撃命令の状態管理
/// 射程外なら接近 射程内かつLoSありなら発砲 LoS無しなら位置調整
/// </summary>
[RequireComponent(typeof(UnitPathAgent))]
[RequireComponent(typeof(GridAgent))]
[RequireComponent(typeof(WeaponController))]
public class UnitAttackAgent : GameTimeBehaviour
{
    [Header("Attack settings")]
    public float desiredRangeCells = 8f;   // 目標射程
    public float repathInterval = 0.25f;   // 追跡の再経路間隔
    float _repathT;

    UnitCore _target;
    UnitCore _core;
    UnitPathAgent _path;
    GridAgent _grid;
    WeaponController _weapon;

    void Awake()
    {
        _core = GetComponent<UnitCore>();
        _path = _core != null ? _core.Path : GetComponent<UnitPathAgent>();
        _grid = _core != null ? _core.Grid : GetComponent<GridAgent>();
        _weapon = _core != null ? _core.Weapon : GetComponent<WeaponController>();
    }

    public void SetTarget(UnitCore target)
    {
        _target = target;
        _repathT = 0f;
    }

    void Update()
    {
        if (_target == null) return;
        if (!UnitCore.IsAlive(_target)) { _target = null; return; }
        
        var myCell = _grid != null ? _grid.Cell : MapManager.Instance.WorldToCell(_core.transform.position);
        var tgtCell = _target.Grid != null ? _target.Grid.Cell : MapManager.Instance.WorldToCell(_target.transform.position);

        // 射程チェック
        int d2 = UnitDirectory.SqrCellDistance(myCell, tgtCell);
        bool inRange = d2 <= desiredRangeCells * desiredRangeCells;

        // LoSチェック
        bool hasLoS = inRange && LoSManager.Instance.CanSeeCells(myCell, tgtCell);

        // 射撃
        _weapon.SetOverrideTarget(_target);
        _weapon.enabled = true;
        // _weapon.allowAutoFire = hasLoS && inRange;

        // 位置調整
        _repathT += dt;
        if (_repathT >= repathInterval)
        {
            _repathT = 0f;

            if (!inRange)
            {
                // 目標に近づく
                var step = MoveTowardRing(tgtCell, desiredRangeCells);
                _path.SetDestination(step);
            }
            else if (!hasLoS)
            {
                // 近距離で視線が通らないときは少しオフセット
                var alt = FindLateralOffset(myCell, tgtCell);
                _path.SetDestination(alt);
            }
        }
    }

    Vector3Int MoveTowardRing(Vector3Int targetCell, float range)
    {
        // 目標の周辺リングのうち最も近い通行可能セルを選ぶ
        var mm = MapManager.Instance;
        Vector3Int best = targetCell;
        int bestScore = int.MaxValue;
        int r = Mathf.RoundToInt(range);

        for (int dx = -r; dx <= r; dx++)
        for (int dy = -r; dy <= r; dy++)
        {
            if (Mathf.Abs(dx) != r && Mathf.Abs(dy) != r) continue;
            var c = new Vector3Int(targetCell.x + dx, targetCell.y + dy, 0);
            if (!mm.IsInsideBounds(c) || !mm.IsPassable(c)) continue;

            int score = UnitDirectory.SqrCellDistance(_grid.Cell, c);
            if (score < bestScore) { bestScore = score; best = c; }
        }
        return best;
    }

    Vector3Int FindLateralOffset(Vector3Int me, Vector3Int target)
    {
        // ターゲット周辺の1セル外周を左右にスキャンしてLoSが通る最短セルを探す
        var mm = MapManager.Instance;
        Vector3Int best = me;
        int bestScore = int.MaxValue;

        for (int k = -1; k <= 1; k += 2)
        {
            var c = new Vector3Int(target.x + k, target.y, 0);
            Try(c);
            c = new Vector3Int(target.x, target.y + k, 0);
            Try(c);
        }

        return best;

        void Try(Vector3Int c)
        {
            if (!mm.IsInsideBounds(c) || !mm.IsPassable(c)) return;
            if (!LoSManager.Instance.CanSeeCells(c, target)) return;
            int score = UnitDirectory.SqrCellDistance(me, c);
            if (score < bestScore) { bestScore = score; best = c; }
        }
    }
}
