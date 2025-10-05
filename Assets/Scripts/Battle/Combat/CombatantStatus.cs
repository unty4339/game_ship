using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ユニットの体力・気絶値・ステータス効果を管理するクラス
/// 
/// このクラスは以下の機能を提供します：
/// - HP（体力）の管理
/// - 気絶値の管理
/// - ステータス効果の適用・管理
/// - 死亡・気絶状態の判定
/// - 効果の自動更新（ティック処理）
/// 
/// ステータス効果はIStatusEffectインターフェースを実装したクラスで拡張可能です。
/// </summary>
public class CombatantStatus : GameTimeBehaviour
{
    [Header("Vitals")]
    /// <summary>最大HP</summary>
    [SerializeField] public int maxHP = 100;
    /// <summary>現在のHP</summary>
    [SerializeField] public int currentHP = 100;
    /// <summary>気絶値（現在HPを超えると気絶状態）</summary>
    [SerializeField] public float stunValue = 0f;
    /// <summary>気絶状態かどうか</summary>
    [SerializeField] public bool isKO = false;
    /// <summary>死亡状態かどうか</summary>
    [SerializeField] public bool isDead = false;

    [Header("Tick")]
    /// <summary>効果の更新間隔秒（小さくすると高精度）</summary>
    [Tooltip("効果の更新間隔秒 小さくすると高精度")]
    [SerializeField] public float tickSec = 0.1f;
    /// <summary>ティック累積時間</summary>
    float _accum;

    /// <summary>アクティブな効果一覧（種別キーで管理）</summary>
    Dictionary<string, IStatusEffect> _effects = new Dictionary<string, IStatusEffect>();

    /// <summary>HPや気絶に対するイベント</summary>
    public event Action OnDeath;
    public event Action OnKO;
    public event Action OnRevive;

    /// <summary>
    /// 初期化処理
    /// HPの正規化と状態の再計算を行います
    /// </summary>
    void Awake()
    {
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        RecalcStates();
    }

    /// <summary>
    /// 毎フレームの更新処理
    /// ステータス効果のティック処理と期限切れ効果の削除を行います
    /// </summary>
    void Update()
    {
        if (isDead) return;
        
        // ティック累積時間を更新
        _accum += dt;
        if (_accum < tickSec) return;
        
        float step = _accum;
        _accum = 0f;

        // 全効果のティック処理を実行
        foreach (var kv in _effects) kv.Value.OnTick(this, step);

        // 期限切れの効果を削除
        _tmpClearList.Clear();
        foreach (var kv in _effects) 
        {
            if (kv.Value.IsExpired) 
                _tmpClearList.Add(kv.Key);
        }
        foreach (var k in _tmpClearList) 
            _effects.Remove(k);

        // 状態を再計算
        RecalcStates();
    }

    /// <summary>期限切れ効果の一時リスト</summary>
    List<string> _tmpClearList = new List<string>();

    /// <summary>
    /// ダメージを適用する
    /// </summary>
    /// <param name="amount">適用するダメージ量</param>
    public void ApplyDamage(int amount)
    {
        if (isDead) return;
        currentHP = Mathf.Max(0, currentHP - Mathf.Max(0, amount));
        RecalcStates();
    }

    /// <summary>
    /// 気絶値を加算する
    /// </summary>
    /// <param name="amount">加算する気絶値</param>
    public void AddStun(float amount)
    {
        if (isDead) return;
        stunValue = Mathf.Max(0f, stunValue + amount);
        RecalcStates();
    }

    /// <summary>
    /// ステータス効果を追加またはスタック更新する
    /// </summary>
    /// <param name="effect">追加する効果</param>
    public void AddOrStackEffect(IStatusEffect effect)
    {
        if (effect == null) return;
        string id = effect.Id;
        if (_effects.TryGetValue(id, out var existing))
        {
            // 既存効果がある場合はスタック処理
            existing.OnStack(effect);
        }
        else
        {
            // 新規効果の場合は追加
            _effects[id] = effect;
            effect.OnApply(this);
        }
    }

    /// <summary>
    /// 指定IDの効果を削除する
    /// </summary>
    /// <param name="id">削除する効果のID</param>
    public void RemoveEffect(string id)
    {
        if (_effects.TryGetValue(id, out var eff))
        {
            eff.OnRemove(this);
            _effects.Remove(id);
        }
    }

    /// <summary>
    /// 現在の効果一覧を取得する（読み取り専用）
    /// </summary>
    public IReadOnlyDictionary<string, IStatusEffect> Effects => _effects;

    [Header("Death handling")]
    /// <summary>死亡時に自動でGameObjectを破棄するかどうか</summary>
    [SerializeField] public bool autoDestroyOnDeath = true;
    /// <summary>破棄までの遅延時間（秒）</summary>
    [SerializeField] public float destroyDelay = 0f;

    /// <summary>
    /// 状態を再計算する
    /// 死亡・気絶状態の判定とイベント発火を行います
    /// </summary>
    void RecalcStates()
    {
        bool wasKO = isKO;
        bool wasDead = isDead;

        // 死亡判定（HPが0以下）
        isDead = currentHP <= 0;
        if (isDead)
        {
            isKO = false; // 死亡時は気絶状態を解除
            if (!wasDead)
            {
                // 死亡イベントを発火
                OnDeath?.Invoke();
                if (autoDestroyOnDeath)
                    Destroy(gameObject, Mathf.Max(0f, destroyDelay));
            }
            return;
        }

        // 気絶判定（気絶値が現在HP以上）
        isKO = stunValue >= currentHP;

        // 気絶状態の変化イベント
        if (isKO && !wasKO) OnKO?.Invoke();
        if (!isKO && wasKO) OnRevive?.Invoke();
    }
}
