using UnityEngine;

/// <summary>
/// 射撃制御の最小実装
/// 外部からターゲットを指定され 自動射撃が許可されたらレートに従い発砲
/// </summary>
public class WeaponController : MonoBehaviour
{
    [Header("Weapon")]
    public float fireRate = 4f;
    public float rangeCells = 12f;

    [Header("Effects")]
    public WeaponEffectsSO onHitEffects;  // ← 追加 命中時に付与する効果セット

    [Header("Control")]
    public bool allowAutoFire = false;

    UnitCore _overrideTarget;
    float _cool;
    [SerializeField] bool useUnitTargeting = true;

    public void SetOverrideTarget(UnitCore tgt) => _overrideTarget = tgt;

    void Update()
    {
        _cool -= Time.deltaTime;

        // ターゲット取得
        UnitCore tgt = _overrideTarget;
        if (useUnitTargeting && tgt == null)
        {
            var ut = GetComponent<UnitTargeting>();
            if (ut != null) tgt = ut.Current;
        }
        if (tgt == null) return;

        if (_cool > 0f) return;

        var myGrid = GetComponent<GridAgent>();
        var tgtGrid = tgt.Grid;
        if (myGrid == null || tgtGrid == null) return;

        int d2 = UnitDirectory.SqrCellDistance(myGrid.Cell, tgtGrid.Cell);
        if (d2 > rangeCells * rangeCells) return;
        if (!LoSManager.Instance.CanSeeCells(myGrid.Cell, tgtGrid.Cell)) return;

        _cool = 1f / Mathf.Max(0.01f, fireRate);
        CombatResolver.Instance?.RequestHitScan(gameObject, tgt.gameObject);
    }
}
