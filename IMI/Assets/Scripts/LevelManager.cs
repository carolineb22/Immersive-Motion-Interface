using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public LevelData[] levels;

    private int currentLevel = 0;

    public void StartGame()
    {
        currentLevel = 0;
        LoadLevel();
    }

    public void LoadLevel()
    {
        LevelData data = levels[currentLevel];

        // Pass data to next scene (simple static holder)
        GameData.currentLevel = data;

        SceneManager.LoadScene("BattleScene");
    }

    public void ChooseNextLevel(int index)
    {
        currentLevel = index;
        LoadLevel();
    }
}