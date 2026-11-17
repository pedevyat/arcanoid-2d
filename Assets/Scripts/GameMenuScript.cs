using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameMenuScript : MonoBehaviour
{
    public static GameMenuScript Instance;

    Canvas canvas;
    GameObject startPanel;
    GameObject pausePanel;
    GameObject startExitButton;
    GameObject gameOverLabel;
    Text startTitleText;

    Toggle startMusicToggle;
    Toggle startSoundToggle;
    Toggle pauseMusicToggle;
    Toggle pauseSoundToggle;

    Slider startMusicSlider;
    Slider startSoundSlider;
    Slider pauseMusicSlider;
    Slider pauseSoundSlider;

    GameDataScript gameData;
    GameAudioScript audioManager;

    bool syncing;
    bool gameOver;
    static Font cachedFont;

    void Awake()
    {
        Instance = this;
        FindReferences();
        BuildUI();
    }

    void Start()
    {
        if (GameSession.AutoStart)
            HideMenus();
        else
            ShowStartScreen(false);
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    void FindReferences()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            var playerScript = player.GetComponent<PlayerScript>();
            if (playerScript != null)
                gameData = playerScript.gameData;
        }

        audioManager = GetComponent<GameAudioScript>();
        if (audioManager == null)
            audioManager = FindObjectOfType<GameAudioScript>();
    }

    void Update()
    {
        if (startPanel == null || pausePanel == null)
            return;

        if (startPanel.activeSelf || gameOver)
            return;

        if (Input.GetButtonDown("Pause"))
            TogglePause();
    }

    void TogglePause()
    {
        if (pausePanel.activeSelf)
            ResumeGame();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        SyncControls();
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
        Cursor.visible = false;
    }

    public void RefreshSettings()
    {
        SyncControls();
    }

    public void ShowStartScreen(bool fromGameOver)
    {
        gameOver = fromGameOver;
        startPanel.SetActive(true);
        pausePanel.SetActive(false);
        startExitButton.SetActive(fromGameOver);
        gameOverLabel.SetActive(fromGameOver);
        Time.timeScale = 0f;
        Cursor.visible = true;
        SyncControls();
    }

    public void ShowGameOver()
    {
        GameSession.AutoStart = false;
        GameSession.GameOver = true;
        ShowStartScreen(true);
    }

    void HideMenus()
    {
        startPanel.SetActive(false);
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
        Cursor.visible = false;
        gameOver = false;
    }

    void PlayGame()
    {
        GameSession.AutoStart = true;
        GameSession.GameOver = false;
        Time.timeScale = 1f;
        Cursor.visible = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void StartNewGame()
    {
        if (gameData != null)
            gameData.Reset();

        GameSession.AutoStart = true;
        GameSession.GameOver = false;
        Time.timeScale = 1f;
        Cursor.visible = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    void SyncControls()
    {
        if (gameData == null)
            return;

        syncing = true;

        startMusicToggle.isOn = gameData.music;
        pauseMusicToggle.isOn = gameData.music;
        startSoundToggle.isOn = gameData.sound;
        pauseSoundToggle.isOn = gameData.sound;

        startMusicSlider.value = gameData.musicVolume;
        pauseMusicSlider.value = gameData.musicVolume;
        startSoundSlider.value = gameData.soundVolume;
        pauseSoundSlider.value = gameData.soundVolume;

        startMusicSlider.interactable = gameData.music;
        pauseMusicSlider.interactable = gameData.music;
        startSoundSlider.interactable = gameData.sound;
        pauseSoundSlider.interactable = gameData.sound;

        syncing = false;
    }

    void OnMusicToggleChanged(bool value)
    {
        if (syncing)
            return;

        if (audioManager != null)
            audioManager.SetMusicEnabled(value);
        else if (gameData != null)
            gameData.music = value;

        SyncControls();
    }

    void OnSoundToggleChanged(bool value)
    {
        if (syncing)
            return;

        if (audioManager != null)
            audioManager.SetSoundEnabled(value);
        else if (gameData != null)
            gameData.sound = value;

        SyncControls();
    }

    void OnMusicVolumeChanged(float value)
    {
        if (syncing)
            return;

        if (audioManager != null)
            audioManager.SetMusicVolume(value);
        else if (gameData != null)
            gameData.musicVolume = value;
    }

    void OnSoundVolumeChanged(float value)
    {
        if (syncing)
            return;

        if (audioManager != null)
            audioManager.SetSoundVolume(value);
        else if (gameData != null)
            gameData.soundVolume = value;
    }

    void SetupListeners()
    {
        startMusicToggle.onValueChanged.AddListener(OnMusicToggleChanged);
        pauseMusicToggle.onValueChanged.AddListener(OnMusicToggleChanged);
        startSoundToggle.onValueChanged.AddListener(OnSoundToggleChanged);
        pauseSoundToggle.onValueChanged.AddListener(OnSoundToggleChanged);

        startMusicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        pauseMusicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        startSoundSlider.onValueChanged.AddListener(OnSoundVolumeChanged);
        pauseSoundSlider.onValueChanged.AddListener(OnSoundVolumeChanged);
    }

    void BuildUI()
    {
        var canvasObject = new GameObject("Menu Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        if (FindObjectOfType<EventSystem>() == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        var startImage = CreatePanel(canvas.transform, "Start Panel", new Color(0.03f, 0.05f, 0.12f, 0.97f));
        startPanel = startImage.gameObject;

        startTitleText = CreateText(startPanel.transform, "Title", "ARKANOID", 80, TextAnchor.MiddleCenter, new Vector2(0f, 320f), new Vector2(1000f, 120f));
        gameOverLabel = CreateText(startPanel.transform, "Game Over Label", "ИГРА ОКОНЧЕНА", 42, TextAnchor.MiddleCenter, new Vector2(0f, 220f), new Vector2(800f, 80f)).gameObject;
        gameOverLabel.SetActive(false);

        startMusicToggle = CreateToggle(startPanel.transform, "Music Toggle", new Vector2(-410f, 60f));
        CreateText(startPanel.transform, "Music Label", "Фоновая музыка", 32, TextAnchor.MiddleLeft, new Vector2(-220f, 60f), new Vector2(300f, 50f));
        startMusicSlider = CreateSlider(startPanel.transform, "Music Slider", new Vector2(230f, 60f), new Vector2(440f, 28f));

        startSoundToggle = CreateToggle(startPanel.transform, "Sound Toggle", new Vector2(-410f, -40f));
        CreateText(startPanel.transform, "Sound Label", "Звуковые эффекты", 32, TextAnchor.MiddleLeft, new Vector2(-220f, -40f), new Vector2(300f, 50f));
        startSoundSlider = CreateSlider(startPanel.transform, "Sound Slider", new Vector2(230f, -40f), new Vector2(440f, 28f));

        CreateButton(startPanel.transform, "Play Button", "Играть", new Vector2(0f, -180f), new Vector2(320f, 72f), new Color(0.15f, 0.45f, 0.85f, 1f), PlayGame);
        startExitButton = CreateButton(startPanel.transform, "Exit Button", "Выход", new Vector2(0f, -280f), new Vector2(320f, 72f), new Color(0.55f, 0.15f, 0.15f, 1f), QuitGame).gameObject;
        startExitButton.SetActive(false);

        var pauseImage = CreatePanel(canvas.transform, "Pause Panel", new Color(0.02f, 0.03f, 0.08f, 0.9f));
        pausePanel = pauseImage.gameObject;

        CreateText(pausePanel.transform, "Pause Title", "ПАУЗА", 72, TextAnchor.MiddleCenter, new Vector2(0f, 300f), new Vector2(1000f, 100f));

        pauseMusicToggle = CreateToggle(pausePanel.transform, "Music Toggle", new Vector2(-410f, 100f));
        CreateText(pausePanel.transform, "Music Label", "Фоновая музыка", 32, TextAnchor.MiddleLeft, new Vector2(-220f, 100f), new Vector2(300f, 50f));
        pauseMusicSlider = CreateSlider(pausePanel.transform, "Music Slider", new Vector2(230f, 100f), new Vector2(440f, 28f));

        pauseSoundToggle = CreateToggle(pausePanel.transform, "Sound Toggle", new Vector2(-410f, 0f));
        CreateText(pausePanel.transform, "Sound Label", "Звуковые эффекты", 32, TextAnchor.MiddleLeft, new Vector2(-220f, 0f), new Vector2(300f, 50f));
        pauseSoundSlider = CreateSlider(pausePanel.transform, "Sound Slider", new Vector2(230f, 0f), new Vector2(440f, 28f));

        CreateButton(pausePanel.transform, "Resume Button", "Продолжить", new Vector2(0f, -140f), new Vector2(360f, 68f), new Color(0.15f, 0.45f, 0.85f, 1f), ResumeGame);
        CreateButton(pausePanel.transform, "New Game Button", "Новая игра", new Vector2(0f, -230f), new Vector2(360f, 68f), new Color(0.2f, 0.55f, 0.25f, 1f), StartNewGame);
        CreateButton(pausePanel.transform, "Exit Button", "Выход", new Vector2(0f, -320f), new Vector2(360f, 68f), new Color(0.55f, 0.15f, 0.15f, 1f), QuitGame);

        pausePanel.SetActive(false);
        SetupListeners();
        SyncControls();
    }

    Image CreatePanel(Transform parent, string name, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var image = go.GetComponent<Image>();
        image.color = color;
        Stretch(image.rectTransform);
        return image;
    }

    Text CreateText(Transform parent, string name, string text, int fontSize, TextAnchor alignment, Vector2 position, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        var textComponent = go.AddComponent<Text>();
        textComponent.font = GetFont();
        textComponent.fontSize = fontSize;
        textComponent.text = text;
        textComponent.color = Color.white;
        textComponent.alignment = alignment;
        textComponent.horizontalOverflow = HorizontalWrapMode.Overflow;
        textComponent.verticalOverflow = VerticalWrapMode.Overflow;

        Place(textComponent.rectTransform, position, size);
        return textComponent;
    }

    Toggle CreateToggle(Transform parent, string name, Vector2 position)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        Place(go.GetComponent<RectTransform>(), position, new Vector2(40f, 40f));

        var toggle = go.AddComponent<Toggle>();

        var backgroundObject = new GameObject("Background", typeof(RectTransform), typeof(Image));
        backgroundObject.transform.SetParent(go.transform, false);
        var background = backgroundObject.GetComponent<Image>();
        background.color = new Color(0.8f, 0.8f, 0.9f, 1f);
        Stretch(background.rectTransform);

        var checkmarkObject = new GameObject("Checkmark", typeof(RectTransform), typeof(Image));
        checkmarkObject.transform.SetParent(backgroundObject.transform, false);
        var checkmark = checkmarkObject.GetComponent<Image>();
        checkmark.color = new Color(0.2f, 0.85f, 0.35f, 1f);
        var checkmarkRect = checkmark.rectTransform;
        checkmarkRect.anchorMin = new Vector2(0.2f, 0.2f);
        checkmarkRect.anchorMax = new Vector2(0.8f, 0.8f);
        checkmarkRect.offsetMin = Vector2.zero;
        checkmarkRect.offsetMax = Vector2.zero;

        toggle.targetGraphic = background;
        toggle.graphic = checkmark;
        toggle.isOn = true;

        return toggle;
    }

    Slider CreateSlider(Transform parent, string name, Vector2 position, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        Place(go.GetComponent<RectTransform>(), position, size);

        var slider = go.AddComponent<Slider>();

        var backgroundObject = new GameObject("Background", typeof(RectTransform), typeof(Image));
        backgroundObject.transform.SetParent(go.transform, false);
        var background = backgroundObject.GetComponent<Image>();
        background.color = new Color(0.25f, 0.25f, 0.32f, 1f);
        Stretch(background.rectTransform);

        var fillAreaObject = new GameObject("Fill Area", typeof(RectTransform));
        fillAreaObject.transform.SetParent(go.transform, false);
        var fillArea = fillAreaObject.GetComponent<RectTransform>();
        fillArea.anchorMin = new Vector2(0f, 0.25f);
        fillArea.anchorMax = new Vector2(1f, 0.75f);
        fillArea.offsetMin = new Vector2(6f, 0f);
        fillArea.offsetMax = new Vector2(-16f, 0f);

        var fillObject = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fillObject.transform.SetParent(fillAreaObject.transform, false);
        var fill = fillObject.GetComponent<Image>();
        fill.color = new Color(0.2f, 0.75f, 0.35f, 1f);
        Stretch(fill.rectTransform);

        var handleAreaObject = new GameObject("Handle Slide Area", typeof(RectTransform));
        handleAreaObject.transform.SetParent(go.transform, false);
        var handleArea = handleAreaObject.GetComponent<RectTransform>();
        Stretch(handleArea);
        handleArea.offsetMin = new Vector2(10f, 0f);
        handleArea.offsetMax = new Vector2(-10f, 0f);

        var handleObject = new GameObject("Handle", typeof(RectTransform), typeof(Image));
        handleObject.transform.SetParent(handleAreaObject.transform, false);
        var handle = handleObject.GetComponent<Image>();
        handle.color = Color.white;
        var handleRect = handle.rectTransform;
        handleRect.anchorMin = new Vector2(0f, 0.5f);
        handleRect.anchorMax = new Vector2(0f, 0.5f);
        handleRect.pivot = new Vector2(0.5f, 0.5f);
        handleRect.sizeDelta = new Vector2(24f, 36f);

        slider.fillRect = fill.rectTransform;
        slider.handleRect = handleRect;
        slider.targetGraphic = handle;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0.5f;

        return slider;
    }

    Button CreateButton(Transform parent, string name, string label, Vector2 position, Vector2 size, Color color, UnityAction action)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        Place(go.GetComponent<RectTransform>(), position, size);

        var image = go.GetComponent<Image>();
        image.color = color;

        var button = go.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(action);

        var text = CreateText(go.transform, "Label", label, 30, TextAnchor.MiddleCenter, Vector2.zero, size);
        Stretch(text.rectTransform);

        return button;
    }

    void Place(RectTransform rectTransform, Vector2 position, Vector2 size)
    {
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = size;
    }

    void Stretch(RectTransform rectTransform)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }

    Font GetFont()
    {
        if (cachedFont == null)
        {
            cachedFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (cachedFont == null)
                cachedFont = Font.CreateDynamicFontFromOSFont("Arial", 32);
        }

        return cachedFont;
    }
}
