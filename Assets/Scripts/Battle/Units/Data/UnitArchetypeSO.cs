using UnityEngine;

/// <summary>
/// ユニットのアーキタイプ（種別）を定義するScriptableObject
/// 
/// このクラスは以下の情報を保持します：
/// - ユニットの表示名
/// - 生成に使用するプレハブ
/// - 基礎ステータス（HP、ダメージ、射撃レート）
/// 
/// 各ユニットの基本性能を定義し、装備によるボーナスは別途計算されます。
/// </summary>
[CreateAssetMenu(menuName = "Game/UnitArchetype")]
public class UnitArchetypeSO : ScriptableObject
{
    /// <summary>ユニット種別の表示名</summary>
    public string displayName;

    /// <summary>生成に使用する基礎プレハブ（UnitCoreなどを搭載済み前提）</summary>
    public GameObject unitPrefab;

    /// <summary>基礎最大HP</summary>
    public int baseHP = 100;

    /// <summary>基礎ダメージ（互換性のため保持、現在は未使用）</summary>
    public int baseDamage = 20;

    /// <summary>基礎発射レート（毎秒発射数、互換性のため保持、現在は未使用）</summary>
    public float baseFireRate = 3f;
}
