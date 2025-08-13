using System;
using UnityEngine;

/// <summary>
/// 編成用のユニット設定データ
/// シーンを跨いで保持され戦闘時に実体化される
/// </summary>
[Serializable]
public class UnitLoadout
{
    /// <summary>ユニットの名称やコールサイン</summary>
    public string nickname;

    /// <summary>所属陣営ID</summary>
    public int factionId;

    /// <summary>使用するユニット種別</summary>
    public UnitArchetypeSO archetype;

    /// <summary>装備スロットA</summary>
    public EquipmentSO equipA;

    /// <summary>装備スロットB</summary>
    public EquipmentSO equipB;

    /// <summary>戦闘開始時のレベル 任意</summary>
    public int level = 1;
}
