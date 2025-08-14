using UnityEngine;

/// <summary>
/// 失血デバフ
/// 量に応じて毎秒気絶値を増やす
/// </summary>
public class HemorrhageEffect : IStatusEffect
{
    public string Id => "Hemorrhage";

    public bool IsExpired => hemoAmount <= 0f && remainTime <= 0f;

    public float hemoAmount;  // 失血量
    public float remainTime;  // 効果時間 負値で無限

    // パラメータ
    public float stunPerSecPerAmount = 0.5f; // 失血量1あたりの気絶上昇毎秒
    public float decayPerSec = 0.0f;         // 自然回復量毎秒

    public HemorrhageEffect(float amount, float durationSec)
    {
        hemoAmount = Mathf.Max(0f, amount);
        remainTime = durationSec;
    }

    public void OnApply(CombatantStatus target) { }

    public void OnTick(CombatantStatus target, float dt)
    {
        if (remainTime > 0f)
        {
            remainTime -= dt;
            if (remainTime < 0f) remainTime = 0f;
        }

        // 気絶値上昇
        float stunGain = stunPerSecPerAmount * hemoAmount * dt;
        if (stunGain > 0f) target.AddStun(stunGain);

        // 減衰
        if (decayPerSec > 0f)
        {
            hemoAmount = Mathf.Max(0f, hemoAmount - decayPerSec * dt);
        }
    }

    public void OnStack(IStatusEffect from)
    {
        if (from is HemorrhageEffect h)
        {
            hemoAmount += h.hemoAmount;
            remainTime = Mathf.Max(remainTime, h.remainTime);
        }
    }

    public void OnRemove(CombatantStatus target) { }
}
