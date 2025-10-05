using UnityEngine;

/// <summary>
/// 陣営識別コンポーネント
/// 
/// このクラスは以下の機能を提供します：
/// - ユニットの陣営IDの管理
/// - 味方・敵の判定
/// - フレンドリーファイアの制御
/// 
/// 各ユニットにアタッチされ、戦闘システムで陣営判定に使用されます。
/// </summary>
public class FactionTag : MonoBehaviour
{
    /// <summary>陣営ID（0=味方、1=敵など）</summary>
    public int FactionId = 0;
}
