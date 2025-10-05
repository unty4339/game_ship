using UnityEngine;

/// <summary>
/// 武器のステータスを定義するScriptableObject
/// 
/// このクラスは以下の武器性能を定義します：
/// - 射撃性能（射撃レート、射程距離）
/// - 命中・ダメージ性能（命中率、ダメージ範囲、クリティカル）
/// 
/// 各武器の基本性能を定義し、WeaponControllerで使用されます。
/// </summary>
[CreateAssetMenu(menuName = "Game/WeaponStats")]
public class WeaponStatsSO : ScriptableObject
{
    [Header("Firing")]
    /// <summary>毎秒発射数</summary>
    [Tooltip("毎秒発射数")]
    public float fireRate = 4f;
    /// <summary>セル単位の射程距離</summary>
    [Tooltip("セル単位の射程")]
    public float rangeCells = 12f;

    [Header("Accuracy & Damage")]
    /// <summary>基礎命中率（0-1）</summary>
    [Range(0f, 1f)] public float baseAccuracy = 0.7f;
    /// <summary>最小ダメージ</summary>
    public int damageMin = 5;
    /// <summary>最大ダメージ</summary>
    public int damageMax = 12;
    /// <summary>クリティカル発生率（0-1）</summary>
    [Range(0f, 1f)] public float critChance = 0.1f;
    /// <summary>クリティカル倍率</summary>
    public float critMultiplier = 1.5f;
}
