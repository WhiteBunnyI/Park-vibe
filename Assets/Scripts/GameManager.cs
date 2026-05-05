using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    static GameManager _instance;

    public static event Action OnGameOver;

    private void Start()
    {
        if (_instance != null)
        {
            Destroy(this);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        InputSystem.actions.FindAction("Exit").performed += (e) => ExitGame();
    }

    public static void LoadGame()
    {
        SceneManager.LoadScene(1);
        Cursor.lockState = CursorLockMode.Locked;
    }

    public static void GameOver()
    {
        OnGameOver.Invoke();
        _instance.StartCoroutine(ChangeToMainMenu());
    }

    public static void ExitGame()
    {
        Application.Quit();
    }

    private static IEnumerator ChangeToMainMenu()
    {
        yield return new WaitForSeconds(3.5f);
        SceneManager.LoadScene(0);
        Cursor.lockState = CursorLockMode.None;
    }
}