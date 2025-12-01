using UnityEngine;
using TMPro;

/// <summary>
/// 簡易ユニット情報ウィンドウのサンプル実装
/// 
/// このスクリプトはUnitInfoUIの簡易UI Prefabにアタッチして使用します。
/// IUnitInfoWindowインターフェースを実装し、ユニットの基本情報を表示します。
/// 
/// 使用方法：
/// 1. 新しいGameObjectを作成（例: "SimpleUnitInfoWindow"）
/// 2. このスクリプトをアタッチ
/// 3. 背景画像とテキストUIを設定
/// 4. Prefabとして保存
/// 5. UnitInfoUIのsimpleInfoWindowPrefabに設定
/// </summary>
public class SimpleUnitInfoWindow : MonoBehaviour, IUnitInfoWindow
{
    [Header("UI要素")]
    /// <summary>ユニット名を表示するテキスト</summary>
    [Tooltip("ユニット名を表示するテキスト")]
    public TextMeshProUGUI nameText;

    /// <summary>装備情報を表示するテキスト</summary>
    [Tooltip("装備情報を表示するテキスト")]
    public TextMeshProUGUI equipmentText;

    /// <summary>重量情報を表示するテキスト</summary>
    [Tooltip("重量情報を表示するテキスト")]
    public TextMeshProUGUI weightText;

    /// <summary>過積載警告を表示するテキスト（オプション）</summary>
    [Tooltip("過積載警告を表示するテキスト（オプション）")]
    public TextMeshProUGUI overloadWarningText;

    /// <summary>対象のUnitCore</summary>
    private UnitCore _unitCore;

    /// <summary>
    /// UnitCoreへの参照を設定
    /// </summary>
    public void SetUnitCore(UnitCore unitCore)
    {
        _unitCore = unitCore;
    }

    /// <summary>
    /// 情報を更新
    /// </summary>
    public void UpdateInfo()
    {
        if (_unitCore == null) return;

        var inventory = _unitCore.Inventory;
        if (inventory == null)
        {
            if (nameText != null) nameText.text = _unitCore.name;
            if (equipmentText != null) equipmentText.text = "インベントリ未設定";
            if (weightText != null) weightText.text = "";
            if (overloadWarningText != null) overloadWarningText.gameObject.SetActive(false);
            return;
        }

        // ユニット名
        if (nameText != null)
        {
            nameText.text = _unitCore.name;
        }

        // 装備情報
        if (equipmentText != null)
        {
            string weaponName = inventory.mainWeapon != null ? inventory.mainWeapon.itemName : "なし";
            string helmetName = inventory.helmet != null ? inventory.helmet.itemName : "なし";
            string suitName = inventory.suit != null ? inventory.suit.itemName : "なし";

            equipmentText.text = $"武器: {weaponName}\nヘルメット: {helmetName}\nスーツ: {suitName}";
        }

        // 重量情報
        if (weightText != null)
        {
            float currentWeight = inventory.GetCurrentWeight();
            float capacity = inventory.baseCarryingCapacity;
            weightText.text = $"重量: {currentWeight:F1} / {capacity:F1} kg";
        }

        // 過積載警告
        if (overloadWarningText != null)
        {
            bool isOverloaded = inventory.IsOverloaded();
            overloadWarningText.gameObject.SetActive(isOverloaded);
            if (isOverloaded)
            {
                overloadWarningText.text = "過積載！移動速度低下";
                overloadWarningText.color = Color.red;
            }
        }
    }
}

