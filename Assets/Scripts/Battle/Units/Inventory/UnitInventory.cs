using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ユニットの装備・インベントリ管理クラス
/// 
/// このクラスは以下の機能を提供します：
/// - 装備スロット管理（武器、ヘルメット、スーツ）
/// - バックパック（インベントリ）管理
/// - 重量計算と移動速度ペナルティの計算
/// 
/// UnitCoreからアクセス可能で、同じGameObjectにアタッチされます。
/// </summary>
public class UnitInventory : MonoBehaviour
{
    [Header("装備スロット")]
    /// <summary>メイン武器</summary>
    [Tooltip("メイン武器")]
    public WeaponItemSO mainWeapon;

    /// <summary>ヘルメット</summary>
    [Tooltip("ヘルメット")]
    public EquipmentSO helmet;

    /// <summary>スーツ（防具）</summary>
    [Tooltip("スーツ（防具）")]
    public EquipmentSO suit;

    [Header("インベントリ")]
    /// <summary>バックパック内のアイテムリスト（装備以外の所持品）</summary>
    [Tooltip("バックパック内のアイテムリスト")]
    public List<ItemDataSO> backpack = new List<ItemDataSO>();

    [Header("重量設定")]
    /// <summary>基本の重量制限（kg単位、例: 35.0kg）</summary>
    [Tooltip("基本の重量制限（kg単位）")]
    [Range(0f, 200f)]
    public float baseCarryingCapacity = 35.0f;

    /// <summary>
    /// 現在の総重量を計算
    /// 装備品（武器、ヘルメット、スーツ）とバックパック内の全アイテムの重量を合計
    /// </summary>
    /// <returns>総重量（kg単位）</returns>
    public float GetCurrentWeight()
    {
        float totalWeight = 0f;

        // 装備品の重量を加算
        if (mainWeapon != null)
            totalWeight += mainWeapon.weight;

        if (helmet != null)
            totalWeight += helmet.weight;

        if (suit != null)
            totalWeight += suit.weight;

        // バックパック内のアイテムの重量を加算
        foreach (var item in backpack)
        {
            if (item != null)
                totalWeight += item.weight;
        }

        return totalWeight;
    }

    /// <summary>
    /// 移動速度ペナルティを計算
    /// 
    /// 計算式：
    /// - 現在重量 <= 許容量 なら 1.0 (ペナルティなし)
    /// - 超過した場合、超過率に応じて 0.0 ～ 1.0 の値を返す
    /// - 重量が倍なら速度は半分になる（例: 70kg / 35kg = 2.0倍 → 速度は 1.0 / 2.0 = 0.5倍）
    /// - 最小値は0.1（完全に停止はしない）
    /// </summary>
    /// <returns>移動速度倍率（0.1 ～ 1.0）</returns>
    public float GetMoveSpeedPenalty()
    {
        float currentWeight = GetCurrentWeight();
        float capacity = baseCarryingCapacity;

        // 許容量以下ならペナルティなし
        if (currentWeight <= capacity)
        {
            return 1.0f;
        }

        // 超過した場合、超過率に応じて速度を低下
        // 例: 重量が2倍なら速度は1/2、3倍なら1/3
        float overloadRatio = currentWeight / capacity;
        float speedMultiplier = 1.0f / overloadRatio;

        // 最小値は0.1（完全に停止はしない）
        return Mathf.Max(0.1f, speedMultiplier);
    }

    /// <summary>
    /// 過積載状態かどうかを判定
    /// </summary>
    /// <returns>過積載の場合はtrue</returns>
    public bool IsOverloaded()
    {
        return GetCurrentWeight() > baseCarryingCapacity;
    }

    /// <summary>
    /// バックパックにアイテムを追加
    /// </summary>
    /// <param name="item">追加するアイテム</param>
    /// <returns>追加に成功した場合はtrue</returns>
    public bool AddToBackpack(ItemDataSO item)
    {
        if (item == null) return false;
        backpack.Add(item);
        return true;
    }

    /// <summary>
    /// バックパックからアイテムを削除
    /// </summary>
    /// <param name="item">削除するアイテム</param>
    /// <returns>削除に成功した場合はtrue</returns>
    public bool RemoveFromBackpack(ItemDataSO item)
    {
        if (item == null) return false;
        return backpack.Remove(item);
    }

    /// <summary>
    /// バックパック内のアイテム数を取得
    /// </summary>
    /// <returns>アイテム数</returns>
    public int GetBackpackItemCount()
    {
        return backpack.Count;
    }
}

