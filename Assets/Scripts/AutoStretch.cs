using UnityEngine;

public class AutoStretch : MonoBehaviour
{
    void Start()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        float spriteWidth = sr.sprite.bounds.size.x;
        float worldScreenHeight = Camera.main.orthographicSize * 2f;
        float worldScreenWidth = worldScreenHeight * Screen.width / Screen.height;

        Vector3 s = transform.localScale;
        s.x = worldScreenWidth / spriteWidth;
        transform.localScale = s;
    }
}