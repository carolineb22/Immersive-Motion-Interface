using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelNode : MonoBehaviour
{
    public int levelIndex;
    public bool isHomeBase = false;
    public GameObject mist;

    private bool isLocked = false;
    private bool isHighlighted = false;

    private SpriteRenderer sr;

    public Color normalColor = Color.black;
    public Color selectedColor = Color.cyan;
    public Color lockedColor = Color.gray;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void UpdateVisual()
    {
        if (isLocked)
        {
            sr.color = lockedColor;
        }
        else if (isHighlighted)
        {
            sr.color = selectedColor;
        }
        else
        {
            sr.color = normalColor;
        }
    }

    public void SetHighlighted(bool highlighted)
    {
        if (isLocked) return;

        isHighlighted = highlighted;

        transform.localScale = highlighted ? Vector3.one * 1.2f : Vector3.one;

        UpdateVisual();
    }

    public void SetLocked(bool locked)
    {
        isLocked = locked;

        GetComponent<Collider2D>().enabled = !locked;

        if (mist != null)
            mist.SetActive(locked);

        UpdateVisual();
    }

    public void OnSelected()
    {
        if (isLocked) return;

        var manager = FindFirstObjectByType<LevelSelectManager>();

        GameData.currentLevelIndex = levelIndex;

        if (isHomeBase)
        {
            manager.ShowHomeBaseMessage(this);
        }
        else
        {
            manager.ShowStartPrompt(this);
        }
    }

    public void ConfirmStart()
    {
        SceneManager.LoadScene("BattleScene");
    }
}