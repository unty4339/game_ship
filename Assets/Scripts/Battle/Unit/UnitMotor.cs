using UnityEngine;

/// <summary>
/// リアルタイムな位置補間を担当
/// GridAgentが承認したセルへ移動
/// </summary>
public class UnitMotor : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float snapThreshold = 0.03f; // 到達とみなす距離
    [SerializeField] bool autoSnapOnArrival = true; // 到達時に目標へスナップするか
    public bool IsMoving { get; private set; }
    
    /// <summary>
    /// 目標座標へ向けて移動し到達したらtrueを返す
    /// autoSnapOnArrivalが有効なら最終位置を目標にスナップ
    /// </summary>
    public bool MoveTowards(Vector3 worldTarget)
    {
        var pos = transform.position;
        var dir = worldTarget - pos;
        dir.z = 0f;

        float dist = dir.magnitude;
        if (dist <= snapThreshold)
        {
            if (autoSnapOnArrival)
                transform.position = new Vector3(worldTarget.x, worldTarget.y, 0f);
            IsMoving = false;
            return true;
        }

        IsMoving = true;
        transform.position = pos + (dir / dist) * moveSpeed * Time.deltaTime;
        return false;
    }
}
