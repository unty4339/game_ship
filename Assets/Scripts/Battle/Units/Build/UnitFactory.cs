using UnityEngine;
using TMPro;

public class UnitFactory : MonoBehaviour
{
    [Header("HUD")]
    public Canvas hudCanvas;
    public TextMeshProUGUI statusLabelPrefab;

    [Header("Combat Effects")]
    public WeaponEffectsSO defaultOnHitEffects;

    [Header("Spawn Safety")]
    [Tooltip("スポーン時に床セルへ補正する")]
    public bool enforcePassableSpawn = true;

    [Tooltip("床検索の最大半径セル数")]
    public int passableSearchRadius = 6;

    [Tooltip("スポーン時に占有セルを避ける")]
    public bool avoidOccupiedOnSpawn = true;

    public GameObject Create(UnitLoadout loadout, Vector3 worldPos)
    {
        // マップに合わせてセル中心へ補正し壁セルなら近傍の床へ補正
        var mm = MapManager.Instance;
        if (mm != null)
        {
            var cell = mm.WorldToCell(worldPos);
            if (enforcePassableSpawn)
            {
                cell = FindNearestFreePassableCell(
                    cell,
                    passableSearchRadius,
                    mm,
                    avoidOccupiedOnSpawn
                );
            }
            worldPos = mm.CellToWorldCenter(cell);
        }
        worldPos.z = 0f;

        var go = Instantiate(loadout.archetype.unitPrefab, worldPos, Quaternion.identity);
        if (!go.activeSelf) go.SetActive(true);

        // 既存の初期化処理はそのまま
        var core = go.GetComponent<UnitCore>();
        var faction = go.GetComponent<FactionTag>();
        var weapon = go.GetComponent<WeaponController>();
        var grid = go.GetComponent<GridAgent>();

        // 陣営を設定
        if (faction != null) faction.FactionId = loadout.factionId;

        int baseHP = loadout.archetype.baseHP;
        int baseDmg = loadout.archetype.baseDamage;
        float baseFR = loadout.archetype.baseFireRate;

        int hpBonus = 0;
        int dmgBonus = 0;
        float frBonus = 0f;
        if (loadout.equipA != null) { hpBonus += loadout.equipA.hpBonus; dmgBonus += loadout.equipA.damageBonus; frBonus += loadout.equipA.fireRateBonus; }
        if (loadout.equipB != null) { hpBonus += loadout.equipB.hpBonus; dmgBonus += loadout.equipB.damageBonus; frBonus += loadout.equipB.fireRateBonus; }

        // ステータスを初期化
        var status = go.GetComponent<CombatantStatus>();
        if (status == null) status = go.AddComponent<CombatantStatus>();
        status.maxHP = Mathf.Max(1, baseHP + hpBonus);
        status.currentHP = status.maxHP;

        if (weapon != null && defaultOnHitEffects != null && weapon.onHitEffects == null)
            weapon.onHitEffects = defaultOnHitEffects;

        // 生成直後にセル中心へスナップ
        if (grid != null) grid.SnapToGrid();

        // ステータスUIを設定
        var ui = go.GetComponent<CombatStatusTMPUI>();
        if (ui == null) ui = go.AddComponent<CombatStatusTMPUI>();
        ui.targetCanvas = hudCanvas != null ? hudCanvas : ui.targetCanvas;
        ui.labelPrefab = statusLabelPrefab;

        // ディレクトリに登録
        if (core == null) core = go.GetComponent<UnitCore>();
        if (core != null) UnitDirectory.Instance.Register(core);

        // エフェクトブリッジを作成
        if (FindObjectOfType<WeaponEffectApplier>() == null)
        {
            var bridge = new GameObject("WeaponEffectApplier_Auto");
            bridge.AddComponent<WeaponEffectApplier>();
            DontDestroyOnLoad(bridge);
        }

        return go;
    }

    /// <summary>
    /// 指定セルが壁や占有なら周囲リングを広げ最寄りの床セルを返す
    /// 占有回避は任意
    /// </summary>
    Vector3Int FindNearestFreePassableCell(
        Vector3Int center,
        int maxRadius,
        MapManager mm,
        bool avoidOccupied
    )
    {
        // 中心が有効なら即返す
        if (IsCellAcceptable(center, mm, avoidOccupied)) return center;

        // 半径1からmaxRadiusまで外周を走査
        for (int r = 1; r <= Mathf.Max(1, maxRadius); r++)
        {
            // 上下辺
            for (int dx = -r; dx <= r; dx++)
            {
                var c1 = new Vector3Int(center.x + dx, center.y + r, 0);
                if (IsCellAcceptable(c1, mm, avoidOccupied)) return c1;

                var c2 = new Vector3Int(center.x + dx, center.y - r, 0);
                if (IsCellAcceptable(c2, mm, avoidOccupied)) return c2;
            }
            // 左右辺
            for (int dy = -r + 1; dy <= r - 1; dy++)
            {
                var c3 = new Vector3Int(center.x + r, center.y + dy, 0);
                if (IsCellAcceptable(c3, mm, avoidOccupied)) return c3;

                var c4 = new Vector3Int(center.x - r, center.y + dy, 0);
                if (IsCellAcceptable(c4, mm, avoidOccupied)) return c4;
            }
        }

        // 見つからない場合は中心を返す
        return center;
    }

    /// <summary>
    /// セルが通行可能で範囲内であり必要なら占有されていないかを判定
    /// </summary>
    bool IsCellAcceptable(Vector3Int cell, MapManager mm, bool avoidOccupied)
    {
        if (!mm.IsInsideBounds(cell)) return false;
        if (!mm.IsPassable(cell)) return false;

        if (avoidOccupied && UnitDirectory.Instance != null)
        {
            if (UnitDirectory.Instance.IsCellOccupied(cell)) return false;
        }
        return true;
    }
}
