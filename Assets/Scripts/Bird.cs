using CodeMonkey;
using System;
using Unity.VisualScripting;
using UnityEngine;

public class Bird : MonoBehaviour
{
    private const float JUMP_AMOUNT = 100f;
    private Rigidbody2D birdRigidbody2D;

    public event EventHandler OnDied;
    public event EventHandler OnStartedPlaying;

    private static Bird instance;
    public static Bird GetInstance() {
        return instance;
    }

    private State state;
    private enum State
    {
        WaitingToStart,
        Playing,
        Dead
    }

    private void Awake()
    {
        instance = this;
        birdRigidbody2D = GetComponent<Rigidbody2D>();
        birdRigidbody2D.bodyType = RigidbodyType2D.Static;
        state = State.WaitingToStart;
    }

    private void Update()
    {
        switch(state) {
            case State.WaitingToStart:
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButton(0))
                {
                    state = State.Playing;
                    birdRigidbody2D.bodyType = RigidbodyType2D.Dynamic;
                    Jump();

                    // Call the event to notify that the bird has started playing
                    if (OnStartedPlaying != null)
                    {
                        OnStartedPlaying(this, EventArgs.Empty);
                    }
                }
                break;
            case State.Playing:
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButton(0))
                {
                    Jump();
                }

                transform.eulerAngles = new Vector3(0, 0, birdRigidbody2D.linearVelocity.y * 0.2f);   // Se la velocità è positiva, il bird guarda in alto, se è negativa, guarda in basso


                // FOR FUN: fai in modo che se la velocità è positiva, il bird guarda avanti, se è negativa, guarda indietro:
                //transform.localScale = new Vector3(birdRigidbody2D.linearVelocity.y > 0 ? 1 : -1, 1, 1);

                break;
            case State.Dead:

                break;
        }
    }

    private void Jump()
    {
        birdRigidbody2D.linearVelocity = Vector2.up * JUMP_AMOUNT;
        SoundManager.PlaySound(SoundManager.Sound.BirdJump);
    }

    /// <summary>
    /// Il flusso completo step-by-step
    /// 
    /// Start del gioco:
    /// Level fa Bird.GetInstance().OnDied += Bird_OnDied
    /// Level si registra: "Quando Bird muore, chiamami Bird_OnDied"
    /// 
    /// Gioco in corso:
    /// Bird salta, vola, tutto normale
    /// 
    /// Bird collide con un tubo:
    /// OnTriggerEnter2D viene chiamato
    /// Bird esegue: OnDied(this, EventArgs.Empty) → lancia l'evento
    /// 
    /// Unity vede che OnDied è stato lanciato:
    /// Controlla chi si è registrato
    /// Trova Level con il metodo Bird_OnDied
    /// Chiama automaticamente Bird_OnDied(bird, EventArgs.Empty)
    /// 
    /// Level reagisce:
    /// Bird_OnDied viene eseguito
    /// Cambia state = State.BirdDead
    /// 
    /// 
    /// 
    /// Perchè usare gli eventi?
    /// Bird non sa CHI ascolta, gli basta dire "sono morto"
    /// Level, SoundManager, UIManager si registrano indipendentemente
    /// Bird rimane pulito e indipendente
    /// 
    /// </summary>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        birdRigidbody2D.bodyType = RigidbodyType2D.Static;
        SoundManager.PlaySound(SoundManager.Sound.Lose);
        // Call the event to notify that the bird has died
        if (OnDied != null) // controllo se qualcuno si è registrato -> "C'è almeno uno che ascolta?"
        {
            OnDied(this, EventArgs.Empty);  // LANCIO l'evento -> "Ok, chiamo TUTTI quelli che si sono registrati"
        }
    }
}
