using System;
using UnityEngine;

/// <summary>
/// 編成用のユニット設定データ
/// 
/// このクラスは以下の情報を保持します：
/// - ユニットの基本情報（名前、陣営、レベル）
/// - ユニットの種類（アーキタイプ）
/// - 装備情報（装備A、装備B）
/// - 武器情報（武器ステータス、命中時効果）
/// 
/// シーンを跨いで保持され、戦闘時にUnitFactoryによって実際のGameObjectに変換されます。
/// </summary>
[Serializable]
public class UnitLoadout
{
    /// <summary>ユニットの名称やコールサイン</summary>
    public string nickname;

    /// <summary>所属陣営ID（0=味方、1=敵など）</summary>
    public int factionId;

    /// <summary>使用するユニット種別（アーキタイプ）</summary>
    public UnitArchetypeSO archetype;

    /// <summary>装備スロットA（HP、ダメージ、射撃レートのボーナス）</summary>
    public EquipmentSO equipA;

    /// <summary>装備スロットB（HP、ダメージ、射撃レートのボーナス）</summary>
    public EquipmentSO equipB;

    /// <summary>戦闘開始時のレベル（任意、将来の拡張用）</summary>
    public int level = 1;

    [Header("Weapon")]
    /// <summary>武器のステータス（射撃レート、射程、ダメージ、命中率など）</summary>
    public WeaponStatsSO weaponStats;

    /// <summary>命中時に発動する効果（出血、毒など）</summary>
    public WeaponEffectsSO onHitEffects;
}
