using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerScript : MonoBehaviour
{
    const int maxLevel = 30; 
    [Range(1, maxLevel)]
    public int level = 1;
    public float ballVelocityMult = 0.02f;
    public GameObject bluePrefab; 
    public GameObject redPrefab; 
    public GameObject greenPrefab; 
    public GameObject yellowPrefab;
    public GameObject movingYellowPrefab;
    public GameObject ballPrefab;
    static Collider2D[] colliders = new Collider2D[50]; 
    static ContactFilter2D contactFilter = new ContactFilter2D();
    public GameDataScript gameData;
    static bool gameStarted = false;
    AudioSource audioSrc; public AudioClip pointSound;
    public bool sticky;
    BallScript stuckBall;
    Vector3 initialScale;
    SpriteRenderer spriteRenderer;
    Color normalColor = Color.white;
    static readonly Color StickyColor = new Color(0.6f, 1f, 0.6f, 1f);
    const float maxSizeFactor = 1.5f * 1.5f;
    int requiredPointsToBall { get { return 400 + (level - 1) * 20; } }

    // Start is called before the first frame update
    void CreateBlocks(GameObject prefab, float xMax, float yMax, int count, int maxCount)
    {
        if (count > maxCount)
            count = maxCount;
        for (int i = 0; i < count; i++) for (int k = 0; k < 20; k++)
            {
                var obj = Instantiate(prefab, new Vector3((Random.value * 2 - 1) * xMax, Random.value * yMax, 0), Quaternion.identity);
                if (obj.GetComponent<Collider2D>().OverlapCollider(contactFilter.NoFilter(), colliders) == 0)
                    break;
                Destroy(obj);
            }
    }

    void CreateBalls() 
    {
        int count = 2;
        if (gameData.balls == 1)
            count = 1;
        CreateBalls(count, false);
    }

    void CreateBalls(int count, bool launchImmediately)
    {
        for (int i = 0; i < count; i++)
        { 
            var obj = Instantiate(ballPrefab); 
            var ball = obj.GetComponent<BallScript>();
            ball.ballInitialForce += new Vector2(10 * i, 0);
            ball.ballInitialForce *= 1 + level * ballVelocityMult;
            if (launchImmediately)
                ball.Launch();
        } 
    }

    public void AddBalls(int count, bool spawnImmediately)
    {
        gameData.balls += count;
        if (spawnImmediately)
            CreateBalls(count, true);
    }

    public void ChangeBallsSpeed(float multiplier)
    {
        foreach (var ballObject in GameObject.FindGameObjectsWithTag("Ball"))
        {
            var ball = ballObject.GetComponent<BallScript>();
            if (ball != null)
                ball.ChangeSpeed(multiplier);
        }
    }

    public void SetBallsDamage(int damage, Color color)
    {
        foreach (var ballObject in GameObject.FindGameObjectsWithTag("Ball"))
        {
            var ball = ballObject.GetComponent<BallScript>();
            if (ball != null)
                ball.SetDamage(damage, color);
        }
    }

    public void Expand()
    {
        var scale = transform.localScale;
        scale.x = Mathf.Min(scale.x * 1.5f, initialScale.x * maxSizeFactor);
        scale.y = Mathf.Min(scale.y * 1.5f, initialScale.y * maxSizeFactor);
        transform.localScale = scale;
    }

    public void Shrink()
    {
        var scale = transform.localScale;
        scale.x = Mathf.Max(scale.x / 1.5f, initialScale.x / maxSizeFactor);
        scale.y = Mathf.Max(scale.y / 1.5f, initialScale.y / maxSizeFactor);
        transform.localScale = scale;
    }

    public void SetSticky(bool value)
    {
        sticky = value;

        if (spriteRenderer != null)
            spriteRenderer.color = sticky ? StickyColor : normalColor;

        if (!sticky && stuckBall != null)
        {
            stuckBall.ReleaseFromStick();
            stuckBall = null;
        }
    }

    public bool TryStickBall(BallScript ball)
    {
        if (!sticky || stuckBall != null)
            return false;

        stuckBall = ball;
        ball.StickToPlayer();
        return true;
    }

    public void BallLaunched(BallScript ball)
    {
        if (stuckBall == ball)
            stuckBall = null;
    }

    public void CreateBonus(Vector3 position, GameObject bonusPrefab)
    {
        if (bonusPrefab == null)
            return;

        var bonus = Instantiate(bonusPrefab, position, Quaternion.identity);
        BonusBase bonusBase = null;
        switch (gameData.GetRandomBonus())
        {
            case BonusType.Slow:
                bonusBase = bonus.AddComponent<SlowBonus>();
                break;
            case BonusType.Fast:
                bonusBase = bonus.AddComponent<FastBonus>();
                break;
            case BonusType.Ball:
                bonusBase = bonus.AddComponent<BallBonus>();
                break;
            case BonusType.Plus2:
                bonusBase = bonus.AddComponent<Plus2Bonus>();
                break;
            case BonusType.Plus10:
                bonusBase = bonus.AddComponent<Plus10Bonus>();
                break;
            case BonusType.Fire:
                bonusBase = bonus.AddComponent<FireBonus>();
                break;
            case BonusType.Steel:
                bonusBase = bonus.AddComponent<SteelBonus>();
                break;
            case BonusType.Norm:
                bonusBase = bonus.AddComponent<NormBonus>();
                break;
            case BonusType.Expand:
                bonusBase = bonus.AddComponent<ExpandBonus>();
                break;
            case BonusType.Shrink:
                bonusBase = bonus.AddComponent<ShrinkBonus>();
                break;
            case BonusType.Sticky:
                bonusBase = bonus.AddComponent<StickyBonus>();
                break;
            case BonusType.Simple:
                bonusBase = bonus.AddComponent<SimpleBonus>();
                break;
        }

        if (bonusBase != null)
            bonusBase.RefreshAppearance();
    }

    void SetBackground() 
    { 
        var bg = GameObject.Find("Background").GetComponent<SpriteRenderer>();
        bg.sprite = Resources.Load(level.ToString("d2"), typeof(Sprite)) as Sprite; 
    }

    IEnumerator BallDestroyedCoroutine() 
    { 
        yield return new WaitForSeconds(0.1f); 
        if (GameObject.FindGameObjectsWithTag("Ball").Length == 0)
            if (gameData.balls > 0)
                CreateBalls();
            else 
            { 
                gameData.Reset(); 
                SceneManager.LoadScene("UnityArcanoid2D"); 
            }
    }

    public void BallDestroyed()     
    {
        gameData.balls--;
        StartCoroutine(BallDestroyedCoroutine());
    }

    IEnumerator BlockDestroyedCoroutine() 
    { 
        yield return new WaitForSeconds(0.1f); 
        if (!LevelBlocksRemain())
        { 
            if (level < maxLevel) 
                gameData.level++; 
            SceneManager.LoadScene("UnityArcanoid2D"); 
        }
    }

    bool LevelBlocksRemain()
    {
        foreach (var block in GameObject.FindGameObjectsWithTag("Block"))
        {
            var blockScript = block.GetComponent<BlockScript>();
            if (blockScript == null || blockScript.countsForLevelCompletion)
                return true;
        }
        return false;
    }

    IEnumerator BlockDestroyedCoroutine2() 
    {
        for (int i = 0; i < 10; i++) 
        { 
            yield return new WaitForSeconds(0.2f); 
            audioSrc.PlayOneShot(pointSound, 5); 
        }
    }
    public void BlockDestroyed(int points) 
    {
        gameData.points += points;
        if (gameData.sound)
            audioSrc.PlayOneShot(pointSound, 5);
        gameData.pointsToBall += points; 
        if (gameData.pointsToBall >= requiredPointsToBall) 
        {
            gameData.balls++;
            gameData.pointsToBall -= requiredPointsToBall;
            if (gameData.sound) 
                StartCoroutine(BlockDestroyedCoroutine2());
        }
        StartCoroutine(BlockDestroyedCoroutine()); 
    }
    
    void SetMusic() 
    { 
        if (gameData.music) 
            audioSrc.Play();
        else audioSrc.Stop();
    }

    void OnApplicationQuit()
    {
        gameData.Save();
    }


    void StartLevel() 
    {
        SetBackground();
        var yMax = Camera.main.orthographicSize * 0.8f;
        var xMax = Camera.main.orthographicSize * Camera.main.aspect * 0.85f; 
        CreateBlocks(bluePrefab, xMax, yMax, level, 8);
        CreateBlocks(redPrefab, xMax, yMax, 1 + level, 10);
        CreateBlocks(greenPrefab, xMax, yMax, 1 + level, 12);
        CreateBlocks(yellowPrefab, xMax, yMax, 2 + level, 15); 
        CreateBlocks(movingYellowPrefab, xMax, yMax, 1 + level / 5, 5);
        CreateBalls(); 
    }
    void Start()
    {
        audioSrc = Camera.main.GetComponent<AudioSource>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            normalColor = spriteRenderer.color;
        initialScale = transform.localScale;
        Cursor.visible = false;
        if (!gameStarted) 
        { 
            gameStarted = true; 
            if (gameData.resetOnStart) 
                gameData.Load(); 
        }
        level = gameData.level;
        SetMusic();
        StartLevel();

    }

    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale > 0)
        {
            var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var pos = transform.position;
            pos.x = mousePos.x;
            transform.position = pos;
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            gameData.music = !gameData.music; 
            SetMusic(); 
        }
        if (Input.GetKeyDown(KeyCode.S))
            gameData.sound = !gameData.sound;
        if (Input.GetButtonDown("Pause"))
            if (Time.timeScale > 0)
                Time.timeScale = 0; 
            else 
                Time.timeScale = 1;

        if (Input.GetKeyDown(KeyCode.N)) 
        { 
            gameData.Reset(); 
            SceneManager.LoadScene("UnityArcanoid2D"); 
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
            #if UNITY_EDITOR             
            UnityEditor.EditorApplication.isPlaying = false; 
            #endif
        }  
    }

    string OnOff(bool boolVal) { return boolVal ? "on" : "off"; }

    void OnGUI() 
    { 
        GUI.Label(new Rect(5, 4, Screen.width - 10, 100), string.Format(
            "<color=yellow><size=30>Level <b>{0}</b>  Balls <b>{1}</b>" + "  Score <b>{2}</b></size></color>",
            gameData.level, gameData.balls, gameData.points));
        GUIStyle style = new GUIStyle(); 
        style.alignment = TextAnchor.UpperRight; GUI.Label(new Rect(5, 14, Screen.width - 10, 100),
            string.Format(
                "<color=yellow><size=20><color=white>Space</color>-pause {0}" +
                " <color=white>N</color>-new" + " <color=white>J</color>-jump" + 
                " <color=white>M</color>-music {1}" + " <color=white>S</color>-sound {2}" +
                " <color=white>Esc</color>-exit</size></color>", OnOff(Time.timeScale > 0), 
                OnOff(!gameData.music), OnOff(!gameData.sound)), style);
    }

    


}
