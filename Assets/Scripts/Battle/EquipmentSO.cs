using UnityEngine;

[CreateAssetMenu(menuName = "Game/Equipment")]
public class EquipmentSO : ScriptableObject
{
    /// <summary>装備の名称</summary>
    public string equipmentName;

    /// <summary>HP修正値</summary>
    public int hpBonus;

    /// <summary>基礎ダメージ修正値</summary>
    public int damageBonus;

    /// <summary>発射レート修正値</summary>
    public float fireRateBonus;

    /// <summary>見た目差し替え用のプレハブ 任意</summary>
    public GameObject overrideVisualPrefab;
}
