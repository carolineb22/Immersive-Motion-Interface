using UnityEngine;
using UnityEngine.InputSystem;

public class LevelSelectManager : MonoBehaviour
{
    public GameObject startPrompt;
    public GameObject homeBasePanel;
    public LevelNode[] levelNodes;

    private LevelNode currentNode;
    private int currentIndex = 0;
    private bool hasShown = false;

    void Start()
    {
        var gesture = HandManager.Instance;
        gesture.webcamFeed.RestartWebcam();

        HandManager.onGestureComplete.AddListener((gesture) =>
        {
            if (gesture == "fingergun_up")
            {
                MoveSelection(Vector2.up);

            }
            else if (gesture == "fingergun_right")
            {
                MoveSelection(Vector2.right);

            }
            else if (gesture == "fingergun_down")
            {
                MoveSelection(Vector2.down);

            }
            else if (gesture == "thumbs_down")
            {
                MoveSelection(Vector2.left);

            }
            else if (gesture == "closed_fist")
            {
                if (currentNode != null)
                {
                    
                    currentNode.ConfirmStart();
                }
                else
                {

                    levelNodes[currentIndex].OnSelected();

                }
            }
        });

        // Lock/unlock nodes
        for (int i = 0; i < levelNodes.Length; i++)
        {
            bool unlocked = i <= GameData.unlockedLevel;
            levelNodes[i].SetLocked(!unlocked);
        }

        startPrompt.SetActive(false);

        // Start on first node
        SelectNode(0);
    }

    void Update()
    {
        // If a node is selected (prompt open)
        if (currentNode != null)
        {
            if (Keyboard.current.enterKey.wasPressedThisFrame)
            {
                currentNode.ConfirmStart();
            }
            return;
        }

        // Directional movement
        if (Keyboard.current.leftArrowKey.wasPressedThisFrame || Keyboard.current.aKey.wasPressedThisFrame)
        {
            MoveSelection(Vector2.left);
        }

        if (Keyboard.current.rightArrowKey.wasPressedThisFrame || Keyboard.current.dKey.wasPressedThisFrame)
        {
            MoveSelection(Vector2.right);
        }

        if (Keyboard.current.upArrowKey.wasPressedThisFrame || Keyboard.current.wKey.wasPressedThisFrame)
        {
            MoveSelection(Vector2.up);
        }

        if (Keyboard.current.downArrowKey.wasPressedThisFrame || Keyboard.current.sKey.wasPressedThisFrame)
        {
            MoveSelection(Vector2.down);
        }

        // Select node
        if (Keyboard.current.enterKey.wasPressedThisFrame)
        {
            levelNodes[currentIndex].OnSelected();
        }
    }


    void MoveSelection(Vector2 direction)
    {
        LevelNode current = levelNodes[currentIndex];
        Vector2 currentPos = current.transform.position;

        float bestScore = float.MaxValue;
        int bestIndex = currentIndex;

        for (int i = 0; i < levelNodes.Length; i++)
        {
            if (i == currentIndex) continue;
            if (i > GameData.unlockedLevel) continue;

            Vector2 toNode = (Vector2)levelNodes[i].transform.position - currentPos;

            // Only consider nodes in the intended direction
            if (Vector2.Dot(direction, toNode.normalized) < 0.5f)
                continue;

            float distance = toNode.magnitude;

            if (distance < bestScore)
            {
                bestScore = distance;
                bestIndex = i;
            }
        }

        SelectNode(bestIndex);
    }

    void SelectNode(int index)
    {
        if (index < 0 || index >= levelNodes.Length) return;
        if (index > GameData.unlockedLevel) return;

        currentIndex = index;

        for (int i = 0; i < levelNodes.Length; i++)
        {
            levelNodes[i].SetHighlighted(i == index);
        }
        Debug.Log("Highlighting node: " + index);

        // Move camera
        Camera.main.GetComponent<MapCameraController>()
            .FocusOn(levelNodes[index].transform.position);
            
    }

    public void ShowStartPrompt(LevelNode node)
    {
        if (hasShown) return;
        hasShown = true;
        currentNode = node;
        startPrompt.SetActive(true);
    }

    public void ShowHomeBaseMessage(LevelNode node)
    {
        currentNode = node;
        homeBasePanel.SetActive(true);
    }

    public void OnHomeBaseContinue()
    {
        homeBasePanel.SetActive(false);

        // Unlock next level
        GameData.unlockedLevel = Mathf.Max(GameData.unlockedLevel, 1);
        
        for (int i = 0; i < levelNodes.Length; i++)
        {
            bool unlocked = i <= GameData.unlockedLevel;
            levelNodes[i].SetLocked(!unlocked);
        }
        currentNode = null;
    }
}