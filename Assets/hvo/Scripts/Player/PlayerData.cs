using System;
using System.Collections.Generic;

[Serializable]
public class PlayerData
{
    public string playerName;
    public int level;
    public int experiencePoints;
    public List<IngredientEntry> ingredientList = new();

    [NonSerialized]
    public Dictionary<IngredientType, int> ingredientStorage = new();

    public PlayerData(string name)
    {
        playerName = name;
        level = 1;
        experiencePoints = 0;

        foreach (IngredientType ingredient in Enum.GetValues(typeof(IngredientType)))
        {
            ingredientStorage[ingredient] = 0;
        }

        // Initialize with one Flour
        ingredientStorage[IngredientType.Flour] = 1;
        UpdateIngredientList();
    }

    public void UpdateIngredientList()
    {
        ingredientList.Clear();
        foreach (var pair in ingredientStorage)
        {
            ingredientList.Add(new IngredientEntry { ingredient = pair.Key, amount = pair.Value });
        }
    }

    public void RestoreStorageFromList()
    {
        ingredientStorage = new();
        foreach (var entry in ingredientList)
        {
            ingredientStorage[entry.ingredient] = entry.amount;
        }
    }
}

[Serializable]
public class IngredientEntry
{
    public IngredientType ingredient;
    public int amount;
}
