using UnityEngine;


public class MainMenu : MonoBehaviour
{
    private SceneController sceneController;

    void Awake()
    {
        sceneController = GameObject.FindObjectOfType<SceneController>();
    }

    public void PlayGame()
    {
        sceneController.LoadScene("Game");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
