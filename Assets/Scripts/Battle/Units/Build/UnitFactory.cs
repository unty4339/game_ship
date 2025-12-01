using UnityEngine;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// ユニット生成工場
/// 
/// このクラスは以下の機能を提供します：
/// - UnitLoadoutから実際のGameObjectを生成
/// - スポーン位置の安全性チェック（壁や占有セルの回避）
/// - 装備・武器・ステータスの初期化
/// - UI要素の設定
/// - ユニットディレクトリへの登録
/// 
/// 使用例：
/// - バトル開始時のユニット生成
/// - 戦闘中のユニット追加
/// - テスト用のユニット生成
/// </summary>
public class UnitFactory : MonoBehaviour
{
    [Header("HUD")]
    /// <summary>ステータス表示用のCanvas</summary>
    public Canvas hudCanvas;
    /// <summary>ステータス表示用のラベルプレハブ</summary>
    public TextMeshProUGUI statusLabelPrefab;

    [Header("UnitInfoUI")]
    /// <summary>簡易情報ウィンドウのPrefab（マウスオーバー時に表示）</summary>
    [Tooltip("簡易情報ウィンドウのPrefab（マウスオーバー時に表示）")]
    public GameObject simpleInfoWindowPrefab;

    /// <summary>詳細情報ウィンドウのPrefab（右クリック時に表示）</summary>
    [Tooltip("詳細情報ウィンドウのPrefab（右クリック時に表示）")]
    public GameObject detailedInfoWindowPrefab;

    [Header("Combat Effects")]
    /// <summary>デフォルトの命中時効果</summary>
    public WeaponEffectsSO defaultOnHitEffects;

    [Header("Spawn Safety")]
    /// <summary>スポーン時に床セルへ補正する</summary>
    [Tooltip("スポーン時に床セルへ補正する")]
    public bool enforcePassableSpawn = true;

    /// <summary>床検索の最大半径セル数</summary>
    [Tooltip("床検索の最大半径セル数")]
    public int passableSearchRadius = 6;

    /// <summary>スポーン時に占有セルを避ける</summary>
    [Tooltip("スポーン時に占有セルを避ける")]
    public bool avoidOccupiedOnSpawn = true;

    /// <summary>
    /// ユニットを生成する
    /// </summary>
    /// <param name="loadout">ユニットの装備・ステータス情報</param>
    /// <param name="worldPos">生成位置（ワールド座標）</param>
    /// <returns>生成されたユニットのGameObject</returns>
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

        // ユニットプレハブをインスタンス化
        var go = Instantiate(loadout.archetype.unitPrefab, worldPos, Quaternion.identity);
        if (!go.activeSelf) go.SetActive(true);

        // 必要なコンポーネントの参照を取得
        var core = go.GetComponent<UnitCore>();
        var faction = go.GetComponent<FactionTag>();
        var weapon = go.GetComponent<WeaponController>();
        var grid = go.GetComponent<GridAgent>();

        // 陣営IDを設定
        if (faction != null) faction.FactionId = loadout.factionId;

        // UnitInventoryを取得または作成
        var inventory = go.GetComponent<UnitInventory>();
        if (inventory == null) inventory = go.AddComponent<UnitInventory>();

        // 新しい装備システムでインベントリを初期化
        if (loadout.mainWeapon != null)
        {
            inventory.mainWeapon = loadout.mainWeapon;
            // WeaponItemSOからWeaponStatsSOを取得してWeaponControllerに設定
            if (weapon != null && loadout.mainWeapon.weaponStats != null)
            {
                weapon.Equip(loadout.mainWeapon.weaponStats);
            }
        }
        else if (loadout.weaponStats != null)
        {
            // 後方互換性: weaponStatsからWeaponItemSOを作成（ランタイムでは作成できないため、警告を出す）
            Debug.LogWarning($"Loadout for {go.name} uses legacy weaponStats. Consider using WeaponItemSO instead.");
            if (weapon != null)
            {
                weapon.Equip(loadout.weaponStats);
            }
        }

        inventory.helmet = loadout.helmet;
        inventory.suit = loadout.suit;

        // バックパックをコピー
        if (loadout.backpack != null)
        {
            inventory.backpack = new List<ItemDataSO>(loadout.backpack);
        }

        // ベースステータスを取得
        int baseHP = loadout.archetype.baseHP;
        int baseDmg = loadout.archetype.baseDamage;
        float baseFR = loadout.archetype.baseFireRate;

        // 装備によるボーナスを計算（新しいシステム + 後方互換性）
        int hpBonus = 0;
        int dmgBonus = 0;
        float frBonus = 0f;

        // 新しい装備システム
        if (loadout.helmet != null)
        {
            hpBonus += loadout.helmet.hpBonus;
            dmgBonus += loadout.helmet.damageBonus;
            frBonus += loadout.helmet.fireRateBonus;
        }
        if (loadout.suit != null)
        {
            hpBonus += loadout.suit.hpBonus;
            dmgBonus += loadout.suit.damageBonus;
            frBonus += loadout.suit.fireRateBonus;
        }

