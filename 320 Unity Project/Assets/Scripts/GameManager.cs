using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public bool paused = false;
    private int cheeseCount = 0;
    private PlayerController controllerScript;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject controlsMenu;
    [SerializeField] private GameObject playerUI;
    [SerializeField] private TMP_Text cheeseCounter;

    private static GameManager instance;
    public static GameManager Instance { get { return instance; } }
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }

    public void IncrementCheeseCount()
    {
        cheeseCount++;
        string cheeseCountText = "";
        if (cheeseCount < 10) cheeseCountText = "0" + cheeseCount.ToString();
        else cheeseCountText = cheeseCount.ToString();
        cheeseCounter.text = cheeseCountText;
    }

    public void SetPlayerController(PlayerController script)
    {
        controllerScript = script;
        Debug.Log("player script added!");
    }

    public void PauseGame()
    {
        paused = true;
        Time.timeScale = 0.0f;
        Cursor.lockState = CursorLockMode.None;
        playerUI.SetActive(false);
        pauseMenu.SetActive(true);
    }

    public void OpenControlsMenu(bool isOpen)
    {
        controlsMenu.SetActive(!isOpen);
        pauseMenu.SetActive(isOpen);
    }

    public void ResumeGame()
    {
        paused = false;
        Time.timeScale = 1.0f;
        Cursor.lockState = CursorLockMode.Locked;
        pauseMenu.SetActive(false);
        controlsMenu.SetActive(false);
        playerUI.SetActive(true);
    }
}
