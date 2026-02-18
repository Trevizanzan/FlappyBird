using CodeMonkey;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Level : MonoBehaviour
{
    private static Level instance;

    private const float CAMERA_ORTHO_SIZE = 50f;
    private const float BIRD_X_POSITION = 0f;

    // PIPE
    private const float PIPE_WIDTH = 7.8f;
    private const float PIPE_HEAD_HEIGHT = 3.75f;
    private const float PIPE_MOVE_SPEED = 30f;
    private const float PIPE_DESTROY_X_POSITION = -100f;
    private const float PIPE_SPAWN_X_POSITION = +100f;
    private List<Pipe> pipeList;
    private int pipesPassedCount = 0;
    private int pipesSpawned;
    private float pipeSpawnTimer;
    private float pipeSpawnTimerMax;
    private float gapSize;

    // FLOOR
    private const float FLOOR_MOVE_SPEED = 30f; // stessa velocità delle pipe per dare l'effetto di movimento coordinato
    private const float FLOOR_WIDTH = 192f;
    private Floor floor1;
    private Floor floor2;

    // CLOUDS
    private const float CLOUDS_MOVE_SPEED = 50f;
    private const float CAMERA_WIDTH = 178f;    // La larghezza totale della visuale della camera è 178 unità (da -89 a +89), quindi per posizionare le nuvole all'estrema destra, usiamo CAMERA_WIDTH / 2
    private const float CAMERA_RIGHT = CAMERA_WIDTH / 2f;  // +89
    private const float CAMERA_LEFT = -CAMERA_WIDTH / 2f;  // -89
    private const float CLOUD_WIDTH = 128f;
    private const float CLOUD_HEIGHT = 26f;

    private Cloud cloud1;
    private Cloud cloud2;
    private Cloud cloud3;
    private int currentCloudIndex = 0;  // aggiungi in cima alla classe Level

    public static Level GetInstance()
    {
        return instance;
    }

    private State state;

    private enum State
    {
        WaitingToStart,
        Playing,
        BirdDead
    }

    public enum Difficulty
    {
        Easy,
        Medium,
        Hard,
        Impossible
    }

    private void Awake()
    {
        instance = this;
        pipeList = new List<Pipe>();
        pipeSpawnTimerMax = 1f;
        SetDifficulty(Difficulty.Easy);
        state = State.WaitingToStart;
    }

    /// <summary>
    /// la classe che ASCOLTA l'evento
    /// Traduzione:
    ///     Bird.GetInstance() → prendo l'istanza del Bird
    ///     .OnDied → accedo al suo evento
    ///     += Bird_OnDied → "Quando lanci OnDied, chiamami questo metodo: Bird_OnDied"
    /// </summary>
    private void Start()
    {
        // Inizializza i due floor
        floor1 = CreateFloor(0f);  // primo floor a posizione 0
        floor2 = CreateFloor(FLOOR_WIDTH);  // secondo floor affiancato

        cloud1 = CreateClouds(CAMERA_RIGHT - CLOUD_WIDTH, CLOUD_HEIGHT);     // 26f è l'altezza delle nuvole, fissa per ora, poi si può variare per creare più varietà
        cloud2 = CreateClouds(CAMERA_RIGHT, CLOUD_HEIGHT);
        cloud3 = CreateClouds(CAMERA_RIGHT + CLOUD_WIDTH, CLOUD_HEIGHT);

        // MI REGISTRO agli eventi del Bird
        Bird.GetInstance().OnDied += Bird_OnDied;   // MI REGISTRO all'evento
        Bird.GetInstance().OnStartedPlaying += Bird_OnStartedPlaying;   // MI REGISTRO all'evento
    }

    private void Bird_OnStartedPlaying(object sender, EventArgs e)
    {
        state = State.Playing;
    }

    private void Bird_OnDied(object sender, EventArgs e)
    {
        //CMDebug.TextPopupMouse("Game Over!");
        state = State.BirdDead;
    }

    private void Update()
    {
        if (state == State.Playing)
        {
            // pipes 
            HandlePipeMovement();
            HandlePipeSpawning();

            // floor
            HandleFloorMovement();

            // clouds
            HandleCloudsMovement();
        }
    }

    private void HandleCloudsMovement()
    {
        cloud1.Move();
        cloud2.Move();
        cloud3.Move();

        // Se la nuvola è completamente fuori dalla visuale della camera (considerando la sua larghezza), allora la distruggiamo e ne creiamo una nuova a destra
        bool isOutOfCamera = cloud1.GetXPosition() < (CAMERA_LEFT - (CLOUD_WIDTH / 2f));  
        if (isOutOfCamera)
        {
            float nextCloudX = cloud3.GetXPosition() + CLOUD_WIDTH;  // prossima nuvola subito dopo la nuvola3 (quella più a destra)

            // Shift tutto a sinistra delle nuvole: cloud2 diventa cloud1, cloud3 diventa cloud2, e creiamo una nuova cloud3 a destra
            cloud1.DestroySelf();
            cloud1 = cloud2;
            cloud2 = cloud3;
            cloud3 = CreateClouds(nextCloudX, CLOUD_HEIGHT);
        }
    }

    private Cloud CreateClouds(float xPosition, float yPosition)
    {
        // Array delle 3 texture
        Transform[] cloudPrefabs = new Transform[] {
            GameAssets.GetInstance().pfClouds1,
            GameAssets.GetInstance().pfClouds2,
            GameAssets.GetInstance().pfClouds3
        };

        // Prendi quella corrente e ruota l'index
        Transform cloudsTransform = Instantiate(cloudPrefabs[currentCloudIndex]);
        currentCloudIndex = (currentCloudIndex + 1) % 3;

        cloudsTransform.position = new Vector3(xPosition, yPosition, 0f);

        // Imposta l'ordine di rendering per assicurarti che le nuvole siano dietro a tutto il resto (dietro alle pipe in questo caso)
        cloudsTransform.GetComponent<SpriteRenderer>().sortingOrder = -1;  // dietro a tutto

        return new Cloud(cloudsTransform);
    }

    private void HandleFloorMovement()
    {
        floor1.Move();  // quello più a sinistra
        floor2.Move();  // quello più a destra

        bool isOutOfCamera = floor1.GetXPosition() < (CAMERA_LEFT - (FLOOR_WIDTH / 2f));
        if (isOutOfCamera)
        {
            float nextFloorX = floor2.GetXPosition() + FLOOR_WIDTH;  // nuovo floor dopo floor2

            // Shift tutto a sinistra
            floor1.DestroySelf();
            floor1 = floor2;
            floor2 = CreateFloor(nextFloorX);
        }
    }

    private Floor CreateFloor(float xPosition)
    {
        Transform floorTransform = Instantiate(GameAssets.GetInstance().pfFloor);
        floorTransform.position = new Vector3(xPosition, -CAMERA_ORTHO_SIZE, 0f);
        return new Floor(floorTransform);
    }

    /// <summary>
    /// Updates the position of each pipe in the pipe list to simulate movement over time.
    /// </summary>
    private void HandlePipeMovement()
    {
        for (int i = 0; i < pipeList.Count; i++)
        {
            Pipe pipe = pipeList[i];

            bool isToTheRightOfBird = pipe.GetXPosition() > BIRD_X_POSITION;

            pipe.Move();

            if (isToTheRightOfBird && pipe.GetXPosition() <= BIRD_X_POSITION)
            {
                // The pipe has just passed the bird's horizontal position, so we can count it as a successfully passed pipe
                if (pipe.IsBottom())
                {
                    // Only count the bottom pipe as passed to avoid counting the same pipe twice
                    pipesPassedCount++;
                    SoundManager.PlaySound(SoundManager.Sound.Score);
                }
            }

            if (pipe.GetXPosition() < PIPE_DESTROY_X_POSITION) // Check if the pipe has moved off-screen to the left
            {
                pipe.DestroySelf(); // Destroy the pipe's GameObjects
                pipeList.Remove(pipe); // Remove the pipe from the list
                i--; // Decrement the index to account for the removed pipe
            }
        }
    }

    private void HandlePipeSpawning()
    {
        pipeSpawnTimer -= Time.deltaTime;
        if (pipeSpawnTimer < 0)
        {
            // Time to spawn a new pipe
            pipeSpawnTimer = pipeSpawnTimerMax;

            float heightEdgeLimit = 10f;
            float minHeight = heightEdgeLimit + (gapSize * 0.5f);
            float totalHeight = (CAMERA_ORTHO_SIZE * 2f);
            float maxHeight = totalHeight - heightEdgeLimit - (gapSize * 0.5f);

            float height = UnityEngine.Random.Range(minHeight, maxHeight);
            CreateGapPipes(height, gapSize, PIPE_SPAWN_X_POSITION);
        }
    }

    private void SetDifficulty(Difficulty difficulty)
    {
        switch (difficulty)
        {
            case Difficulty.Easy:
                gapSize = 50f;
                pipeSpawnTimerMax = 1.5f;
                break;
            case Difficulty.Medium:
                gapSize = 40f;
                pipeSpawnTimerMax = 1.2f;
                break;
            case Difficulty.Hard:
                gapSize = 33f;
                pipeSpawnTimerMax = 1f;
                break;
            case Difficulty.Impossible:
                gapSize = 22f;
                pipeSpawnTimerMax = 0.8f;
                break;
        }
    }

    private Difficulty GetDifficulty()
    {
        if (pipesSpawned >= 30)
            return Difficulty.Impossible;
        else if (pipesSpawned >= 20)
            return Difficulty.Hard;
        else if (pipesSpawned >= 10)
            return Difficulty.Medium;
        else
            return Difficulty.Easy;
    }

    // The CreateGapPipes method is responsible for creating a pair of pipes with a gap between them.
    // It takes three parameters:
    // gapY, which is the vertical position of the center of the gap;
    // gapSize, which is the size of the gap;
    // and xPosition, which is the horizontal position where the pipes will be created.
    // The method first calls the CreatePipe method to create the bottom pipe. The height of the bottom pipe is calculated as gapY minus half of the gap size, which ensures that the bottom pipe ends at the correct position to create the gap.
    // Next, the method calls the CreatePipe method again to create the top pipe. The height of the top pipe is calculated as the total height of the camera's orthographic view (which is twice the CAMERA_ORTHO_SIZE) minus gapY minus half of the gap size. This ensures that the top pipe starts at the correct position to create the gap.
    private void CreateGapPipes(float gapY, float gapSize, float xPosition)
    {
        CreatePipe(gapY - (gapSize * 0.5f), xPosition, true);
        CreatePipe(CAMERA_ORTHO_SIZE * 2f - gapY - (gapSize * 0.5f), xPosition, false);
        pipesSpawned++;
        SetDifficulty(GetDifficulty());
    }

    /// <summary>
    /// Creates a new pipe at the specified horizontal position and height, optionally as a bottom or top pipe segment.
    /// </summary>
    private void CreatePipe(float height, float xPosition, bool createBottom)
    {
        // Instantiate the pipe 

        // Create the pipe head
        Transform pipeHead = Instantiate(GameAssets.GetInstance().pfPipeHead);
        float pipeHeadYPosition;
        if (createBottom)
        {
            pipeHeadYPosition = -CAMERA_ORTHO_SIZE + height - (PIPE_HEAD_HEIGHT * 0.5f);
        }
        else
        {
            pipeHeadYPosition = +CAMERA_ORTHO_SIZE - height + (PIPE_HEAD_HEIGHT * 0.5f);
        }
        pipeHead.position = new Vector3(xPosition, pipeHeadYPosition);

        // Create the pipe body
        Transform pipeBody = Instantiate(GameAssets.GetInstance().pfPipeBody);
        float pipeBodyYPosition;
        if (createBottom)
        {
            pipeBodyYPosition = -CAMERA_ORTHO_SIZE;
        }
        else
        {
            pipeBodyYPosition = +CAMERA_ORTHO_SIZE;
            pipeBody.localScale = new Vector3(1f, -1f, 1f); // Flip the pipe body vertically for top pipes
        }
        pipeBody.position = new Vector3(xPosition, pipeBodyYPosition);

        // Adjust the size of the pipe body based on the height
        SpriteRenderer pipeBodySpriteRenderer = pipeBody.GetComponent<SpriteRenderer>();
        pipeBodySpriteRenderer.size = new Vector2(PIPE_WIDTH, height);

        // Adjust the BoxCollider2D size of the pipe body to match the new size
        BoxCollider2D pipeBodyBoxCollider = pipeBody.GetComponent<BoxCollider2D>();
        pipeBodyBoxCollider.size = new Vector2(PIPE_WIDTH, height);
        // Adjust the offset to position the collider correctly
        pipeBodyBoxCollider.offset = new Vector2(0f, height * 0.5f);

        // create a new Pipe object to manage the movement of the pipe head and body together
        Pipe pipe = new Pipe(pipeHead, pipeBody, createBottom);
        pipeList.Add(pipe);
    }

    public int GetPipesSpawned()
    {
        return pipesSpawned;
    }

    public int GetPipesPassedCount()
    {
        return pipesPassedCount;
    }

    private class Pipe
    {
        private Transform pipeHeadTransform;
        private Transform pipeBodyTransform;
        private bool isBottom;

        public Pipe(Transform pipeHead, Transform pipeBody, bool isBottom)
        {
            this.pipeHeadTransform = pipeHead;
            this.pipeBodyTransform = pipeBody;
            this.isBottom = isBottom;
        }

        public void Move()
        {
            pipeHeadTransform.position += new Vector3(-1, 0, 0) * Time.deltaTime * PIPE_MOVE_SPEED;
            pipeBodyTransform.position += new Vector3(-1, 0, 0) * Time.deltaTime * PIPE_MOVE_SPEED;
        }

        public float GetXPosition()
        {
            return pipeHeadTransform.position.x;
        }

        public bool IsBottom()
        {
            return isBottom;
        }

        public void DestroySelf()
        {
            Destroy(pipeHeadTransform.gameObject);
            Destroy(pipeBodyTransform.gameObject);
        }
    }

    private class Floor
    {
        private Transform floorTransform;

        public Floor(Transform floor)
        {
            this.floorTransform = floor;
        }

        public void Move()
        {
            floorTransform.position += new Vector3(-1, 0, 0) * Time.deltaTime * FLOOR_MOVE_SPEED;
        }

        public float GetXPosition()
        {
            return floorTransform.position.x;
        }

        public void DestroySelf()
        {
            Destroy(floorTransform.gameObject);
        }
    }

    private class Cloud
    {
        private Transform cloudsTransform;
        public Cloud(Transform clouds)
        {
            this.cloudsTransform = clouds;
        }
        public void Move()
        {
            cloudsTransform.position += new Vector3(-1, 0, 0) * Time.deltaTime * CLOUDS_MOVE_SPEED;
        }
        public float GetXPosition()
        {
            return cloudsTransform.position.x;
        }
        public void DestroySelf()
        {
            Destroy(cloudsTransform.gameObject);
        }
    }
}
