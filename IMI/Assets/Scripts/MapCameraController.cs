using UnityEngine;

public class MapCameraController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Vector2 minBounds;
    public Vector2 maxBounds;

    private bool isFocusing = false;
    private Vector3 targetPosition;
    public float focusSpeed = 5f;

    void Update()
    {
        if (isFocusing)
        {
            transform.position = Vector3.Lerp(
                transform.position,
                targetPosition,
                Time.deltaTime * focusSpeed
            );

            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                transform.position = targetPosition;
                isFocusing = false;
            }

            return;
        }

        // normal movement
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 move = new Vector3(h, v, 0) * moveSpeed * Time.deltaTime;
        transform.position += move;

        // clamp AFTER movement
        float clampedX = Mathf.Clamp(transform.position.x, minBounds.x, maxBounds.x);
        float clampedY = Mathf.Clamp(transform.position.y, minBounds.y, maxBounds.y);

        transform.position = new Vector3(clampedX, clampedY, transform.position.z);
    }

    public void FocusOn(Vector3 target)
    {
        targetPosition = new Vector3(target.x, target.y, transform.position.z);
        isFocusing = true;
    }
}