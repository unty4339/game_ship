using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// ユニット情報を表示するUI管理クラス
/// 
/// このクラスは以下の機能を提供します：
/// - マウスオーバー時に簡易UIウィンドウを表示
/// - 右クリック時に詳細UIウィンドウを表示
/// - 各ウィンドウは独立したPrefabとして管理
/// 
/// 使用方法：
/// - UnitCoreと同じGameObjectにアタッチ
/// - Collider2Dが必要（マウス検出用）
/// - 簡易UIと詳細UIのPrefabを設定
/// - 各PrefabにはIUnitInfoWindowインターフェースを実装することを推奨
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class UnitInfoUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("UI設定")]
    /// <summary>情報表示用のCanvas（nullの場合は自動検索）</summary>
    [Tooltip("情報表示用のCanvas")]
    public Canvas targetCanvas;

    [Header("UIウィンドウPrefab")]
    /// <summary>簡易情報ウィンドウのPrefab（マウスオーバー時に表示）</summary>
    [Tooltip("簡易情報ウィンドウのPrefab（マウスオーバー時に表示）")]
    public GameObject simpleInfoWindowPrefab;

    /// <summary>詳細情報ウィンドウのPrefab（右クリック時に表示）</summary>
    [Tooltip("詳細情報ウィンドウのPrefab（右クリック時に表示）")]
    public GameObject detailedInfoWindowPrefab;

    /// <summary>簡易UIのユニット位置からのオフセット（ピクセル単位）</summary>
    [Tooltip("簡易UIのユニット位置からのオフセット")]
    public Vector2 simpleUIOffset = new Vector2(200, 100);

    /// <summary>詳細UIの表示位置タイプ</summary>
    public enum DetailedUIPositionType
    {
        /// <summary>ユニット位置基準</summary>
        UnitPosition,
        /// <summary>画面中央</summary>
        ScreenCenter,
        /// <summary>マウス位置</summary>
        MousePosition
    }

    /// <summary>詳細UIの表示位置タイプ</summary>
    [Tooltip("詳細UIの表示位置タイプ")]
    public DetailedUIPositionType detailedUIPositionType = DetailedUIPositionType.ScreenCenter;

    /// <summary>詳細UIのオフセット（positionTypeがUnitPositionまたはMousePositionの場合）</summary>
    [Tooltip("詳細UIのオフセット")]
    public Vector2 detailedUIOffset = Vector2.zero;

    /// <summary>UnitCoreへの参照</summary>
    private UnitCore _core;

    /// <summary>現在表示中の簡易UIパネル</summary>
    private GameObject _currentSimplePanel;

    /// <summary>現在表示中の詳細UIパネル</summary>
    private GameObject _currentDetailedPanel;

    // 注: IUnitInfoWindowインターフェースが実装されている場合は自動的に使用されます
    // Prefab側でIUnitInfoWindowを実装しているコンポーネントにUnitCoreへの参照を設定してください

    /// <summary>カメラへの参照</summary>
    private Camera _camera;

    void Awake()
    {
        _core = GetComponent<UnitCore>();
        if (_core == null)
        {
            Debug.LogWarning($"UnitInfoUI on {gameObject.name} requires UnitCore component.");
        }

        // カメラを取得
        _camera = Camera.main;
        if (_camera == null)
        {
            _camera = FindFirstObjectByType<Camera>();
        }

        // Canvasが設定されていない場合は自動検索
        if (targetCanvas == null)
        {
            targetCanvas = FindFirstObjectByType<Canvas>();
            if (targetCanvas == null)
            {
                Debug.LogWarning($"UnitInfoUI on {gameObject.name} requires a Canvas in the scene.");
            }
        }
    }

    /// <summary>
    /// マウスがオブジェクトに入った時に呼ばれる
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        ShowSimpleInfo();
    }

    /// <summary>
    /// マウスがオブジェクトから出た時に呼ばれる
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        HideSimpleInfo();
    }

    /// <summary>
    /// マウスがオブジェクト上でクリックされた時に呼ばれる
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        // 右クリックの場合のみ詳細UIを表示
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            ShowDetailedInfo();
        }
    }

    /// <summary>
    /// 簡易情報パネルを表示
    /// </summary>
    void ShowSimpleInfo()
    {
        if (_core == null || targetCanvas == null) return;

        // 詳細UIが表示されている場合は簡易UIは表示しない
        if (_currentDetailedPanel != null && _currentDetailedPanel.activeSelf)
        {
            return;
        }

        // Prefabが設定されていない場合は何もしない
        if (simpleInfoWindowPrefab == null) return;

        // 既にパネルが作成されている場合は更新のみ
        if (_currentSimplePanel == null)
        {
            CreateSimplePanel();
        }

        if (_currentSimplePanel != null)
        {
            // Prefab側でUnitInfoUIまたはUnitCoreへの参照を設定できるようにする
            // 例: Prefab側のスクリプトで GetComponent<UnitInfoUI>().GetUnitCore() を使用
            _currentSimplePanel.SetActive(true);
            
            // Prefab側でIUnitInfoWindowインターフェースを実装しているコンポーネントを取得して設定
            var infoWindows = _currentSimplePanel.GetComponentsInChildren<IUnitInfoWindow>();
            foreach (var infoWindow in infoWindows)
            {
                infoWindow.SetUnitCore(_core);
                infoWindow.UpdateInfo();
            }

            UpdateSimplePanelPosition();
        }
    }

    /// <summary>
    /// 簡易情報パネルを非表示
    /// </summary>
    void HideSimpleInfo()
    {
        if (_currentSimplePanel != null)
        {
            _currentSimplePanel.SetActive(false);
        }
    }

    /// <summary>
    /// 詳細情報パネルを表示
    /// </summary>
    void ShowDetailedInfo()
    {
        if (_core == null || targetCanvas == null) return;

        // Prefabが設定されていない場合は何もしない
        if (detailedInfoWindowPrefab == null)
        {
            Debug.LogWarning($"UnitInfoUI on {gameObject.name}: detailedInfoWindowPrefab is not set.");
            return;
        }

        // 既にパネルが作成されている場合は更新のみ
        if (_currentDetailedPanel == null)
        {
            CreateDetailedPanel();
        }

        if (_currentDetailedPanel != null)
        {
            // 簡易UIを非表示
            HideSimpleInfo();

            // Prefab側でUnitInfoUIまたはUnitCoreへの参照を設定できるようにする
            _currentDetailedPanel.SetActive(true);
            
            // Prefab側でIUnitInfoWindowインターフェースを実装しているコンポーネントを取得して設定
            var infoWindows = _currentDetailedPanel.GetComponentsInChildren<IUnitInfoWindow>();
            foreach (var infoWindow in infoWindows)
            {
                infoWindow.SetUnitCore(_core);
                infoWindow.UpdateInfo();
            }

            UpdateDetailedPanelPosition();
        }
    }

    /// <summary>
    /// 詳細情報パネルを非表示
    /// </summary>
    public void HideDetailedInfo()
    {
        if (_currentDetailedPanel != null)
        {
            _currentDetailedPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 簡易情報パネルを作成
    /// </summary>
    void CreateSimplePanel()
    {
        if (targetCanvas == null || simpleInfoWindowPrefab == null) return;

        _currentSimplePanel = Instantiate(simpleInfoWindowPrefab, targetCanvas.transform);
        _currentSimplePanel.name = "SimpleInfoWindow_" + gameObject.name;

        // Prefab側でUnitCoreを参照できるように、UnitInfoUIコンポーネントへの参照を設定
        // Prefab側のスクリプトで、このUnitInfoUIを経由してUnitCoreにアクセスできます

        _currentSimplePanel.SetActive(false);
    }

    /// <summary>
    /// 詳細情報パネルを作成
    /// </summary>
    void CreateDetailedPanel()
    {
        if (targetCanvas == null || detailedInfoWindowPrefab == null) return;

        _currentDetailedPanel = Instantiate(detailedInfoWindowPrefab, targetCanvas.transform);
        _currentDetailedPanel.name = "DetailedInfoWindow_" + gameObject.name;

        // Prefab側でUnitCoreを参照できるように、UnitInfoUIコンポーネントへの参照を設定
        // Prefab側のスクリプトで、このUnitInfoUIを経由してUnitCoreにアクセスできます

        _currentDetailedPanel.SetActive(false);
    }

    /// <summary>
    /// 簡易パネルの位置を更新
    /// </summary>
    void UpdateSimplePanelPosition()
    {
        UpdatePanelPosition(_currentSimplePanel, simpleUIOffset);
    }

    /// <summary>
    /// 詳細パネルの位置を更新
    /// </summary>
    void UpdateDetailedPanelPosition()
    {
        if (_currentDetailedPanel == null || targetCanvas == null) return;

        RectTransform panelRect = _currentDetailedPanel.GetComponent<RectTransform>();

        Vector2 finalPosition = Vector2.zero;
        Vector2 finalOffset = detailedUIOffset;

        switch (detailedUIPositionType)
        {
            case DetailedUIPositionType.UnitPosition:
                // ユニット位置基準
                finalPosition = GetWorldToScreenPosition(transform.position);
                break;

            case DetailedUIPositionType.ScreenCenter:
                // 画面中央
                RectTransform canvasRect = targetCanvas.GetComponent<RectTransform>();
                if (targetCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    finalPosition = new Vector2(Screen.width / 2f, Screen.height / 2f);
                }
                else
                {
                    finalPosition = canvasRect.sizeDelta / 2f;
                }
                finalOffset = Vector2.zero;
                break;

            case DetailedUIPositionType.MousePosition:
                // マウス位置
                finalPosition = Input.mousePosition;
                break;
        }

        SetPanelPosition(_currentDetailedPanel, finalPosition + finalOffset);
    }

    /// <summary>
    /// パネル位置を更新（共通処理）
    /// </summary>
    void UpdatePanelPosition(GameObject panel, Vector2 offset)
    {
        if (panel == null || targetCanvas == null) return;

        Vector3 worldPos = transform.position;
        Vector2 screenPos = GetWorldToScreenPosition(worldPos);
        SetPanelPosition(panel, screenPos + offset);
    }

    /// <summary>
    /// ワールド座標をスクリーン座標に変換
    /// </summary>
    Vector2 GetWorldToScreenPosition(Vector3 worldPos)
    {
        // カメラが取得できていない場合は再試行
        if (_camera == null)
        {
            _camera = Camera.main;
            if (_camera == null)
            {
                _camera = FindFirstObjectByType<Camera>();
            }
            if (_camera == null) return Vector2.zero;
        }

        Vector3 screenPos = _camera.WorldToScreenPoint(worldPos);
        return new Vector2(screenPos.x, screenPos.y);
    }

    /// <summary>
    /// パネルの位置を設定
    /// </summary>
    void SetPanelPosition(GameObject panel, Vector2 position)
    {
        if (panel == null || targetCanvas == null) return;

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        if (panelRect == null) return;

        // ScreenSpaceOverlayの場合とScreenSpaceCameraの場合で処理を分ける
        if (targetCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            // ScreenSpaceOverlayの場合：スクリーン座標を直接使用
            panelRect.position = new Vector3(position.x, position.y, 0);
        }
        else
        {
            // ScreenSpaceCameraまたはWorldSpaceの場合：Canvas座標に変換
            RectTransform canvasRect = targetCanvas.GetComponent<RectTransform>();
            Vector2 localPos;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                position,
                targetCanvas.worldCamera != null ? targetCanvas.worldCamera : _camera,
                out localPos))
            {
                panelRect.anchoredPosition = localPos;
            }
        }
    }

    void Update()
    {
        // 簡易パネルが表示されている場合は位置を更新
        if (_currentSimplePanel != null && _currentSimplePanel.activeSelf)
        {
            UpdateSimplePanelPosition();
        }

        // 詳細パネルが表示されている場合は位置を更新（必要に応じて）
        if (_currentDetailedPanel != null && _currentDetailedPanel.activeSelf)
        {
            // UnitPositionまたはMousePositionの場合は更新
            if (detailedUIPositionType == DetailedUIPositionType.MousePosition)
            {
                UpdateDetailedPanelPosition();
            }
        }
    }

    void OnDestroy()
    {
        // パネルを破棄
        if (_currentSimplePanel != null)
        {
            Destroy(_currentSimplePanel);
        }
        if (_currentDetailedPanel != null)
        {
            Destroy(_currentDetailedPanel);
        }
    }

    /// <summary>
    /// UnitCoreへの参照を取得（Prefab側のスクリプトから呼び出し可能）
    /// </summary>
    public UnitCore GetUnitCore()
    {
        return _core;
    }
}

