using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

/// <summary>
/// マウスオーバー情報UIを管理するマネージャークラス
/// 
/// このクラスは以下の機能を提供します：
/// - Cameraからレイキャストを飛ばしてマウスオーバー対象を検出
/// - 対象に応じて適切なPrefabのウィンドウを表示
/// - ウィンドウをマウス位置に追従
/// - すべてのPrefab参照を一元管理
/// 
/// 使用方法：
/// 1. シーンにこのスクリプトをアタッチしたGameObjectを配置
/// 2. InspectorでPrefabとCanvasを設定
/// 3. 自動的にレイキャストでマウスオーバーを検出して表示
/// </summary>
public class HoverInfoUIManager : MonoBehaviour
{
    [Header("UI設定")]
    /// <summary>情報表示用のCanvas（nullの場合は自動検索）</summary>
    [Tooltip("情報表示用のCanvas")]
    public Canvas targetCanvas;

    [Header("情報ウィンドウPrefab")]
    /// <summary>ユニット用の簡易情報ウィンドウPrefab</summary>
    [Tooltip("ユニット用の簡易情報ウィンドウPrefab")]
    public GameObject unitSimpleInfoWindowPrefab;

    /// <summary>ユニット用の詳細情報ウィンドウPrefab</summary>
    [Tooltip("ユニット用の詳細情報ウィンドウPrefab（右クリック時）")]
    public GameObject unitDetailedInfoWindowPrefab;

    /// <summary>障害物用の情報ウィンドウPrefab</summary>
    [Tooltip("障害物用の情報ウィンドウPrefab")]
    public GameObject obstacleInfoWindowPrefab;

    [Header("レイキャスト設定")]
    /// <summary>レイキャスト対象のレイヤーマスク</summary>
    [Tooltip("レイキャスト対象のレイヤーマスク")]
    public LayerMask raycastLayerMask = ~0;

    /// <summary>レイキャスト最大距離</summary>
    [Tooltip("レイキャスト最大距離")]
    public float raycastMaxDistance = 100f;

    [Header("表示設定")]
    /// <summary>ウィンドウのマウス位置からのオフセット（ピクセル単位）</summary>
    [Tooltip("ウィンドウのマウス位置からのオフセット")]
    public Vector2 windowOffset = new Vector2(10, 10);

    /// <summary>詳細UIの表示位置タイプ</summary>
    public enum DetailedUIPositionType
    {
        /// <summary>マウス位置</summary>
        MousePosition,
        /// <summary>画面中央</summary>
        ScreenCenter,
    }

    /// <summary>詳細UIの表示位置タイプ</summary>
    [Tooltip("詳細UIの表示位置タイプ")]
    public DetailedUIPositionType detailedUIPositionType = DetailedUIPositionType.ScreenCenter;

    /// <summary>詳細UIのオフセット</summary>
    [Tooltip("詳細UIのオフセット")]
    public Vector2 detailedUIOffset = Vector2.zero;

    /// <summary>カメラへの参照</summary>
    private Camera _camera;

    /// <summary>現在マウスオーバー中のオブジェクト</summary>
    private GameObject _currentHoverTarget;

    /// <summary>現在表示中の簡易UIウィンドウ</summary>
    private GameObject _currentSimpleWindow;

    /// <summary>現在表示中の詳細UIウィンドウ</summary>
    private GameObject _currentDetailedWindow;

    /// <summary>簡易ウィンドウのPrefabマッピング（InfoKey -> Prefab）</summary>
    private Dictionary<string, GameObject> _simpleWindowPrefabs = new Dictionary<string, GameObject>();

    /// <summary>詳細ウィンドウのPrefabマッピング（InfoKey -> Prefab）</summary>
    private Dictionary<string, GameObject> _detailedWindowPrefabs = new Dictionary<string, GameObject>();

