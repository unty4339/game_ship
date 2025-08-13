using UnityEngine;

/// <summary>
/// 選択対象であることを示すコンポーネント
/// ハイライト表示などの切り替えを提供
/// </summary>
[RequireComponent(typeof(UnitCore))]
public class Selectable : MonoBehaviour
{
    public bool IsSelected { get; private set; }

    [Header("Visual")]
    [Tooltip("選択時のハイライト用オブジェクト 任意")]
    public GameObject highlight;

    public void SetSelected(bool v)
    {
        IsSelected = v;
        if (highlight != null) highlight.SetActive(v);
    }
}
