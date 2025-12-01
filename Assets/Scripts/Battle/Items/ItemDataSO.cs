using UnityEngine;

/// <summary>
/// 全てのアイテムの基底となる抽象クラス
/// 
/// このクラスは以下の共通情報を定義します：
/// - アイテム名
/// - UI表示用アイコン
/// - 重量（kg単位）
/// - 説明文
/// 
/// 全てのアイテム（装備、武器、消費アイテムなど）はこのクラスを継承します。
/// </summary>
public abstract class ItemDataSO : ScriptableObject
{
    [Header("基本情報")]
    /// <summary>アイテム名</summary>
    [Tooltip("アイテム名")]
    public string itemName = "新規アイテム";

    /// <summary>UI表示用アイコン</summary>
    [Tooltip("UI表示用アイコン")]
    public Sprite icon;

    /// <summary>重量（kg単位）</summary>
    [Tooltip("重量（kg単位）")]
    [Range(0f, 100f)]
    public float weight = 0f;

    [TextArea(3, 5)]
    /// <summary>アイテムの説明文</summary>
    [Tooltip("アイテムの説明文")]
    public string description = "";
}

