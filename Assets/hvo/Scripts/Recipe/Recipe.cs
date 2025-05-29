using System.Collections.Generic;

public class Recipe
{
    public ItemType ResultItem { get; private set; }
    public List<IngredientType> Ingredients { get; private set; }
    public EffectType Effect { get; private set; }

    public Recipe(ItemType resultItem, List<IngredientType> ingredients, EffectType effect)
    {
        ResultItem = resultItem;
        Ingredients = ingredients;
        Effect = effect;
    }

    public bool CanCraft(List<IngredientType> availableIngredients)
    {
        foreach (var ingredient in Ingredients)
        {
            if (!availableIngredients.Contains(ingredient))
                return false;
        }
        return true;
    }
}

public enum IngredientType
{
    Flour,
    Egg,
    Berry,
    Apple,
    Coal,
    Chicken,
    Meat,
    Corn,
    Cheese,
    Onion,
    Milk,
    Fish,
    Water,
    Bread,
    Potato,
    Tomato
}

public enum ItemType
{
    Bread,
    BerryCake,
    BerryMuffin,
    ApplePie,
    CheeseEmpanada,
    MeatEmpanada,
    MilkFlan,
    HotMilk,
    MarineBroth,
    FishSandwich,
    FarmerPizza,
    FarmerStew,
    RoastedChicken,
    RoastedMeat,
    ScrambledEggs,
    ToastedEggs
}

public enum EffectType
{
    RestoreSmallHP,
    RestoreHP,
    RestoreMana,
    IncreaseHealth,
    IncreaseStrength,
    IncreaseDefense,
    IncreaseAgility,
    IncreaseSpeed,
    MediumHeal,
    HighHeal,
    IncreaseAttack,
    IncreaseAttackAndDefense,
    MassiveHealAndAttackBoost
}
