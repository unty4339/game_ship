using UnityEngine;

/// <summary>
/// 効果IDからIStatusEffectを生成するファクトリ
/// 新しい効果を追加したらここに分岐を足す
/// </summary>
public static class StatusEffectFactory
{
    public static IStatusEffect Create(string id, float amount, float duration)
    {
        if (string.IsNullOrEmpty(id)) return null;
        switch (id)
        {
            case "Bleed":
                return new BleedEffect(amount, duration);
            case "Hemorrhage":
                return new HemorrhageEffect(amount, duration);
            default:
                Debug.LogWarning($"Unknown effect id {id}");
                return null;
        }
    }
}
