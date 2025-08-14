using UnityEngine;

/// <summary>
/// 体力管理
/// </summary>
public class Health : MonoBehaviour
{
    public int Max = 100;
    public int Current = 100;

    public void Damage(int value)
    {
        Current = Mathf.Max(0, Current - value);
        if (Current == 0) Destroy(gameObject);
    }
}
