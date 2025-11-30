using UnityEngine;

/// <summary>
/// 射撃制御を行うコントローラ
/// 
/// このクラスは以下の機能を提供します：
/// - 自動射撃の制御（レート制限付き）
/// - 射程距離のチェック
/// - 視界（LoS）の確認
/// - 武器ステータスの管理
/// - 命中時効果の管理
/// 
/// 使用例：
/// - ユニットの自動攻撃
/// - 手動射撃の制御
/// - 武器の装備・変更
/// </summary>
public class WeaponController : GameTimeBehaviour
{
    [Header("Stats")]
    /// <summary>武器のステータスデータ</summary>
    public WeaponStatsSO stats;

    [Header("Effects")]
    /// <summary>命中時に発動する効果</summary>
    public WeaponEffectsSO onHitEffects;

    /// <summary>オーバーライドターゲット（手動指定）</summary>
    UnitCore _overrideUnit;
    /// <summary>射撃クールダウン</summary>
    float _cool;

    /// <summary>
    /// オーバーライドターゲットを設定
    /// </summary>
    /// <param name="tgt">手動で指定するターゲット</param>
    public void SetOverrideTarget(UnitCore tgt) => _overrideUnit = tgt;

    /// <summary>射撃レート（秒間発射数）</summary>
    public float FireRate => stats != null ? stats.fireRate : 4f;
    /// <summary>射程距離（セル単位）</summary>
    public float RangeCells => stats != null ? stats.rangeCells : 12f;

    /// <summary>
    /// 毎フレームの更新処理
    /// 自動射撃の判定と実行を行います
    /// </summary>
    void Update()
    {
        _cool -= dt;

        // ターゲット解決
        UnitCore tgt = _overrideUnit;
        if (tgt == null)
        {
            var ut = GetComponent<UnitTargeting>();
            if (ut != null) tgt = ut.Current;
        }
        if (tgt == null) return; // ターゲットがいなければ撃たない
        if (_cool > 0f) return;

        var myGrid = GetComponent<GridAgent>();
        var tgGrid = tgt != null ? tgt.Grid : null;
        if (myGrid == null || tgGrid == null) return;

        // 射程とLoS判定
        int d2 = UnitDirectory.SqrCellDistance(myGrid.Cell, tgGrid.Cell);
        float rc = RangeCells;
        if (d2 > rc * rc) return;
        if (!LoSManager.Instance.CanSeeCells(myGrid.Cell, tgGrid.Cell)) return;

        // 発砲
        _cool = 1f / Mathf.Max(0.01f, FireRate);
        CombatResolver.Instance?.RequestHitScan(gameObject, tgt.gameObject);
    }

    /// <summary>命中率を取得</summary>
    /// <returns>命中率（0-1）</returns>
    public float GetAccuracy() => stats != null ? Mathf.Clamp01(stats.baseAccuracy) : 0.7f;
    
    /// <summary>最小ダメージを取得</summary>
    /// <returns>最小ダメージ値</returns>
    public int GetDamageMin() => stats != null ? stats.damageMin : 5;
    
    /// <summary>最大ダメージを取得</summary>
    /// <returns>最大ダメージ値</returns>
    public int GetDamageMax() => stats != null ? stats.damageMax : 12;
    
    /// <summary>クリティカル発生率を取得</summary>
    /// <returns>クリティカル発生率（0-1）</returns>
    public float GetCritChance() => stats != null ? Mathf.Clamp01(stats.critChance) : 0.1f;
    
    /// <summary>クリティカル倍率を取得</summary>
    /// <returns>クリティカル倍率</returns>
    public float GetCritMultiplier() => stats != null ? Mathf.Max(1f, stats.critMultiplier) : 1.5f;

    /// <summary>
    /// 新しい武器を装備する
    /// </summary>
    /// <param name="newStats">装備する武器のステータス</param>
    public void Equip(WeaponStatsSO newStats)
    {
        stats = newStats;
        _cool = 0f; // 装備時にクールダウンをリセット
    }
}