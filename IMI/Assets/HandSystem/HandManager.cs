using Assets.HandSystem;
using Mediapipe.Tasks.Core;
using Mediapipe.Tasks.Vision.HandDetector;
using Mediapipe.Tasks.Vision.HandLandmarker;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.UI;
using static Assets.HandSystem.CompleteGesture;
using static UnityEngine.AdaptivePerformance.Provider.AdaptivePerformanceSubsystemDescriptor;




[System.Serializable]
public class GestureInput : UnityEvent<string> { }

public class HandManager : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        
        GameObject go = new GameObject("HandManager");
        go.AddComponent<HandManager>();
    }

    public WebcamFeed webcamFeed;
    
    public static GestureInput onGestureComplete = new GestureInput();

    public static HandManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            
            DontDestroyOnLoad(this.gameObject);
        }
    }



    internal List<CompleteGesture> gesturesToTrack;
    internal List<GestureInfo> trackedInfo;
    internal HandLandmarker handLandmarker;

    private RawImage WebcamTexture;
    private RawImage WebcamSnippet;

    private void Start()
    {
        


        
        gesturesToTrack = new List<CompleteGesture>();
        trackedInfo = new List<GestureInfo>();

        handLandmarker = HandLandmarker.CreateFromOptions(new HandLandmarkerOptions
        (
            new BaseOptions
            (
                modelAssetPath: Path.Combine(Application.streamingAssetsPath, "hand_landmarker.task")
            ),
            numHands: 2
        ));


        CreateHiddenUI();
        webcamFeed = new WebcamFeed(WebcamTexture, WebcamSnippet);
        UnityEngine.Debug.Log("webcam made" + webcamFeed.IsActive());


        Resources.LoadAll<TextAsset>("Gestures").ToList<TextAsset>().ForEach(rawJson => { 


            gesturesToTrack.Add(JsonUtility.FromJson<CompleteGesture>(rawJson.text));

        });






    }

    

    void CreateHiddenUI()
    {
        GameObject canvasGO = new GameObject("GestureCanvas");
        canvasGO.hideFlags = HideFlags.HideAndDontSave;

        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        DontDestroyOnLoad(canvasGO);

        WebcamTexture = CreateRawImage("CamA", canvas.transform);
        WebcamSnippet = CreateRawImage("CamB", canvas.transform);

        canvasGO.SetActive(false); // fully hidden
    }

    RawImage CreateRawImage(string name, Transform parent)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent);

        return go.AddComponent<RawImage>();
    }


    private void Update()
    {
       
        // Safety check to ensure webcamFeed is assigned
        if (webcamFeed == null) throw new System.Exception("WebcamFeed not assigned in HandManager");

        // Ignores frames where the webcam feed is not active or hasn't updated, preventing unnecessary processing
        if (!webcamFeed.IsActive() || !webcamFeed.DidUpdateThisFrame()) return;

        // Retrieves the latest frame from the webcam feed and processes it to detect hand landmarks and validate gestures
        ProcessHandLogic(webcamFeed.GetMediapipeImageSnippet());
        CheckForCompleteInputs();
    }

    private void ProcessHandLogic(Mediapipe.Image snippet)
    {
        HandLandmarkerResult handLandmarks = handLandmarker.Detect(snippet);
        ProcessedHandLandmark? leftHand = null;
        ProcessedHandLandmark? rightHand = null;

        int handCount = 0;

        try
        {
            foreach (var handedness in handLandmarks.handedness)
            {
                if (handedness.categories.Exists(x => x.categoryName == "Right"))
                {
                    //left is set when right due to mirroring of webcam feed
                    leftHand = new ProcessedHandLandmark(handLandmarks.handLandmarks[handCount]);
                }

                if (handedness.categories.Exists(x => x.categoryName == "Left"))
                {
                    // right is set when left due to mirroring of webcam feed
                    rightHand = new ProcessedHandLandmark(handLandmarks.handLandmarks[handCount]);
                }

                handCount++;
            }

        }
        catch (Exception ex)
        {
            return;
        }



        if (leftHand != null) {
            gesturesToTrack
                .Where(wrappedGesture => (wrappedGesture.leftHanded || wrappedGesture.ambidextrous) && !wrappedGesture.twoHanded)
                .ToList()
                .ForEach(gesture =>
                {
                    GestureInfo? gestureInfo = trackedInfo.Find(trackedInfo => trackedInfo.name == gesture.primary.name);

                    if (gestureInfo == null)
                    {
                        trackedInfo.Add(new GestureInfo(gesture.primary.name));
                        UnityEngine.Debug.Log($"Ticked gesture {gesture.primary.name} as active for left hand.");


                    }
                    else
                    {

                 
                         gestureInfo!.UpdateActiveState(gesture.primary.Validate(leftHand, true));
                        
                    }
                }
            );
        }

        if (rightHand != null)
        {
            gesturesToTrack
                .Where(wrappedGesture => (!wrappedGesture.leftHanded || wrappedGesture.ambidextrous) && !wrappedGesture.twoHanded)
                .ToList()
                .ForEach(gesture =>
                {
                    GestureInfo? gestureInfo = trackedInfo.Find(trackedInfo => trackedInfo.name == gesture.primary.name);

                    if (gestureInfo == null)
                    {
                        trackedInfo.Add(new GestureInfo(gesture.primary.name));
                        UnityEngine.Debug.Log($"Ticked gesture {gesture.primary.name} as active for right.");
                    }
                    else
                    {
                        gestureInfo!.UpdateActiveState(gesture.primary.Validate(rightHand));
                    }
                }
            );
        }

        if (leftHand != null && rightHand != null)
        {
            gesturesToTrack
                .Where(wrappedGesture => wrappedGesture.twoHanded)
                .ToList()
                .ForEach(gesture =>
                {
                    if (gesture.secondary == null) throw new System.Exception("Two handed gesture missing secondary gesture in HandManager.ProcessHandLogic");


                    gesture.primary.Validate(leftHand);
                    gesture.secondary.Validate(rightHand);
                }
            );
        }

    }

    private void CheckForCompleteInputs() 
    {
        trackedInfo.ForEach(info =>
        {
            if (info.timeout <= 0f)
            {
                UnityEngine.Debug.Log($"Gesture {info.name} timed out after {info.millisActive} milliseconds.");
                info.Reset();
                return;
            }

            CompleteGesture? gesture = gesturesToTrack.Find(wrappedGesture => wrappedGesture.primary.name == info.name);
            if (gesture != null && info.millisActive >= gesture.timeThreshold)
            {
                // Trigger input event here, using info.name to identify the gesture
                UnityEngine.Debug.Log($"Gesture {info.name} completed after {info.millisActive} milliseconds!");

                onGestureComplete.Invoke(info.name);

                // Reset the active time for this gesture after triggering the event
                ResetAllGestureTimes();
            }
        });


    }

    private void ResetAllGestureTimes()
    {
        trackedInfo.ForEach(info => info.Reset());
    }

    public void AcceptGestureToTracking(CompleteGesture gesture)
    {
        gesturesToTrack.Add(gesture);
    }

    public void RemoveGestureFromTracking(CompleteGesture gesture)
    {
        gesturesToTrack.Remove(gesture);
    }

    public void SetGestureTracking(List<CompleteGesture> gestures)
    {
        gesturesToTrack = gestures;
    }

    internal class GestureInfo
    {
        public string name;
        public float millisActive;
        public float timeout;
        private long lastChecked;

        public GestureInfo(string name)
        {
            this.name = name;
            millisActive = 0f;
            timeout = 3000;
            lastChecked = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        public void UpdateActiveState(bool isActive)
        {
            long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            if (isActive)
            {
                millisActive += currentTime - lastChecked;
            }
            else
            {
                timeout -= currentTime - lastChecked;
            }
            lastChecked = currentTime;
        }

        public void Reset()
        {
            millisActive = 0f;
            timeout = 3000;
        }
    }





    private static CompleteGesture LeftClosedFist()
    {
        FingerTarget[] targets = new FingerTarget[5];
        targets[0] = new FingerTarget
        (
            FingerName.Thumb,
            Vector3.down,
            360f,
            1f,
            true
        );
        targets[1] = new FingerTarget
        (
            FingerName.Index,
            Vector3.down,
            360f,
            1f,
            true
        );
        targets[2] = new FingerTarget
        (
            FingerName.Middle,
            Vector3.down,
            360f,
            1f,
            true
        );
        targets[3] = new FingerTarget
        (
            FingerName.Ring,
            Vector3.down,
            360f,
            1f,
            true
        );
        targets[4] = new FingerTarget
        (
            FingerName.Pinky,
            Vector3.down,
            360f,
            1f,
            true
        );

        return new CompleteGesture(
            timeThreshold: 1000f,
            leftHanded: true,
            twoHanded: false,
            ambidextrous: false,
            primary: new Gesture("closed_fist", true, 4.5f, targets) // Example gesture: all fingers curled
        );
    }

    private static CompleteGesture LeftHighFive()
    {
        FingerTarget[] targets = new FingerTarget[5];
        targets[0] = new FingerTarget
        (
            FingerName.Thumb,
            Vector3.up,
            360f,
            1f,
            false
        );
        targets[1] = new FingerTarget
        (
            FingerName.Index,
            Vector3.up,
            25f,
            1f,
            false
        );
        targets[2] = new FingerTarget
        (
            FingerName.Middle,
            Vector3.up,
            360f,
            1f,
            false
        );
        targets[3] = new FingerTarget
        (
            FingerName.Ring,
            Vector3.up,
            360f,
            1f,
            false
        );
        targets[4] = new FingerTarget
        (
            FingerName.Pinky,
            Vector3.up,
            360f,
            1f,
            false
        );

        return new CompleteGesture(
            timeThreshold: 1000f,
            leftHanded: true,
            twoHanded: false,
            ambidextrous: false,
            primary: new Gesture("open_hand", true, 4.5f, targets) // Example gesture: all fingers extended
        );
    }


    private static CompleteGesture LeftPeaceSign()
    {
        FingerTarget[] targets = new FingerTarget[5];
        targets[0] = new FingerTarget
        (
            FingerName.Thumb,
            Vector3.down,
            360f,
            0f,
            true
        );
        targets[1] = new FingerTarget
        (
            FingerName.Index,
            Vector3.Lerp(Vector3.left, Vector3.up, 0.7f).normalized,
            25f,
            1f,
            false
        );
        targets[2] = new FingerTarget
        (
            FingerName.Middle,
            Vector3.Lerp(Vector3.right, Vector3.up, 0.7f).normalized,
            25f,
            1f,
            false
        );
        targets[3] = new FingerTarget
        (
            FingerName.Ring,
            Vector3.down,
            360f,
            0f,
            true
        );
        targets[4] = new FingerTarget
        (
            FingerName.Pinky,
            Vector3.down,
            360f,
            0f,
            true
        );
        return new CompleteGesture(
            timeThreshold: 1000f,
            leftHanded: true,
            twoHanded: false,
            ambidextrous: false,
            primary: new Gesture("peace_sign", true, 1.5f, targets) // Example gesture: index and middle fingers extended, others curled
        );
    }

    private static CompleteGesture LeftL() { 
        FingerTarget[] targets = new FingerTarget[5];
        targets[0] = new FingerTarget
        (
            FingerName.Thumb,
            Vector3.Lerp(Vector3.left, Vector3.up, 0.5f).normalized,
            25f,
            1.1f,
            false
        );
        targets[1] = new FingerTarget
        (
            FingerName.Index,
            Vector3.up,
            45f,
            1.1f,
            false
        );
        targets[2] = new FingerTarget
        (
            FingerName.Middle,
            Vector3.down,
            360f,
            1f,
            true
        );
        targets[3] = new FingerTarget
        (
            FingerName.Ring,
            Vector3.down,
            360f,
            1f,
            true
        );
        targets[4] = new FingerTarget
        (
            FingerName.Pinky,
            Vector3.down,
            360f,
            1f,
            true
        );
        return new CompleteGesture(
            timeThreshold: 1000f,
            leftHanded: true,
            twoHanded: false,
            ambidextrous: false,
            primary: new Gesture("L_sign", true, 5f, targets) // Example gesture: thumb and index finger extended, others curled
        );
    }


    private static CompleteGesture LeftThumbsUp()
    {
        FingerTarget[] targets = new FingerTarget[5];
        targets[0] = new FingerTarget
        (
            FingerName.Thumb,
            Vector3.up,
            45f,
            1.1f,
            false
        );
        targets[1] = new FingerTarget
        (
            FingerName.Index,
            Vector3.down,
            360f,
            1f,
            true
        );
        targets[2] = new FingerTarget
        (
            FingerName.Middle,
            Vector3.down,
            360f,
            1f,
            true
        );
        targets[3] = new FingerTarget
        (
            FingerName.Ring,
            Vector3.down,
            360f,
            1f,
            true
        );
        targets[4] = new FingerTarget
        (
            FingerName.Pinky,
            Vector3.down,
            360f,
            1f,
            true
        );
        return new CompleteGesture(
            timeThreshold: 1000f,
            leftHanded: true,
            twoHanded: false,
            ambidextrous: false,
            primary: new Gesture("thumbs_up", true, 4.5f, targets) // Example gesture: thumb extended, others curled
        );
    }

    private static CompleteGesture LeftASLLove()
    {
        FingerTarget[] targets = new FingerTarget[5];
        targets[0] = new FingerTarget
        (
            FingerName.Thumb,
            Vector3.Lerp(Vector3.left, Vector3.up, 0.5f).normalized,
            25f,
            1.1f,
            false
        );
        targets[1] = new FingerTarget
        (
            FingerName.Index,
            Vector3.up,
            45f,
            1.1f,
            false
        );
        targets[2] = new FingerTarget
        (
            FingerName.Middle,
            Vector3.down,
            360f,
            1f,
            true
        );
        targets[3] = new FingerTarget
        (
            FingerName.Ring,
            Vector3.down,
            360f,
            1f,
            true
        );
        targets[4] = new FingerTarget
        (
            FingerName.Pinky,
            Vector3.up,
            45f,
            1.1f,
            false
        );
        return new CompleteGesture(
            timeThreshold: 1000f,
            leftHanded: true,
            twoHanded: false,
            ambidextrous: false,
            primary: new Gesture("ASL_love", true, 5f, targets) // Example gesture: thumb, index, and pinky extended, others curled
        );
    }

    private static CompleteGesture LeftFingergunUp()
    {
        FingerTarget[] targets = new FingerTarget[5];
        targets[0] = new FingerTarget
        (
            FingerName.Thumb,
            Vector3.Lerp(Vector3.left, Vector3.up, 0.5f).normalized,
            25f,
            1.1f,
            false
        );
        targets[1] = new FingerTarget
        (
            FingerName.Index,
            Vector3.up,
            45f,
            1.1f,
            false
        );
        targets[2] = new FingerTarget
        (
            FingerName.Middle,
            Vector3.up,
            45,
            1f,
            false
        );
        targets[3] = new FingerTarget
        (
            FingerName.Ring,
            Vector3.down,
            360f,
            1f,
            true
        );
        targets[4] = new FingerTarget
        (
            FingerName.Pinky,
            Vector3.down,
            360f,
            1f,
            true
        );
        return new CompleteGesture(
            timeThreshold: 1000f,
            leftHanded: true,
            twoHanded: false,
            ambidextrous: false,
            primary: new Gesture("fingergun_up", true, 5f, targets) // Example gesture: thumb and index extended, others curled
        );

    }

    private static CompleteGesture LeftFingergunRight()
    {
        FingerTarget[] targets = new FingerTarget[5];
        targets[0] = new FingerTarget
        (
            FingerName.Thumb,
            Vector3.Lerp(Vector3.up, Vector3.left, 0.5f).normalized,
            25f,
            1.1f,
            false
        );
        targets[1] = new FingerTarget
        (
            FingerName.Index,
            Vector3.left,
            45f,
            1.1f,
            false
        );
        targets[2] = new FingerTarget
        (
            FingerName.Middle,
            Vector3.left,
            45,
            1f,
            false
        );
        targets[3] = new FingerTarget
        (
            FingerName.Ring,
            Vector3.down,
            360f,
            1f,
            true
        );
        targets[4] = new FingerTarget
        (
            FingerName.Pinky,
            Vector3.down,
            360f,
            1f,
            true
        );
        return new CompleteGesture(
            timeThreshold: 1000f,
            leftHanded: true,
            twoHanded: false,
            ambidextrous: false,
            primary: new Gesture("fingergun_right", true, 5f, targets) // Example gesture: thumb and index extended, others curled
        );

    }

    private static CompleteGesture LeftFingergunDown()
    {
        FingerTarget[] targets = new FingerTarget[5];
        targets[0] = new FingerTarget
        (
            FingerName.Thumb,
            Vector3.Lerp(Vector3.down, Vector3.up, 0.5f).normalized,
            25f,
            1.1f,
            false
        );
        targets[1] = new FingerTarget
        (
            FingerName.Index,
            Vector3.down,
            45f,
            1.1f,
            false
        );
        targets[2] = new FingerTarget
        (
            FingerName.Middle,
            Vector3.down,
            45,
            1f,
            false
        );
        targets[3] = new FingerTarget
        (
            FingerName.Ring,
            Vector3.down,
            360f,
            1f,
            true
        );
        targets[4] = new FingerTarget
        (
            FingerName.Pinky,
            Vector3.down,
            360f,
            1f,
            true
        );
        return new CompleteGesture(
            timeThreshold: 1000f,
            leftHanded: true,
            twoHanded: false,
            ambidextrous: false,
            primary: new Gesture("fingergun_down", true, 5f, targets) // Example gesture: thumb and index extended, others curled
        );
    }

    private static CompleteGesture LeftThumbsDown()
    {
        FingerTarget[] targets = new FingerTarget[5];
        targets[0] = new FingerTarget
        (
            FingerName.Thumb,
            Vector3.down,
            45f,
            1.1f,
            false
        );
        targets[1] = new FingerTarget
        (
            FingerName.Index,
            Vector3.down,
            360f,
            1f,
            true
        );
        targets[2] = new FingerTarget
        (
            FingerName.Middle,
            Vector3.down,
            360f,
            1f,
            true
        );
        targets[3] = new FingerTarget
        (
            FingerName.Ring,
            Vector3.down,
            360f,
            1f,
            true
        );
        targets[4] = new FingerTarget
        (
            FingerName.Pinky,
            Vector3.down,
            360f,
            1f,
            true
        );
        return new CompleteGesture(
            timeThreshold: 1000f,
            leftHanded: true,
            twoHanded: false,
            ambidextrous: false,
            primary: new Gesture("thumbs_down", true, 4.5f, targets) // Example gesture: thumb extended downwards, others curled
        );
    }
 


































    }










