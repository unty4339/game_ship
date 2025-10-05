using UnityEngine;

/// <summary>
/// 命中イベントを受けて武器設定の効果を適用
/// Resolverの再生成に耐える再購読ロジック
/// </summary>
public class WeaponEffectApplier : MonoBehaviour
{
    CombatResolver _hookedResolver;

    void OnEnable()  { TryRehook(); }
    void Update()    { TryRehook(); }
    void OnDisable() { Unhook(); }

    void TryRehook()
    {
        var cur = CombatResolver.Instance;
        if (cur == _hookedResolver) return;

        Unhook();
        if (cur != null)
        {
            cur.OnHit += HandleHit;
            _hookedResolver = cur;
        }
    }

    void Unhook()
    {
        if (_hookedResolver != null)
        {
            _hookedResolver.OnHit -= HandleHit;
            _hookedResolver = null;
        }
    }

    void HandleHit(CombatResolver.HitEvent e)
    {
        if (e.attacker == null || e.target == null) return;

        var wc = e.attacker.GetComponent<WeaponController>();
        if (wc == null || wc.onHitEffects == null) return;

        var status = e.target.GetComponent<CombatantStatus>();
        if (status == null) return;

        foreach (var spec in wc.onHitEffects.onHitEffects)
        {
            if (spec.chance < 1f && Random.value > spec.chance) continue;
            var eff = StatusEffectFactory.Create(spec.id, spec.amount, spec.duration);
            if (eff != null) status.AddOrStackEffect(eff);
        }
    }
}