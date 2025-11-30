using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 視界の候補ユニットを収集
/// LoSManagerで可視チェックを行いTargetingへ渡す
/// </summary>
public class UnitPerception : GameTimeBehaviour
{
    [SerializeField] float tickSec = 0.2f;
    float _accum;

    public List<UnitCore> VisibleEnemies { get; } = new List<UnitCore>();

    UnitCore _core;

    void Awake()
    {
        _core = GetComponent<UnitCore>();
    }

    void Update()
    {
        _accum += dt;
        if (_accum < tickSec) return;
        _accum = 0f;

        VisibleEnemies.Clear();

        if (_core == null || _core.Faction == null || _core.Grid == null) return;

        // 例 ユニット索引から敵候補を取得
        foreach (var enemy in UnitDirectory.Instance.GetEnemiesOf(_core.Faction))
        {
            if (enemy.Grid == null) continue;
            var a = _core.Grid.Cell;
            var b = enemy.Grid.Cell;
            if (LoSManager.Instance.CanSeeCells(a, b))
                VisibleEnemies.Add(enemy);
        }
    }
}
