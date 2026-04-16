using System.Collections;
using UnityEngine;

[System.Serializable]
public class LevelData
{
    public string levelName;
    public ElementType enemyType;
    public Sprite enemySprite;
    public int enemyHP;
    public int enemyAttack;
    public AudioClip battleMusic;
}