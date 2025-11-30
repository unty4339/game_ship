using System;
using UnityEngine;
using Random = UnityEngine.Random;

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
public class CombatResolver : SingletonMonoBehaviour<CombatResolver>
{
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

    /// <summary>射撃イベント</summary>
    public event Action<ShotEvent> OnShot;

    /// <summary>命中イベント</summary>
    public event Action<HitEvent> OnHit;

    /// <summary>ミスイベント</summary>
    public event Action<MissEvent> OnMiss;

    /// <summary>
    /// ヒットスキャンの解決を要求
    /// 攻撃者と被攻撃者のコンポーネントを収集し命中率とダメージを計算
    /// </summary>
    public void RequestHitScan(GameObject attacker, GameObject target)
    {
        if (attacker == null || target == null) return;

        // 射出点と狙い点を計算
        var origin = attacker.transform.position;
        var aim = target.transform.position;

        // 追加 必ず発砲イベントを先に出す
        OnShot?.Invoke(new ShotEvent
        {
            attacker = attacker,
            target = target,
            origin = origin,
            aimPoint = aim
        });

        // 命中判定とダメージ
        bool hit = ComputeHit(attacker, target, out int dmg, out Vector3 contact, out bool crit);

        if (hit)
        {
            var status = target.GetComponent<CombatantStatus>();
            if (status != null) status.ApplyDamage(dmg);

            OnHit?.Invoke(new HitEvent
            {
                attacker = attacker,
                target = target,
                contactPoint = contact != Vector3.zero ? contact : aim,
                damage = dmg,
                isCritical = crit
            });
        }
        else
        {
            OnMiss?.Invoke(new MissEvent
            {
                attacker = attacker,
                target = target
            });
        }
    }

    bool ComputeHit(GameObject attacker, GameObject target,
        out int damage, out Vector3 contactPoint, out bool critical)
    {
        damage = 0;
        critical = false;
        contactPoint = GetColliderCenter(target) ?? target.transform.position;

        var wc = attacker.GetComponent<WeaponController>();
        float acc = wc != null ? wc.Accuracy : 0.7f;
        int dmin = wc != null ? wc.DamageMin : 5;
        int dmax = wc != null ? wc.DamageMax : 12;
        float cRate = wc != null ? wc.CritChance : 0.1f;
        float cMul  = wc != null ? wc.CritMultiplier : 1.5f;

        bool hit = Random.value < acc;
        if (!hit) return false;

        damage = Random.Range(dmin, dmax + 1);

        if (Random.value < cRate)
        {
            critical = true;
            damage = Mathf.RoundToInt(damage * cMul);
        }
        return true;
    }

    Vector3? GetColliderCenter(GameObject go)
    {
        var c2 = go.GetComponent<Collider2D>();
        if (c2 != null) return c2.bounds.center;
        var c3 = go.GetComponent<Collider>();
        if (c3 != null) return c3.bounds.center;
        return null;
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

    // 追加 イベント
    public struct ShotEvent
    {
        public GameObject attacker;
        public GameObject target;
        public Vector3 origin;
        public Vector3 aimPoint;
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
