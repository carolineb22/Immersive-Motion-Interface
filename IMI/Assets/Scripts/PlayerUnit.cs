public class PlayerUnit : Unit
{
    public Spell[] spells;

    public Spell GetSpell(int index)
    {
        return spells[index];
    }
}