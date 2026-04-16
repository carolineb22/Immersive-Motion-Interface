public static class ElementSystem
{
    public static float GetMultiplier(ElementType attack, ElementType target)
    {
        //fire is strong against tree
        if (attack == ElementType.Fire && target == ElementType.Tree)
            return 1.5f;

        //tree is strong against water
        if (attack == ElementType.Tree && target == ElementType.Water)
            return 1.5f;

        //water is strong against fire
        if (attack == ElementType.Water && target == ElementType.Fire)
            return 1.5f;

        // Weak cases
        if (attack == target)
            return 0.75f;

        return 1f;
    }
}