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
    #region Default Values
    /// <summary>デフォルトの射撃レート（秒間発射数）</summary>
    private const float DEFAULT_FIRE_RATE = 4f;
    /// <summary>デフォルトの射程距離（セル単位）</summary>
    private const float DEFAULT_RANGE_CELLS = 12f;
    /// <summary>デフォルトの命中率（0-1）</summary>
    private const float DEFAULT_ACCURACY = 0.7f;
    /// <summary>デフォルトの最小ダメージ</summary>
    private const int DEFAULT_DAMAGE_MIN = 5;
    /// <summary>デフォルトの最大ダメージ</summary>
    private const int DEFAULT_DAMAGE_MAX = 12;
    /// <summary>デフォルトのクリティカル発生率（0-1）</summary>
    private const float DEFAULT_CRIT_CHANCE = 0.1f;
    /// <summary>デフォルトのクリティカル倍率</summary>
    private const float DEFAULT_CRIT_MULTIPLIER = 1.5f;
    #endregion

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
    public float FireRate => GetStatValue(s => s.fireRate, DEFAULT_FIRE_RATE);
    
    /// <summary>射程距離（セル単位）</summary>
    public float RangeCells => GetStatValue(s => s.rangeCells, DEFAULT_RANGE_CELLS);
    
    /// <summary>命中率（0-1）</summary>
    public float Accuracy => GetStatValue(s => Mathf.Clamp01(s.baseAccuracy), DEFAULT_ACCURACY);
    
    /// <summary>最小ダメージ</summary>
    public int DamageMin => GetStatValue(s => s.damageMin, DEFAULT_DAMAGE_MIN);
    
    /// <summary>最大ダメージ</summary>
    public int DamageMax => GetStatValue(s => s.damageMax, DEFAULT_DAMAGE_MAX);
    
    /// <summary>クリティカル発生率（0-1）</summary>
    public float CritChance => GetStatValue(s => Mathf.Clamp01(s.critChance), DEFAULT_CRIT_CHANCE);
    
    /// <summary>クリティカル倍率</summary>
    public float CritMultiplier => GetStatValue(s => Mathf.Max(1f, s.critMultiplier), DEFAULT_CRIT_MULTIPLIER);

    /// <summary>
    /// 武器ステータスから値を取得するヘルパーメソッド
    /// statsがnullの場合はデフォルト値を返す
    /// </summary>
    private T GetStatValue<T>(System.Func<WeaponStatsSO, T> getter, T defaultValue)
    {
        return stats != null ? getter(stats) : defaultValue;
    }

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

    /// <summary>
    /// 新しい武器を装備する
    /// </summary>
    /// <param name="newStats">装備する武器のステータス</param>
    public void Equip(WeaponStatsSO newStats)
    {
        stats = newStats;
        _cool = 0f; // 装備時にクールダウンをリセット
    }

    #region Legacy Getters (後方互換性のため)
    /// <summary>命中率を取得（後方互換性のため、Accuracyプロパティを使用してください）</summary>
    /// <returns>命中率（0-1）</returns>
    [System.Obsolete("Accuracyプロパティを使用してください")]
    public float GetAccuracy() => Accuracy;
    
    /// <summary>最小ダメージを取得（後方互換性のため、DamageMinプロパティを使用してください）</summary>
    /// <returns>最小ダメージ値</returns>
    [System.Obsolete("DamageMinプロパティを使用してください")]
    public int GetDamageMin() => DamageMin;
    
    /// <summary>最大ダメージを取得（後方互換性のため、DamageMaxプロパティを使用してください）</summary>
    /// <returns>最大ダメージ値</returns>
    [System.Obsolete("DamageMaxプロパティを使用してください")]
    public int GetDamageMax() => DamageMax;
    
    /// <summary>クリティカル発生率を取得（後方互換性のため、CritChanceプロパティを使用してください）</summary>
    /// <returns>クリティカル発生率（0-1）</returns>
    [System.Obsolete("CritChanceプロパティを使用してください")]
    public float GetCritChance() => CritChance;
    
    /// <summary>クリティカル倍率を取得（後方互換性のため、CritMultiplierプロパティを使用してください）</summary>
    /// <returns>クリティカル倍率</returns>
    [System.Obsolete("CritMultiplierプロパティを使用してください")]
    public float GetCritMultiplier() => CritMultiplier;
    #endregion
}