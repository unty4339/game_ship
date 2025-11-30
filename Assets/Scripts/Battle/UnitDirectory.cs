using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ユニットの索引管理クラス
/// シングルトンとして全ユニットを登録し陣営別とセル空間で検索を提供
/// 近傍探索のためセル単位の空間ハッシュを保持
///
/// 使い方例
///   UnitDirectory.Instance.Register(unitCore)                    // 生成時に登録
///   UnitDirectory.Instance.Unregister(unitCore)                  // 破棄時に解除
///   UnitDirectory.Instance.UpdateUnitCell(unitCore, oldC, newC)  // セル移動時に呼ぶ
///   var enemies = UnitDirectory.Instance.GetEnemiesOf(faction)   // 敵ユニット列を取得
///   var e = UnitDirectory.Instance.FindNearestEnemy(myCore, 12)  // 射程セル12で最も近い敵
/// </summary>
public class UnitDirectory : SingletonMonoBehaviour<UnitDirectory>
{
    /// <summary>全ユニット集合</summary>
    private readonly HashSet<UnitCore> _all = new HashSet<UnitCore>();

    /// <summary>陣営IDごとの集合</summary>
    private readonly Dictionary<int, HashSet<UnitCore>> _byFaction = new Dictionary<int, HashSet<UnitCore>>();

    /// <summary>セル座標ごとの集合 近傍検索の高速化に使用</summary>
    private readonly Dictionary<Vector3Int, HashSet<UnitCore>> _cellIndex = new Dictionary<Vector3Int, HashSet<UnitCore>>();

    /// <summary>
    /// ユニットを登録
    /// 陣営リストとセルインデックスに追加
    /// </summary>
    public void Register(UnitCore unit)
    {
        if (unit == null) return;
        if (_all.Contains(unit)) return;

        _all.Add(unit);

        // 陣営
        int fid = unit.Faction != null ? unit.Faction.FactionId : 0;
        if (!_byFaction.TryGetValue(fid, out var setF)) { setF = new HashSet<UnitCore>(); _byFaction[fid] = setF; }
        setF.Add(unit);

        // セル
        var cell = unit.Grid != null ? unit.Grid.Cell : MapManager.Instance.WorldToCell(unit.transform.position);
        AddToCellIndex(unit, cell);
    }

    /// <summary>
    /// ユニットを解除
    /// すべての索引から除去
    /// </summary>
    public void Unregister(UnitCore unit)
    {
        if (unit == null) return;
        if (!_all.Remove(unit)) return;

        int fid = unit.Faction != null ? unit.Faction.FactionId : 0;
        if (_byFaction.TryGetValue(fid, out var setF)) setF.Remove(unit);

        var cell = unit.Grid != null ? unit.Grid.Cell : MapManager.Instance.WorldToCell(unit.transform.position);
        RemoveFromCellIndex(unit, cell);
    }

    /// <summary>
    /// セル移動の通知
    /// oldCellから削除しnewCellへ追加
    /// </summary>
    public void UpdateUnitCell(UnitCore unit, Vector3Int oldCell, Vector3Int newCell)
    {
        if (unit == null) return;
        if (oldCell == newCell) return;
        RemoveFromCellIndex(unit, oldCell);
        AddToCellIndex(unit, newCell);
    }

    /// <summary>
    /// 全ユニット列を返す 読み取り専用用途
    /// </summary>
    public IReadOnlyCollection<UnitCore> GetAll() => _all;

    /// <summary>
    /// 指定陣営IDのユニット列を返す
    /// </summary>
    public IEnumerable<UnitCore> GetByFaction(int factionId)
    {
        if (_byFaction.TryGetValue(factionId, out var set)) return set;
        return Array.Empty<UnitCore>();
    }

    /// <summary>
    /// 指定陣営の敵ユニット列を返す
    /// </summary>
    public IEnumerable<UnitCore> GetEnemiesOf(FactionTag me)
    {
        if (me == null) yield break;
        foreach (var kv in _byFaction)
        {
            if (kv.Key == me.FactionId) continue;
            foreach (var u in kv.Value) yield return u;
        }
    }

    /// <summary>
    /// 指定セルに存在するユニットを返す
    /// </summary>
    public IEnumerable<UnitCore> GetUnitsInCell(Vector3Int cell)
    {
        if (_cellIndex.TryGetValue(cell, out var set))
            foreach (var u in set) yield return u;
    }

    /// <summary>
    /// セル半径以内のユニットを列挙
    /// 正方領域を走査し存在セルのみ返す
    /// </summary>
    public IEnumerable<UnitCore> QueryByCellRadius(Vector3Int center, int radius)
    {
        if (radius < 0) yield break;
        for (int y = center.y - radius; y <= center.y + radius; y++)
        for (int x = center.x - radius; x <= center.x + radius; x++)
        {
            var c = new Vector3Int(x, y, 0);
            if (_cellIndex.TryGetValue(c, out var set))
                foreach (var u in set) yield return u;
        }
    }

    /// <summary>
    /// 指定ユニットから最も近い敵を検索
    /// maxRangeCellsが負なら無制限
    /// </summary>
    public UnitCore FindNearestEnemy(UnitCore seeker, float maxRangeCells)
    {
        if (seeker == null) return null;

        var myFaction = seeker.Faction != null ? seeker.Faction.FactionId : 0;
        var myCell = seeker.Grid != null ? seeker.Grid.Cell : MapManager.Instance.WorldToCell(seeker.transform.position);

        float bestSqr = float.PositiveInfinity;
        UnitCore best = null;

        foreach (var kv in _byFaction)
        {
            if (kv.Key == myFaction) continue;
            foreach (var e in kv.Value)
            {
                var ec = e.Grid != null ? e.Grid.Cell : MapManager.Instance.WorldToCell(e.transform.position);
                int d2 = SqrCellDistance(myCell, ec);
                if (maxRangeCells >= 0 && d2 > maxRangeCells * maxRangeCells) continue;
                if (d2 < bestSqr) { bestSqr = d2; best = e; }
            }
        }
        return best;
    }

    /// <summary>
    /// セル距離の二乗を返す
    /// </summary>
    public static int SqrCellDistance(Vector3Int a, Vector3Int b)
    {
        int dx = a.x - b.x;
        int dy = a.y - b.y;
        return dx * dx + dy * dy;
    }

    void AddToCellIndex(UnitCore u, Vector3Int cell)
    {
        if (!_cellIndex.TryGetValue(cell, out var set))
        {
            set = new HashSet<UnitCore>();
            _cellIndex[cell] = set;
        }
        set.Add(u);
    }

    void RemoveFromCellIndex(UnitCore u, Vector3Int cell)
    {
        if (_cellIndex.TryGetValue(cell, out var set))
        {
            set.Remove(u);
            if (set.Count == 0) _cellIndex.Remove(cell);
        }
    }

    /// <summary>
    /// 指定セルが誰かに占有されているかを返す
    /// </summary>
    public bool IsCellOccupied(Vector3Int cell)
    {
        return _cellIndex.TryGetValue(cell, out var set) && set != null && set.Count > 0;
    }

    /// <summary>
    /// 指定セルを占有しているユニットを返す 列挙用
    /// </summary>
    public IEnumerable<UnitCore> GetOccupants(Vector3Int cell)
    {
        if (_cellIndex.TryGetValue(cell, out var set))
            foreach (var u in set) yield return u;
    }
}
