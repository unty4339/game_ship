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
    /// </summary>
    public CombatantStatus Status { get; private set; }
    
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
    /// 初期化処理
    /// 必要なコンポーネントの参照を取得します
    /// 各コンポーネントは同じGameObjectにアタッチされている必要があります
    /// </summary>
    void Awake()
    {
        // 各コンポーネントの参照を取得
        Faction = GetComponent<FactionTag>();
        Status = GetComponent<CombatantStatus>();
        Grid = GetComponent<GridAgent>();
        Motor = GetComponent<UnitMotor>();
        Path = GetComponent<UnitPathAgent>();
        Perception = GetComponent<UnitPerception>();
        Targeting = GetComponent<UnitTargeting>();
        Weapon = GetComponent<WeaponController>();
    }
}
