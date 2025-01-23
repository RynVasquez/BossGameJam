using UnityEngine;

public class GameManager : MonoBehaviour
{
    private SceneController sceneController;
    private PlayerMovement playerMovement;
    private PlayerCombat playerCombat;
    private PlayerCamera playerCamera;

    [SerializeField] private GameObject pauseMenu;

    private bool gameIsPaused;

    // Start is called before the first frame update
    void Awake()
    {
        sceneController = GameObject.FindObjectOfType<SceneController>();
        playerMovement = GameObject.FindObjectOfType<PlayerMovement>();
        playerCombat = GameObject.FindObjectOfType<PlayerCombat>();
        playerCamera = GameObject.FindObjectOfType<PlayerCamera>();

        pauseMenu.SetActive(false);
        gameIsPaused = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape) && !gameIsPaused)
        {
            pauseMenu.SetActive(true);
            PausePlayer();
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
            gameIsPaused = true;
            Time.timeScale = 0;
        }
        else if(Input.GetKeyDown(KeyCode.Escape) && gameIsPaused)
        {
            pauseMenu.SetActive(false);
            UnpausePlayer();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            gameIsPaused = false;
            Time.timeScale = 1;
        }
    }

    void PausePlayer()
    {
        playerMovement.enabled = false;
        playerCombat.enabled = false;
        playerCamera.enabled = false;
    }

    void UnpausePlayer()
    {
        playerMovement.enabled = true;
        playerCombat.enabled = true;
        playerCamera.enabled = true;
    }

    public void ResumeGame()
    {
        pauseMenu.SetActive(false);
        UnpausePlayer();
        gameIsPaused = false;
    }

    public void ReturnToMenu()
    {
        Time.timeScale = 1;
        sceneController.LoadScene("Main Menu");
    }
}
