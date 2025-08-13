using UnityEngine;

public class ContactCollider : MonoBehaviour
{
    IContactable lastContactable;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Unitsレイヤーの物体が侵入した時の処理
        if (other.gameObject.layer == LayerMask.NameToLayer("Units"))
        {
            // Debug.Log($"{other.gameObject.name}が侵入しました");
            // 侵入した物体がIContactableを実装しているか確認
            IContactable contactable = other.gameObject.GetComponent<IContactable>();
            if (contactable != null)
            {
                lastContactable = contactable;
                ContactNameTag.SetTarget(contactable);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other) 
    {
        // Unitsレイヤーの物体が出て行った時の処理
        if (other.gameObject.layer == LayerMask.NameToLayer("Units"))
        {
            // Debug.Log($"{other.gameObject.name}が出て行きました");
            // 出て行った物体がIContactableを実装しているか確認
            IContactable contactable = other.gameObject.GetComponent<IContactable>();
            if (contactable != null && contactable == lastContactable)
            {
                ContactNameTag.RemoveTarget();
                lastContactable = null;
            }
        }
    }

    public IContactable GetLastContactable()
    {
        return lastContactable;
    }
}
