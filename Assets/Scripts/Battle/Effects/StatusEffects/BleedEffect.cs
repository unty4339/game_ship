using UnityEngine;

/// <summary>
/// 出血デバフ
/// 量に応じて毎秒HPを減らす
/// 継続中は失血へ徐々に変換する
/// </summary>
public class BleedEffect : IStatusEffect
{
    public string Id => "Bleed";

    public bool IsExpired => bleedAmount <= 0f && remainTime <= 0f;

    public float bleedAmount;   // 出血量
    public float remainTime;    // 効果時間 負値で無限

    // パラメータ
    public float dpsPerAmount = 1.0f;          // 出血量1あたりの毎秒ダメージ
    public float decayPerSec = 0.0f;           // 自然回復量毎秒
    public float toHemoPerSecPerAmount = 0.2f; // 出血から失血へ変換率

    public BleedEffect(float amount, float durationSec)
    {
        bleedAmount = Mathf.Max(0f, amount);
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

        // HP減少
        float dmgF = dpsPerAmount * bleedAmount * dt;
        int dmg = Mathf.FloorToInt(dmgF);
        if (dmg > 0) target.ApplyDamage(dmg);

        // 端数を次へ残したい場合は蓄積ロジックを追加

        // 失血へ変換
        float hemoGain = toHemoPerSecPerAmount * bleedAmount * dt;
        if (hemoGain > 0f)
        {
            target.AddOrStackEffect(new HemorrhageEffect(hemoGain, -1f));
        }

        // 減衰
        if (decayPerSec > 0f)
        {
            bleedAmount = Mathf.Max(0f, bleedAmount - decayPerSec * dt);
        }
    }

    public void OnStack(IStatusEffect from)
    {
        if (from is BleedEffect b)
        {
            bleedAmount += b.bleedAmount;
            remainTime = Mathf.Max(remainTime, b.remainTime);
        }
    }

    public void OnRemove(CombatantStatus target) { }
}
