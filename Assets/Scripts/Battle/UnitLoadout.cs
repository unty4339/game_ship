using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 編成用のユニット設定データ
/// 
/// このクラスは以下の情報を保持します：
/// - ユニットの基本情報（名前、陣営、レベル）
/// - ユニットの種類（アーキタイプ）
/// - 装備情報（武器、ヘルメット、スーツ）
/// - バックパック（インベントリ）内のアイテム
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

    [Header("装備スロット")]
    /// <summary>メイン武器（WeaponItemSO）</summary>
    [Tooltip("メイン武器")]
    public WeaponItemSO mainWeapon;

    /// <summary>ヘルメット</summary>
    [Tooltip("ヘルメット")]
    public EquipmentSO helmet;

    /// <summary>スーツ（防具）</summary>
    [Tooltip("スーツ（防具）")]
    public EquipmentSO suit;

    [Header("バックパック（インベントリ）")]
    /// <summary>バックパック内のアイテムリスト</summary>
    [Tooltip("バックパック内のアイテムリスト")]
    public List<ItemDataSO> backpack = new List<ItemDataSO>();

    [Header("後方互換性（旧システム）")]
    /// <summary>装備スロットA（後方互換性のため保持、equipAがHelmetまたはSuitとして解釈される可能性あり）</summary>
    [Tooltip("装備スロットA（後方互換性）")]
    public EquipmentSO equipA;

    /// <summary>装備スロットB（後方互換性のため保持、equipBがHelmetまたはSuitとして解釈される可能性あり）</summary>
    [Tooltip("装備スロットB（後方互換性）")]
    public EquipmentSO equipB;

    /// <summary>戦闘開始時のレベル（任意、将来の拡張用）</summary>
    public int level = 1;

    [Header("Weapon（後方互換性）")]
    /// <summary>武器のステータス（後方互換性のため保持、WeaponItemSOが優先される）</summary>
    [Tooltip("武器のステータス（後方互換性）")]
    public WeaponStatsSO weaponStats;

    /// <summary>命中時に発動する効果（出血、毒など）</summary>
    [Tooltip("命中時に発動する効果")]
    public WeaponEffectsSO onHitEffects;
}
