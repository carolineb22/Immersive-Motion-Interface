using Mediapipe;
using Mediapipe.Tasks.Vision.HandLandmarker;
using Mediapipe.Tasks.Vision.HandDetector;
using Mediapipe.Unity;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mediapipe.Tasks.Core;
using Mediapipe.Tasks.Vision.Core;
using System.Runtime.CompilerServices;


public class Test : MonoBehaviour
{
    public RawImage displayArea;
    private WebCamTexture webcamTexture;
    private HandLandmarker handLandmarker;
    [SerializeField] private HandVisualizer visualizer;
    private Texture2D readableTexture;


    void Start()
    {

        webcamTexture = new WebCamTexture();
        displayArea.texture = webcamTexture;
        displayArea.material.mainTexture = webcamTexture;
        displayArea.rectTransform.localScale = new Vector3(-1, 1, 1);

        webcamTexture.Play();
        readableTexture = new Texture2D(webcamTexture.width, webcamTexture.height);

        handLandmarker = HandLandmarker.CreateFromOptions(new HandLandmarkerOptions
        (
            new BaseOptions
            (
                modelAssetPath: "C:\\Users\\glenn\\Downloads\\hand_landmarker.task"
            ),
            numHands: 2
        ));

    }
    void OnDestroy()
    {
        if (webcamTexture != null)
        {
            webcamTexture.Stop();
        }
    }




    private void Update()
    {
        if (webcamTexture.didUpdateThisFrame)
        {
            readableTexture.SetPixels(webcamTexture.GetPixels());
            readableTexture.Apply();

            Mediapipe.Image image = new Mediapipe.Image(readableTexture); 
            HandLandmarkerResult result = handLandmarker.Detect(image);

            visualizer.UpdateHands(result);


        }
    }
}