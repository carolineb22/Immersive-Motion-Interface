using Mediapipe.Tasks.Components.Containers;
using UnityEngine;


public static class HandUtils
{
    public static Vector3 Vector3FromLandmark(NormalizedLandmark landmark)
    {
        return new Vector3(landmark.x, landmark.y, landmark.z);
    }

    public static Vector3 Vector3Direction(Vector3 target, Vector3 origin)
    {
        return Vector3.Normalize(target - origin);
    }
}

public enum Handedness
{
    Left,
    Right
}

public enum FingerName
{
    Thumb,
    Index,
    Middle,
    Ring,
    Pinky
}