    void Awake()
    {
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
                Debug.LogWarning($"HoverInfoUIManager on {gameObject.name} requires a Canvas in the scene.");
            }
        }

        // Prefabマッピングを初期化
        InitializePrefabMappings();
    }

    /// <summary>
    /// Prefabマッピングを初期化
    /// </summary>
    void InitializePrefabMappings()
    {
        _simpleWindowPrefabs["Unit"] = unitSimpleInfoWindowPrefab;
        _simpleWindowPrefabs["Obstacle"] = obstacleInfoWindowPrefab;

        _detailedWindowPrefabs["Unit"] = unitDetailedInfoWindowPrefab;
        // 障害物は詳細UIなし（必要に応じて追加）
    }

    void Update()
    {
        // UI上にマウスがある場合は何もしない
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            ClearHover();
            return;
        }

        // レイキャストでマウスオーバー対象を検出
        DetectHoverTarget();

        // 右クリックで詳細UIを表示
        if (Input.GetMouseButtonDown(1))
        {
            ShowDetailedWindow();
        }

        // 簡易ウィンドウの位置をマウス位置に更新
        UpdateWindowPosition();
    }

    /// <summary>
    /// レイキャストでマウスオーバー対象を検出
    /// </summary>
    void DetectHoverTarget()
    {
        if (_camera == null) return;

        // マウス位置からレイキャスト
        Vector3 mouseWorld = _camera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        RaycastHit2D hit = Physics2D.Raycast(mouseWorld, Vector2.zero, raycastMaxDistance, raycastLayerMask);

        GameObject newTarget = null;
        string infoKey = null;

        if (hit.collider != null)
        {
            // UnitCoreを検出
            var unitCore = hit.collider.GetComponentInParent<UnitCore>();
            if (unitCore != null)
            {
                newTarget = unitCore.gameObject;
                infoKey = "Unit";
            }
            // Obstacleを検出
            else
            {
                var obstacle = hit.collider.GetComponentInParent<Obstacle>();
                if (obstacle != null)
                {
                    newTarget = obstacle.gameObject;
                    infoKey = "Obstacle";
                }
            }

            // IInfoDisplayableインターフェースを実装している場合
            if (newTarget == null)
            {
                var infoDisplayable = hit.collider.GetComponentInParent<IInfoDisplayable>();
                if (infoDisplayable != null && infoDisplayable is MonoBehaviour mb)
                {
                    newTarget = mb.gameObject;
                    infoKey = infoDisplayable.GetInfoKey();
                }
            }
        }

        // 対象が変わった場合
        if (newTarget != _currentHoverTarget)
        {
            ClearHover();
            _currentHoverTarget = newTarget;

            if (newTarget != null && !string.IsNullOrEmpty(infoKey))
            {
                ShowSimpleWindow(newTarget, infoKey);
            }
        }
    }

    /// <summary>
    /// 簡易ウィンドウを表示
    /// </summary>
    void ShowSimpleWindow(GameObject target, string infoKey)
    {
        // 詳細ウィンドウが表示されている場合は簡易ウィンドウは表示しない
        if (_currentDetailedWindow != null && _currentDetailedWindow.activeSelf)
        {
            return;
        }

        // Prefabを取得
        if (!_simpleWindowPrefabs.TryGetValue(infoKey, out GameObject prefab) || prefab == null)
        {
            return;
        }

        // ウィンドウを作成または取得
        if (_currentSimpleWindow == null)
        {
            _currentSimpleWindow = Instantiate(prefab, targetCanvas.transform);
            _currentSimpleWindow.name = $"SimpleInfoWindow_{infoKey}";
        }
        else
        {
            // 既存のウィンドウを破棄して新しく作成
            Destroy(_currentSimpleWindow);
            _currentSimpleWindow = Instantiate(prefab, targetCanvas.transform);
            _currentSimpleWindow.name = $"SimpleInfoWindow_{infoKey}";
        }

        // 対象オブジェクトの情報を設定
        SetWindowTarget(_currentSimpleWindow, target);

        _currentSimpleWindow.SetActive(true);
    }

    /// <summary>
    /// 詳細ウィンドウを表示
    /// </summary>
    void ShowDetailedWindow()
    {
        if (_currentHoverTarget == null) return;

        // InfoKeyを取得
        string infoKey = GetInfoKey(_currentHoverTarget);
        if (string.IsNullOrEmpty(infoKey)) return;

        // Prefabを取得
        if (!_detailedWindowPrefabs.TryGetValue(infoKey, out GameObject prefab) || prefab == null)
        {
            return;
        }

        // 簡易ウィンドウを非表示
        HideSimpleWindow();

        // ウィンドウを作成または取得
        if (_currentDetailedWindow == null)
        {
            _currentDetailedWindow = Instantiate(prefab, targetCanvas.transform);
            _currentDetailedWindow.name = $"DetailedInfoWindow_{infoKey}";
        }
        else
        {
            // 既存のウィンドウを破棄して新しく作成
            Destroy(_currentDetailedWindow);
            _currentDetailedWindow = Instantiate(prefab, targetCanvas.transform);
            _currentDetailedWindow.name = $"DetailedInfoWindow_{infoKey}";
        }

        // 対象オブジェクトの情報を設定
        SetWindowTarget(_currentDetailedWindow, _currentHoverTarget);

        // 位置を設定
        UpdateDetailedWindowPosition();

        _currentDetailedWindow.SetActive(true);
    }

    /// <summary>
    /// InfoKeyを取得
    /// </summary>
    string GetInfoKey(GameObject target)
    {
        if (target == null) return null;

        // IInfoDisplayableインターフェースを確認
        var infoDisplayable = target.GetComponent<IInfoDisplayable>();
        if (infoDisplayable != null)
        {
            return infoDisplayable.GetInfoKey();
        }

        // UnitCoreを確認
        if (target.GetComponent<UnitCore>() != null)
        {
            return "Unit";
        }

        // Obstacleを確認
        if (target.GetComponent<Obstacle>() != null)
        {
            return "Obstacle";
        }

        return null;
    }

    /// <summary>
    /// ウィンドウに対象オブジェクトの情報を設定
    /// </summary>
    void SetWindowTarget(GameObject window, GameObject target)
    {
        if (window == null || target == null) return;

        // IUnitInfoWindowインターフェースを実装しているコンポーネントを取得
        var unitInfoWindows = window.GetComponentsInChildren<IUnitInfoWindow>();
        foreach (var infoWindow in unitInfoWindows)
        {
            var unitCore = target.GetComponent<UnitCore>();
            if (unitCore != null)
            {
                infoWindow.SetUnitCore(unitCore);
                infoWindow.UpdateInfo();
            }
        }

        // IObstacleInfoWindowインターフェースを実装しているコンポーネントを取得
        var obstacleInfoWindows = window.GetComponentsInChildren<IObstacleInfoWindow>();
        foreach (var infoWindow in obstacleInfoWindows)
        {
            var obstacle = target.GetComponent<Obstacle>();
            if (obstacle != null)
            {
                infoWindow.SetObstacle(obstacle);
                infoWindow.UpdateInfo();
            }
        }
    }

    /// <summary>
    /// マウスオーバーをクリア
    /// </summary>
    void ClearHover()
    {
        if (_currentHoverTarget != null)
        {
            _currentHoverTarget = null;
            HideSimpleWindow();
        }
    }

    /// <summary>
    /// 簡易ウィンドウを非表示
    /// </summary>
    void HideSimpleWindow()
    {
        if (_currentSimpleWindow != null)
        {
            _currentSimpleWindow.SetActive(false);
        }
    }

    /// <summary>
    /// 詳細ウィンドウを非表示
    /// </summary>
    public void HideDetailedWindow()
    {
        if (_currentDetailedWindow != null)
        {
            _currentDetailedWindow.SetActive(false);
        }
    }

    /// <summary>
    /// ウィンドウ位置を更新（簡易ウィンドウはマウス位置に追従）
    /// </summary>
    void UpdateWindowPosition()
    {
        if (_currentSimpleWindow != null && _currentSimpleWindow.activeSelf)
        {
            SetPanelPosition(_currentSimpleWindow, (Vector2)Input.mousePosition + windowOffset);
        }
    }

    /// <summary>
    /// 詳細ウィンドウの位置を更新
    /// </summary>
    void UpdateDetailedWindowPosition()
    {
        if (_currentDetailedWindow == null || targetCanvas == null) return;

        Vector2 targetPosition = Vector2.zero;

        switch (detailedUIPositionType)
        {
            case DetailedUIPositionType.MousePosition:
                targetPosition = (Vector2)Input.mousePosition + detailedUIOffset;
                break;

            case DetailedUIPositionType.ScreenCenter:
                RectTransform canvasRect = targetCanvas.GetComponent<RectTransform>();
                if (targetCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    targetPosition = new Vector2(Screen.width / 2f, Screen.height / 2f);
                }
                else
                {
                    targetPosition = canvasRect.sizeDelta / 2f;
                }
                targetPosition += detailedUIOffset;
                break;
        }

        SetPanelPosition(_currentDetailedWindow, targetPosition);
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

    void OnDestroy()
    {
        // ウィンドウを破棄
        if (_currentSimpleWindow != null)
        {
            Destroy(_currentSimpleWindow);
        }
        if (_currentDetailedWindow != null)
        {
            Destroy(_currentDetailedWindow);
        }
    }
}

