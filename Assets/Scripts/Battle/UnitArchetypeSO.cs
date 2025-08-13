using UnityEngine;

[CreateAssetMenu(menuName = "Game/UnitArchetype")]
public class UnitArchetypeSO : ScriptableObject
{
    /// <summary>ユニット種別の表示名</summary>
    public string displayName;

    /// <summary>生成に使用する基礎プレハブ UnitCoreなどを搭載済み前提</summary>
    public GameObject unitPrefab;

    /// <summary>基礎最大HP</summary>
    public int baseHP = 100;

    /// <summary>基礎ダメージ</summary>
    public int baseDamage = 20;

    /// <summary>基礎発射レート 毎秒発射数</summary>
    public float baseFireRate = 3f;
}
