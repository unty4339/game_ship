using System.Text;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// CombatantStatusの内容をTMPで画面上に表示するデバッグUI
/// 2D向けにスクリーン座標で追従表示
/// </summary>
[RequireComponent(typeof(CombatantStatus))]
public class CombatStatusTMPUI : MonoBehaviour
{
    [Header("Prefab and Canvas")]
    [Tooltip("TextMeshProUGUIのプレハブ")]
    public TextMeshProUGUI labelPrefab;

    [Tooltip("表示先のCanvas 無指定なら自動探索または自動生成")]
    public Canvas targetCanvas;

    [Header("Screen offset")]
    public Vector2 screenOffset = new Vector2(0f, 30f);

    CombatantStatus _status;
    Camera _cam;
    RectTransform _rt;

    void Awake()
    {
        _status = GetComponent<CombatantStatus>() ?? gameObject.AddComponent<CombatantStatus>();
        _cam = Camera.main;
        EnsureCanvas();
        EnsureLabel();
    }

    void EnsureCanvas()
    {
        if (targetCanvas != null) return;

        var tagged = GameObject.FindGameObjectWithTag("MainHUD");
        if (tagged != null) targetCanvas = tagged.GetComponent<Canvas>();
        if (targetCanvas != null) return;

        targetCanvas = FindObjectOfType<Canvas>();
        if (targetCanvas != null) return;

        // 無ければOverlayのCanvasを生成
        var go = new GameObject("HUDCanvas_Auto");
        targetCanvas = go.AddComponent<Canvas>();
        targetCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        go.AddComponent<CanvasScaler>();
        go.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        // タグがあるなら付与し再利用しやすく
        // go.tag = "MainHUD"; // 必要なら有効化
    }

    void EnsureLabel()
    {
        if (labelPrefab == null)
        {
            // 最小限の動作のためランタイム生成
            var go = new GameObject("StatusLabel_TMP");
            go.transform.SetParent(targetCanvas.transform, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = 16;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
            _rt = tmp.rectTransform;
        }
        else
        {
            var inst = Instantiate(labelPrefab, targetCanvas.transform);
            inst.raycastTarget = false;
            _rt = inst.rectTransform;
        }
        _rt.sizeDelta = new Vector2(260f, 40f);
    }

    void LateUpdate()
    {
        if (_rt == null) return;
        if (_cam == null) _cam = Camera.main;

        // スクリーン座標へ変換して追従
        var screen = RectTransformUtility.WorldToScreenPoint(_cam, transform.position);
        _rt.position = screen + screenOffset;

        // 表示更新
        var text = BuildText();
        var tmp = _rt.GetComponent<TextMeshProUGUI>();
        if (tmp != null) tmp.text = text;

        // 死亡時は自動破棄
        if (_status.isDead) Destroy(_rt.gameObject);
    }

    string BuildText()
    {
        var sb = new StringBuilder();
        sb.Append($"HP {_status.currentHP}/{_status.maxHP}  ");
        sb.Append($"Stun {_status.stunValue:0.0}\n");

        foreach (var kv in _status.Effects)
        {
            switch (kv.Value)
            {
                case BleedEffect b:
                    sb.Append($"Bleed {b.bleedAmount:0.0}  ");
                    break;
                case HemorrhageEffect h:
                    sb.Append($"Hemo {h.hemoAmount:0.0}  ");
                    break;
                default:
                    sb.Append($"{kv.Key}  ");
                    break;
            }
        }
        return sb.ToString();
    }

    void OnDisable()
    {
        if (_rt != null) Destroy(_rt.gameObject);
    }
}
