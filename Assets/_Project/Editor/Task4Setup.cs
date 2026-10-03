using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class Task4Setup
{
    [MenuItem("Archery/Setup Task 4 (UI, HUD, Menus, GameManager)")]
    public static void RunSetup()
    {
        var scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/ArcheryGame.unity");
        Debug.Log("[Task4Setup] Starting Task 4 UI & Game Flow Setup on: " + scene.name);

        // 1. Setup EventSystem
        EnsureEventSystem();

        // 2. Setup GameManager on GAME SYSTEM
        GameObject gameSystem = GameObject.Find("GAME SYSTEM");
        if (gameSystem == null) gameSystem = new GameObject("GAME SYSTEM");

        GameManager gm = gameSystem.GetComponent<GameManager>();
        if (gm == null) gm = gameSystem.AddComponent<GameManager>();

        GameObject player = GameObject.Find("PC_Player");
        if (player != null)
        {
            gm.playerObject = player;
            gm.bowShoot = player.GetComponentInChildren<BowShoot>();
            gm.playerMovement = player.GetComponent<PlayerMovement>();
            gm.playerCameraController = player.GetComponentInChildren<PlayerCameraController>();
        }

        // 3. Setup Canvas under UI root
        GameObject uiRoot = GameObject.Find("UI");
        if (uiRoot == null) uiRoot = new GameObject("UI");

        // Clear any old Canvas under UI to ensure clean build
        for (int i = uiRoot.transform.childCount - 1; i >= 0; i--)
        {
            Object.DestroyImmediate(uiRoot.transform.GetChild(i).gameObject);
        }

        GameObject canvasObj = new GameObject("Canvas");
        canvasObj.transform.SetParent(uiRoot.transform, false);

        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();
        UIManager uim = canvasObj.AddComponent<UIManager>();

        // Load Default TMP Font
        TMP_FontAsset fontAsset = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");

        // 4. Build 4 Panels
        // 4.1 MainMenuPanel
        GameObject mainMenuPanel = CreateFullStretchPanel("MainMenuPanel", canvasObj.transform, new Color(0.04f, 0.05f, 0.08f, 0.88f));
        CreateText("TitleText", mainMenuPanel.transform, "ARCHERY 3D", 72, FontStyles.Bold, new Color(0.95f, 0.78f, 0.2f), TextAlignmentOptions.Center, new Vector2(0, 160), new Vector2(900, 120), fontAsset);
        CreateText("SubtitleText", mainMenuPanel.transform, "MEDIEVAL MARKSMAN CHALLENGE", 24, FontStyles.Normal, new Color(0.75f, 0.8f, 0.9f), TextAlignmentOptions.Center, new Vector2(0, 90), new Vector2(900, 50), fontAsset);
        Button startBtn = CreateButton("StartButton", mainMenuPanel.transform, "START GAME", new Vector2(0, -30), new Vector2(280, 60), new Color(0.18f, 0.45f, 0.22f), Color.white, fontAsset);
        Button quitBtn = CreateButton("QuitButton", mainMenuPanel.transform, "QUIT", new Vector2(0, -110), new Vector2(280, 55), new Color(0.25f, 0.15f, 0.15f), new Color(0.9f, 0.8f, 0.8f), fontAsset);

        // 4.2 GameplayHUD
        GameObject gameplayHUD = CreateFullStretchPanel("GameplayHUD", canvasObj.transform, Color.clear);

        // Score Badge (Top-Left)
        GameObject scoreBadge = CreatePanel("ScoreBadge", gameplayHUD.transform, new Vector2(180, -65), new Vector2(240, 75), new Vector2(0, 1), new Vector2(0, 1), new Color(0.06f, 0.08f, 0.12f, 0.75f));
        CreateText("ScoreLabel", scoreBadge.transform, "SCORE", 15, FontStyles.Bold, new Color(0.7f, 0.75f, 0.85f), TextAlignmentOptions.Center, new Vector2(0, 18), new Vector2(220, 30), fontAsset);
        TMP_Text scoreText = CreateText("ScoreValue", scoreBadge.transform, "0", 38, FontStyles.Bold, new Color(1f, 0.84f, 0.1f), TextAlignmentOptions.Center, new Vector2(0, -14), new Vector2(220, 45), fontAsset);

        // Ammo Badge (Top-Right)
        GameObject ammoBadge = CreatePanel("AmmoBadge", gameplayHUD.transform, new Vector2(-180, -65), new Vector2(240, 75), new Vector2(1, 1), new Vector2(1, 1), new Color(0.06f, 0.08f, 0.12f, 0.75f));
        CreateText("AmmoLabel", ammoBadge.transform, "ARROWS", 15, FontStyles.Bold, new Color(0.7f, 0.75f, 0.85f), TextAlignmentOptions.Center, new Vector2(0, 18), new Vector2(220, 30), fontAsset);
        TMP_Text ammoText = CreateText("AmmoValue", ammoBadge.transform, "20 / 20", 34, FontStyles.Bold, Color.white, TextAlignmentOptions.Center, new Vector2(0, -14), new Vector2(220, 45), fontAsset);

        // Crosshair (Center)
        GameObject crosshair = CreateCrosshair("Crosshair", gameplayHUD.transform);

        // Charge Bar (Center, below crosshair)
        GameObject chargeBarRoot = CreatePanel("ChargeBarRoot", gameplayHUD.transform, new Vector2(0, -65), new Vector2(200, 16), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Color(0.08f, 0.08f, 0.1f, 0.8f));
        Image chargeFill = CreateChargeBarFill("ChargeBarFill", chargeBarRoot.transform);

        // Hit Marker (Center)
        GameObject hitMarker = CreatePanel("HitMarker", gameplayHUD.transform, Vector2.zero, new Vector2(40, 40), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Color.clear);
        TMP_Text hitMarkerText = CreateText("MarkerSymbol", hitMarker.transform, "×", 42, FontStyles.Bold, new Color(1f, 0.2f, 0.2f, 0.95f), TextAlignmentOptions.Center, Vector2.zero, new Vector2(40, 40), fontAsset);

        // Floating Score Container
        GameObject scoreContainer = CreateFullStretchPanel("FloatingScoreContainer", gameplayHUD.transform, Color.clear);
        RectTransform scoreContainerRect = scoreContainer.GetComponent<RectTransform>();

        // 4.3 PausePanel
        GameObject pausePanel = CreateFullStretchPanel("PausePanel", canvasObj.transform, new Color(0f, 0f, 0f, 0.78f));
        CreateText("PauseTitle", pausePanel.transform, "GAME PAUSED", 56, FontStyles.Bold, Color.white, TextAlignmentOptions.Center, new Vector2(0, 140), new Vector2(800, 80), fontAsset);
        Button resumeBtn = CreateButton("ResumeButton", pausePanel.transform, "RESUME", new Vector2(0, 30), new Vector2(260, 55), new Color(0.18f, 0.45f, 0.22f), Color.white, fontAsset);
        Button pauseRestartBtn = CreateButton("RestartButton", pausePanel.transform, "RESTART", new Vector2(0, -40), new Vector2(260, 55), new Color(0.2f, 0.35f, 0.5f), Color.white, fontAsset);
        Button pauseMainMenuBtn = CreateButton("MainMenuButton", pausePanel.transform, "MAIN MENU", new Vector2(0, -110), new Vector2(260, 55), new Color(0.25f, 0.15f, 0.15f), new Color(0.9f, 0.85f, 0.85f), fontAsset);

        // 4.4 ResultPanel
        GameObject resultPanel = CreateFullStretchPanel("ResultPanel", canvasObj.transform, new Color(0.03f, 0.04f, 0.07f, 0.92f));
        CreateText("ResultTitle", resultPanel.transform, "ROUND COMPLETED", 56, FontStyles.Bold, new Color(0.95f, 0.78f, 0.2f), TextAlignmentOptions.Center, new Vector2(0, 160), new Vector2(800, 80), fontAsset);
        TMP_Text finalScoreText = CreateText("FinalScoreText", resultPanel.transform, "FINAL SCORE: 0", 40, FontStyles.Bold, new Color(1f, 0.88f, 0.2f), TextAlignmentOptions.Center, new Vector2(0, 60), new Vector2(800, 60), fontAsset);
        TMP_Text totalHitsText = CreateText("TotalHitsText", resultPanel.transform, "TOTAL HITS: 0", 26, FontStyles.Normal, new Color(0.85f, 0.9f, 0.95f), TextAlignmentOptions.Center, new Vector2(0, 0), new Vector2(800, 45), fontAsset);
        TMP_Text bullseyeHitsText = CreateText("BullseyeHitsText", resultPanel.transform, "BULLSEYES: 0", 26, FontStyles.Normal, new Color(0.2f, 1f, 0.5f), TextAlignmentOptions.Center, new Vector2(0, -45), new Vector2(800, 45), fontAsset);
        Button resultRestartBtn = CreateButton("RestartButton", resultPanel.transform, "PLAY AGAIN", new Vector2(0, -125), new Vector2(260, 55), new Color(0.18f, 0.45f, 0.22f), Color.white, fontAsset);
        Button resultMainMenuBtn = CreateButton("MainMenuButton", resultPanel.transform, "MAIN MENU", new Vector2(0, -195), new Vector2(260, 55), new Color(0.25f, 0.15f, 0.15f), new Color(0.9f, 0.85f, 0.85f), fontAsset);

        // 5. Connect UIManager References
        uim.mainMenuPanel = mainMenuPanel;
        uim.gameplayHUD = gameplayHUD;
        uim.pausePanel = pausePanel;
        uim.resultPanel = resultPanel;

        uim.scoreText = scoreText;
        uim.ammoText = ammoText;
        uim.crosshair = crosshair;
        uim.chargeBarFill = chargeFill;
        uim.chargeBarRoot = chargeBarRoot;
        uim.hitMarker = hitMarker;
        uim.floatingScoreContainer = scoreContainerRect;
        uim.floatingScorePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/FloatingScoreItem.prefab");

        uim.finalScoreText = finalScoreText;
        uim.totalHitsText = totalHitsText;
        // bullseyeHitsText removed in Task 6 — now using arrowsUsedText and accuracyText

        uim.startButton = startBtn;
        uim.quitButton = quitBtn;
        uim.resumeButton = resumeBtn;
        uim.pauseRestartButton = pauseRestartBtn;
        uim.pauseMainMenuButton = pauseMainMenuBtn;
        uim.resultRestartButton = resultRestartBtn;
        uim.resultMainMenuButton = resultMainMenuBtn;

        if (player != null)
        {
            uim.bowShoot = player.GetComponentInChildren<BowShoot>();
        }

        // Set initial panels active state: MainMenu active, others inactive
        mainMenuPanel.SetActive(true);
        gameplayHUD.SetActive(false);
        pausePanel.SetActive(false);
        resultPanel.SetActive(false);

        // 6. Save Scene
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();

        Debug.Log("[Task4Setup] UI Setup completed successfully!");
    }

    private static void EnsureEventSystem()
    {
        EventSystem es = Object.FindAnyObjectByType<EventSystem>();
        if (es == null)
        {
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<EventSystem>();
            esObj.AddComponent<StandaloneInputModule>();
            Debug.Log("[Task4Setup] Created EventSystem with StandaloneInputModule.");
        }
    }

    private static GameObject CreateFullStretchPanel(string name, Transform parent, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image img = go.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = (color.a > 0.001f);

        return go;
    }

    private static GameObject CreatePanel(string name, Transform parent, Vector2 anchoredPos, Vector2 size, Vector2 anchorMin, Vector2 anchorMax, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        Image img = go.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = (color.a > 0.001f);

        return go;
    }

    private static TMP_Text CreateText(string name, Transform parent, string content, float fontSize, FontStyles style, Color color, TextAlignmentOptions align, Vector2 anchoredPos, Vector2 size, TMP_FontAsset fontAsset)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = content;
        tmp.fontSize = fontSize;
        tmp.fontStyle = style;
        tmp.color = color;
        tmp.alignment = align;
        tmp.raycastTarget = false;
        if (fontAsset != null) tmp.font = fontAsset;

        return tmp;
    }

    private static Button CreateButton(string name, Transform parent, string label, Vector2 anchoredPos, Vector2 size, Color btnColor, Color textColor, TMP_FontAsset fontAsset)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        Image img = go.AddComponent<Image>();
        img.color = btnColor;
        img.raycastTarget = true;

        Button btn = go.AddComponent<Button>();
        btn.targetGraphic = img;

        ColorBlock cb = btn.colors;
        cb.normalColor = btnColor;
        cb.highlightedColor = btnColor * 1.25f;
        cb.pressedColor = btnColor * 0.8f;
        cb.selectedColor = btnColor * 1.1f;
        btn.colors = cb;

        // Label text
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(go.transform, false);

        RectTransform labelRt = labelObj.AddComponent<RectTransform>();
        labelRt.anchorMin = Vector2.zero;
        labelRt.anchorMax = Vector2.one;
        labelRt.offsetMin = Vector2.zero;
        labelRt.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = labelObj.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 22;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = textColor;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        if (fontAsset != null) tmp.font = fontAsset;

        return btn;
    }

    private static GameObject CreateCrosshair(string name, Transform parent)
    {
        GameObject root = new GameObject(name);
        root.transform.SetParent(parent, false);

        RectTransform rt = root.AddComponent<RectTransform>();
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(30, 30);

        // Center dot
        CreatePanel("CenterDot", root.transform, Vector2.zero, new Vector2(4, 4), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Color(1f, 1f, 1f, 0.9f));

        // 4 lines
        CreatePanel("TopLine", root.transform, new Vector2(0, 10), new Vector2(2, 8), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Color(1f, 1f, 1f, 0.75f));
        CreatePanel("BottomLine", root.transform, new Vector2(0, -10), new Vector2(2, 8), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Color(1f, 1f, 1f, 0.75f));
        CreatePanel("LeftLine", root.transform, new Vector2(-10, 0), new Vector2(8, 2), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Color(1f, 1f, 1f, 0.75f));
        CreatePanel("RightLine", root.transform, new Vector2(10, 0), new Vector2(8, 2), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Color(1f, 1f, 1f, 0.75f));

        return root;
    }

    private static Image CreateChargeBarFill(string name, Transform parent)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.01f, 0.1f);
        rt.anchorMax = new Vector2(0.99f, 0.9f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image img = go.AddComponent<Image>();
        img.color = new Color(0.95f, 0.7f, 0.1f, 1f);
        img.type = Image.Type.Filled;
        img.fillMethod = Image.FillMethod.Horizontal;
        img.fillOrigin = (int)Image.OriginHorizontal.Left;
        img.fillAmount = 0f;
        img.raycastTarget = false;

        return img;
    }
}
