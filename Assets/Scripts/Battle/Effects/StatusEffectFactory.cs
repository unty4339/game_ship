using UnityEngine;

/// <summary>
/// 効果IDからIStatusEffectを生成
/// 新効果はここに分岐を追加
/// </summary>
public static class StatusEffectFactory
{
    public static IStatusEffect Create(string id, float amount, float duration)
    {
        switch (id)
        {
            case "Bleed": return new BleedEffect(amount, duration);
            case "Hemorrhage": return new HemorrhageEffect(amount, duration);
            default:
                Debug.LogWarning($"Unknown effect id {id}");
                return null;
        }
    }
}
