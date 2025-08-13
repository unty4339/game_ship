using UnityEngine;

/// <summary>
/// 単発のフラッシュ演出を行う簡易コンポーネント
/// SpriteRenderer のスケールとアルファを時間で減衰
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class SimpleFlash : MonoBehaviour
{
    public float life = 0.08f;
    public float startScale = 1f;
    public float endScale = 0.3f;
    public float startAlpha = 1f;
    public float endAlpha = 0f;

    float t;
    SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// 一度だけ再生開始
    /// </summary>
    public void PlayOnce()
    {
        t = 0f;
        transform.localScale = Vector3.one * startScale;
        SetAlpha(startAlpha);
        // ランダム回転で見た目に変化
        transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
        // 自動破棄
        Destroy(gameObject, life);
    }

    void Update()
    {
        t += Time.deltaTime / Mathf.Max(0.0001f, life);
        float s = Mathf.Lerp(startScale, endScale, t);
        float a = Mathf.Lerp(startAlpha, endAlpha, t);
        transform.localScale = Vector3.one * s;
        SetAlpha(a);
    }

    void SetAlpha(float a)
    {
        var c = sr.color;
        c.a = a;
        sr.color = c;
    }
}
