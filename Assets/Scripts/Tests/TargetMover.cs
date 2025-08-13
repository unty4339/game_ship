using UnityEngine;

public class TargetMover : MonoBehaviour
{
    public Vector2 position1;
    public Vector2 position2;
    public float moveTimeSpan = 10f;

    int currentIndex = 0;
    float currentTime = 0f;

    // Update is called once per frame
    void FixedUpdate()
    {
        currentTime += Time.fixedDeltaTime;
        if (currentTime >= moveTimeSpan)
        {
            currentIndex = (currentIndex + 1) % 2;
            currentTime = 0f;
            transform.position = currentIndex == 0 ? position1 : position2;
        }
    }
}
