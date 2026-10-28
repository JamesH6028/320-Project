using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuControls : MonoBehaviour
{
    private GameManager gameManager;

    private void Start()
    {
        gameManager = GameManager.Instance;
    }

    public void Resume()
    {
        gameManager.ResumeGame();
    }

    public void OpenControls()
    {
        gameManager.OpenControlsMenu(false);
    }

    public void ResetLevel()
    {
        Resume();
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
