using UnityEngine;

/// <summary>
/// ステータス効果の基本インタフェース
/// Idで一意化しOnStackで量を更新
/// </summary>
public interface IStatusEffect
{
    string Id { get; }
    bool IsExpired { get; }

    void OnApply(CombatantStatus target);
    void OnTick(CombatantStatus target, float dt);
    void OnStack(IStatusEffect from);
    void OnRemove(CombatantStatus target);
}
