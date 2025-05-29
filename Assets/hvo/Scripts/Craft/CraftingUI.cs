using System.Collections.Generic;
using hvo.Scripts.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftingUI : MonoBehaviour
{
    [SerializeField] private GameObject craftingButtonPrefab;
    [SerializeField] private Transform craftingPanel;

    private Player player;
    private CraftingSystem craftingSystem;

    void Start()
    {
        player = BaseGameManager.Get().Player;
        craftingSystem = FindObjectOfType<CraftingSystem>();

        GenerateCraftingUI();
    }

    void GenerateCraftingUI()
    {
        foreach (var recipe in craftingSystem.recipes)
        {
            GameObject buttonObj = Instantiate(craftingButtonPrefab, craftingPanel);
            Button button = buttonObj.GetComponent<Button>();
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();

            buttonText.text = $"{recipe.ResultItem} - Requires: {string.Join(", ", recipe.Ingredients)}";

            bool canCraft = player.HasIngredients(recipe.Ingredients);
            button.interactable = canCraft;

            if (canCraft)
            {
                button.onClick.AddListener(() =>
                {
                    player.ConsumeIngredients(recipe.Ingredients);
                    player.GainExperience(10); // Or another value
                    var crafted = craftingSystem.Craft(recipe.Ingredients);
                    if (crafted != null)
                    {
                        player.AddCraftedItemAsIngredient(crafted.ResultItem);
                    }

                    player.SavePlayer();
                    RefreshUI();
                });
            }
        }
    }

    void RefreshUI()
    {
        foreach (Transform child in craftingPanel)
        {
            Destroy(child.gameObject);
        }

        GenerateCraftingUI();
    }
}