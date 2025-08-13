using System.Text;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// CombatantStatusの内容をTextで頭上に表示するデバッグUI
/// 確認用に最小限で実装
/// </summary>
[RequireComponent(typeof(CombatantStatus))]
public class CombatStatusDebugUI : MonoBehaviour
{
    [Header("UI Prefab")]
    [Tooltip("WorldSpace Canvasに配置するTextプレハブ")]
    public Text worldTextPrefab;

    [Header("Offset")]
    public Vector3 offset = new Vector3(0, 1.5f, 0);

    CombatantStatus _status;
    Camera _cam;
    Text _text;
    Transform _uiObj;

    void Awake()
    {
        _status = GetComponent<CombatantStatus>();
        _cam = Camera.main;

        if (worldTextPrefab != null)
        {
            _uiObj = Instantiate(worldTextPrefab, transform.position + offset, Quaternion.identity, null).transform;
            _text = _uiObj.GetComponent<Text>();
        }
        else
        {
            Debug.LogWarning("worldTextPrefabが未設定");
        }
    }

    void LateUpdate()
    {
        if (_text == null) return;

        // 頭上位置へ追従
        _uiObj.position = transform.position + offset;

        // 常にカメラ正面を向く
        _uiObj.rotation = _cam.transform.rotation;

        // テキスト更新
        var sb = new StringBuilder();
        sb.Append($"HP:{_status.currentHP}/{_status.maxHP} ");
        sb.Append($"Stun:{_status.stunValue:0.0} ");

        sb.AppendLine();

        foreach (var eff in _status.Effects)
        {
            switch (eff.Value)
            {
                case BleedEffect b:
                    sb.Append($"Bleed:{b.bleedAmount:0.0} ");
                    break;
                case HemorrhageEffect h:
                    sb.Append($"Hemo:{h.hemoAmount:0.0} ");
                    break;
                default:
                    sb.Append($"{eff.Key} ");
                    break;
            }
        }

        _text.text = sb.ToString();
    }
}
