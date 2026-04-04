using UnityEngine;
using UnityEngine.InputSystem;

public class BattleManager : MonoBehaviour
{
    public PlayerUnit player;
    public Unit enemy;
    public GameObject fireballPrefab;
    public Transform playerFirePoint;

    private bool playerTurn = true;

    void Update()
    {
        if (playerTurn)
        {
            HandlePlayerInput();
        }
    }

    void HandlePlayerInput()
    {
        // TEMP: simulate gestures with keys
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            UseSpell(0);
        }
        else if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            UseSpell(1);
        }
    }

    void UseSpell(int index)
{
    Spell spell = player.GetSpell(index);

    Debug.Log("Player used " + spell.spellName);

    SpawnFireball();

    enemy.TakeDamage(spell.damage);

    if (enemy.IsDead())
    {
        Debug.Log("Enemy defeated!");
        return;
    }

    playerTurn = false;
    Invoke(nameof(EnemyTurn), 1.0f);
}

    void EnemyTurn()
    {
        Debug.Log("Enemy attacks!");

        player.TakeDamage(enemy.attack);

        if (player.IsDead())
        {
            Debug.Log("Player defeated!");
            return;
        }

        playerTurn = true;
    }

    void SpawnFireball()
    {
        GameObject fb = Instantiate(fireballPrefab, playerFirePoint.position, Quaternion.identity);

        Projectile proj = fb.GetComponent<Projectile>();
        proj.target = enemy.transform;
    }
}