using UnityEngine;

/// <summary>
/// 射撃ターゲットの選択ルールを管理
/// 仕様
/// 1 ターゲット不在で敵に射線が通るなら最も近い可視の敵を選択
/// 2 ターゲットがいる間は射線が通る限り維持
/// 3 ターゲットが撃破されたら非選択に戻す
/// 4 右クリックで優先攻撃ターゲットを指名 射線が通るときのみ選択
/// </summary>
[RequireComponent(typeof(UnitCore))]
[RequireComponent(typeof(GridAgent))]
[RequireComponent(typeof(FactionTag))]
public class UnitTargeting : GameTimeBehaviour
{
    [Header("Tick")]
    [Tooltip("選択評価の更新間隔秒")]
    public float tickSec = 0.15f;

    [Header("Range")]
    [Tooltip("検索上限セル距離 負値で無制限")]
    public float maxSeekRangeCells = -1f;

    public UnitCore Current { get; private set; }
    public UnitCore Priority { get; private set; }

    float _accum;
    UnitCore _core;

    public System.Action<UnitCore> OnTargetChanged;

    void Awake()
    {
        _core = GetComponent<UnitCore>();
    }

    void Start()
    {
        // Awakeで取得できなかった場合の再試行
        if (_core == null)
        {
            _core = GetComponent<UnitCore>();
        }
    }

    public void SetPriorityTarget(UnitCore enemy)
    {
        Priority = enemy;
        // 次ティックで即評価
        _accum = tickSec;
    }

    public void ClearPriority() => Priority = null;

    void Update()
    {
        _accum += dt;
        if (_accum < tickSec) return;
        _accum = 0f;

        TickSelect();
    }

    void TickSelect()
    {
        // UnitCoreが取得できていない場合は処理をスキップ
        if (_core == null) return;

        // 現在ターゲットの存続判定
        if (Current != null)
        {
            if (!IsAlive(Current)) { SetCurrent(null); return; }

            // LoSが通っている間は維持
            if (HasLoSTo(Current)) return;

            // LoSが切れたら解除
            SetCurrent(null);
        }

        // ここから未選択時の取得
        // まず優先ターゲット
        if (Priority != null && IsAlive(Priority) && HasLoSTo(Priority))
        {
            SetCurrent(Priority);
            return;
        }

        // 優先がダメなら最も近い可視の敵
        var best = FindNearestVisibleEnemy();
        SetCurrent(best);
    }

    void SetCurrent(UnitCore next)
    {
        if (Current == next) return;
        Current = next;
        if (Current != null && !IsAlive(Current)) Current = null;
        OnTargetChanged?.Invoke(Current);
    }

    bool IsAlive(UnitCore u)
    {
        return UnitCore.IsAlive(u);
    }

    bool HasLoSTo(UnitCore u)
    {
        if (u == null || _core == null || _core.Grid == null || u.Grid == null) return false;
        // 射程上限はここでは切らず 武器側で別途判定
        return LoSManager.Instance.CanSeeCells(_core.Grid.Cell, u.Grid.Cell);
    }

    UnitCore FindNearestVisibleEnemy()
    {
        var dir = UnitDirectory.Instance;
        if (dir == null || _core == null || _core.Faction == null) return null;

        var myCell = _core.Grid != null ? _core.Grid.Cell : MapManager.Instance.WorldToCell(_core.transform.position);
        float best = float.PositiveInfinity;
        UnitCore bestU = null;

        foreach (var e in dir.GetEnemiesOf(_core.Faction))
        {
            if (!IsAlive(e)) continue;
            if (e.Grid == null) continue;

            int d2 = UnitDirectory.SqrCellDistance(myCell, e.Grid.Cell);
            if (maxSeekRangeCells >= 0 && d2 > maxSeekRangeCells * maxSeekRangeCells) continue;

            if (!LoSManager.Instance.CanSeeCells(myCell, e.Grid.Cell)) continue;

            if (d2 < best) { best = d2; bestU = e; }
        }

        return bestU;
    }
}
