using UnityEngine;

/// <summary>
/// ユニットの中核情報と他コンポーネントへのアクセスポイント
/// 
/// このクラスは各ユニットの中心となるコンポーネントで、以下の役割を担います：
/// - ユニットの一意識別子（UnitId）の管理
/// - 陣営情報（FactionTag）へのアクセス
/// - 戦闘ステータス（CombatantStatus）へのアクセス
/// - グリッド位置管理（GridAgent）へのアクセス
/// - 移動制御（UnitMotor）へのアクセス
/// - パスファインディング（UnitPathAgent）へのアクセス
/// - 感知システム（UnitPerception）へのアクセス
/// - ターゲティング（UnitTargeting）へのアクセス
/// - 武器制御（WeaponController）へのアクセス
/// 
/// 他のスクリプトからユニットの各機能にアクセスする際の統一されたインターフェースとして機能します。
/// </summary>
public class UnitCore : MonoBehaviour
{
    /// <summary>
    /// ユニットの一意識別子
    /// UnitDirectoryで管理される際に自動的に設定されます
    /// </summary>
    public int UnitId { get; private set; }
    
    /// <summary>
    /// 陣営情報コンポーネントへの参照
    /// 味方・敵の判定に使用されます
    /// </summary>
    public FactionTag Faction { get; private set; }
    
    /// <summary>
    /// 戦闘ステータスコンポーネントへの参照
    /// HP、ダメージ、状態異常などの管理を行います
    /// 遅延初期化に対応（UnitFactoryで後から追加される可能性があるため）
    /// </summary>
    public CombatantStatus Status
    {
        get
        {
            if (_status == null)
            {
                _status = GetComponent<CombatantStatus>();
            }
            return _status;
        }
        private set => _status = value;
    }
    private CombatantStatus _status;
    
    /// <summary>
    /// グリッド位置管理コンポーネントへの参照
    /// マップ上のセル位置の管理を行います
    /// </summary>
    public GridAgent Grid { get; private set; }
    
    /// <summary>
    /// 移動制御コンポーネントへの参照
    /// ユニットの移動アニメーションと制御を行います
    /// </summary>
    public UnitMotor Motor { get; private set; }
    
    /// <summary>
    /// パスファインディングコンポーネントへの参照
    /// A*アルゴリズムを使用した経路探索を行います
    /// </summary>
    public UnitPathAgent Path { get; private set; }
    
    /// <summary>
    /// 感知システムコンポーネントへの参照
    /// 周囲の敵や味方の検出を行います
    /// </summary>
    public UnitPerception Perception { get; private set; }
    
    /// <summary>
    /// ターゲティングコンポーネントへの参照
    /// 攻撃対象の選択と管理を行います
    /// </summary>
    public UnitTargeting Targeting { get; private set; }
    
    /// <summary>
    /// 武器制御コンポーネントへの参照
    /// 射撃、攻撃の制御を行います
    /// </summary>
    public WeaponController Weapon { get; private set; }

    /// <summary>
    /// インベントリ管理コンポーネントへの参照
    /// 装備・インベントリ・重量管理を行います
    /// 遅延初期化に対応（UnitFactoryで後から追加される可能性があるため）
    /// </summary>
    public UnitInventory Inventory
    {
        get
        {
            if (_inventory == null)
            {
                _inventory = GetComponent<UnitInventory>();
            }
            return _inventory;
        }
        private set => _inventory = value;
    }
    private UnitInventory _inventory;

    /// <summary>
    /// 初期化処理
    /// 必要なコンポーネントの参照を取得します
    /// 各コンポーネントは同じGameObjectにアタッチされている必要があります
    /// </summary>
    void Awake()
    {
        // 各コンポーネントの参照を取得
        Faction = GetComponent<FactionTag>();
        // Statusは遅延初期化（UnitFactoryで後から追加される可能性があるため）
        _status = GetComponent<CombatantStatus>();
        Grid = GetComponent<GridAgent>();
        Motor = GetComponent<UnitMotor>();
        Path = GetComponent<UnitPathAgent>();
        Perception = GetComponent<UnitPerception>();
        // TargetingとWeaponはStartで取得（循環参照を避けるため）
        Weapon = GetComponent<WeaponController>();
        // Inventoryは遅延初期化（UnitFactoryで後から追加される可能性があるため）
        _inventory = GetComponent<UnitInventory>();
    }

    void Start()
    {
        // Statusが取得できていない場合は再試行（UnitFactoryで後から追加された場合）
        if (_status == null)
        {
            _status = GetComponent<CombatantStatus>();
        }
        
        // TargetingはStartで取得（UnitTargetingのAwakeが完了していることを保証）
        if (Targeting == null)
        {
            Targeting = GetComponent<UnitTargeting>();
        }

        // Inventoryが取得できていない場合は再試行（UnitFactoryで後から追加された場合）
        if (_inventory == null)
        {
            _inventory = GetComponent<UnitInventory>();
        }
    }

    /// <summary>
    /// ユニットが生存しているかどうかを判定
    /// </summary>
    /// <returns>生存している場合はtrue、死亡またはStatusがnullの場合はfalse</returns>
    public bool IsAlive()
    {
        return Status != null && !Status.isDead && Status.currentHP > 0;
    }

    /// <summary>
    /// 指定されたユニットが生存しているかどうかを判定（静的メソッド）
    /// </summary>
    /// <param name="unit">判定するユニット</param>
    /// <returns>生存している場合はtrue、死亡またはnullの場合はfalse</returns>
    public static bool IsAlive(UnitCore unit)
    {
        return unit != null && unit.IsAlive();
    }
}
