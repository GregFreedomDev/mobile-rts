using System;
using System.Collections.Generic;
using UnityEngine;

public class Player
{
    public PlayerData Data;
    private string SaveKey;

    public Player(string playerName = "Player1")
    {
        LoadPlayer(playerName);
        Data.RestoreStorageFromList();
    }
    
    public void AddCraftedItemAsIngredient(ItemType item)
    {
        // Usa un mapeo 1:1 si ItemType tiene el mismo nombre que IngredientType
        if (Enum.TryParse(item.ToString(), out IngredientType ingredient))
        {
            AddIngredient(ingredient, 1);
            Debug.Log($"Crafted item added to inventory: {ingredient}");
        }
        else
        {
            Debug.LogWarning($"No matching ingredient for crafted item: {item}");
        }
    }


    public void AddIngredient(IngredientType ingredient, int amount)
    {
        if (!Data.ingredientStorage.ContainsKey(ingredient))
            Data.ingredientStorage[ingredient] = 0;

        Data.ingredientStorage[ingredient] += amount;
    }

    public bool HasIngredients(List<IngredientType> required)
    {
        Dictionary<IngredientType, int> tempStorage = new(Data.ingredientStorage);

        foreach (var ing in required)
        {
            if (!tempStorage.ContainsKey(ing) || tempStorage[ing] <= 0)
                return false;
            tempStorage[ing]--;
        }

        return true;
    }

    public void ConsumeIngredients(List<IngredientType> required)
    {
        foreach (var ing in required)
        {
            if (Data.ingredientStorage.ContainsKey(ing))
                Data.ingredientStorage[ing] = Mathf.Max(0, Data.ingredientStorage[ing] - 1);
        }
    }

    public void GainExperience(int xp)
    {
        Data.experiencePoints += xp;
        if (Data.experiencePoints >= 100)
        {
            Data.level++;
            Data.experiencePoints = 0;
            Debug.Log("Level up! Current level: " + Data.level);
        }
    }

    public void SavePlayer()
    {
        if (string.IsNullOrEmpty(SaveKey)) return;
        string json = JsonUtility.ToJson(Data);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
        Debug.Log("Player data saved.");
    }

    public void LoadPlayer(string playerName)
    {
        if (PlayerPrefs.HasKey($"PlayerData_{playerName}"))
        {
            string json = PlayerPrefs.GetString($"PlayerData_{playerName}");
            Data = JsonUtility.FromJson<PlayerData>(json);
            
        }
        else
        {
            Data = new PlayerData(playerName);
            SaveKey = $"PlayerData_{playerName}";
            SavePlayer();
        }

        Debug.Log("Player data loaded: " + Data.playerName);
    }
}
