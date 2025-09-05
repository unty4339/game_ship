using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ユニットの体力 気絶値 ステータス効果を管理
/// 効果はIStatusEffectとして拡張可能
/// </summary>
public class CombatantStatus : GameTimeBehaviour
{
    [Header("Vitals")]
    [SerializeField] public int maxHP = 100;
    [SerializeField] public int currentHP = 100;
    [SerializeField] public float stunValue = 0f;     // 気絶値
    [SerializeField] public bool isKO = false;        // 気絶状態
    [SerializeField] public bool isDead = false;      // 死亡状態

    [Header("Tick")]
    [Tooltip("効果の更新間隔秒 小さくすると高精度")]
    [SerializeField] public float tickSec = 0.1f;
    float _accum;

    /// <summary>アクティブな効果一覧 種別キーで管理</summary>
    Dictionary<string, IStatusEffect> _effects = new Dictionary<string, IStatusEffect>();

    /// <summary>HPや気絶に対するイベント</summary>
    public event Action OnDeath;
    public event Action OnKO;
    public event Action OnRevive;

    void Awake()
    {
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        RecalcStates();
    }

    void Update()
    {
        if (isDead) return;
        _accum += dt;   // ← ここを Time.deltaTime から変更
        if (_accum < tickSec) return;
        float step = _accum;
        _accum = 0f;

        foreach (var kv in _effects) kv.Value.OnTick(this, step);

        _tmpClearList.Clear();
        foreach (var kv in _effects) if (kv.Value.IsExpired) _tmpClearList.Add(kv.Key);
        foreach (var k in _tmpClearList) _effects.Remove(k);

        RecalcStates();
    }

    List<string> _tmpClearList = new List<string>();

    /// <summary>
    /// 生のダメージを適用
    /// </summary>
    public void ApplyDamage(int amount)
    {
        if (isDead) return;
        currentHP = Mathf.Max(0, currentHP - Mathf.Max(0, amount));
        RecalcStates();
    }

    /// <summary>
    /// 気絶値を加算
    /// </summary>
    public void AddStun(float amount)
    {
        if (isDead) return;
        stunValue = Mathf.Max(0f, stunValue + amount);
        RecalcStates();
    }

    /// <summary>
    /// 効果を追加またはスタック更新
    /// </summary>
    public void AddOrStackEffect(IStatusEffect effect)
    {
        if (effect == null) return;
        string id = effect.Id;
        if (_effects.TryGetValue(id, out var existing))
        {
            existing.OnStack(effect);
        }
        else
        {
            _effects[id] = effect;
            effect.OnApply(this);
        }
    }

    /// <summary>
    /// 効果を外す
    /// </summary>
    public void RemoveEffect(string id)
    {
        if (_effects.TryGetValue(id, out var eff))
        {
            eff.OnRemove(this);
            _effects.Remove(id);
        }
    }

    /// <summary>
    /// 現在の効果一覧を返す 読み取り用途
    /// </summary>
    public IReadOnlyDictionary<string, IStatusEffect> Effects => _effects;

    void RecalcStates()
    {
        bool wasKO = isKO;
        bool wasDead = isDead;

        isDead = currentHP <= 0;
        if (isDead)
        {
            isKO = false;
            if (!wasDead) OnDeath?.Invoke();
            return;
        }

        isKO = stunValue >= currentHP;

        if (isKO && !wasKO) OnKO?.Invoke();
        if (!isKO && wasKO) OnRevive?.Invoke();
    }
}
