using UnityEngine;

/// <summary>
/// 消費アイテムのデータを定義するScriptableObject
/// 
/// このクラスは以下の消費アイテムを定義します：
/// - 回復薬
/// - グレネード
/// - その他の消耗品
/// 
/// ItemDataSOを継承し、重量情報も含まれます。
/// </summary>
[CreateAssetMenu(menuName = "Game/Consumable")]
public class ConsumableSO : ItemDataSO
{
    [Header("消費アイテムタイプ")]
    /// <summary>消費アイテムのタイプ</summary>
    [Tooltip("消費アイテムのタイプ")]
    public ConsumableType consumableType = ConsumableType.HealthPack;

    [Header("効果")]
    /// <summary>回復量（HealthPackの場合）</summary>
    [Tooltip("回復量")]
    public int healAmount = 50;

    /// <summary>ダメージ量（Grenadeの場合）</summary>
    [Tooltip("ダメージ量")]
    public int damageAmount = 30;

    /// <summary>効果範囲（セル単位、Grenadeの場合）</summary>
    [Tooltip("効果範囲（セル単位）")]
    public float effectRadius = 2f;
}

/// <summary>
/// 消費アイテムタイプの列挙型
/// </summary>
public enum ConsumableType
{
    /// <summary>回復薬</summary>
    HealthPack,
    /// <summary>グレネード</summary>
    Grenade,
    /// <summary>その他</summary>
    Other
}

