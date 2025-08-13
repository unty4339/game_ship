using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMover : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed = 8f;

    void Start()
    {
        
    }

    void Update()
    {
        // StopTime();
        InputToContact();
    }
    void FixedUpdate()
    {
        InputToMove();
    }
    
    void InputToMove()
    {
        // キーボード接続チェック
        var current = Keyboard.current;
        if (current == null) return;

        // 移動方向を格納するベクトル
        Vector3 moveDirection = Vector3.zero;

        // WASDキーの入力チェック
        if (current.wKey.isPressed)
        {
            moveDirection.y += 1f; // 上方向
        }
        if (current.sKey.isPressed) 
        {
            moveDirection.y -= 1f; // 下方向
        }
        if (current.dKey.isPressed)
        {
            moveDirection.x += 1f; // 右方向
        }
        if (current.aKey.isPressed)
        {
            moveDirection.x -= 1f; // 左方向
        }

        // 移動方向の正規化
        moveDirection.Normalize();

        // 実際の移動処理
        transform.position += moveDirection * moveSpeed * Time.fixedDeltaTime;
    }

    void StopTime()
    {
        if (Time.timeScale == 0f)
        {
            Time.timeScale = 1f;
        }
        else
        {
            Time.timeScale = 0f;
        }
    }

    ContactCollider ContactCollider;
    void InputToContact()
    {
        var current = Keyboard.current;
        if (current == null) return;

        if (current.spaceKey.wasPressedThisFrame)
        {
            if (ContactCollider == null)
            {
                ContactCollider = gameObject.transform.Find("ContactCollider").GetComponent<ContactCollider>();
            }
            // 接触している物体があるか確認
            IContactable contactable = ContactCollider.GetLastContactable();
            Debug.Log(contactable);
            if (contactable != null)
            {
                // 接触している物体があれば接触処理を行う
                // contactable.Contact();
                if (Time.timeScale == 1f)
                {
                    Time.timeScale = 0f;
                }
                else
                {
                    Time.timeScale = 1f;
                }
            }
        }
        
    }
}
