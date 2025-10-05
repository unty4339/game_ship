using UnityEngine;

/// <summary>
/// 戦闘イベントを受け取りVFXを生成するブリッジ
/// Resolverの再生成に耐える再購読ロジック
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

    CombatResolver _hooked;

    void OnEnable()  { TryRehook(); }
    void Update()    { TryRehook(); }
    void OnDisable() { Unhook(); }

    void TryRehook()
    {
        var cur = CombatResolver.Instance;
        if (cur == _hooked) return;

        Unhook();
        if (cur != null)
        {
            cur.OnShot += HandleShot;   // 追加
            cur.OnHit  += HandleHit;
            cur.OnMiss += HandleMiss;
            _hooked = cur;
        }
    }

    void Unhook()
    {
        if (_hooked != null)
        {
            _hooked.OnShot -= HandleShot; // 追加
            _hooked.OnHit  -= HandleHit;
            _hooked.OnMiss -= HandleMiss;
            _hooked = null;
        }
    }

    void HandleShot(CombatResolver.ShotEvent e)
    {
        var origin = GetMuzzleWorld(e.attacker);
        var end = e.aimPoint;

        SpawnMuzzle(origin, e.attacker.transform);
        SpawnTracer(origin, end, false);
    }

    void HandleHit(CombatResolver.HitEvent e)
    {
        var end = e.contactPoint != Vector3.zero ? e.contactPoint : e.target.transform.position;
        SpawnImpact(end, e.target.transform);
    }

    void HandleMiss(CombatResolver.MissEvent e)
    {
        // ミス時は着弾スパーク無し OnShotでトレーサーは出ている
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
