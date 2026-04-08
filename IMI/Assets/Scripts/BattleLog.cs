using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BattleLog : MonoBehaviour
{
    public Transform content;
    public GameObject logTextPrefab;
    public ScrollRect scrollRect;

    public void Log(string message)
    {
        GameObject entry = Instantiate(logTextPrefab, content);
        TMP_Text text = entry.GetComponent<TMP_Text>();
        text.text = message;

        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }

    public void LogPlayer(string message)
    {
        Log("<color=green>" + message + "</color>");
    }

    public void LogEnemy(string message)
    {
        Log("<color=red>" + message + "</color>");
    }

    public void LogSystem(string message)
    {
        Log("<color=yellow>" + message + "</color>");
    }
}