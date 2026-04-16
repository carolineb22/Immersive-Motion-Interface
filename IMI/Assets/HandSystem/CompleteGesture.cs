using System;
using System.Collections.Generic;
using System.Text;

namespace Assets.HandSystem
{
    [System.Serializable]
    public class CompleteGesture
    {
        /// <summary>
        /// A holder for a complete gesture. 
        /// Used by the HandManager class to store gestures to scan, and to check whether that gesture
        /// should be applied to one hand, both hands, or either hand.
        /// </summary>
        /// 
        public float timeThreshold; // time in milliseconds that the gesture must be held for to be processed as an input
        public bool leftHanded; // left hand if true, right hand if false (ignored if twoHanded is true)
        public bool twoHanded; // single handed if false, two handed if true
        public bool ambidextrous; // if true, the gesture can be performed with either hand, if false the gesture is hand specific (ignored if twoHanded is true)
        public Gesture primary; // main gesture to track, if twoHanded is true this will be the left hand gesture
        public Gesture? secondary; // secondary gesture to track, only used if twoHanded is true, this will be the right hand gesture


        public CompleteGesture(
            float timeThreshold,
            bool leftHanded,
            bool twoHanded,
            bool ambidextrous,
            Gesture primary,
            Gesture? secondary = null
            )
        {
            this.timeThreshold = timeThreshold;
            this.leftHanded = leftHanded;
            this.twoHanded = twoHanded;
            this.ambidextrous = ambidextrous;

            this.primary = primary;
            if (secondary != null)
            {
                this.secondary = secondary;
                this.twoHanded = true;
            }

        }
    }
}
