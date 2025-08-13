using UnityEngine;
using TMPro;
using UnityEngine.AddressableAssets;
public class ContactNameTag : MonoBehaviour
{
    [SerializeField] 
    private GameObject targetObject;  // 追従する対象のGameObject

    [SerializeField]
    private Vector3 offset = new Vector3(0f, 1f, 0f);  // 表示位置のオフセット

    [SerializeField]
    private TextMeshProUGUI textMeshProUGUI;

    private RectTransform rectTransform;  // UIの位置制御用
    private Camera mainCamera;

    static ContactNameTag instance;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (targetObject == null) return;

        // ワールド座標からスクリーン座標に変換
        Vector3 screenPos = mainCamera.WorldToScreenPoint(targetObject.transform.position + offset);

        // UIの位置を更新
        rectTransform.position = screenPos;
    }

    // 追従対象を設定するメソッド
    public static void SetTarget(IContactable contactable)
    {
        if (instance == null)
        {
            // インスタンスがないなら作成する
            var op = Addressables.LoadAssetAsync<GameObject>("NameTag.prefab");
            var prefab = op.WaitForCompletion();
            instance = Instantiate(prefab).GetComponent<ContactNameTag>();
            instance.gameObject.transform.SetParent(GameObject.Find("Canvas").transform);
            Addressables.Release(op);
        }
        instance.gameObject.SetActive(true);
        instance.targetObject = contactable.GetGameObject();
        instance.textMeshProUGUI.text = contactable.GetContactableName();
    }

    public static void RemoveTarget()
    {
        if (instance != null)
        {
            instance.gameObject.SetActive(false);
        }
    }
}
