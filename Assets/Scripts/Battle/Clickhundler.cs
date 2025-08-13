using UnityEngine;

public class Clickhundler : MonoBehaviour
{
    UnitMover unitMover;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject target = GameObject.Find("Square");
        unitMover = target.GetComponent<UnitMover>();
    }

    // Update is called once per frame
    void Update()
    {
        // マウスの左クリックを検知
        if (Input.GetMouseButtonDown(0))
        {
            // マウスのスクリーン座標を取得
            Vector3 mousePosition = Input.mousePosition;
            // スクリーン座標をワールド座標に変換
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
            // Z座標を0に設定して2D座標として扱う
            Vector2 position2D = new Vector2(worldPosition.x, worldPosition.y);
            
            // クリックした座標をデバッグログに出力
            Debug.Log("クリックした座標: " + position2D);
            unitMover.MoveTo(position2D);
        }
    }
}
