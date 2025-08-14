using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 編成を保持するマネージャ
/// DontDestroyOnLoadで画面遷移を跨いで保持
/// </summary>
public class RosterManager : MonoBehaviour
{
    /// <summary>シングルトン参照</summary>
    public static RosterManager Instance { get; private set; }

    /// <summary>現在の編成</summary>
    public List<UnitLoadout> roster = new List<UnitLoadout>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 編成にユニットを追加
    /// </summary>
    public void Add(UnitLoadout loadout)
    {
        if (loadout == null) return;
        roster.Add(loadout);
    }

    /// <summary>
    /// 編成からユニットを削除
    /// </summary>
    public void Remove(UnitLoadout loadout)
    {
        if (loadout == null) return;
        roster.Remove(loadout);
    }

    /// <summary>
    /// 指定陣営のユニット列を返す
    /// </summary>
    public IEnumerable<UnitLoadout> GetByFaction(int factionId)
    {
        foreach (var l in roster) if (l.factionId == factionId) yield return l;
    }
}
