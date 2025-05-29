using System.Collections.Generic;
using UnityEngine;

public class CraftingSystem : MonoBehaviour
{
    public List<Recipe> recipes = new List<Recipe>();

    void Start()
    {
        recipes.Add(new Recipe(ItemType.Bread, new List<IngredientType> { IngredientType.Flour }, EffectType.RestoreSmallHP));
        recipes.Add(new Recipe(ItemType.BerryCake, new List<IngredientType> { IngredientType.Flour, IngredientType.Egg, IngredientType.Berry }, EffectType.RestoreHP));
        recipes.Add(new Recipe(ItemType.ApplePie, new List<IngredientType> { IngredientType.Flour, IngredientType.Egg, IngredientType.Apple }, EffectType.IncreaseHealth));
        recipes.Add(new Recipe(ItemType.BerryMuffin, new List<IngredientType> { IngredientType.Flour, IngredientType.Berry }, EffectType.RestoreMana));
        recipes.Add(new Recipe(ItemType.ScrambledEggs, new List<IngredientType> { IngredientType.Egg }, EffectType.IncreaseStrength));
        recipes.Add(new Recipe(ItemType.ToastedEggs, new List<IngredientType> { IngredientType.Egg, IngredientType.Coal }, EffectType.IncreaseDefense));
        recipes.Add(new Recipe(ItemType.RoastedChicken, new List<IngredientType> { IngredientType.Chicken }, EffectType.MediumHeal));
        recipes.Add(new Recipe(ItemType.RoastedMeat, new List<IngredientType> { IngredientType.Meat, IngredientType.Coal }, EffectType.HighHeal));
        recipes.Add(new Recipe(ItemType.CheeseEmpanada, new List<IngredientType> { IngredientType.Corn, IngredientType.Cheese }, EffectType.RestoreHP));
        recipes.Add(new Recipe(ItemType.MeatEmpanada, new List<IngredientType> { IngredientType.Meat, IngredientType.Flour, IngredientType.Onion }, EffectType.RestoreHP));
        recipes.Add(new Recipe(ItemType.MilkFlan, new List<IngredientType> { IngredientType.Milk, IngredientType.Egg, IngredientType.Flour }, EffectType.IncreaseAgility));
        recipes.Add(new Recipe(ItemType.HotMilk, new List<IngredientType> { IngredientType.Milk }, EffectType.IncreaseSpeed));
        recipes.Add(new Recipe(ItemType.MarineBroth, new List<IngredientType> { IngredientType.Fish, IngredientType.Water }, EffectType.IncreaseAttack));
        recipes.Add(new Recipe(ItemType.FishSandwich, new List<IngredientType> { IngredientType.Bread, IngredientType.Fish }, EffectType.IncreaseDefense));
        recipes.Add(new Recipe(ItemType.FarmerPizza, new List<IngredientType> { IngredientType.Flour, IngredientType.Cheese, IngredientType.Tomato }, EffectType.IncreaseAttackAndDefense));
        recipes.Add(new Recipe(ItemType.FarmerStew, new List<IngredientType> { IngredientType.Meat, IngredientType.Potato, IngredientType.Water }, EffectType.MassiveHealAndAttackBoost));
    }

    public Recipe Craft(List<IngredientType> ingredients)
    {
        foreach (var recipe in recipes)
        {
            if (recipe.CanCraft(ingredients))
            {
                Debug.Log($"Crafted: {recipe.ResultItem} with effect {recipe.Effect}");
                return recipe;
            }
        }

        Debug.Log("No recipe could be crafted.");
        return null;
    }
}
