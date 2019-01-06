using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneLoader : MonoBehaviour
{
    void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }
}
