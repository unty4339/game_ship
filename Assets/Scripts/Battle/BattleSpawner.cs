using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 戦闘開始時に編成をマップへ配置するスポーナ
/// 陣営ごとのスポーンポイント配列を持ち順番に割り当て
/// </summary>
public class BattleSpawner : MonoBehaviour
{
    /// <summary>味方スポーン地点群</summary>
    public List<Transform> allySpawns = new List<Transform>();

    /// <summary>敵スポーン地点群</summary>
    public List<Transform> enemySpawns = new List<Transform>();

    /// <summary>味方陣営ID</summary>
    public int allyFactionId = 0;

    /// <summary>敵陣営ID</summary>
    public int enemyFactionId = 1;

    /// <summary>ユニット生成に使用するファクトリ</summary>
    public UnitFactory factory;

    void Start()
    {
        if (factory == null) factory = FindObjectOfType<UnitFactory>();
        SpawnFaction(allyFactionId, allySpawns);
        SpawnFaction(enemyFactionId, enemySpawns);
    }

    /// <summary>
    /// 指定陣営の編成を指定スポーン地点へ配置
    /// スポーン地点が不足する場合はループ割り当て
    /// </summary>
    void SpawnFaction(int factionId, List<Transform> points)
    {
        if (factory == null || RosterManager.Instance == null) return;
        if (points == null || points.Count == 0) return;

        int i = 0;
        foreach (var loadout in RosterManager.Instance.GetByFaction(factionId))
        {
            var t = points[i % points.Count];
            var pos = t.position;
            factory.Create(loadout, pos);
            i++;
        }
    }
}
