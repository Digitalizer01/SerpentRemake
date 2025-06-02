using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameBehavior : MonoBehaviour
{
    [Header("UI & Grid")]
    public int time;
    public GameObject panel;
    public Image imagePrefab;
    public int gridSize = 10;

    [Header("Game Settings")]
    private const int dangerDuration = 7;
    public float gameSpeed = 0.1f;
    public float speedEffectTime = 10f;
    
    private Dictionary<string, Sprite> spriteLibrary = new();
    private Dictionary<Tuple<int, int>, Meal> mealPositions = new();
    private Dictionary<Tuple<int, int>, Vector2> gridToUIPosition = new();
    private Dictionary<Snake, Coroutine> speedResetCoroutines = new();

    private Snake playerSnake;
    private Snake enemySnake;

    private bool gameRunning = true;

    private float elapsedTime = 0f;

    public UIBehavior uIBehavior;

    #region Unity Methods

    void Start()
    {
        LoadSprites();
        GenerateGrid();
        SpawnPlayerSnake();
        SpawnEnemySnake();
        StartCoroutine(StartRoundRoutine());
    }

    void Update()
    {
        uIBehavior.RefreshUI(playerSnake, enemySnake);
    }

    private IEnumerator TimerRoutine()
    {
        while (gameRunning)
        {
            time = Mathf.FloorToInt(elapsedTime);
            yield return new WaitForSeconds(1f);
            elapsedTime += 1f;
        }
    }

    private IEnumerator StartRoundRoutine()
    {
        AudioManager.Instance.StopMusic();

        AudioClip startClip = Resources.Load<AudioClip>("Sounds/round_start_song");
        if (startClip != null)
        {
            AudioManager.Instance.PlayMusic(startClip);
            yield return new WaitForSeconds(startClip.length);
        }

        StartCoroutine(PlayerSnakeMovement());
        if (GameResultInfo.IsTwoPlayerMode)
            StartCoroutine(Player2SnakeMovement());
        else
            StartCoroutine(EnemySnakeMovement());

        AudioClip loopClip = Resources.Load<AudioClip>("Sounds/round_song");
        if (loopClip != null)
        {
            AudioManager.Instance.PlayMusic(loopClip);
        }

        StartCoroutine(TimerRoutine());
    }

    #endregion

    #region Movement

    private IEnumerator PlayerSnakeMovement()
    {
        float cellSize = panel.GetComponent<RectTransform>().rect.width / gridSize;

        while (gameRunning)
        {
            SnakeHeadDirection inputDirection = GetInputDirection(playerSnake.CurrentHeadDirection);
            Tuple<int, int> nextPosition = GetNextPosition(playerSnake, inputDirection);
            playerSnake.NextPlannedPosition = nextPosition;

            if (nextPosition.Equals(enemySnake.NextPlannedPosition))
            {
                yield return new WaitForSeconds(0.1f);
                continue;
            }

            UpdateSnakeDirection(playerSnake, inputDirection);

            if (!HandleCollision(playerSnake, nextPosition))
            {
                yield return new WaitForSeconds(0.1f);
                continue;
            }

            yield return AnimateSnakeMovement(playerSnake, nextPosition, cellSize);
            SpawnMealIfNoneExists();
        }
    }

    private IEnumerator Player2SnakeMovement()
    {
        float cellSize = panel.GetComponent<RectTransform>().rect.width / gridSize;

        while (gameRunning)
        {
            SnakeHeadDirection inputDirection = GetPlayer2InputDirection(enemySnake.CurrentHeadDirection);
            Tuple<int, int> nextPosition = GetNextPosition(enemySnake, inputDirection);
            enemySnake.NextPlannedPosition = nextPosition;

            if (nextPosition.Equals(playerSnake.NextPlannedPosition))
            {
                yield return new WaitForSeconds(0.1f);
                continue;
            }

            UpdateSnakeDirection(enemySnake, inputDirection);

            if (!HandleCollision(enemySnake, nextPosition))
            {
                yield return new WaitForSeconds(0.1f);
                continue;
            }

            yield return AnimateSnakeMovement(enemySnake, nextPosition, cellSize);
            SpawnMealIfNoneExists();
        }
    }


    private IEnumerator EnemySnakeMovement()
    {
        float cellSize = panel.GetComponent<RectTransform>().rect.width / gridSize;

        while (gameRunning)
        {
            var enemyPos = enemySnake.Tuples[enemySnake.ListOfParts[0]];
            var playerPos = playerSnake.Tuples[playerSnake.ListOfParts[0]];
            SnakeHeadDirection inputDirection = GetChasingDirection(enemySnake.CurrentHeadDirection, enemyPos, playerPos);

            Tuple<int, int> nextPosition = GetNextPosition(enemySnake, inputDirection);
            enemySnake.NextPlannedPosition = nextPosition;

            if (nextPosition.Equals(playerSnake.NextPlannedPosition))
            {
                yield return new WaitForSeconds(0.1f);
                continue;
            }

            UpdateSnakeDirection(enemySnake, inputDirection);

            if (!HandleCollision(enemySnake, nextPosition))
            {
                yield return new WaitForSeconds(0.1f);
                continue;
            }

            yield return AnimateSnakeMovement(enemySnake, nextPosition, cellSize);
            SpawnMealIfNoneExists();
        }
    }

    private SnakeHeadDirection GetChasingDirection(SnakeHeadDirection currentDirection, Tuple<int, int> enemyPos, Tuple<int, int> playerPos)
    {
        List<SnakeHeadDirection> possibleDirections = new();

        int dx = playerPos.Item1 - enemyPos.Item1;
        int dy = playerPos.Item2 - enemyPos.Item2;

        // Priorizar movimiento en el eje mayor
        if (Mathf.Abs(dx) > Mathf.Abs(dy))
        {
            if (dx > 0 && currentDirection != SnakeHeadDirection.Left)
                possibleDirections.Add(SnakeHeadDirection.Right);
            else if (dx < 0 && currentDirection != SnakeHeadDirection.Right)
                possibleDirections.Add(SnakeHeadDirection.Left);

            if (dy > 0 && currentDirection != SnakeHeadDirection.Bottom)
                possibleDirections.Add(SnakeHeadDirection.Top);
            else if (dy < 0 && currentDirection != SnakeHeadDirection.Top)
                possibleDirections.Add(SnakeHeadDirection.Bottom);
        }
        else
        {
            if (dy > 0 && currentDirection != SnakeHeadDirection.Bottom)
                possibleDirections.Add(SnakeHeadDirection.Top);
            else if (dy < 0 && currentDirection != SnakeHeadDirection.Top)
                possibleDirections.Add(SnakeHeadDirection.Bottom);

            if (dx > 0 && currentDirection != SnakeHeadDirection.Left)
                possibleDirections.Add(SnakeHeadDirection.Right);
            else if (dx < 0 && currentDirection != SnakeHeadDirection.Right)
                possibleDirections.Add(SnakeHeadDirection.Left);
        }

        // Si no se pudo mover en dirección ideal, tomar una aleatoria válida
        if (possibleDirections.Count == 0)
        {
            foreach (SnakeHeadDirection dir in Enum.GetValues(typeof(SnakeHeadDirection)))
            {
                if (dir == SnakeHeadDirection.Top && currentDirection == SnakeHeadDirection.Bottom) continue;
                if (dir == SnakeHeadDirection.Bottom && currentDirection == SnakeHeadDirection.Top) continue;
                if (dir == SnakeHeadDirection.Left && currentDirection == SnakeHeadDirection.Right) continue;
                if (dir == SnakeHeadDirection.Right && currentDirection == SnakeHeadDirection.Left) continue;
                possibleDirections.Add(dir);
            }
        }

        return possibleDirections[UnityEngine.Random.Range(0, possibleDirections.Count)];
    }

    private void UpdateSnakeDirection(Snake snake, SnakeHeadDirection newDirection)
    {
        float rotationZ = newDirection switch
        {
            SnakeHeadDirection.Top => 0f,
            SnakeHeadDirection.Right => -90f,
            SnakeHeadDirection.Bottom => 180f,
            SnakeHeadDirection.Left => 90f,
            _ => 0f
        };

        RectTransform headTransform = snake.ListOfParts[0].GetComponent<RectTransform>();
        headTransform.rotation = Quaternion.Euler(0, 0, rotationZ);
        snake.CurrentHeadDirection = newDirection;
    }

    private SnakeHeadDirection GetInputDirection(SnakeHeadDirection currentDirection)
    {
        float dpadX = Input.GetAxisRaw("DPad1Horizontal");
        float dpadY = Input.GetAxisRaw("DPad1Vertical");

        if ((Input.GetKey(KeyCode.UpArrow) || dpadY > 0) && currentDirection != SnakeHeadDirection.Bottom)
            return SnakeHeadDirection.Top;

        if ((Input.GetKey(KeyCode.DownArrow) || dpadY < 0) && currentDirection != SnakeHeadDirection.Top)
            return SnakeHeadDirection.Bottom;

        if ((Input.GetKey(KeyCode.RightArrow) || dpadX > 0) && currentDirection != SnakeHeadDirection.Left)
            return SnakeHeadDirection.Right;

        if ((Input.GetKey(KeyCode.LeftArrow) || dpadX < 0) && currentDirection != SnakeHeadDirection.Right)
            return SnakeHeadDirection.Left;

        return currentDirection;
    }
    
    private SnakeHeadDirection GetPlayer2InputDirection(SnakeHeadDirection currentDirection)
    {
        float dpadX = Input.GetAxisRaw("DPad2Horizontal");
        float dpadY = Input.GetAxisRaw("DPad2Vertical");

        if ((Input.GetKey(KeyCode.UpArrow) || dpadY > 0) && currentDirection != SnakeHeadDirection.Bottom)
            return SnakeHeadDirection.Top;

        if ((Input.GetKey(KeyCode.DownArrow) || dpadY < 0) && currentDirection != SnakeHeadDirection.Top)
            return SnakeHeadDirection.Bottom;

        if ((Input.GetKey(KeyCode.RightArrow) || dpadX > 0) && currentDirection != SnakeHeadDirection.Left)
            return SnakeHeadDirection.Right;

        if ((Input.GetKey(KeyCode.LeftArrow) || dpadX < 0) && currentDirection != SnakeHeadDirection.Right)
            return SnakeHeadDirection.Left;

        return currentDirection;
    }

    private SnakeHeadDirection GetRandomDirection(SnakeHeadDirection currentDirection)
    {
        int randomNumber = UnityEngine.Random.Range(1, 5);

        if (randomNumber == 1 && currentDirection != SnakeHeadDirection.Bottom) return SnakeHeadDirection.Top;
        if (randomNumber == 2 && currentDirection != SnakeHeadDirection.Top) return SnakeHeadDirection.Bottom;
        if (randomNumber == 3 && currentDirection != SnakeHeadDirection.Left) return SnakeHeadDirection.Right;
        if (randomNumber == 4 && currentDirection != SnakeHeadDirection.Right) return SnakeHeadDirection.Left;

        return currentDirection;
    }

    private Tuple<int, int> GetNextPosition(Snake snake, SnakeHeadDirection direction)
    {
        var (x, y) = snake.Tuples[snake.ListOfParts[0]];
        return direction switch
        {
            SnakeHeadDirection.Top => Tuple.Create(x, y + 1),
            SnakeHeadDirection.Bottom => Tuple.Create(x, y - 1),
            SnakeHeadDirection.Left => Tuple.Create(x - 1, y),
            SnakeHeadDirection.Right => Tuple.Create(x + 1, y),
            _ => Tuple.Create(x, y)
        };
    }

    private IEnumerator AnimateSnakeMovement(Snake snake, Tuple<int, int> targetPosition, float cellSize)
    {
        List<Image> previousParts = new(snake.ListOfParts);
        Dictionary<Tuple<int, int>, Image> newParts = new();
        Dictionary<Image, Tuple<int, int>> newTuples = new();
        Image newPart = null;

        bool consumedMeal = mealPositions.TryGetValue(targetPosition, out Meal meal);
        MealType mealType = consumedMeal ? meal.Type : MealType.Normal;

        for (int frame = 0; frame < 100; frame++)
        {
            yield return new WaitForSeconds(snake.GetSpeed());

            for (int i = 0; i < snake.ListOfParts.Count; i++)
            {
                Image part = snake.ListOfParts[i];
                Vector2 targetPos = (i == 0)
                    ? gridToUIPosition[targetPosition]
                    : gridToUIPosition[snake.Tuples[previousParts[i - 1]]];

                RectTransform rt = part.GetComponent<RectTransform>();
                rt.anchoredPosition = Vector2.MoveTowards(rt.anchoredPosition, targetPos, 0.5f);

                if (frame == 30 && i == 0 && consumedMeal)
                {
                    Debug.Log($"Consumed meal: {mealType}");

                    AudioClip obtainClip = Resources.Load<AudioClip>("Sounds/item_obtained_sound");
                    if (obtainClip != null)
                    {
                        AudioManager.Instance.PlaySFX(obtainClip);
                    }
                    
                    meal.Image.gameObject.SetActive(false);
                    mealPositions.Clear();
                }

                if (frame == 1 && consumedMeal && mealType == MealType.Normal && i == snake.ListOfParts.Count - 1)
                {
                    newPart = Instantiate(imagePrefab, panel.transform);
                    newPart.GetComponent<RectTransform>().sizeDelta = new Vector2(cellSize * 0.95f, cellSize * 0.95f);
                    if (snake.IsPlayer)
                    {
                        newPart.sprite = spriteLibrary["Body"];
                    }
                    else if (!snake.IsPlayer)
                    {
                        newPart.sprite = spriteLibrary["BodyEnemy"];
                    }
                    var pos = snake.Tuples[previousParts.Last()];
                    newPart.GetComponent<RectTransform>().anchoredPosition = gridToUIPosition[pos];
                }

                if (frame == 99)
                {
                    var newPos = (i == 0) ? targetPosition : snake.Tuples[previousParts[i - 1]];
                    newParts[newPos] = part;
                    newTuples[part] = newPos;
                }
            }
        }

        if (consumedMeal)
            ApplyMealEffect(snake, mealType, newParts, newTuples, newPart, previousParts.Last());

        snake.PartsOfSnake = newParts;
        snake.Tuples = newTuples;
    }

    private void ApplyMealEffect(Snake snake, MealType type, Dictionary<Tuple<int, int>, Image> parts, Dictionary<Image, Tuple<int, int>> tuples, Image newPart, Image tail)
    {
        switch (type)
        {
            case MealType.Normal:
                Debug.Log("Normal Effect");
                AddSnakePart(snake, newPart, tail, parts, tuples);
                break;

            case MealType.Blue:
                Debug.Log("Blue Effect - Shorten snake");
                if (snake.ListOfParts.Count > 2)
                    RemoveTailPart(snake, parts, tuples);
                break;

            case MealType.Yellow:
                Debug.Log("Yellow Effect - Slow down");
                if (snake.currentSpeed != Snake.SpeedState.Slow)
                {
                    snake.currentSpeed = Snake.SpeedState.Slow;
                    ResetSpeedAfterDelay(snake);
                }
                break;

            case MealType.Green:
                Debug.Log("Green Effect - Speed up");
                if (snake.currentSpeed != Snake.SpeedState.Fast)
                {
                    snake.currentSpeed = Snake.SpeedState.Fast;
                    ResetSpeedAfterDelay(snake);
                }
                break;
        }
    }

    private void ResetSpeedAfterDelay(Snake snake)
    {
        if (speedResetCoroutines.TryGetValue(snake, out var runningCoroutine))
        {
            StopCoroutine(runningCoroutine);
        }

        Coroutine coroutine = StartCoroutine(SpeedResetCountdown(snake));
        speedResetCoroutines[snake] = coroutine;
    }

    private IEnumerator SpeedResetCountdown(Snake snake)
    {
        yield return new WaitForSeconds(speedEffectTime);
        snake.currentSpeed = Snake.SpeedState.Normal;
        Debug.Log("Speed reset to Normal.");

        speedResetCoroutines.Remove(snake);
    }

    private void AddSnakePart(Snake snake, Image part, Image after, Dictionary<Tuple<int, int>, Image> parts, Dictionary<Image, Tuple<int, int>> tuples)
    {
        var pos = snake.Tuples[after];
        parts[pos] = part;
        tuples[part] = pos;
        snake.ListOfParts.Add(part);
    }

    private void RemoveTailPart(
        Snake snake,
        Dictionary<Tuple<int, int>, Image> parts,
        Dictionary<Image, Tuple<int, int>> tuples)
    {
        Image tail = snake.ListOfParts.Last();
        Tuple<int, int> pos = snake.Tuples[tail];

        snake.ListOfParts.RemoveAt(snake.ListOfParts.Count - 1);
        snake.Tuples.Remove(tail);
        snake.PartsOfSnake.Remove(pos);

        tuples.Remove(tail);
        parts.Remove(pos);

        if (tail != null)
        {
            GameObject.Destroy(tail.gameObject);
        }
    }

    #endregion

    #region Collision & Danger Logic

    private bool HandleCollision(Snake snake, Tuple<int, int> nextPos)
    {
        bool isCollision = 
            nextPos.Item1 <= 0 || nextPos.Item1 >= gridSize - 1 ||
            nextPos.Item2 <= 0 || nextPos.Item2 >= gridSize - 1 ||
            playerSnake.PartsOfSnake.ContainsKey(nextPos) || 
            enemySnake.PartsOfSnake.ContainsKey(nextPos);

        if (isCollision)
        {
            if (!snake.isInDanger)
            {
                snake.isInDanger = true;
                snake.dangerCoroutine = StartCoroutine(DangerCountdown(snake));

                AudioClip dangerClip = Resources.Load<AudioClip>("Sounds/danger_sound");
                if (dangerClip != null)
                    AudioManager.Instance.PlayLoopedSFX(dangerClip);
            }

            return false;
        }
        else
        {
            if (snake.isInDanger)
            {
                if (snake.dangerCoroutine != null)
                    StopCoroutine(snake.dangerCoroutine);

                snake.isInDanger = false;
                SetSnakeVisibility(snake, true);

                AudioManager.Instance.StopLoopedSFX();
            }

            return true;
        }
    }

    private IEnumerator DangerCountdown(Snake snake)
    {
        int timeLeft = dangerDuration;

        while (timeLeft > 0)
        {
            ToggleSnakeVisibility(snake);
            Debug.Log($"DANGER! {timeLeft} seconds remaining...");
            yield return new WaitForSeconds(1f);
            timeLeft--;
        }

        SetSnakeVisibility(snake, true);
        if (object.ReferenceEquals(snake, playerSnake))
            EndGame(false);
        else
        {
            EndGame(true);
        }
    }

    private void ToggleSnakeVisibility(Snake snake)
    {
        if (!snake.isInDanger) return;

        foreach (var part in snake.ListOfParts)
        {
            var color = part.color;
            color.a = (color.a == 1f) ? 0.3f : 1f;
            part.color = color;
        }
    }

    private void SetSnakeVisibility(Snake snake, bool visible)
    {
        foreach (var part in snake.ListOfParts)
        {
            var color = part.color;
            color.a = visible ? 1f : 0.3f;
            part.color = color;
        }
    }

    private void EndGame(bool win)
    {
        StartCoroutine(EndGameRoutine(win));
    }

    private IEnumerator EndGameRoutine(bool win)
    {
        AudioManager.Instance.StopLoopedSFX();
        AudioManager.Instance.StopMusic();

        AudioClip deathClip = Resources.Load<AudioClip>("Sounds/death_sound");
        if (deathClip != null)
        {
            AudioSource tempSource = gameObject.AddComponent<AudioSource>();
            tempSource.clip = deathClip;
            tempSource.volume = PlayerPrefs.GetFloat("MasterVolume", 1f);
            tempSource.Play();

            yield return new WaitForSeconds(deathClip.length);
        }

        GameResultInfo.PlayerWon = win;
        GameResultInfo.LogGameResult(win);
        SceneManager.LoadScene("GameOverMenu");
    }


    #endregion

    #region Initialization

    private void LoadSprites()
    {
        var sprites = Resources.LoadAll<Sprite>("Images");
        foreach (var sprite in sprites)
            spriteLibrary[sprite.name] = sprite;
    }

    private void GenerateGrid()
    {
        float panelWidth = panel.GetComponent<RectTransform>().rect.width;
        float panelHeight = panel.GetComponent<RectTransform>().rect.height;
        float cellSize = Mathf.Min(panelWidth, panelHeight) / gridSize;

        float gridWidth = cellSize * gridSize;
        float gridHeight = cellSize * gridSize;

        Vector2 origin = new Vector2(-gridWidth / 2 + cellSize / 2, -gridHeight / 2 + cellSize / 2);

        for (int y = 0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                var image = Instantiate(imagePrefab, panel.transform);
                image.GetComponent<RectTransform>().sizeDelta = new Vector2(cellSize * 0.95f, cellSize * 0.95f);

                image.color = (x == 0 || x == gridSize - 1 || y == 0 || y == gridSize - 1) ? Color.black : Color.white;

                Vector2 anchoredPos = origin + new Vector2(x * cellSize, y * cellSize);
                image.GetComponent<RectTransform>().anchoredPosition = anchoredPos;

                gridToUIPosition[new Tuple<int, int>(x, y)] = anchoredPos;
            }
        }
    }

    private void SpawnPlayerSnake()
    {
        playerSnake = new Snake();
        playerSnake.IsPlayer = true;
        float cellSize = panel.GetComponent<RectTransform>().rect.width / gridSize;

        Tuple<int, int> headPos = new Tuple<int, int>(gridSize/2, 3);
        AddSnakePart(playerSnake, headPos, "Head");

        playerSnake.CurrentHeadDirection = SnakeHeadDirection.Top;

        for (int i = 0; i < 2; i++)
        {
            headPos = GetBodyPositionBehind(headPos, playerSnake.CurrentHeadDirection);
            AddSnakePart(playerSnake, headPos, "Body");
        }
    }

    private void SpawnEnemySnake()
    {
        enemySnake = new Snake();
        playerSnake.IsPlayer = false;
        float cellSize = panel.GetComponent<RectTransform>().rect.width / gridSize;

        Tuple<int, int> headPos = new Tuple<int, int>(gridSize/2, gridSize-4);
        AddSnakePart(enemySnake, headPos, "HeadEnemy");

        enemySnake.CurrentHeadDirection = SnakeHeadDirection.Bottom;

        for (int i = 0; i < 2; i++)
        {
            headPos = GetBodyPositionBehind(headPos, enemySnake.CurrentHeadDirection);
            AddSnakePart(enemySnake, headPos, "BodyEnemy");
        }
    }

    private void AddSnakePart(Snake snake, Tuple<int, int> pos, string spriteName)
    {
        var image = Instantiate(imagePrefab, panel.transform);
        image.GetComponent<RectTransform>().sizeDelta = new Vector2(panel.GetComponent<RectTransform>().sizeDelta.x / gridSize * 0.95f, panel.GetComponent<RectTransform>().sizeDelta.x / gridSize * 0.95f);
        image.sprite = spriteLibrary[spriteName];
        image.GetComponent<RectTransform>().anchoredPosition = gridToUIPosition[pos];

        snake.PartsOfSnake[pos] = image;
        snake.Tuples[image] = pos;
        snake.ListOfParts.Add(image);
    }

    private Tuple<int, int> GetRandomPosition()
    {
        int x = UnityEngine.Random.Range(3, gridSize - 3);
        int y = UnityEngine.Random.Range(3, gridSize - 3);
        return Tuple.Create(x, y);
    }

    private SnakeHeadDirection GetRandomDirection()
    {
        return (SnakeHeadDirection)UnityEngine.Random.Range(0, 4);
    }

    private Tuple<int, int> GetBodyPositionBehind(Tuple<int, int> headPos, SnakeHeadDirection dir)
    {
        return dir switch
        {
            SnakeHeadDirection.Top => Tuple.Create(headPos.Item1, headPos.Item2 - 1),
            SnakeHeadDirection.Bottom => Tuple.Create(headPos.Item1, headPos.Item2 + 1),
            SnakeHeadDirection.Left => Tuple.Create(headPos.Item1 + 1, headPos.Item2),
            SnakeHeadDirection.Right => Tuple.Create(headPos.Item1 - 1, headPos.Item2),
            _ => headPos
        };
    }

    #endregion

    #region Meals

    private void SpawnMealIfNoneExists()
    {
        if (mealPositions.Count > 0) return;

        Tuple<int, int> mealPos;
        do
        {
            mealPos = Tuple.Create(UnityEngine.Random.Range(1, gridSize - 1), UnityEngine.Random.Range(1, gridSize - 1));
        } while (playerSnake.PartsOfSnake.ContainsKey(mealPos));

        MealType type = (MealType)UnityEngine.Random.Range(0, 4);
        string spriteName = type switch
        {
            MealType.Normal => "apple",
            MealType.Blue => "blue_meal",
            MealType.Yellow => "yellow_meal",
            MealType.Green => "green_meal",
            _ => "apple"
        };

        Image mealImage = Instantiate(imagePrefab, panel.transform);
        float cellSize = panel.GetComponent<RectTransform>().sizeDelta.x / gridSize;
        mealImage.GetComponent<RectTransform>().sizeDelta = new Vector2(cellSize * 0.95f, cellSize * 0.95f);
        mealImage.sprite = spriteLibrary[spriteName];
        mealImage.GetComponent<RectTransform>().anchoredPosition = gridToUIPosition[mealPos];

        mealPositions.Add(mealPos, new Meal(type, mealImage));
        AudioClip appearClip = Resources.Load<AudioClip>("Sounds/item_appear_sound");
        if (appearClip != null)
        {
            AudioManager.Instance.PlaySFX(appearClip);
        }
    }

    #endregion
}
