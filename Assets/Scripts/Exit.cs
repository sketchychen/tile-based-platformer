using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Exit : MonoBehaviour
{
    Scene currentScene;
    // Start is called before the first frame update
    void Start()
    {
        currentScene = SceneManager.GetActiveScene();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Exit triggered!");
        StartCoroutine(LoadNextLevel());
    }

    IEnumerator LoadNextLevel()
    {
        /*
        Need to check for valid indexes!!!
        Is there another after this or is this the last one?
        */
        int buildIndex = currentScene.buildIndex;
        yield return new WaitForSeconds(3.0f);
        SceneManager.LoadScene(buildIndex+1);
    }
}
