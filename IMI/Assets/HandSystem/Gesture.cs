using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static HandUtils;

namespace Assets.HandSystem
{

    [System.Serializable]
    public class Gesture
    {
        public string name;
        public bool wristDirAgnostic;
        public FingerTarget[] fingerDirs;

        public float scoreThreshold;



        public Gesture(string name, bool wristDirAgnostic, float scoreThreshold, FingerTarget[] fingerDirs)
        {
            this.name = name;
            this.wristDirAgnostic = wristDirAgnostic;
            this.scoreThreshold = scoreThreshold;
            this.fingerDirs = fingerDirs;
        }


        public bool Validate(ProcessedHandLandmark hand, bool printInfo = false)
        {
            float score = 0f;
            foreach (var target in fingerDirs)
            {
                Finger finger = hand.GetFingerByName(target.targetFinger);
                if (finger.curled != target.curled)
                {
                    score += 0;
                }
                else if (target.IsMatch(hand.handDirection, hand.handRotation, finger.knuckleDirection, wristDirAgnostic, printInfo))
                {
                    score += target.weight;
                }
                
            }

            return score > scoreThreshold;
        }
        
    }

    [System.Serializable]
    public class FingerTarget 
    {
        public FingerName targetFinger;
        public Vector3 targetDirection;
        public float angleThreshold;
        public float weight;
        public bool curled;

        public FingerTarget(FingerName targetFinger, Vector3 targetDirection, float angleThreshold, float weight, bool curled)
        {
            this.targetFinger = targetFinger;
            this.targetDirection = targetDirection;
            this.angleThreshold = angleThreshold;
            this.weight = weight;
            this.curled = curled;
        }

        public bool IsMatch(Vector3 wristDir, Vector3 wristRot, Vector3 fingerDir, bool wristDirAgnostic, bool printInfo = false)
        {
            if (wristDirAgnostic)
            {
                return Vector3.Angle(targetDirection, fingerDir) < angleThreshold;
            }
            else
            {
                
                Vector3 adjustedFingerDir = (Quaternion.LookRotation(new Vector3(wristDir.x, wristDir.y, wristDir.z * -1), wristRot) * fingerDir).normalized;
                Vector3 foo = new Vector3(adjustedFingerDir.x, adjustedFingerDir.y, -adjustedFingerDir.z).normalized;
                return Vector3.Angle(targetDirection, foo) < angleThreshold;
            }

            
        }


    }
    
}
