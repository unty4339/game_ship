using UnityEngine;
using Pathfinding;
using UnityEngine.EventSystems;

/// <summary>
/// ユニットの移動を管理するクラス
/// </summary>
public class UnitMover : MonoBehaviour
{
    private Path path;
    private float speed = 2f;
    private float skipFirstNodeDistance = 0.5f;
    private float waypointReachedDistance = 0.01f;
    private int currentWaypoint = 0;
    private bool reachedEndOfPath = false;
    private LineRenderer lineRenderer;

    void Awake()
    {
        InitializeLineRenderer();
    }

    public void InitializeLineRenderer()
    {
        // LineRendererを初期化するメソッド
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.03f;
        lineRenderer.endWidth = 0.03f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
    }

    public void SetPathToLineRenderer(Path path)
    {
        // パスの表示を更新
        lineRenderer.positionCount = path.vectorPath.Count;
        for (int i = 0; i < path.vectorPath.Count; i++)
        {
            lineRenderer.SetPosition(i, path.vectorPath[i]);
        }
    }
    public void UpdateLineRenderer()
    {
        // LineRendererを更新するメソッド - 現在位置から目的地までのパスを表示
        if (path == null || currentWaypoint >= path.vectorPath.Count) 
        {
            // パスがない場合はLineRendererを非表示にする
            lineRenderer.positionCount = 0;
            return;
        }

        // 現在のウェイポイントから残りのパスを表示
        lineRenderer.positionCount = path.vectorPath.Count - currentWaypoint + 1;
        lineRenderer.SetPosition(0, transform.position); // 現在位置を開始点に

        for (int i = currentWaypoint; i < path.vectorPath.Count; i++)
        {
            lineRenderer.SetPosition(i - currentWaypoint + 1, path.vectorPath[i]);
        }
    }

    public void MoveTo(Vector2 target)
    {
        Debug.Log("MoveTo" + target);
        ABPath newPath = SetPath(target);
        if (!newPath.error)
        {
            path = newPath;
            currentWaypoint = 0;
            SetPathToLineRenderer(path);
        }
    }

    private ABPath SetPath(Vector2 target)
    {
        ABPath newPath = ABPath.Construct(transform.position, target);
        AstarPath.StartPath(newPath);
        newPath.BlockUntilCalculated();

        // 最初のウェイポイントまでの距離を確認
        if (newPath.vectorPath.Count > 0)
        {
            float distanceToFirstNode = Vector2.Distance(transform.position, newPath.vectorPath[0]);
            // 距離が閾値未満なら最初のノードをスキップ
            if (distanceToFirstNode < skipFirstNodeDistance && newPath.vectorPath.Count > 1)
            {
                newPath.vectorPath.RemoveAt(0);
            }
        }
        return newPath;
    }

    void Update()
    {
        // 移動処理
        UpdateMove();

        // 

        // パスの表示を更新
        UpdateLineRenderer();
    }

    void UpdateMove()
    {
        // パスが設定されていない場合は処理を終了
        if (path == null)
        {
            return;
        }

        // 目的地に到達したかどうかをチェック
        reachedEndOfPath = currentWaypoint >= path.vectorPath.Count;
        if (reachedEndOfPath)
        {
            return;
        }

        // 次のウェイポイントへの方向ベクトルを計算
        Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - (Vector2)transform.position).normalized;
        
        // 重量による移動速度ペナルティを取得
        float speedPenalty = 1.0f;
        var core = GetComponent<UnitCore>();
        if (core != null && core.Inventory != null)
        {
            speedPenalty = core.Inventory.GetMoveSpeedPenalty();
        }

        // 速度と時間に基づいて移動量を計算（重量ペナルティを適用）
        Vector2 movement = direction * speed * speedPenalty * Time.deltaTime;
        // ユニットを移動
        transform.Translate(movement);

        // 現在位置と次のウェイポイントとの距離を計算
        float distance = Vector2.Distance(transform.position, path.vectorPath[currentWaypoint]);
        // 十分に近づいたら次のウェイポイントへ
        if (distance < waypointReachedDistance)
        {
            currentWaypoint++;
        }        
    }

    void Start()
    {
        MoveTo(new Vector2(10, 2));
    }

}
