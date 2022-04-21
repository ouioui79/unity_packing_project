using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class sceneSwitch : MonoBehaviour
{
    public string sceneName;
    public void sceneSwitcher()
    {
        SceneManager.LoadScene(sceneName);
    }
}
