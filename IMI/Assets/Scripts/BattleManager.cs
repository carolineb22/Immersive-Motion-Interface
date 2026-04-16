using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
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
    public AudioSource musicSource;


    void Start()
    {
       
        if (GameData.currentLevel == null)
        {
            Debug.LogError("GameData.currentLevel is NULL!");
            return;
        }

        if (enemy == null)
        {
            Debug.LogError("Enemy is NOT assigned!");
            return;
        }

        if (player == null)
        {
            Debug.LogError("Player is NOT assigned!");
            return;
        }

        if (battleLog == null)
        {
            Debug.LogError("BattleLog is NOT assigned!");
            return;
        }

        
        LevelData data = GameData.currentLevel;

        if (data.battleMusic != null && musicSource != null)
        {
            musicSource.clip = data.battleMusic;
            musicSource.loop = true;
            musicSource.Play();
        }

        Debug.Log("enemy = " + enemy);
        enemy.maxHP = data.enemyHP;
        enemy.currentHP = data.enemyHP;
        enemy.attack = data.enemyAttack;
        enemy.element = data.enemyType;

        enemy.GetComponent<SpriteRenderer>().sprite = data.enemySprite;

        int index = GameData.currentLevelIndex;

        for (int i = 0; i < spellSlots.Length; i++)
        {
            Spell spell = player.GetSpell(i);

            if (spell != null)
            {
                spellSlots[i].Setup(spell);
            }
        }

        var gesture = HandManager.Instance;
        gesture.webcamFeed.RestartWebcam();

        HandManager.onGestureComplete.AddListener((gesture) =>
        {
            if (!playerTurn || battleOver) return;

            if (gesture == "peace_sign")
            {
                UseSpell(0);
            }
            else if (gesture == "L_sign")
            {
                UseSpell(1);
            }
            else if (gesture == "thumbs_up")
            {
                UseSpell(2);
            }
            else if (gesture == "ASL_love")
            {
                UseSpell(3);
            }
            
            
        });
    }

    void Update()
    {
        if (battleOver) return;

        if (playerTurn)
        {
            //HandlePlayerInput();
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
        if (spellSlots == null || index >= spellSlots.Length)
        {
            Debug.LogError("Invalid spellSlots!");
            return;
        }
        SpellSlot slot = spellSlots[index];

        // Block if on cooldown
        if (!slot.IsReady())
        {
            battleLog.LogSystem("Spell is on cooldown!");
            return;
        }

        if (slot == null)
        {
            Debug.LogError("SpellSlot is NULL at index " + index);
            return;
        }

        Spell spell = slot.GetSpell();

         if (spell == null)
        {
            Debug.LogError("Spell is NULL in slot " + index);
            return;
        }
        
        // calculate elemental damage
        float multiplier = ElementSystem.GetMultiplier(spell.element, enemy.element);
        int finalDamage = Mathf.RoundToInt(spell.damage * multiplier);

        HighlightSlot(index);

        // Log updated damage
        battleLog.LogPlayer($"Player used {spell.spellName} ({finalDamage} dmg)");

        // Pass modified damage
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
            StartCoroutine(ReturnToLevelSelect());
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
        if (fireballPrefab == null || playerFirePoint == null)
        {
            Debug.LogError("Fireball prefab or fire point not assigned!");
            return;
        }
        GameObject fb = Instantiate(fireballPrefab, playerFirePoint.position, Quaternion.identity);

        Projectile proj = fb.GetComponent<Projectile>();
        proj.target = enemy.transform;
        proj.damage = damage;
        proj.battleManager = this;
    }

    public void EndBattle()
    {
        battleOver = true;
        StartCoroutine(ReturnToLevelSelect());
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

    public void OnEnemyDefeated()
    {
        if (battleOver) return;

        battleLog.LogSystem("Enemy defeated!");

        GameData.unlockedLevel = Mathf.Max(
            GameData.unlockedLevel,
            GameData.currentLevelIndex + 1
        );

        EndBattle();
    }

    IEnumerator ReturnToLevelSelect()
    {
        yield return new WaitForSeconds(2f); // gives time to read "Enemy defeated!"

        SceneManager.LoadScene("LevelSelect");
    }
}
