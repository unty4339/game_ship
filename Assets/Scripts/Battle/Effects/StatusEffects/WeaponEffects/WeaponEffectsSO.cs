using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/WeaponEffects")]
public class WeaponEffectsSO : ScriptableObject
{
    /// <summary>命中時に与える効果の一覧</summary>
    public List<EffectSpec> onHitEffects = new List<EffectSpec>();

    [Serializable]
    public struct EffectSpec
    {
        /// <summary>効果ID 例 Bleed Hemorrhage</summary>
        public string id;
        /// <summary>効果量</summary>
        public float amount;
        /// <summary>持続秒 負値で無限</summary>
        public float duration;
        /// <summary>付与確率 0から1</summary>
        [Range(0f, 1f)] public float chance;
    }
}
