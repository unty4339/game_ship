using UnityEngine;

/// <summary>
/// ゲーム内の時間スケールを管理
/// Delta はスケール適用 Unscaled は非適用
/// </summary>
public class GameTime : MonoBehaviour
{
    public static GameTime Instance { get; private set; }

    [Range(0f, 4f)] public float timeScale = 1f;
    public bool paused => Mathf.Approximately(timeScale, 0f);

    float _delta;
    float _unscaled;

    public static float Delta => Instance != null ? Instance._delta : Time.deltaTime;
    public static float Unscaled => Instance != null ? Instance._unscaled : Time.unscaledDeltaTime;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        _unscaled = Time.unscaledDeltaTime;
        _delta = _unscaled * Mathf.Max(0f, timeScale);
    }

    public static void SetSpeed(float scale)
    {
        if (Instance == null) return;
        Instance.timeScale = Mathf.Max(0f, scale);
    }

    public static void Pause()  => SetSpeed(0f);
    public static void Slow()   => SetSpeed(0.5f);
    public static void Normal() => SetSpeed(1f);
    public static void Fast()   => SetSpeed(2f);
}
