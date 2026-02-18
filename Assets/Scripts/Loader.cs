using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Loader
{
    public enum Scene
    {
        MainMenu,
        GameScene,
        Loading,
    }

    private static Scene targetScene;

    public static void Load(Scene scene)
    {
        // Set the target scene to load after the loading screen is done
        SceneManager.LoadScene(Scene.Loading.ToString());

        targetScene = scene;
    }

    public static void LoadTargetScene()
    {
        // Load the target scene
        SceneManager.LoadScene(targetScene.ToString());
    }
}
