using UnityEngine;

public class ControlsMenuControls : MonoBehaviour
{
    private GameManager gameManager;

    private void Start()
    {
        gameManager = GameManager.Instance;
    }

    public void ReturnToPauseMenu()
    {
        gameManager.OpenControlsMenu(true);
    }
}
