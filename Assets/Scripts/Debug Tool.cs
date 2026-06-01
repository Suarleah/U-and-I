using System;
using UnityEngine;

public class DebugTool : MonoBehaviour
{
    private MainMenuManager mainMenuManager;
    void Start()
    {
        mainMenuManager = FindFirstObjectByType<MainMenuManager>();
    }

    // Update is called once per frame
    public void ChangeScene(String sceneName)
    {
        mainMenuManager.sceneToLoad = sceneName;
    }
}
