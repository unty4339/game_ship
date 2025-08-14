using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 視界の候補ユニットを収集
/// LoSManagerで可視チェックを行いTargetingへ渡す
/// </summary>
public class UnitPerception : MonoBehaviour
{
    [SerializeField] float tickSec = 0.2f;
    float _accum;

    public List<UnitCore> VisibleEnemies { get; } = new List<UnitCore>();

    void Update()
    {
        _accum += Time.deltaTime;
        if (_accum < tickSec) return;
        _accum = 0f;

        VisibleEnemies.Clear();

        // 例 ユニット索引から敵候補を取得
        foreach (var enemy in UnitDirectory.Instance.GetEnemiesOf(GetComponent<FactionTag>()))
        {
            var a = GetComponent<GridAgent>().Cell;
            var b = enemy.Grid.Cell;
            if (LoSManager.Instance.CanSeeCells(a, b))
                VisibleEnemies.Add(enemy);
        }
    }
}
