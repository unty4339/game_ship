using UnityEngine;

/// <summary>
/// 戦闘イベントを受け取りVFXを生成するブリッジ
/// マズルフラッシュ トレーサー 命中スパークを生成
/// </summary>
public class CombatVFXBridge : MonoBehaviour
{
    [Header("Prefabs")]
    [Tooltip("トレーサー用プレハブ LineRendererとTracerコンポーネントを持つこと")]
    public Tracer tracerPrefab;

    [Tooltip("マズルフラッシュ用プレハブ SimpleFlashを持つこと")]
    public SimpleFlash muzzleFlashPrefab;

    [Tooltip("命中スパーク用プレハブ SimpleFlashを持つこと")]
    public SimpleFlash impactFlashPrefab;

    [Header("Fallbacks")]
    [Tooltip("マズルが見つからない場合のオフセット")]
    public Vector3 muzzleOffset = new Vector3(0.2f, 0f, 0f);

    bool hooked;  // 購読済みフラグ

    void Start()
    {
        if (CombatResolver.Instance != null)
        {
            CombatResolver.Instance.OnHit += HandleHit;
            CombatResolver.Instance.OnMiss += HandleMiss;
        }
    }

    void OnDisable()
    {
        if (CombatResolver.Instance != null)
        {
            CombatResolver.Instance.OnHit -= HandleHit;
            CombatResolver.Instance.OnMiss -= HandleMiss;
        }
    }

    void HandleHit(CombatResolver.HitEvent e)
    {
        var origin = GetMuzzleWorld(e.attacker);
        var end = e.contactPoint != Vector3.zero ? e.contactPoint : e.target.transform.position;

        SpawnMuzzle(origin, e.attacker.transform);
        SpawnTracer(origin, end, true);
        SpawnImpact(end, e.target.transform);
    }

    void HandleMiss(CombatResolver.MissEvent e)
    {
        var origin = GetMuzzleWorld(e.attacker);
        var end = e.target != null ? e.target.transform.position : origin + Vector3.right * 3f;

        SpawnMuzzle(origin, e.attacker.transform);
        SpawnTracer(origin, end, false);
        // ミス時は着弾スパークは生成しない
    }

    Vector3 GetMuzzleWorld(GameObject attacker)
    {
        var t = attacker.transform.Find("muzzle");
        if (t != null) return t.position;
        return attacker.transform.position + muzzleOffset;
    }

    void SpawnMuzzle(Vector3 pos, Transform parent)
    {
        if (muzzleFlashPrefab == null) return;
        var fx = Instantiate(muzzleFlashPrefab, pos, Quaternion.identity, parent);
        fx.PlayOnce();
    }

    void SpawnTracer(Vector3 origin, Vector3 end, bool hit)
    {
        if (tracerPrefab == null) return;
        var tr = Instantiate(tracerPrefab, origin, Quaternion.identity);
        tr.Launch(origin, end, hit);
    }

    void SpawnImpact(Vector3 pos, Transform parent)
    {
        if (impactFlashPrefab == null) return;
        var fx = Instantiate(impactFlashPrefab, pos, Quaternion.identity, parent);
        fx.PlayOnce();
    }
}
