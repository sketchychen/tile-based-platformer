using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameSession : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI livesText;
    [SerializeField] TextMeshProUGUI scoreText;

    int playerLives = 3;
    int coins = 0;
    float reloadDelay = 3.0f;

    void Awake()
    {
        int numGameSessions = FindObjectsOfType<GameSession>().Length;
        if (numGameSessions > 1)
        {
            Destroy(gameObject);  // destroy this instance of GameSession
        }
        else
        {
            DontDestroyOnLoad(gameObject); // persist if it's not already created
        }
    }

    void Start()
    {
        livesText.text = playerLives.ToString();
        scoreText.text = coins.ToString();
    }

    public void ProcessPlayerDeath()
    {
        if (playerLives > 1)
        {
            TakeLife();
        }
        else
        {
            Invoke("ResetGameSession", reloadDelay);
        }
    }

    void TakeLife()
    {
        playerLives--;
        livesText.text = playerLives.ToString();
        Invoke("ReloadScene", reloadDelay);
    }

    void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void ResetGameSession()
    {
        FindObjectOfType<ScenePersist>().ResetScenePersist();
        SceneManager.LoadScene(0);
        Destroy(gameObject);  // destroy this instance of GameSession
    }

    public void IncrementCoin()
    {
        coins++;
        scoreText.text = coins.ToString();
    }
}
