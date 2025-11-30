using UnityEngine;

/// <summary>
/// シングルトンパターンを実装するMonoBehaviourの基底クラス
/// 
/// このクラスを継承することで、自動的にシングルトンパターンが適用されます。
/// 重複インスタンスの検出と破棄、オプションでDontDestroyOnLoadの設定が可能です。
/// 
/// 使用例：
/// public class MyManager : SingletonMonoBehaviour&lt;MyManager&gt;
/// {
///     protected override void OnAwake()
///     {
///         // 初期化処理
///     }
/// }
/// </summary>
/// <typeparam name="T">シングルトンとして管理する型</typeparam>
public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
    /// <summary>シングルトンインスタンス</summary>
    public static T Instance { get; private set; }

    /// <summary>
    /// DontDestroyOnLoadを適用するかどうか
    /// デフォルトはfalse
    /// </summary>
    protected virtual bool ShouldPersistAcrossScenes => false;

    /// <summary>
    /// 重複インスタンス検出時に警告を表示するかどうか
    /// デフォルトはtrue
    /// </summary>
    protected virtual bool ShowDuplicateWarning => true;

    /// <summary>
    /// 重複インスタンス検出時の警告メッセージ
    /// </summary>
    protected virtual string DuplicateWarningMessage => $"Multiple {typeof(T).Name} instances detected. Destroying this one.";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            if (ShowDuplicateWarning)
            {
                Debug.LogWarning(DuplicateWarningMessage);
            }
            Destroy(gameObject);
            return;
        }

        Instance = this as T;

        if (ShouldPersistAcrossScenes)
        {
            DontDestroyOnLoad(gameObject);
        }

        OnAwake();
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    /// <summary>
    /// 初期化処理
    /// 派生クラスでオーバーライドして使用します
    /// </summary>
    protected virtual void OnAwake() { }
}

