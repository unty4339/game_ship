using UnityEngine;

/// <summary>
/// 装備アイテムのステータスを定義するScriptableObject
/// 
/// このクラスは以下の装備効果を定義します：
/// - HP、ダメージ、射撃レートのボーナス
/// - 見た目の差し替え（任意）
/// 
/// ユニットの装備スロットA・Bに装備され、UnitFactoryでステータスに反映されます。
/// </summary>
[CreateAssetMenu(menuName = "Game/Equipment")]
public class EquipmentSO : ScriptableObject
{
    /// <summary>装備の名称</summary>
    public string equipmentName;

    /// <summary>HP修正値（正の値で増加、負の値で減少）</summary>
    public int hpBonus;

    /// <summary>基礎ダメージ修正値（正の値で増加、負の値で減少）</summary>
    public int damageBonus;

    /// <summary>発射レート修正値（正の値で増加、負の値で減少）</summary>
    public float fireRateBonus;

    /// <summary>見た目差し替え用のプレハブ（任意、nullの場合は変更なし）</summary>
    public GameObject overrideVisualPrefab;
}
