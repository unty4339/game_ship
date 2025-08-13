using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A*により経路を取得しウェイポイント列をMotorへ供給
/// 再探索と経路追従を管理
/// </summary>
public class UnitPathAgent : MonoBehaviour
{
    [SerializeField] float repathInterval = 0.3f;
    Queue<Vector3Int> _queue = new Queue<Vector3Int>();
    float _accum;

    public void SetDestination(Vector3Int goal)
    {
        var start = GetComponent<GridAgent>().Cell;
        var path = FindObjectOfType<AStarPathfinder>().FindPath(start, goal);
        _queue.Clear();
        if (path == null) return;
        for (int i = 1; i < path.Count; i++) _queue.Enqueue(path[i]);
    }

    void Update()
    {
        var motor = GetComponent<UnitMotor>();
        var mm = MapManager.Instance;
        if (_queue.Count == 0) return;

        var nextCell = _queue.Peek();
        var world = mm.CellToWorldCenter(nextCell);
        motor.MoveTowards(world);

        if (motor.MoveTowards(world))
        {
            GetComponent<GridAgent>().TryReserve(nextCell);
            _queue.Dequeue();
        }
    }
}
