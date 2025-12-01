using UnityEngine;

/// <summary>
/// 武器アイテムのデータを定義するScriptableObject
/// 
/// このクラスはWeaponStatsSOをラップし、ItemDataSOを継承します。
/// 武器も重量を持つため、インベントリシステムで管理できます。
/// </summary>
[CreateAssetMenu(menuName = "Game/WeaponItem")]
public class WeaponItemSO : ItemDataSO
{
    [Header("武器ステータス")]
    /// <summary>武器のステータスデータ（射撃レート、射程、ダメージなど）</summary>
    [Tooltip("武器のステータスデータ")]
    public WeaponStatsSO weaponStats;
}

