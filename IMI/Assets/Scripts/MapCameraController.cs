using UnityEngine;
using UnityEngine.InputSystem;

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
        float h = 0f;
        float v = 0f;

        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            h = -1f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            h = 1f;

        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
            v = -1f;
        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
            v = 1f;

        Vector3 move = new Vector3(h, v, 0) * moveSpeed * Time.deltaTime;
        transform.position += move;

        // clamp AFTER movement
        float clampedX = Mathf.Clamp(transform.position.x, minBounds.x, maxBounds.x);
        float clampedY = Mathf.Clamp(transform.position.y, minBounds.y, maxBounds.y);

        transform.position = new Vector3(clampedX, clampedY, transform.position.z);
    }

    public void FocusOn(Vector3 target)
    {
        float clampedX = Mathf.Clamp(target.x, minBounds.x, maxBounds.x);
        float clampedY = Mathf.Clamp(target.y, minBounds.y, maxBounds.y);

        targetPosition = new Vector3(clampedX, clampedY, transform.position.z);
        isFocusing = true;
    }
}