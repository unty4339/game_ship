using UnityEngine;

/// <summary>
/// 命中イベントを受けて武器設定の効果を適用
/// </summary>
public class WeaponEffectApplier : MonoBehaviour
{
    bool hooked;

    void OnEnable() { TryHook(); }
    void Update() { if (!hooked) TryHook(); }
    void OnDisable()
    {
        if (CombatResolver.Instance != null)
            CombatResolver.Instance.OnHit -= HandleHit;
        hooked = false;
    }

    void TryHook()
    {
        if (CombatResolver.Instance == null || hooked) return;
        CombatResolver.Instance.OnHit += HandleHit;
        hooked = true;
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
