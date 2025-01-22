using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    [SerializeField] private float sceneFadeDuration;
    private SceneFader sceneFade;

    void Awake()
    {
        sceneFade = GameObject.FindObjectOfType<SceneFader>();
    }

    private IEnumerator Start()
    {
        yield return sceneFade.FadeIn(sceneFadeDuration);
    }

    private IEnumerator LoadSceneCoroutine(string sceneName)
    {
        yield return sceneFade.FadeOut(sceneFadeDuration);
        yield return SceneManager.LoadSceneAsync(sceneName);
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneCoroutine(sceneName)); 
    }
}
