using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// 数字キーとスペースでゲーム速度を切り替えTMPで倍率を表示
/// 1=0.5x 2=1x 3=2x Space=0x
/// </summary>
public class GameSpeedController : MonoBehaviour
{
    [Header("Keys")]
    public KeyCode slowKey = KeyCode.Alpha1;   // 0.5x
    public KeyCode normalKey = KeyCode.Alpha2; // 1x
    public KeyCode fastKey = KeyCode.Alpha3;   // 2x
    public KeyCode pauseKey = KeyCode.Space;   // 0x

    [Header("UI")]
    [Tooltip("倍率表示に使うTextMeshProUGUIのプレハブ 任意")]
    public TextMeshProUGUI labelPrefab;
    [Tooltip("表示先のCanvas 任意 未指定なら自動で探すか生成")]
    public Canvas targetCanvas;
    [Tooltip("画面上の位置 Pivotは左上で固定")]
    public Vector2 screenOffset = new Vector2(12f, -12f);
    [Tooltip("フォントサイズ")]
    public float fontSize = 20f;

    [Header("Integration")]
    [Tooltip("UnityのTime.timeScaleも同期するか 任意")]
    public bool alsoAffectUnityTimeScale = false;

    TextMeshProUGUI _label;
    RectTransform _rt;
    float _lastShown = -1f;

    void Start()
    {
        EnsureCanvas();
        EnsureLabel();
        RefreshLabel(true);
    }

    void Update()
    {
        if (Input.GetKeyDown(slowKey))   SetSpeed(0.5f);
        if (Input.GetKeyDown(normalKey)) SetSpeed(1f);
        if (Input.GetKeyDown(fastKey))   SetSpeed(2f);
        if (Input.GetKeyDown(pauseKey))  SetSpeed(0f);

        RefreshLabel(false);
    }

    void EnsureCanvas()
    {
        if (targetCanvas != null) return;

        // タグMainHUDを優先的に探す
        var tagged = GameObject.FindGameObjectWithTag("MainHUD");
        if (tagged != null) targetCanvas = tagged.GetComponent<Canvas>();
        if (targetCanvas != null) return;

        // 既存のCanvasから一つ拝借
        targetCanvas = FindObjectOfType<Canvas>();
        if (targetCanvas != null) return;

        // 無ければOverlayのCanvasを自動生成
        var go = new GameObject("HUDCanvas_Auto");
        targetCanvas = go.AddComponent<Canvas>();
        targetCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        go.AddComponent<CanvasScaler>();
        go.AddComponent<GraphicRaycaster>();
    }

    void EnsureLabel()
    {
        if (labelPrefab != null)
        {
            _label = Instantiate(labelPrefab, targetCanvas.transform);
            _rt = _label.rectTransform;
        }
        else
        {
            var go = new GameObject("GameSpeedLabel_TMP");
            go.transform.SetParent(targetCanvas.transform, false);
            _label = go.AddComponent<TextMeshProUGUI>();
            _label.fontSize = fontSize;
            _label.alignment = TextAlignmentOptions.TopLeft;
            _label.raycastTarget = false;
            _rt = _label.rectTransform;
            _rt.sizeDelta = new Vector2(220f, 36f);
        }

        // 左上固定
        _rt.anchorMin = new Vector2(0f, 1f);
        _rt.anchorMax = new Vector2(0f, 1f);
        _rt.pivot = new Vector2(0f, 1f);
        _rt.anchoredPosition = screenOffset;
    }

    void SetSpeed(float scale)
    {
        GameTime.SetSpeed(scale);
        if (alsoAffectUnityTimeScale)
            Time.timeScale = Mathf.Max(0f, scale);
        RefreshLabel(true);
    }

    void RefreshLabel(bool force)
    {
        if (_label == null || GameTime.Instance == null) return;
        float s = GameTime.Instance.timeScale;
        if (!force && Mathf.Approximately(s, _lastShown)) return;

        string text = s <= 0f ? "Time x0 PAUSED" :
                      Mathf.Approximately(s, 0.5f) ? "Time x0.5" :
                      Mathf.Approximately(s, 1f) ? "Time x1" :
                      Mathf.Approximately(s, 2f) ? "Time x2" :
                      $"Time x{s:0.##}";

        _label.text = text;
        _lastShown = s;
    }
}
