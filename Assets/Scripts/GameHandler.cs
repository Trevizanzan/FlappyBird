using UnityEngine;
using CodeMonkey;
using CodeMonkey.Utils;

public class GameHandler : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        Debug.Log("GameHandler.Start");

        //int count = 0; 
        //FunctionPeriodic.Create(() => {
        //    CMDebug.TextPopupMouse("Hello World " + count);
        //    count++;
        //    //Debug.Log("GameHandler.Update: " + count);
        //}, .300f);

        //GameObject gameObject = new GameObject("Pipe", typeof(SpriteRenderer));
        //gameObject.GetComponent<SpriteRenderer>().sprite = GameAssets.GetInstance().pipeHeadSprite;

        Score.Start();
    }
}
