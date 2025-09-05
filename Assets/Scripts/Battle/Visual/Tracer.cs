using UnityEngine;

/// <summary>
/// トレーサーを移動させフェードアウトで消すコンポーネント
/// LineRendererのポジションを制御
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class Tracer : GameTimeBehaviour
{
    [Header("Motion")]
    public float speed = 40f;          // 単位はワールド毎秒
    public float lifeAfterArrive = 0.05f;

    [Header("Appearance")]
    public float startWidth = 0.05f;
    public float endWidth = 0.0f;
    public Gradient colorGradient;

    LineRenderer lr;
    Vector3 startPos;
    Vector3 endPos;
    float dist;
    float t;
    bool launched;
    [Header("Sorting")]
    public string sortingLayerName = "Units"; // 事前に用意
    public int sortingOrder = 50;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.startWidth = startWidth;
        lr.endWidth = endWidth;
        lr.useWorldSpace = true;          // ワールド空間で描画
        lr.numCapVertices = 2;            // 先端を少し丸める
        lr.sortingLayerName = sortingLayerName;
        lr.sortingOrder = sortingOrder;

        // マテリアル未設定ならSprites/Defaultを自動で適用
        if (lr.sharedMaterial == null)
            lr.material = new Material(Shader.Find("Sprites/Default"));

        if (colorGradient.colorKeys.Length > 0) lr.colorGradient = colorGradient;
    }

    /// <summary>
    /// トレーサーを開始位置から終端へ発射
    /// </summary>
    public void Launch(Vector3 origin, Vector3 end, bool hit)
    {
        startPos = origin;
        endPos = end;
        dist = Vector3.Distance(startPos, endPos);
        t = 0f;
        launched = true;

        lr.SetPosition(0, startPos);
        lr.SetPosition(1, startPos);
    }

    void Update()
    {
        if (!launched) return;

        float travel = speed * dt;
        t = Mathf.Min(1f, t + (dist > 0.0001f ? travel / dist : 1f));

        var head = Vector3.Lerp(startPos, endPos, t);
        lr.SetPosition(0, startPos);
        lr.SetPosition(1, head);

        if (t >= 1f)
        {
            launched = false;
            // 少し残してから破棄
            Destroy(gameObject, lifeAfterArrive);
        }
    }
}
