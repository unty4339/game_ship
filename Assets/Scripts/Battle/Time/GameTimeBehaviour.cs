using UnityEngine;

/// <summary>
/// スケール適用と非適用の時間を提供する基底
/// Gameplayはdt UIや入力はudtを使う
/// </summary>
public abstract class GameTimeBehaviour : MonoBehaviour
{
    protected float dt  => GameTime.Delta;
    protected float udt => GameTime.Unscaled;
}
