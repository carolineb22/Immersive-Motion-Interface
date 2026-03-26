using Mediapipe;
using Mediapipe.Tasks.Components.Containers;
using Mediapipe.Tasks.Components.Containers.Proto;
using Mediapipe.Tasks.Vision.HandLandmarker;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class HandVisualizer : MonoBehaviour
{
    public RectTransform canvasRect;
    public GameObject pointPrefab2;
    public GameObject pointPrefab;
    public int maxLandmarks = 21;

    private List<UnityEngine.UI.Image> pointsLeft = new List<UnityEngine.UI.Image>();
    private List<UnityEngine.UI.Image> pointsRight = new List<UnityEngine.UI.Image>();

    public UnityEngine.UI.Image leftBox;
    public UnityEngine.UI.Image rightBox;


    void Start()
    {
        // Create landmark point objects
        for (int i = 0; i < maxLandmarks; i++)
        {
            GameObject objL = Instantiate(pointPrefab, canvasRect);
            pointsLeft.Add(objL.GetComponent<UnityEngine.UI.Image>());

            GameObject objR = Instantiate(pointPrefab2, canvasRect);
            pointsRight.Add(objR.GetComponent<UnityEngine.UI.Image>());
        }
    }

    public void UpdateHands(HandLandmarkerResult result)
    {

        int i = 0;
        bool hasRight = false;
        bool hasLeft = false;
        foreach (var category in result.handedness) {
            if (category.categories.Exists(x => x.categoryName == "Right"))
            {
           
                UpdateHand(result.handLandmarks[i], pointsRight);
                UpdateBoundingBox(pointsRight, rightBox);
                hasRight = true;
            } 

            if (category.categories.Exists(x => x.categoryName == "Left"))
            {
           
                UpdateHand(result.handLandmarks[i], pointsLeft);
                UpdateBoundingBox(pointsLeft, leftBox);
                hasLeft = true;
            } 

            i++;
        }

        if (!hasRight) {
            pointsRight.ForEach(p => p.enabled = false);
            rightBox.enabled = false;
        }
        if (!hasLeft) {
            pointsLeft.ForEach(p => p.enabled = false);
            leftBox.enabled = false;
        }

    }


    private void UpdateHand(NormalizedLandmarks hand, List<UnityEngine.UI.Image> points)
    {
        for (int i = 0; i < hand.landmarks.Count && i < points.Count; i++)
        {
            var lm = hand.landmarks[i];

            // Convert normalized (0-1) → canvas space
            float x = lm.x * canvasRect.rect.width * 0.5f - canvasRect.rect.width/8;
            float y = lm.y * canvasRect.rect.height * 0.5f - canvasRect.rect.height/8;

            points[i].rectTransform.anchoredPosition = new Vector2(x, y);
            points[i].enabled = true;
        }

       
        
    }

    private void UpdateBoundingBox(List<UnityEngine.UI.Image> points, UnityEngine.UI.Image box)
    {
        if (points.Count == 0) return;

        float maxX = points.Max(p => p.rectTransform.anchoredPosition.x) + 5;
        float maxY = points.Max(p => p.rectTransform.anchoredPosition.y) + 5;
        float minX = points.Min(p => p.rectTransform.anchoredPosition.x) - 5;
        float minY = points.Min(p => p.rectTransform.anchoredPosition.y) - 5;

        float width = maxX - minX;
        float height = maxY - minY;

        // Center position
        float centerX = (maxX + minX) / 2f;
        float centerY = (maxY + minY) / 2f;

        RectTransform rect = box.rectTransform;
        rect.anchoredPosition = new Vector2(centerX, centerY);
        rect.sizeDelta = new Vector2(width, height);

        box.enabled = true;
    }


}