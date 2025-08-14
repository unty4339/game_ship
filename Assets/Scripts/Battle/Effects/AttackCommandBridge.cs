using UnityEngine;
using System.Linq;

/// <summary>
/// 右クリックで敵を指定したとき攻撃命令を配信
/// SelectionManagerとCommandControllerの間を橋渡し
/// </summary>
public class AttackCommandBridge : MonoBehaviour
{
    void OnEnable()
    {
        var cmd = FindObjectOfType<CommandController>();
        if (cmd != null) cmd.OnRightClickUnit += HandleAttackCommand;
    }

    void OnDisable()
    {
        var cmd = FindObjectOfType<CommandController>();
        if (cmd != null) cmd.OnRightClickUnit -= HandleAttackCommand;
    }

    void HandleAttackCommand(UnitCore enemy)
    {
        var sels = SelectionManager.Instance?.Current;
        if (sels == null || sels.Count == 0) return;

        foreach (var s in sels)
        {
            if (s == null) continue;
            var tgt = s.GetComponent<UnitTargeting>();
            if (tgt == null) tgt = s.gameObject.AddComponent<UnitTargeting>();
            tgt.SetPriorityTarget(enemy);
        }
    }

}
