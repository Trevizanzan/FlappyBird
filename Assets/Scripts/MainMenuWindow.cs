using CodeMonkey.Utils;
using UnityEngine;

public class MainMenuWindow : MonoBehaviour
{
    private void Awake()
    {
        transform.Find("playBtn").GetComponent<Button_UI>().ClickFunc = () => { Loader.Load(Loader.Scene.GameScene); };
        transform.Find("playBtn").GetComponent<Button_UI>().AddButtonSounds();

        transform.Find("quitBtn").GetComponent<Button_UI>().ClickFunc = () => { Application.Quit(); };
        transform.Find("quitBtn").GetComponent<Button_UI>().AddButtonSounds();
    }

}
