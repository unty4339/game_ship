using UnityEngine;

/// <summary>
/// ユニットの中核情報と他コンポーネントへのアクセスポイント
/// ID 陣営 参照の集約
/// </summary>
public class UnitCore : MonoBehaviour
{
    public int UnitId { get; private set; }
    public FactionTag Faction { get; private set; }
    public CombatantStatus Status { get; private set; }
    public GridAgent Grid { get; private set; }
    public UnitMotor Motor { get; private set; }
    public UnitPathAgent Path { get; private set; }
    public UnitPerception Perception { get; private set; }
    public UnitTargeting Targeting { get; private set; }
    public WeaponController Weapon { get; private set; }

    void Awake()
    {
        Faction = GetComponent<FactionTag>();
        Status = GetComponent<CombatantStatus>();
        Grid = GetComponent<GridAgent>();
        Motor = GetComponent<UnitMotor>();
        Path = GetComponent<UnitPathAgent>();
        Perception = GetComponent<UnitPerception>();
        Targeting = GetComponent<UnitTargeting>();
        Weapon = GetComponent<WeaponController>();
    }
}
