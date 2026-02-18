using UnityEngine;
using UnityEngine.UI;
using CodeMonkey.Utils;
using System;

public class GameOverWindow : MonoBehaviour
{
    private Text scoreText;
    private Text highscoreText;

    private void Awake()
    {
        scoreText = transform.Find("scoreText").GetComponent<Text>();
        highscoreText = transform.Find("highscoreText").GetComponent<Text>();

        transform.Find("retryBtn").GetComponent<Button_UI>().ClickFunc = () => { Loader.Load(Loader.Scene.GameScene); };
        transform.Find("retryBtn").GetComponent<Button_UI>().AddButtonSounds();

        transform.Find("mainMenuBtn").GetComponent<Button_UI>().ClickFunc = () => { Loader.Load(Loader.Scene.MainMenu); };
        transform.Find("mainMenuBtn").GetComponent<Button_UI>().AddButtonSounds();
    }

    private void Start()
    {
        Bird.GetInstance().OnDied += BirdOnDied;
        Hide();
    }

    private void BirdOnDied(object sender, EventArgs e)
    {
        scoreText.text = Level.GetInstance().GetPipesPassedCount().ToString();

        // se c'è un nuovo highscore, aggiorna il testo
        if (Level.GetInstance().GetPipesPassedCount() >= Score.GetHighscore())
        {
            highscoreText.text = "NEW HIGHSCORE";
        }
        else
        {
            highscoreText.text = "HIGHSCORE: " + Score.GetHighscore().ToString();
        }

        Show();
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private void Show()
    {
        Debug.Log("GameOverWindow Show");
        gameObject.SetActive(true);
    }

}