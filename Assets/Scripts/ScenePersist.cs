using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScenePersist : MonoBehaviour
{
    void Awake()
    {
        int numScenePersists = FindObjectsOfType<ScenePersist>().Length;
        if (numScenePersists > 1)
        {
            Destroy(gameObject);  // destroy this instance of GameSession
        }
        else
        {
            DontDestroyOnLoad(gameObject); // persist if it's not already created
        }
    }

    public void ResetScenePersist()
    {
        Destroy(gameObject);
    }
}
