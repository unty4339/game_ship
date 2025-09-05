using System;
using UnityEngine;

/// <summary>
/// 戦闘解決の中枢クラス
/// ヒットスキャンによる命中判定とダメージ適用を担当
/// LoS確認 フレンドリーファイア クリティカル 乱数揺らぎに対応
///
/// 使い方例
///   CombatResolver.Instance.RequestHitScan(attackerGO, targetGO)
///
/// 外部連携例
///   CombatResolver.Instance.OnHit += e => SpawnHitEffect(e.contactPoint)
///   CombatResolver.Instance.OnMiss += e => SpawnMissEffect(e.shootOrigin)
/// </summary>
public class CombatResolver : MonoBehaviour
{
    /// <summary>シングルトン参照</summary>
    public static CombatResolver Instance { get; private set; }

    [Header("Hit rules")]
    [Tooltip("基礎命中率 0から1")]
    [Range(0f, 1f)] public float baseAccuracy = 0.85f;

    [Tooltip("命中率の最小値 近距離でもこの値を下回らない")]
    [Range(0f, 1f)] public float minAccuracy = 0.15f;

    [Tooltip("命中率の距離減衰を開始するセル距離")]
    public float falloffStartCells = 6f;

    [Tooltip("命中率が最小値になるセル距離")]
    public float falloffEndCells = 18f;

    [Header("Damage rules")]
    [Tooltip("基礎ダメージ")]
    public int baseDamage = 20;

    [Tooltip("ダメージのランダム揺らぎ係数 例 0.1で±10パーセント")]
    [Range(0f, 0.5f)] public float damageSpread = 0.1f;

    [Tooltip("クリティカル発生確率 0から1")]
    [Range(0f, 1f)] public float critChance = 0.1f;

    [Tooltip("クリティカル倍率")]
    public float critMultiplier = 1.5f;

    [Header("Logic options")]
    [Tooltip("フレンドリーファイアを許可するか")]
    public bool allowFriendlyFire = false;

    [Tooltip("命中判定の前にLoSを確認するか")]
    public bool checkLineOfSight = true;

    [Tooltip("命中時にHealthを必ず適用するか 外部で処理したい場合はfalse")]
    public bool applyDamageDirectly = true;

    [Header("VFX")]
    [Tooltip("射撃元を推定するためのオフセット 子Transformなどが無い場合に使用")]
    public Vector3 muzzleOffset = Vector3.zero;

    /// <summary>命中イベント</summary>
    public event Action<HitEvent> OnHit;

    /// <summary>ミスイベント</summary>
    public event Action<MissEvent> OnMiss;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple CombatResolver instances detected Destroying this one");
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// ヒットスキャンの解決を要求
    /// 攻撃者と被攻撃者のコンポーネントを収集し命中率とダメージを計算
    /// </summary>
    public void RequestHitScan(GameObject attacker, GameObject target)
    {
        if (attacker == null || target == null) return;

        var aCore = attacker.GetComponent<UnitCore>();
        var tCore = target.GetComponent<UnitCore>();
        if (aCore == null || tCore == null) return;

        var aGrid = aCore.Grid ?? attacker.GetComponent<GridAgent>();
        var tGrid = tCore.Grid ?? target.GetComponent<GridAgent>();
        if (aGrid == null || tGrid == null) return;

        // フレンドリーファイア判定
        var aFaction = aCore.GetComponent<FactionTag>()?.FactionId ?? 0;
        var tFaction = tCore.GetComponent<FactionTag>()?.FactionId ?? 0;
        if (!allowFriendlyFire && aFaction == tFaction) { EmitMiss(attacker, target, Vector3.zero); return; }

        // LoS確認
        if (checkLineOfSight)
        {
            if (!LoSManager.Instance.CanSeeCells(aGrid.Cell, tGrid.Cell))
            {
                EmitMiss(attacker, target, Vector3.zero);
                return;
            }
        }

        // 命中率計算
        int d2 = UnitDirectory.SqrCellDistance(aGrid.Cell, tGrid.Cell);
        float dist = Mathf.Sqrt(d2);
        float acc = ComputeAccuracy(dist);

        bool hit = UnityEngine.Random.value <= acc;

        // 命中ならダメージ決定
        if (hit)
        {
            // 命中なら
            int dmg = ComputeDamage();
            bool crit = UnityEngine.Random.value < critChance;

            var status = target.GetComponent<CombatantStatus>();
            if (status != null) status.ApplyDamage(dmg);
        }
        else
        {
            EmitMiss(attacker, target, GetMuzzleWorld(attacker));
        }
    }

    /// <summary>
    /// 命中率を距離に応じて計算
    /// falloffStartCellsからfalloffEndCellsにかけて線形にbaseAccuracyからminAccuracyまで低下
    /// </summary>
    public float ComputeAccuracy(float distanceCells)
    {
        if (falloffEndCells <= falloffStartCells)
            return Mathf.Clamp01(baseAccuracy);

        if (distanceCells <= falloffStartCells) return Mathf.Clamp01(baseAccuracy);
        if (distanceCells >= falloffEndCells) return Mathf.Clamp01(minAccuracy);

        float t = Mathf.InverseLerp(falloffStartCells, falloffEndCells, distanceCells);
        float acc = Mathf.Lerp(baseAccuracy, minAccuracy, t);
        return Mathf.Clamp01(acc);
    }

    /// <summary>
    /// ダメージ値を計算
    /// baseDamageに対し±damageSpreadのランダムを適用
    /// </summary>
    public int ComputeDamage()
    {
        float spread = 1f + UnityEngine.Random.Range(-damageSpread, damageSpread);
        float raw = baseDamage * spread;
        return Mathf.Max(1, Mathf.RoundToInt(raw));
    }

    /// <summary>
    /// ミスイベントを発火
    /// </summary>
    private void EmitMiss(GameObject attacker, GameObject target, Vector3 origin)
    {
        OnMiss?.Invoke(new MissEvent
        {
            attacker = attacker,
            target = target,
            shootOrigin = origin
        });
    }

    /// <summary>
    /// マズル位置の推定
    /// 子Transformでmuzzleがある場合はそれを優先
    /// 無い場合は本体位置にオフセットを加算
    /// </summary>
    private Vector3 GetMuzzleWorld(GameObject attacker)
    {
        var muzzle = attacker.transform.Find("muzzle");
        if (muzzle != null) return muzzle.position;
        return attacker.transform.position + muzzleOffset;
    }

    /// <summary>
    /// 命中位置の簡易推定
    /// ターゲット中心を返す
    /// 必要ならColliderからレイキャストに置き換え
    /// </summary>
    private Vector3 EstimateContactPoint(GameObject attacker, GameObject target)
    {
        return target.transform.position;
    }

    /// <summary>
    /// 命中イベントデータ
    /// </summary>
    public struct HitEvent
    {
        public GameObject attacker;     // 攻撃者
        public GameObject target;       // 被攻撃者
        public int damage;              // 与ダメージ
        public bool isCritical;         // クリティカル発生
        public float accuracyUsed;      // 使用した命中率
        public float distanceCells;     // セル距離
        public Vector3 shootOrigin;     // 発射位置
        public Vector3 contactPoint;    // 命中位置
    }

    /// <summary>
    /// ミスイベントデータ
    /// </summary>
    public struct MissEvent
    {
        public GameObject attacker;     // 攻撃者
        public GameObject target;       // 対象
        public Vector3 shootOrigin;     // 発射位置
    }
}