        // 後方互換性: 旧システムのequipA, equipB
        if (loadout.equipA != null) 
        { 
            hpBonus += loadout.equipA.hpBonus; 
            dmgBonus += loadout.equipA.damageBonus; 
            frBonus += loadout.equipA.fireRateBonus;
            // equipAがHelmetまたはSuitとして解釈されていない場合、適切なスロットに設定
            if (loadout.helmet == null && loadout.equipA.equipmentType == EquipmentType.Helmet)
            {
                inventory.helmet = loadout.equipA;
            }
            else if (loadout.suit == null && loadout.equipA.equipmentType == EquipmentType.Suit)
            {
                inventory.suit = loadout.equipA;
            }
        }
        if (loadout.equipB != null) 
        { 
            hpBonus += loadout.equipB.hpBonus; 
            dmgBonus += loadout.equipB.damageBonus; 
            frBonus += loadout.equipB.fireRateBonus;
            // equipBがHelmetまたはSuitとして解釈されていない場合、適切なスロットに設定
            if (loadout.helmet == null && loadout.equipB.equipmentType == EquipmentType.Helmet)
            {
                inventory.helmet = loadout.equipB;
            }
            else if (loadout.suit == null && loadout.equipB.equipmentType == EquipmentType.Suit)
            {
                inventory.suit = loadout.equipB;
            }
        }

        // 戦闘ステータスを初期化
        var status = go.GetComponent<CombatantStatus>();
        if (status == null) status = go.AddComponent<CombatantStatus>();
        status.maxHP = Mathf.Max(1, baseHP + hpBonus);
        status.currentHP = status.maxHP;

        // デフォルトの命中時効果を設定（既存の効果がない場合のみ）
        if (weapon != null && defaultOnHitEffects != null && weapon.onHitEffects == null)
            weapon.onHitEffects = defaultOnHitEffects;

        // 生成直後にセル中心へスナップ
        if (grid != null) grid.SnapToGrid();

        // ステータスUIを設定
        var ui = go.GetComponent<CombatStatusTMPUI>();
        if (ui == null) ui = go.AddComponent<CombatStatusTMPUI>();
        ui.targetCanvas = hudCanvas != null ? hudCanvas : ui.targetCanvas;
        ui.labelPrefab = statusLabelPrefab;

        // Collider2Dを確認・追加（UnitInfoUI用）
        var collider = go.GetComponent<Collider2D>();
        if (collider == null)
        {
            // BoxCollider2Dを追加（マウス検出用）
            var boxCollider = go.AddComponent<BoxCollider2D>();
            boxCollider.isTrigger = true;
            // サイズは適切に設定（必要に応じて調整）
            boxCollider.size = new Vector2(1f, 1f);
        }

        // UnitInfoUIを追加（マウスオーバー情報表示用）
        var unitInfoUI = go.GetComponent<UnitInfoUI>();
        if (unitInfoUI == null)
        {
            unitInfoUI = go.AddComponent<UnitInfoUI>();
            unitInfoUI.targetCanvas = hudCanvas;
            
            // UnitFactoryで設定されたPrefabを自動設定
            unitInfoUI.simpleInfoWindowPrefab = simpleInfoWindowPrefab;
            unitInfoUI.detailedInfoWindowPrefab = detailedInfoWindowPrefab;
        }

        // ユニットディレクトリに登録
        if (core == null) core = go.GetComponent<UnitCore>();
        if (core != null) UnitDirectory.Instance.Register(core);

        // 武器エフェクトアプライヤーが存在しない場合は自動作成
        if (FindObjectOfType<WeaponEffectApplier>() == null)
        {
            var bridge = new GameObject("WeaponEffectApplier_Auto");
            bridge.AddComponent<WeaponEffectApplier>();
            DontDestroyOnLoad(bridge);
        }

        // 武器の命中時効果を設定（WeaponItemSOが優先、なければ旧システム）
        if (weapon != null)
        {
            // 命中時効果の適用（優先度: Loadout > 既存設定 > Factoryデフォルト）
            if (loadout.onHitEffects != null)
                weapon.onHitEffects = loadout.onHitEffects;
            else if (weapon.onHitEffects == null && defaultOnHitEffects != null)
                weapon.onHitEffects = defaultOnHitEffects;
        }
        return go;
    }

    /// <summary>
    /// 指定セルが壁や占有なら周囲リングを広げ最寄りの床セルを返す
    /// 占有回避は任意
    /// </summary>
    /// <param name="center">中心セル座標</param>
    /// <param name="maxRadius">検索最大半径</param>
    /// <param name="mm">マップマネージャー</param>
    /// <param name="avoidOccupied">占有セルを避けるかどうか</param>
    /// <returns>利用可能なセル座標</returns>
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
            // 上下辺をチェック
            for (int dx = -r; dx <= r; dx++)
            {
                var c1 = new Vector3Int(center.x + dx, center.y + r, 0);
                if (IsCellAcceptable(c1, mm, avoidOccupied)) return c1;

                var c2 = new Vector3Int(center.x + dx, center.y - r, 0);
                if (IsCellAcceptable(c2, mm, avoidOccupied)) return c2;
            }
            // 左右辺をチェック
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
    /// <param name="cell">チェックするセル座標</param>
    /// <param name="mm">マップマネージャー</param>
    /// <param name="avoidOccupied">占有セルを避けるかどうか</param>
    /// <returns>セルが利用可能かどうか</returns>
    bool IsCellAcceptable(Vector3Int cell, MapManager mm, bool avoidOccupied)
    {
        // マップ範囲内かチェック
        if (!mm.IsInsideBounds(cell)) return false;
        
        // 通行可能かチェック
        if (!mm.IsPassable(cell)) return false;

        // 占有チェック（必要に応じて）
        if (avoidOccupied && UnitDirectory.Instance != null)
        {
            if (UnitDirectory.Instance.IsCellOccupied(cell)) return false;
        }
        return true;
    }
}
