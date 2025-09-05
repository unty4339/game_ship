using UnityEngine;

/// <summary>
/// ゲームスピードをキーで切り替えるデバッグ入力
/// </summary>
public class GameSpeedDebug : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0)) GameTime.Pause();
        if (Input.GetKeyDown(KeyCode.Alpha1)) GameTime.Normal();
        if (Input.GetKeyDown(KeyCode.Alpha2)) GameTime.Slow();
        if (Input.GetKeyDown(KeyCode.Alpha3)) GameTime.Fast();
    }
}
