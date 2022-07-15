using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Exit : MonoBehaviour
{
    float loadDelay = 3.0f;
    bool isActivated = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isActivated) { return; }
        isActivated = true;
        StartCoroutine(LoadNextLevel());
    }

    IEnumerator LoadNextLevel()
    {
        /*
        Need to check for valid indexes!!!
        Is there another after this or is this the last one?
        */
        yield return new WaitForSecondsRealtime(loadDelay);
        int currIndex = SceneManager.GetActiveScene().buildIndex;
        int nextIndex = currIndex + 1;

        if (nextIndex == SceneManager.sceneCountInBuildSettings)
        {
            nextIndex = 0;
        }

        SceneManager.LoadScene(nextIndex);
    }
}
