using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    public void Start()
    {
        var gesture = HandManager.Instance;

        HandManager.onGestureComplete.AddListener((gesture) =>
        {
            if (gesture == "closed_fist")
            {
                StartGame();
            }
            else if (gesture == "open_hand")
               {
                    QuitGame();
            }
        });

        
    }


    public void StartGame()
    {
        HandManager.onGestureComplete.RemoveAllListeners(); // prevent gesture input from affecting battle scene
        SceneManager.LoadScene(2); // loads  battle scene
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game"); // works in editor

        Application.Quit(); // works in build
    }
}