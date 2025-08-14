using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// クリックとドラッグでユニットを選択するマネージャ
/// 左クリックで単体選択 ドラッグで矩形選択 Shiftで加算選択
/// </summary>
public class SelectionManager : MonoBehaviour
{
    public static SelectionManager Instance { get; private set; }

    /// <summary>現在選択中のユニット集合</summary>
    public HashSet<Selectable> Current = new HashSet<Selectable>();

    [Header("Rect select")]
    public KeyCode addKey = KeyCode.LeftShift;
    public Color rectColor = new Color(0.2f, 0.6f, 1f, 0.2f);
    public Color rectBorder = new Color(0.2f, 0.6f, 1f, 1f);

    Vector2 _dragStart;
    bool _dragging;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _dragging = true;
            _dragStart = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            var add = Input.GetKey(addKey);
            var start = _dragStart;
            var end = (Vector2)Input.mousePosition;
            _dragging = false;

            if ((end - start).sqrMagnitude < 6f)
                ClickSelect(end, add);
            else
                BoxSelect(start, end, add);
        }
    }

    void ClickSelect(Vector2 mouseScreen, bool additive)
    {
        if (!additive) ClearSelection();

        var world = Camera.main.ScreenToWorldPoint(mouseScreen);
        var hit = Physics2D.Raycast(world, Vector2.zero, 0.01f);
        if (hit.collider == null) return;

        var sel = hit.collider.GetComponentInParent<Selectable>();
        if (sel == null) return;

        ToggleOn(sel);
    }

    void BoxSelect(Vector2 a, Vector2 b, bool additive)
    {
        if (!additive) ClearSelection();

        var min = Vector2.Min(a, b);
        var max = Vector2.Max(a, b);
        var rect = new Rect(min, max - min);

        // 全Selectableを走査してざっくり中心点で判定
        var all = FindObjectsOfType<Selectable>();
        foreach (var s in all)
        {
            var pos = s.transform.position;
            var screen = Camera.main.WorldToScreenPoint(pos);
            if (rect.Contains(screen)) ToggleOn(s);
        }
    }

    void ToggleOn(Selectable s)
    {
        if (Current.Add(s)) s.SetSelected(true);
    }

    public void ClearSelection()
    {
        foreach (var s in Current) if (s != null) s.SetSelected(false);
        Current.Clear();
    }

    void OnGUI()
    {
        if (!_dragging) return;
        var a = _dragStart;
        var b = (Vector2)Input.mousePosition;
        var min = Vector2.Min(a, b);
        var size = Vector2.Max(a, b) - min;
        var rect = new Rect(min.x, Screen.height - min.y - size.y, size.x, size.y);

        // 塗り
        var c = GUI.color;
        GUI.color = rectColor;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        // 枠
        GUI.color = rectBorder;
        DrawRectBorder(rect, 2f);
        GUI.color = c;
    }

    void DrawRectBorder(Rect r, float thickness)
    {
        GUI.DrawTexture(new Rect(r.xMin, r.yMin, r.width, thickness), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(r.xMin, r.yMax - thickness, r.width, thickness), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(r.xMin, r.yMin, thickness, r.height), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(r.xMax - thickness, r.yMin, thickness, r.height), Texture2D.whiteTexture);
    }
}
