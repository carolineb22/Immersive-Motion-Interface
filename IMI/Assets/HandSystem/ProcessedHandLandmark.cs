using Mediapipe.Tasks.Components.Containers;

using System;
using UnityEngine;
using static HandUtils;

[System.Serializable]
public class ProcessedHandLandmark
{
    public Vector3 handDirection;
    public Vector3 handRotation;

    public Handedness handtype;

    public Finger thumb;
    public Finger index;
    public Finger middle;
    public Finger ring;
    public Finger pinky;

    public ProcessedHandLandmark(NormalizedLandmarks hand)
    {
        handDirection = Vector3Direction(Vector3FromLandmark(hand.landmarks[9]), Vector3FromLandmark(hand.landmarks[0]));
        handRotation = Vector3Direction(Vector3FromLandmark(hand.landmarks[5]), Vector3FromLandmark(hand.landmarks[17]));

        thumb = new Finger(FingerName.Thumb ,Vector3FromLandmark(hand.landmarks[1]), Vector3FromLandmark(hand.landmarks[2]), Vector3FromLandmark(hand.landmarks[3]), Vector3FromLandmark(hand.landmarks[4]));
        index = new Finger(FingerName.Index , Vector3FromLandmark(hand.landmarks[5]), Vector3FromLandmark(hand.landmarks[6]), Vector3FromLandmark(hand.landmarks[7]), Vector3FromLandmark(hand.landmarks[8]));
        middle = new Finger(FingerName.Middle , Vector3FromLandmark(hand.landmarks[9]), Vector3FromLandmark(hand.landmarks[10]), Vector3FromLandmark(hand.landmarks[11]), Vector3FromLandmark(hand.landmarks[12]));
        ring = new Finger(FingerName.Ring , Vector3FromLandmark(hand.landmarks[13]), Vector3FromLandmark(hand.landmarks[14]), Vector3FromLandmark(hand.landmarks[15]), Vector3FromLandmark(hand.landmarks[16]));
        pinky = new Finger(FingerName.Pinky , Vector3FromLandmark(hand.landmarks[17]), Vector3FromLandmark(hand.landmarks[18]), Vector3FromLandmark(hand.landmarks[19]), Vector3FromLandmark(hand.landmarks[20]));
        

    }

    public Finger GetFingerByName(FingerName name)
    {
        return name switch
        {
            FingerName.Thumb => thumb,
            FingerName.Index => index,
            FingerName.Middle => middle,
            FingerName.Ring => ring,
            FingerName.Pinky => pinky,
            _ => throw new ArgumentException("Invalid finger name")
        };
    }

    

}

public class Finger 
{ 
    public Vector3 knuckleDirection;
    public FingerName name;
    public bool curled;

    public Finger(FingerName name, Vector3 knuckle, Vector3 proximalPhalanx, Vector3 middlePhalanx, Vector3 distalPhalanx)
    {
        knuckleDirection = Vector3Direction(proximalPhalanx, knuckle);

        
        curled = false;
        float angle = Vector3.Dot(knuckleDirection, Vector3Direction(distalPhalanx, proximalPhalanx));

       

        if (angle < 0.6f)
        {
            curled = true;
        }
    }

}


