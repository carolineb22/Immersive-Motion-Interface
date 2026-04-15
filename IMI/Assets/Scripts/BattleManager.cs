using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class BattleManager : MonoBehaviour
{
    public PlayerUnit player;
    public Unit enemy;
    public GameObject fireballPrefab;
    public Transform playerFirePoint;
    private bool battleOver = false;
    private bool playerTurn = true;
    public float turnDelay = 1.0f;
    public BattleLog battleLog;
    public SpellSlot[] spellSlots;


void Start()
    {
        for (int i = 0; i < spellSlots.Length; i++)
        {
            Spell spell = player.GetSpell(i);

            if (spell != null)
            {
                spellSlots[i].Setup(spell);
            }
        }
    }

    void Update()
    {
        if (battleOver) return;

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
        else if (Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            UseSpell(2);
        }
        else if (Keyboard.current.digit4Key.wasPressedThisFrame)
        {
            UseSpell(3);
        }
    }

    void UseSpell(int index)
    {
        SpellSlot slot = spellSlots[index];

        // Block if on cooldown
        if (!slot.IsReady())
        {
            battleLog.LogSystem("Spell is on cooldown!");
            return;
        }

        Spell spell = slot.GetSpell();

        HighlightSlot(index);

        battleLog.LogPlayer("Player used " + spell.spellName + " (" + spell.damage + " dmg)");

        SpawnFireball(spell.damage);

        slot.TriggerCooldown();

        playerTurn = false;
    }

    public void EnemyTurn()
    {
        if (battleOver) return;

        battleLog.LogEnemy("Enemy attacks (" + enemy.attack + " dmg)");

        player.TakeDamage(enemy.attack);

        if (player.IsDead())
        {
            battleLog.LogSystem("Player defeated!");
            battleOver = true;
            return;
        }

        // Reduce cooldowns at end of enemy turn
        foreach (var slot in spellSlots)
        {
            slot.ReduceCooldown();
        }
        playerTurn = true;
    }

    void SpawnFireball(int damage)
    {
        GameObject fb = Instantiate(fireballPrefab, playerFirePoint.position, Quaternion.identity);

        Projectile proj = fb.GetComponent<Projectile>();
        proj.target = enemy.transform;
        proj.damage = damage;
        proj.battleManager = this;
    }

    public void EndBattle()
    {
        battleOver = true;
    }

    public IEnumerator DelayedEnemyTurn()
    {
        yield return new WaitForSeconds(turnDelay);

        EnemyTurn();
    }

    public void HighlightSlot(int index)
    {
        for (int i = 0; i < spellSlots.Length; i++)
        {
            spellSlots[i].SetHighlighted(i == index);
        }
    }
}
