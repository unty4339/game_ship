using UnityEngine;
using TMPro;
/// <summary>
/// UnitLoadoutから実体ユニットを生成するファクトリ
/// 見た目とステータスを装備で上書きしマップへ配置
/// </summary>
public class UnitFactory : MonoBehaviour
{
    [Header("HUD")]
    public Canvas hudCanvas;                    // 既存のHUDキャンバス
    public TextMeshProUGUI statusLabelPrefab;   // TMPラベルのプレハブ

    [Header("Combat Effects")]
    public WeaponEffectsSO defaultOnHitEffects;  // 任意 デフォルトの効果セット

    /// <summary>
    /// ロードアウトからユニットを生成
    /// 生成後にUnitDirectoryへ登録
    /// </summary>
    public GameObject Create(UnitLoadout loadout, Vector3 worldPos)
    {
        if (loadout == null || loadout.archetype == null || loadout.archetype.unitPrefab == null) return null;

        var go = Instantiate(loadout.archetype.unitPrefab, worldPos, Quaternion.identity);

        // 基本コンポーネント取得
        var core = go.GetComponent<UnitCore>();
        var faction = go.GetComponent<FactionTag>();
        var hp = go.GetComponent<Health>();
        var weapon = go.GetComponent<WeaponController>();
        var grid = go.GetComponent<GridAgent>();

        // 陣営設定
        if (faction != null) faction.FactionId = loadout.factionId;

        // ステータス適用
        int baseHP = loadout.archetype.baseHP;
        int baseDmg = loadout.archetype.baseDamage;
        float baseFR = loadout.archetype.baseFireRate;

        int hpBonus = 0;
        int dmgBonus = 0;
        float frBonus = 0f;

        if (loadout.equipA != null) { hpBonus += loadout.equipA.hpBonus; dmgBonus += loadout.equipA.damageBonus; frBonus += loadout.equipA.fireRateBonus; }
        if (loadout.equipB != null) { hpBonus += loadout.equipB.hpBonus; dmgBonus += loadout.equipB.damageBonus; frBonus += loadout.equipB.fireRateBonus; }

        if (hp != null)
        {
            hp.Max = Mathf.Max(1, baseHP + hpBonus);
            hp.Current = hp.Max;
        }
        if (weapon != null)
        {
            weapon.enabled = true;
            var resolver = CombatResolver.Instance;
            if (resolver != null) resolver.baseDamage = Mathf.Max(1, baseDmg + dmgBonus);
            if (weapon != null) weapon.GetType(); // 形だけの参照で警告回避
            // 発射レートは武器側にプロパティがあるなら適用する想定
        }

        // CombatantStatusが無ければ付与しHP初期化
        var status = go.GetComponent<CombatantStatus>();
        if (status == null) status = go.AddComponent<CombatantStatus>();
        status.maxHP = Mathf.Max(1, loadout.archetype.baseHP);
        status.currentHP = status.maxHP;

        // 簡易同期 Healthを併用している場合は数値を揃える
        if (hp != null) { hp.Max = status.maxHP; hp.Current = status.currentHP; }

        // TMPデバッグUIを自動付与
        var ui = go.GetComponent<CombatStatusTMPUI>();
        if (ui == null) ui = go.AddComponent<CombatStatusTMPUI>();
        ui.targetCanvas = hudCanvas != null ? hudCanvas : ui.targetCanvas; // 事前指定を優先
        ui.labelPrefab = statusLabelPrefab;

        // Weaponの命中効果を設定
        if (weapon != null && weapon.onHitEffects == null && defaultOnHitEffects != null)
            weapon.onHitEffects = defaultOnHitEffects;

        // 命中イベントの適用ブリッジがシーンに無ければ生成
        if (FindObjectOfType<WeaponEffectOnHit>() == null)
        {
            var bridge = new GameObject("WeaponEffectOnHit_Auto");
            bridge.AddComponent<WeaponEffectOnHit>();
            DontDestroyOnLoad(bridge);
        }

        // 見た目差し替え 任意
        var visual = loadout.equipA != null && loadout.equipA.overrideVisualPrefab != null
            ? loadout.equipA.overrideVisualPrefab
            : loadout.equipB != null && loadout.equipB.overrideVisualPrefab != null
                ? loadout.equipB.overrideVisualPrefab
                : null;

        if (visual != null)
        {
            var v = Instantiate(visual, go.transform);
            v.name = "Visual";
        }

        // グリッドへスナップ
        if (grid != null) grid.SnapToGrid();

        // ディレクトリ登録
        if (core == null) core = go.GetComponent<UnitCore>();
        if (core != null) UnitDirectory.Instance.Register(core);
        if (!go.activeSelf) go.SetActive(true);
        return go;
    }
}
