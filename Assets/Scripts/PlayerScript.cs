using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
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
    public GameObject xoRedPrefab;
    public GameObject greenPrefab; 
    public GameObject yellowPrefab;
    public GameObject movingYellowPrefab;
    public GameObject movingBluePrefab;
    public GameObject ballPrefab;
    public GameObject projectilePrefab;
    static Collider2D[] colliders = new Collider2D[50]; 
    static ContactFilter2D contactFilter = new ContactFilter2D();
    public GameDataScript gameData;
    static bool gameStarted = false;
    public AudioClip pointSound;
    public bool sticky;
    BallScript stuckBall;
    Vector3 initialScale;
    SpriteRenderer spriteRenderer;
    Color normalColor = Color.white;
    static readonly Color StickyColor = new Color(0.6f, 1f, 0.6f, 1f);
    static readonly Color GunColor = new Color(0.75f, 0.6f, 1f, 1f);
    static readonly Color CannonColor = new Color(0.35f, 0.12f, 0.6f, 1f);
    const float maxSizeFactor = 1.5f * 1.5f;
    enum WeaponMode { None, Gun, Cannon }
    WeaponMode weaponMode;
    GameObject gunVisual;
    SpriteRenderer gunVisualRenderer;
    static Sprite gunSprite;
    public float fireCooldown = 0.22f;
    float nextFireTime;
    bool bombActive;
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

    public void EquipGun(bool cannon)
    {
        weaponMode = cannon ? WeaponMode.Cannon : WeaponMode.Gun;

        if (gunVisual == null)
            CreateGunVisual();

        if (gunVisual == null)
            return;

        gunVisual.SetActive(true);
        gunVisualRenderer.color = cannon ? CannonColor : GunColor;
        gunVisual.transform.localScale = cannon
            ? new Vector3(0.16f, 0.4f, 1f)
            : new Vector3(0.12f, 0.3f, 1f);
    }

    public void DisableGuns()
    {
        weaponMode = WeaponMode.None;

        if (gunVisual != null)
            gunVisual.SetActive(false);
    }

    public void ResetPaddleModes()
    {
        SetSticky(false);
        DisableGuns();
    }

    public void DetonateBomb()
    {
        if (bombActive)
            return;

        bombActive = true;
        StopAllCoroutines();

        foreach (var ball in GameObject.FindGameObjectsWithTag("Ball"))
            Destroy(ball);

        if (spriteRenderer != null)
            spriteRenderer.enabled = false;

        var playerCollider = GetComponent<Collider2D>();
        if (playerCollider != null)
            playerCollider.enabled = false;

        if (gunVisual != null)
            gunVisual.SetActive(false);

        HitEffects.Bomb(transform.position);
        StartCoroutine(ReloadLevelAfterBomb());
    }

    IEnumerator ReloadLevelAfterBomb()
    {
        yield return new WaitForSeconds(0.45f);
        GameSession.AutoStart = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void CreateGunVisual()
    {
        if (gunVisual != null)
            return;

        gunVisual = new GameObject("Gun Visual");
        gunVisual.transform.SetParent(transform, false);
        gunVisual.transform.localPosition = new Vector3(0f, 0.55f, 0f);
        gunVisual.transform.localScale = new Vector3(0.12f, 0.3f, 1f);

        gunVisualRenderer = gunVisual.AddComponent<SpriteRenderer>();
        gunVisualRenderer.sprite = GetGunSprite();
        gunVisualRenderer.color = GunColor;
        gunVisualRenderer.sortingOrder = 2;

        gunVisual.SetActive(false);
    }

    static Sprite GetGunSprite()
    {
        if (gunSprite != null)
            return gunSprite;

        var texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();

        gunSprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
        return gunSprite;
    }

    void FireProjectile()
    {
        if (projectilePrefab == null)
            return;

        bool cannon = weaponMode == WeaponMode.Cannon;
        Vector3 spawnPosition = gunVisual != null && gunVisual.activeSelf
            ? gunVisual.transform.position + Vector3.up * 0.35f
            : transform.position + Vector3.up * 1.4f;

        var projectileObject = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
        var projectile = projectileObject.GetComponent<ProjectileScript>();
        if (projectile != null)
            projectile.Initialize(cannon ? 4 : 1, cannon ? CannonColor : GunColor);

        HitEffects.Play(spawnPosition, cannon ? CannonColor : GunColor, 5, 2f);
        nextFireTime = Time.time + fireCooldown;
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
            case BonusType.Gun:
                bonusBase = bonus.AddComponent<GunBonus>();
                break;
            case BonusType.Cannon:
                bonusBase = bonus.AddComponent<CannonBonus>();
                break;
            case BonusType.Bomb:
                bonusBase = bonus.AddComponent<BombBonus>();
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
                int finalScore = gameData.points;
                gameData.Reset(); 
                if (GameMenuScript.Instance != null)
                    GameMenuScript.Instance.ShowGameOver(finalScore);
                else
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
            if (GameAudioScript.Instance != null)
                GameAudioScript.Instance.PlayEffect(pointSound); 
        } 
    }
    public void BlockDestroyed(int points) 
    {
        gameData.points += points;
        if (gameData.sound && GameAudioScript.Instance != null)
            GameAudioScript.Instance.PlayEffect(pointSound);
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

    void OnApplicationQuit()
    {
        gameData.Save();
    }


    bool TryLoadDesignedLevel()
    {
        var levelAsset = Resources.Load<TextAsset>("Levels/level_" + level.ToString("d2"));
        if (levelAsset == null)
            return false;

        var document = new XmlDocument();
        try
        {
            document.LoadXml(levelAsset.text);
        }
        catch
        {
            return false;
        }

        var levelElement = document.DocumentElement;
        if (levelElement == null)
            return false;

        var blockNodes = levelElement.SelectNodes("Block");
        if (blockNodes == null || blockNodes.Count == 0)
            return false;

        var levelRoot = new GameObject("Designed Level " + level).transform;
        bool createdAny = false;

        foreach (XmlNode blockNode in blockNodes)
        {
            if (blockNode.Attributes == null)
                continue;

            var typeAttribute = blockNode.Attributes["type"];
            var xAttribute = blockNode.Attributes["x"];
            var yAttribute = blockNode.Attributes["y"];
            if (typeAttribute == null || xAttribute == null || yAttribute == null)
                continue;

            var prefab = GetBlockPrefab(typeAttribute.Value);
            if (prefab == null)
                continue;

            float x = ParseFloat(xAttribute.Value);
            float y = ParseFloat(yAttribute.Value);
            float z = 0f;

            var zAttribute = blockNode.Attributes["z"];
            if (zAttribute != null)
                z = ParseFloat(zAttribute.Value);

            Instantiate(prefab, new Vector3(x, y, z), Quaternion.identity, levelRoot);
            createdAny = true;
        }

        if (!createdAny)
        {
            Destroy(levelRoot.gameObject);
            return false;
        }

        return true;
    }

    GameObject GetBlockPrefab(string type)
    {
        switch (type)
        {
            case "Blue":
                return bluePrefab;
            case "Red":
                return redPrefab;
            case "Green":
                return greenPrefab;
            case "Yellow":
                return yellowPrefab;
            case "MovingYellow":
                return movingYellowPrefab;
            case "MovingBlue":
                return movingBluePrefab;
            case "RedXO":
                return xoRedPrefab;
        }

        return null;
    }

    float ParseFloat(string value)
    {
        float result;
        if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
            return result;

        return 0f;
    }

    void StartLevel() 
    {
        SetBackground();
        var yMax = Camera.main.orthographicSize * 0.8f;
        var xMax = Camera.main.orthographicSize * Camera.main.aspect * 0.85f; 

        if (!TryLoadDesignedLevel())
        {
            CreateBlocks(bluePrefab, xMax, yMax, level, 8);
            CreateBlocks(movingBluePrefab, xMax, yMax, 1 + level / 5, 3);
            CreateBlocks(redPrefab, xMax, yMax, 1 + level, 10);
            CreateBlocks(xoRedPrefab, xMax, yMax, Mathf.Min(2 + level / 3, 4), 4);
            CreateBlocks(greenPrefab, xMax, yMax, 1 + level, 12);
            CreateBlocks(yellowPrefab, xMax, yMax, 2 + level, 15); 
            CreateBlocks(movingYellowPrefab, xMax, yMax, 1 + level / 5, 5);
        }

        CreateBalls(); 
    }
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            normalColor = spriteRenderer.color;
        initialScale = transform.localScale;
        CreateGunVisual();

        if (!gameStarted) 
        { 
            gameStarted = true; 
            if (gameData.resetOnStart) 
                gameData.Load(); 
        }
        level = gameData.level;

        if (GameAudioScript.Instance != null)
            GameAudioScript.Instance.ApplySettings();
        if (GameMenuScript.Instance != null)
            GameMenuScript.Instance.RefreshSettings();

        if (GameSession.AutoStart)
        {
            Time.timeScale = 1f;
            Cursor.visible = false;
            StartLevel();
        }
        else
        {
            Time.timeScale = 0f;
            Cursor.visible = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale <= 0)
            return;

        if (bombActive)
            return;

        var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var pos = transform.position;
        pos.x = mousePos.x;
        transform.position = pos;

        if (weaponMode != WeaponMode.None && Input.GetButtonDown("Fire1") && Time.time >= nextFireTime)
            FireProjectile();

        if (Input.GetKeyDown(KeyCode.M) && GameAudioScript.Instance != null)
            GameAudioScript.Instance.ToggleMusic();

        if (Input.GetKeyDown(KeyCode.S) && GameAudioScript.Instance != null)
            GameAudioScript.Instance.ToggleSound();

        if (Input.GetKeyDown(KeyCode.N)) 
        { 
            gameData.Reset();
            GameSession.AutoStart = true;
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
        if (Time.timeScale <= 0)
            return;

        GUI.Label(new Rect(5, 4, Screen.width - 10, 100), string.Format(
            "<color=yellow><size=30>Level <b>{0}</b>  Balls <b>{1}</b>" + "  Score <b>{2}</b></size></color>",
            gameData.level, gameData.balls, gameData.points));
        GUIStyle style = new GUIStyle(); 
        style.alignment = TextAnchor.UpperRight; GUI.Label(new Rect(5, 14, Screen.width - 10, 100),
            string.Format(
                "<color=yellow><size=20><color=white>Space</color>-pause" +
                " <color=white>N</color>-new" + " <color=white>J</color>-jump" + 
                " <color=white>M</color>-music {0}" + " <color=white>S</color>-sound {1}" +
                " <color=white>Esc</color>-exit</size></color>",
                OnOff(!gameData.music), OnOff(!gameData.sound)), style);
    }

    


}
