using UnityEngine;

/// <summary>
/// 完全自動のダミー編成セットアップ
/// アーキタイプと装備と武器データをランタイムに生成しテンプレートユニットを用意
/// RosterManagerへ編成を投入
/// </summary>
public class AutoDummyRosterSetup : MonoBehaviour
{
    [Header("Numbers")]
    public int allyCount = 3;
    public int enemyCount = 4;

    [Header("Visual")]
    [Tooltip("テンプレートユニットのスプライト 任意 nullならデフォルト四角")]
    public Sprite unitSprite;

    [Tooltip("味方の色")]
    public Color allyColor = new Color(0.2f, 0.8f, 1f, 1f);

    [Tooltip("敵の色")]
    public Color enemyColor = new Color(1f, 0.4f, 0.3f, 1f);

    void Start()
    {
        if (RosterManager.Instance == null)
        {
            Debug.LogWarning("RosterManager not found this script requires RosterManager in the scene");
            return;
        }

        // 既存編成をクリア
        RosterManager.Instance.roster.Clear();

        // テンプレートユニットを生成して非アクティブで保持
        var allyTemplate = CreateUnitTemplate("UnitTemplate_Ally", allyColor);
        var enemyTemplate = CreateUnitTemplate("UnitTemplate_Enemy", enemyColor);

        // ランタイム専用のアーキタイプSOを生成
        var allyType = ScriptableObject.CreateInstance<UnitArchetypeSO>();
        allyType.displayName = "Auto Ally";
        allyType.unitPrefab = allyTemplate;
        allyType.baseHP = 110;
        allyType.baseDamage = 18;     // 互換フィールド 現在は未使用でも保持
        allyType.baseFireRate = 3.0f; // 互換フィールド 現在は未使用でも保持

        var enemyType = ScriptableObject.CreateInstance<UnitArchetypeSO>();
        enemyType.displayName = "Auto Enemy";
        enemyType.unitPrefab = enemyTemplate;
        enemyType.baseHP = 90;
        enemyType.baseDamage = 22;     // 互換フィールド
        enemyType.baseFireRate = 2.5f; // 互換フィールド

        // ランタイム専用の装備SOを生成 任意
        var equipHP = ScriptableObject.CreateInstance<EquipmentSO>();
        equipHP.equipmentName = "HP Booster";
        equipHP.hpBonus = 20;
        equipHP.damageBonus = 0;
        equipHP.fireRateBonus = 0f;
        equipHP.overrideVisualPrefab = null;

        var equipDMG = ScriptableObject.CreateInstance<EquipmentSO>();
        equipDMG.equipmentName = "Damage Booster";
        equipDMG.hpBonus = 0;
        equipDMG.damageBonus = 6;
        equipDMG.fireRateBonus = 0f;
        equipDMG.overrideVisualPrefab = null;

        var equipROF = ScriptableObject.CreateInstance<EquipmentSO>();
        equipROF.equipmentName = "Rate Booster";
        equipROF.hpBonus = 0;
        equipROF.damageBonus = 0;
        equipROF.fireRateBonus = 0.5f;
        equipROF.overrideVisualPrefab = null;

        // ランタイム専用の武器StatsSOを生成
        var allyWeapon = ScriptableObject.CreateInstance<WeaponStatsSO>();
        allyWeapon.name = "AutoAlly_RifleStats";
        allyWeapon.fireRate = 4.5f;
        allyWeapon.rangeCells = 14f;
        allyWeapon.baseAccuracy = 0.7f;
        allyWeapon.damageMin = 8;
        allyWeapon.damageMax = 12;
        allyWeapon.critChance = 0.1f;
        allyWeapon.critMultiplier = 1.5f;

        var enemyWeapon = ScriptableObject.CreateInstance<WeaponStatsSO>();
        enemyWeapon.name = "AutoEnemy_SMGStats";
        enemyWeapon.fireRate = 5.5f;
        enemyWeapon.rangeCells = 12f;
        enemyWeapon.baseAccuracy = 0.6f;
        enemyWeapon.damageMin = 6;
        enemyWeapon.damageMax = 10;
        enemyWeapon.critChance = 0.08f;
        enemyWeapon.critMultiplier = 1.4f;

        // 必要なら命中時効果のSOもここで生成可能 任意
        WeaponEffectsSO allyOnHit = null;
        WeaponEffectsSO enemyOnHit = null;
        // 例
        // allyOnHit = ScriptableObject.CreateInstance<WeaponEffectsSO>();
        // allyOnHit.onHitEffects.Add(new WeaponEffectsSO.EffectSpec { id = "Bleed", amount = 1.5f, duration = 4f, chance = 0.5f });

        // 味方編成を投入
        for (int i = 0; i < allyCount; i++)
        {
            var loadout = new UnitLoadout
            {
                nickname = $"Ally_{i + 1}",
                factionId = 0,
                archetype = allyType,
                equipA = (i % 2 == 0) ? equipHP : equipROF,
                equipB = (i % 3 == 0) ? equipDMG : null,
                level = 1,
                weaponStats = allyWeapon,          // 追加 武器データを割当
                onHitEffects = allyOnHit           // 任意
            };
            RosterManager.Instance.Add(loadout);
        }

        // 敵編成を投入
        for (int i = 0; i < enemyCount; i++)
        {
            var loadout = new UnitLoadout
            {
                nickname = $"Enemy_{i + 1}",
                factionId = 1,
                archetype = enemyType,
                equipA = (i % 2 == 0) ? equipDMG : equipROF,
                equipB = (i % 3 == 0) ? equipHP : null,
                level = 1,
                weaponStats = enemyWeapon,         // 追加
                onHitEffects = enemyOnHit          // 任意
            };
            RosterManager.Instance.Add(loadout);
        }

        Debug.Log($"AutoDummyRosterSetup ready allies {allyCount} enemies {enemyCount}");
    }

