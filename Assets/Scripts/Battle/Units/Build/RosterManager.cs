using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 編成を保持するマネージャ
/// 
/// このクラスは以下の機能を提供します：
/// - ユニット編成の永続化（DontDestroyOnLoad）
/// - シングルトンパターンによるグローバルアクセス
/// - 編成の追加・削除・検索機能
/// - 陣営別のユニット取得
/// 
/// 使用例：
/// - バトル開始前に編成を設定
/// - 戦闘中にユニットの追加・削除
/// - 陣営別のユニット一覧取得
/// </summary>
public class RosterManager : SingletonMonoBehaviour<RosterManager>
{
    /// <summary>
    /// 現在の編成リスト
    /// 全てのユニットの装備・ステータス情報を保持
    /// </summary>
    public List<UnitLoadout> roster = new List<UnitLoadout>();

    protected override bool ShouldPersistAcrossScenes => true;
    protected override bool ShowDuplicateWarning => false;

    /// <summary>
    /// 編成にユニットを追加
    /// </summary>
    /// <param name="loadout">追加するユニットの装備情報</param>
    public void Add(UnitLoadout loadout)
    {
        if (loadout == null) return;
        roster.Add(loadout);
    }

    /// <summary>
    /// 編成からユニットを削除
    /// </summary>
    /// <param name="loadout">削除するユニットの装備情報</param>
    public void Remove(UnitLoadout loadout)
    {
        if (loadout == null) return;
        roster.Remove(loadout);
    }

    /// <summary>
    /// 指定陣営のユニット一覧を取得
    /// </summary>
    /// <param name="factionId">陣営ID（0=味方、1=敵など）</param>
    /// <returns>指定陣営のユニット一覧</returns>
    public IEnumerable<UnitLoadout> GetByFaction(int factionId)
    {
        foreach (var l in roster) 
        {
            if (l.factionId == factionId) 
                yield return l;
        }
    }
}
