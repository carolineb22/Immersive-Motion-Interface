using Mediapipe;
using Mediapipe.Tasks.Core;
using Mediapipe.Tasks.Vision.Core;
using Mediapipe.Tasks.Vision.HandDetector;
using Mediapipe.Tasks.Vision.HandLandmarker;
using Mediapipe.Unity;
using NUnit.Framework.Internal;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting.FullSerializer;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;





public class WebcamFeed 
{ 
    private WebCamTexture webcamTexture;
    private readonly Texture2D webcamSnippetTexture;

    private readonly RawImage webcamTextureImage;
    private readonly RawImage webcamSnippetImage;

    public WebcamFeed(RawImage WebcamTexture, RawImage WebcamSnippet)
    {
        this.webcamTextureImage = WebcamTexture;
        this.webcamSnippetImage = WebcamSnippet;




        webcamTexture = new WebCamTexture(640, 360);
        webcamSnippetTexture = new Texture2D(640, 360);
        webcamTexture.Play();

        webcamTextureImage.transform.localScale = new Vector3(-1, 1, 1); // Flip the image horizontally
        webcamTextureImage.texture = webcamTexture;
        


    }

    public bool DidUpdateThisFrame()
    {
        return webcamTexture.didUpdateThisFrame;
    }

    public bool IsActive()
    {
        return webcamTexture.isPlaying;
    }

    
    public Mediapipe.Image GetMediapipeImageSnippet()
    {
        webcamSnippetTexture.SetPixels(webcamTexture.GetPixels());
        webcamSnippetTexture.Apply();




        webcamSnippetImage.texture = webcamSnippetTexture;

        webcamSnippetImage.material.mainTexture = webcamSnippetTexture;
        webcamSnippetImage.uvRect = new UnityEngine.Rect(0f, 0f, 1f, 1f);
        webcamSnippetImage.SetNativeSize();

        return new Mediapipe.Image(webcamSnippetTexture);
        
    }

    public void RestartWebcam()
    {
        webcamTexture.Stop();
        webcamTexture.Play();
    }

}
