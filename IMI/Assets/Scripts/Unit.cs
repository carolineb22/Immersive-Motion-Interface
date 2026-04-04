using UnityEngine;

public class Unit : MonoBehaviour
{
    public string unitName;

    public int maxHP = 100;
    public int currentHP;

    public int attack = 15;

    void Start()
    {
        currentHP = maxHP;
    }

    public void TakeDamage(int amount)
    {
        currentHP -= amount;
        currentHP = Mathf.Max(currentHP, 0);
    }

    public bool IsDead()
    {
        return currentHP <= 0;
    }
}