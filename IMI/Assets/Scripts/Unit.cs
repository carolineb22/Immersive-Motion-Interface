using UnityEngine;

public class Unit : MonoBehaviour
{
    public string unitName;

    public int maxHP = 100;
    public int currentHP;

    public int attack = 15;

    public HPBar hpBar; 

    void Start()
    {
        currentHP = maxHP;

        if (hpBar != null)
        {
            hpBar.SetMaxHP(maxHP);
        }
    }

    public void TakeDamage(int amount)
    {
        currentHP -= amount;
        currentHP = Mathf.Max(currentHP, 0);

        if (hpBar != null)
        {
            hpBar.SetHP(currentHP);
        }
    }

    public bool IsDead()
    {
        return currentHP <= 0;
    }
}