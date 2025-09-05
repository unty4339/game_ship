using UnityEngine;

/// <summary>
/// 足りない基盤オブジェクトを自動生成する簡易ブート
/// テスト復旧用
/// </summary>
public class Bootstrapper : MonoBehaviour
{
    void Awake()
    {
        Ensure<GameTime>("GameTime_Auto");
        Ensure<UnitDirectory>("UnitDirectory_Auto");
        // Ensure<MapManager>("MapManager_Auto");
        Ensure<LoSManager>("LoSManager_Auto");
        Ensure<CombatResolver>("CombatResolver_Auto");
        Ensure<WeaponEffectApplier>("WeaponEffectApplier_Auto");
        Ensure<RosterManager>("RosterManager_Auto");
        Ensure<UnitFactory>("UnitFactory_Auto");
        Ensure<BattleSpawner>("BattleSpawner_Auto");
    }

    T Ensure<T>(string name) where T : Component
    {
        var found = FindObjectOfType<T>();
        if (found != null) return found;
        var go = new GameObject(name);
        Debug.Log($"Created {name}");
        DontDestroyOnLoad(go);
        return go.AddComponent<T>();
    }
}
