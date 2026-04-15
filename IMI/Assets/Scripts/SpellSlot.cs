using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpellSlot : MonoBehaviour
{
    public Image icon;
    public TMP_Text keyText;
    public Image cooldownOverlay;

    private Spell spell;
    private int currentCooldown = 0;
    public Image highlightBorder;

    public void Setup(Spell newSpell)
    {
        spell = newSpell;
        icon.sprite = spell.icon;
        keyText.text = spell.spellName;

        UpdateUI();
    }

    public Spell GetSpell()
    {
        return spell;
    }

    public bool IsReady()
    {
        return currentCooldown <= 0;
    }

    public void TriggerCooldown()
    {
        currentCooldown = spell.cooldownTurns;
        UpdateUI();
    }

    public void ReduceCooldown()
    {
        if (currentCooldown > 0)
        {
            currentCooldown--;
            UpdateUI();
        }
    }

    public void SetHighlighted(bool isHighlighted)
{
    if (highlightBorder != null)
    {
        highlightBorder.enabled = isHighlighted;
    }
}

     public void UpdateUI()
    {
        float percent = spell.cooldownTurns > 0 
            ? (float)currentCooldown / spell.cooldownTurns 
            : 0;

        cooldownOverlay.fillAmount = percent;

        icon.color = IsReady() ? Color.white : Color.gray;
    }
}