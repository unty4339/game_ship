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
        public Vector3 contactPoint;    // 命中位置
    }

    /// <summary>
    /// ミスイベントデータ
    /// </summary>
    public struct MissEvent
    {
        public GameObject attacker;     // 攻撃者
        public GameObject target;       // 対象
    }
}
