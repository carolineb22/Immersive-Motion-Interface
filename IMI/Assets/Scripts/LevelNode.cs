using UnityEngine;

public class LevelNode : MonoBehaviour
{
    public int levelIndex;
    public Transform focusPoint;

    public void OnSelected()
    {
        Camera.main.GetComponent<MapCameraController>()
            .FocusOn(focusPoint.position);

        GameData.currentLevelIndex = levelIndex;

    }

    void OnMouseDown()
    {
        OnSelected();
    }
}