    /// <summary>
    /// ランタイムでテンプレートユニットを生成
    /// 非アクティブで保持しInstantiateの元として使用
    /// </summary>
    GameObject CreateUnitTemplate(string name, Color tint)
    {
        var go = new GameObject(name);
        go.SetActive(false);

        // 必須コンポーネントを付与
        var core = go.AddComponent<UnitCore>();
        var fac = go.AddComponent<FactionTag>();
        var grid = go.AddComponent<GridAgent>();
        var motor = go.AddComponent<UnitMotor>();
        var path = go.AddComponent<UnitPathAgent>();
        var per = go.AddComponent<UnitPerception>();
        var tgt = go.AddComponent<UnitTargeting>();
        var wep = go.AddComponent<WeaponController>();
        var col = go.AddComponent<BoxCollider2D>(); // クリック検出用・レイキャスト検出用
        col.isTrigger = true;
        var selectable = go.AddComponent<Selectable>();

        // 注: UnitInfoUIコンポーネントは各ユニットに追加しません
        // HoverInfoUIManagerがレイキャストで検出して情報を表示します

        // 見た目のハイライトを軽く作るなら
        var ring = new GameObject("Highlight");
        ring.transform.SetParent(go.transform, false);
        var ringSr = ring.AddComponent<SpriteRenderer>();
        ringSr.sprite = GenerateFallbackSprite();
        ringSr.color = new Color(1f, 1f, 0f, 0.35f);
        ringSr.sortingLayerName = "Units";
        ringSr.sortingOrder = 100;
        ring.SetActive(false);
        selectable.highlight = ring;

        // 見た目
        var sr = go.AddComponent<SpriteRenderer>();
        if (unitSprite != null)
        {
            sr.sprite = unitSprite;
        }
        else
        {
            sr.sprite = GenerateFallbackSprite();
        }
        sr.color = tint;

        // 位置はスポーン時にFactoryがスナップ

        return go;
    }

    /// <summary>
    /// フォールバック用のシンプルなスプライトを生成
    /// ランタイムのみの暫定表示用
    /// </summary>
    Sprite GenerateFallbackSprite()
    {
        int w = 16, h = 16;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        var pixels = new Color[w * h];
        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            bool border = x == 0 || x == w - 1 || y == 0 || y == h - 1;
            pixels[y * w + x] = border ? new Color(0f, 0f, 0f, 1f) : new Color(1f, 1f, 1f, 1f);
        }
        tex.SetPixels(pixels);
        tex.Apply();

        var rect = new Rect(0, 0, w, h);
        var pivot = new Vector2(0.5f, 0.5f);
        return Sprite.Create(tex, rect, pivot, 16f);
    }
}
