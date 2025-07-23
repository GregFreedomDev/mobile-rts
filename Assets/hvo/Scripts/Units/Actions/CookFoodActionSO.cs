using UnityEngine;

[CreateAssetMenu(fileName = "CookFoodAction", menuName = "HvO/Actions/CookFoodAction")]
public class CookFoodActionSO : ActionSO
{
    public override void Execute(GameManager gameManager)
    {
        gameManager.StartCookProcess(this);
    }
}