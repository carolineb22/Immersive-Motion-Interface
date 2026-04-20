using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Controls the flow of a turn-based battle:
/// - Handles player and enemy turns
/// - Manages spells, cooldowns, and damage
/// - Spawns projectiles
/// - Plays battle music
/// - Ends battle and returns to level select
/// </summary>
public class BattleManager : MonoBehaviour
{
    [Header("Units")]
    public PlayerUnit player;        // Reference to the player unit
    public Unit enemy;               // Reference to the enemy unit

    [Header("Combat Visuals")]
    public GameObject fireballPrefab;   // Projectile prefab for spells
    public Transform playerFirePoint;  // Spawn point for projectiles

    [Header("Battle State")]
    private bool battleOver = false;   // Tracks if battle has ended
    private bool playerTurn = true;    // Tracks whose turn it is

    public float turnDelay = 1.0f;     // Delay before enemy turn

    [Header("UI")]
    public BattleLog battleLog;        // Displays battle messages
    public SpellSlot[] spellSlots;    // Player spell UI slots

    [Header("Audio")]
    public AudioSource musicSource;   // Plays battle music


    /// <summary>
    /// Initializes battle data, enemy stats, UI, and gesture controls.
    /// </summary>
    void Start()
    {
        // Validate required references
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

        // Load level data
        LevelData data = GameData.currentLevel;

        // Setup battle music
        if (data.battleMusic != null && musicSource != null)
        {
            musicSource.clip = data.battleMusic;
            musicSource.loop = true;
            musicSource.Play();
        }

        // Initialize enemy stats from level data
        Debug.Log("enemy = " + enemy);
        enemy.maxHP = data.enemyHP;
        enemy.currentHP = data.enemyHP;
        enemy.attack = data.enemyAttack;
        enemy.element = data.enemyType;

        // Set enemy sprite
        enemy.GetComponent<SpriteRenderer>().sprite = data.enemySprite;

        // Cache level index
        int index = GameData.currentLevelIndex;

        // Initialize spell UI slots
        for (int i = 0; i < spellSlots.Length; i++)
        {
            Spell spell = player.GetSpell(i);

            if (spell != null)
            {
                spellSlots[i].Setup(spell);
            }
        }

        // Restart gesture detection system
        var gesture = HandManager.Instance;
        gesture.webcamFeed.RestartWebcam();

        // Listen for gesture inputs and map them to spells
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

    /// <summary>
    /// Runs every frame. Currently unused for player input (gesture-based instead).
    /// </summary>
    void Update()
    {
        if (battleOver) return;

        if (playerTurn)
        {
            // Optional keyboard input for testing
            // HandlePlayerInput();
        }
    }

    /// <summary>
    /// DEBUG ONLY: Allows keyboard input to simulate gestures.
    /// </summary>
    void HandlePlayerInput()
    {
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

    /// <summary>
    /// Executes a spell from a given slot index.
    /// Handles cooldowns, damage calculation, and projectile spawning.
    /// </summary>
    void UseSpell(int index)
    {
        // Validate slot array
        if (spellSlots == null || index >= spellSlots.Length)
        {
            Debug.LogError("Invalid spellSlots!");
            return;
        }

        SpellSlot slot = spellSlots[index];

        // Prevent use if spell is on cooldown
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

        // Calculate elemental damage multiplier
        float multiplier = ElementSystem.GetMultiplier(spell.element, enemy.element);
        int finalDamage = Mathf.RoundToInt(spell.damage * multiplier);

        // Highlight selected spell slot
        HighlightSlot(index);

        // Log player action
        battleLog.LogPlayer($"Player used {spell.spellName} ({finalDamage} dmg)");

        // Spawn projectile (NOTE: currently uses base damage, not modified)
        SpawnFireball(spell.damage);

        // Start cooldown
        slot.TriggerCooldown();

        // End player turn
        playerTurn = false;
    }

    /// <summary>
    /// Handles the enemy's turn:
    /// - Deals damage to player
    /// - Checks for player death
    /// - Reduces spell cooldowns
    /// </summary>
    public void EnemyTurn()
    {
        if (battleOver) return;

        battleLog.LogEnemy("Enemy attacks (" + enemy.attack + " dmg)");

        player.TakeDamage(enemy.attack);

        // Check if player died
        if (player.IsDead())
        {
            battleLog.LogSystem("Player defeated!");
            battleOver = true;
            StartCoroutine(ReturnToLevelSelect());
            return;
        }

        // Reduce cooldowns after enemy turn
        foreach (var slot in spellSlots)
        {
            slot.ReduceCooldown();
        }

        // Return control to player
        playerTurn = true;
    }

    /// <summary>
    /// Spawns a fireball projectile targeting the enemy.
    /// </summary>
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

    /// <summary>
    /// Ends the battle and returns to level select.
    /// </summary>
    public void EndBattle()
    {
        battleOver = true;
        StartCoroutine(ReturnToLevelSelect());
    }

    /// <summary>
    /// Waits for a delay, then triggers the enemy turn.
    /// </summary>
    public IEnumerator DelayedEnemyTurn()
    {
        yield return new WaitForSeconds(turnDelay);
        EnemyTurn();
    }

    /// <summary>
    /// Highlights the currently selected spell slot.
    /// </summary>
    public void HighlightSlot(int index)
    {
        for (int i = 0; i < spellSlots.Length; i++)
        {
            spellSlots[i].SetHighlighted(i == index);
        }
    }

    /// <summary>
    /// Called when the enemy is defeated.
    /// Unlocks the next level and ends the battle.
    /// </summary>
    public void OnEnemyDefeated()
    {
        if (battleOver) return;

        battleLog.LogSystem("Enemy defeated!");

        // Unlock next level
        GameData.unlockedLevel = Mathf.Max(
            GameData.unlockedLevel,
            GameData.currentLevelIndex + 1
        );

        EndBattle();
    }

    /// <summary>
    /// Waits briefly, then loads the level select scene.
    /// </summary>
    IEnumerator ReturnToLevelSelect()
    {
        yield return new WaitForSeconds(2f); // Allow time for message display
        SceneManager.LoadScene("LevelSelect");
    }
}