using UnityEngine;

/// <summary>
/// 装備アイテムのステータスを定義するScriptableObject
/// 
/// このクラスは以下の装備効果を定義します：
/// - HP、ダメージ、射撃レートのボーナス
/// - 見た目の差し替え（任意）
/// - 装備タイプ（Helmet, Suit, Accessory）
/// 
/// ItemDataSOを継承し、重量情報も含まれます。
/// ユニットの装備スロット（Helmet, Suit）に装備され、UnitFactoryでステータスに反映されます。
/// </summary>
[CreateAssetMenu(menuName = "Game/Equipment")]
public class EquipmentSO : ItemDataSO
{
    [Header("装備タイプ")]
    /// <summary>装備タイプ（Helmet, Suit, Accessory）</summary>
    [Tooltip("装備タイプ")]
    public EquipmentType equipmentType = EquipmentType.Accessory;

    [Header("ステータス補正")]
    /// <summary>HP修正値（正の値で増加、負の値で減少）</summary>
    [Tooltip("HP修正値")]
    public int hpBonus;

    /// <summary>基礎ダメージ修正値（正の値で増加、負の値で減少）</summary>
    [Tooltip("ダメージ修正値")]
    public int damageBonus;

    /// <summary>発射レート修正値（正の値で増加、負の値で減少）</summary>
    [Tooltip("発射レート修正値")]
    public float fireRateBonus;

    [Header("視覚")]
    /// <summary>見た目差し替え用のプレハブ（任意、nullの場合は変更なし）</summary>
    [Tooltip("見た目差し替え用のプレハブ")]
    public GameObject overrideVisualPrefab;

    // 後方互換性のため、equipmentNameプロパティをitemNameにマッピング
    /// <summary>装備の名称（後方互換性のため、itemNameへのエイリアス）</summary>
    public string equipmentName
    {
        get => itemName;
        set => itemName = value;
    }
}
