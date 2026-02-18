using System;
using UnityEngine;

public class GameAssets : MonoBehaviour
{
    private static GameAssets instance;
    
    public static GameAssets GetInstance()
    {
        return instance;
    }

    // Awake is called when the script instance is being loaded
    private void Awake()
    {
        instance = this;
    }

    // Sprites for the game (assigned in the Unity Editor) 
    public Sprite pipeHeadSprite;

    public Transform pfPipeHead;
    public Transform pfPipeBody;

    public Transform pfFloor;

    public Transform pfClouds1;
    public Transform pfClouds2;
    public Transform pfClouds3;

    public SoundAudioClip[] soundAudioClipArray;

    [Serializable]
    public class SoundAudioClip
    {
        public SoundManager.Sound sound;
        public AudioClip audioClip;
    }

}
