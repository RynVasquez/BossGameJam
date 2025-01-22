using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneHandler : MonoBehaviour
{
    [SerializeField] private SceneController sceneController;

    public void PlayGame()
    {
        sceneController.LoadScene("Game");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
