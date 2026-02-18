using System;
using UnityEngine;
using UnityEngine.UI;

public class ScoreWindow : MonoBehaviour
{
    private Text scoreText;
    private Text highscoreText;

    private void Awake()
    {
        scoreText = transform.Find("scoreText").GetComponent<Text>();
        highscoreText = transform.Find("highscoreText").GetComponent<Text>();
        //Debug.Log("scoreText trovato: " + (scoreText != null));
    }

    private void Start()
    {
        highscoreText.text = "HIGHSCORE: " + Score.GetHighscore().ToString();
    }

    private void Update()
    {
        //scoreText.text = Level.GetInstance().GetPipesPassedCount().ToString();
        scoreText.text = Level.GetInstance().GetPipesPassedCount().ToString();
        //Debug.Log("Score: " + score);
        //scoreText.text = score.ToString();

        Bird.GetInstance().OnDied += HideScoreWindow;
    }

    private void HideScoreWindow(object sender, EventArgs e)
    {
        Hide();
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
