using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public enum GameState
    {
        StartMenu,
        Playing,
        GameOver
    }

    public enum GameOverReason
    {
        None,
        ArrowsExhausted,
        TimeExpired
    }

    [System.Serializable]
    public class LeaderboardRecord
    {
        public int score;
        public float timeUsed;
    }

    [System.Serializable]
    public class LeaderboardData
    {
        public List<LeaderboardRecord> records = new List<LeaderboardRecord>();
    }

    private const string LeaderboardPrefKey = "ArcheryVR_Top3_Leaderboard";

    [Header("Session Settings")]
    [SerializeField] private int maxArrows = 10;
    [SerializeField] private int pointsPerHit = 10;

    [Header("Timer Settings")]
    [SerializeField] private float roundDuration = 60f;
    [SerializeField] private float timeRemaining = 60f;
    [SerializeField] private bool isTimerRunning = false;
    [SerializeField] private GameOverReason lastGameOverReason = GameOverReason.None;

    [Header("Bow Reference")]
    [SerializeField] private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable bowInteractable;

    [Header("Runtime State")]
    [SerializeField] private int currentScore = 0;
    [SerializeField] private int arrowsShot = 0;
    [SerializeField] private GameState state = GameState.StartMenu;
    [SerializeField] private bool hasRecordedResultThisRound = false;

    [Header("Leaderboard Data")]
    [SerializeField] private LeaderboardData leaderboardData = new LeaderboardData();

    [Header("In-Game HUD Reference")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private GameObject scorePanel;
    [SerializeField] private TMP_Text hudScoreText;
    [SerializeField] private TMP_Text hudArrowsText;
    [SerializeField] private TMP_Text hudTimeText;

    [Header("Start Panel UI")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private UnityEngine.UI.Button startButton;
    [SerializeField] private UnityEngine.UI.Button guideButton;
    [SerializeField] private TMP_Text startLeaderboardText;

    [Header("Guide Panel UI")]
    [SerializeField] private GameObject guidePanel;
    [SerializeField] private UnityEngine.UI.Button guideBackButton;

    [Header("Result Panel UI")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TMP_Text resultSummaryText;
    [SerializeField] private TMP_Text resultLeaderboardText;
    [SerializeField] private TMP_Text resultScoreBadgeText;
    [SerializeField] private TMP_Text resultDetailText;
    [SerializeField] private UnityEngine.UI.Button restartButton;
    [SerializeField] private UnityEngine.UI.Button mainMenuButton;

    [Header("Display Format")]
    [SerializeField] private string scoreFormat = "<size=60%><color=#E5C158>ĐIỂM SỐ</color></size>\n<b><color=#FFFFFF>{0}</color></b>\n<size=44%><color=#E5C158>MŨI TÊN: </color><color=#FFFFFF>{1}/{2}</color>  <color=#E5C158>THỜI GIAN: </color><color=#FFFFFF>{3}s</color></size>";

    private int lastDisplayedSeconds = -1;

    public int CurrentScore => currentScore;
    public int MaxArrows => maxArrows;
    public int ArrowsShot => arrowsShot;
    public int ArrowsRemaining => Mathf.Max(0, maxArrows - arrowsShot);
    public float TimeRemaining => Mathf.Max(0f, timeRemaining);
    public float RoundDuration => roundDuration;
    public bool IsTimerRunning => isTimerRunning;
    public GameOverReason LastGameOverReason => lastGameOverReason;
    public GameState State => state;
    public bool IsPlaying => state == GameState.Playing;
    public bool CanShoot => state == GameState.Playing && isTimerRunning && ArrowsRemaining > 0 && timeRemaining > 0f;
    public List<LeaderboardRecord> TopRecords => leaderboardData != null ? leaderboardData.records : new List<LeaderboardRecord>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        state = GameState.StartMenu;
        currentScore = 0;
        arrowsShot = 0;
        timeRemaining = roundDuration;
        isTimerRunning = false;
        hasRecordedResultThisRound = false;
        lastGameOverReason = GameOverReason.None;
        LoadLeaderboard();
    }

    private void Start()
    {
        InitializeUIReferences();
        UpdateLeaderboardUI();
        UpdateScoreUI();
    }

    private void Update()
    {
        // 1. Quản lý đồng hồ đếm ngược 60s khi đang chơi VÀ ĐÃ CẦM CUNG
        if (state == GameState.Playing && isTimerRunning)
        {
            if (timeRemaining > 0f)
            {
                timeRemaining -= Time.deltaTime;
                if (timeRemaining <= 0f)
                {
                    timeRemaining = 0f;
                    TriggerGameOver(GameOverReason.TimeExpired);
                }
                else
                {
                    int currentSeconds = Mathf.CeilToInt(timeRemaining);
                    if (currentSeconds != lastDisplayedSeconds)
                    {
                        lastDisplayedSeconds = currentSeconds;
                        UpdateScoreUI();
                    }
                }
            }
        }

        // Hỗ trợ phím B để mô phỏng sự kiện cầm cung khi test PC/Editor
        if (state == GameState.Playing && !isTimerRunning)
        {
            if (UnityEngine.InputSystem.Keyboard.current != null &&
                UnityEngine.InputSystem.Keyboard.current.bKey.wasPressedThisFrame)
            {
                OnBowGrabbed();
            }
        }
        // Phím tắt khi ở StartMenu
        else if (state == GameState.StartMenu)
        {
            if (UnityEngine.InputSystem.Keyboard.current != null)
            {
                // Nếu Guide đang mở
                if (guidePanel != null && guidePanel.activeSelf)
                {
                    if (UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame ||
                        UnityEngine.InputSystem.Keyboard.current.backspaceKey.wasPressedThisFrame)
                    {
                        HideGuide();
                    }
                }
                else
                {
                    if (UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame ||
                        UnityEngine.InputSystem.Keyboard.current.enterKey.wasPressedThisFrame)
                    {
                        StartGame();
                    }
                    else if (UnityEngine.InputSystem.Keyboard.current.hKey.wasPressedThisFrame ||
                             UnityEngine.InputSystem.Keyboard.current.gKey.wasPressedThisFrame)
                    {
                        ShowGuide();
                    }
                }
            }
        }
        // Phím tắt khi GameOver
        else if (state == GameState.GameOver)
        {
            if (UnityEngine.InputSystem.Keyboard.current != null)
            {
                if (UnityEngine.InputSystem.Keyboard.current.rKey.wasPressedThisFrame ||
                    UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame)
                {
                    RestartGame();
                }
                else if (UnityEngine.InputSystem.Keyboard.current.mKey.wasPressedThisFrame ||
                         UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
                {
                    ReturnToMainMenu();
                }
            }
        }
    }

    private void InitializeUIReferences()
    {
        if (scoreText == null)
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                Transform existingHud = cam.transform.Find("Score_HUD_Canvas");
                if (existingHud != null)
                {
                    Transform txtTrans = existingHud.Find("Score_Panel/Score_Text");
                    if (txtTrans != null)
                    {
                        scoreText = txtTrans.GetComponent<TMP_Text>();
                    }
                    else
                    {
                        scoreText = existingHud.GetComponentInChildren<TMP_Text>(true);
                    }
                }
            }

            if (scoreText == null)
            {
                scoreText = FindAnyObjectByType<TMP_Text>();
            }
        }

        Transform hudCanvas = null;
        if (scoreText != null)
        {
            Canvas canvas = scoreText.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                hudCanvas = canvas.transform;
                if (canvas.worldCamera == null)
                {
                    canvas.worldCamera = Camera.main;
                }
            }
        }

        if (hudCanvas == null)
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                hudCanvas = cam.transform.Find("Score_HUD_Canvas");
            }
        }

        if (hudCanvas != null)
        {
            // Tìm Score_Panel
            if (scorePanel == null)
            {
                scorePanel = hudCanvas.Find("Score_Panel")?.gameObject;
            }
            if (scorePanel != null)
            {
                if (hudScoreText == null)
                    hudScoreText = scorePanel.transform.Find("Badge_Score/Score_Value")?.GetComponent<TMP_Text>();
                if (hudArrowsText == null)
                    hudArrowsText = scorePanel.transform.Find("Capsule_Arrows/Text")?.GetComponent<TMP_Text>();
                if (hudTimeText == null)
                    hudTimeText = scorePanel.transform.Find("Capsule_Time/Text")?.GetComponent<TMP_Text>();
            }

            // Tìm Start_Panel và các nút con
            if (startPanel == null)
            {
                startPanel = hudCanvas.Find("Start_Panel")?.gameObject;
            }
            if (startPanel != null)
            {
                if (startButton == null)
                {
                    startButton = startPanel.transform.Find("Start_Button")?.GetComponent<UnityEngine.UI.Button>();
                }
                if (guideButton == null)
                {
                    guideButton = startPanel.transform.Find("Guide_Button")?.GetComponent<UnityEngine.UI.Button>();
                }
                if (startLeaderboardText == null)
                {
                    startLeaderboardText = startPanel.transform.Find("Leaderboard_Section/Leaderboard_Text")?.GetComponent<TMP_Text>();
                }
            }

            // Tìm Guide_Panel và nút con
            if (guidePanel == null)
            {
                guidePanel = hudCanvas.Find("Guide_Panel")?.gameObject;
            }
            if (guidePanel != null && guideBackButton == null)
            {
                guideBackButton = guidePanel.transform.Find("Back_Button")?.GetComponent<UnityEngine.UI.Button>();
            }

            // Tìm Result_Panel và các thành phần con
            if (resultPanel == null)
            {
                resultPanel = hudCanvas.Find("Result_Panel")?.gameObject;
            }
            if (resultPanel != null)
            {
                if (resultScoreBadgeText == null)
                {
                    resultScoreBadgeText = resultPanel.transform.Find("Score_Showcase/Badge_Shield/Score_Value")?.GetComponent<TMP_Text>();
                }
                if (resultDetailText == null)
                {
                    resultDetailText = resultPanel.transform.Find("Score_Showcase/Detail_Text")?.GetComponent<TMP_Text>();
                }
                if (resultSummaryText == null)
                {
                    resultSummaryText = resultPanel.transform.Find("Summary_Text")?.GetComponent<TMP_Text>();
                }
                if (resultLeaderboardText == null)
                {
                    resultLeaderboardText = resultPanel.transform.Find("Leaderboard_Section/Leaderboard_Text")?.GetComponent<TMP_Text>();
                }
                if (restartButton == null)
                {
                    restartButton = resultPanel.transform.Find("Restart_Button")?.GetComponent<UnityEngine.UI.Button>();
                }
                if (mainMenuButton == null)
                {
                    mainMenuButton = resultPanel.transform.Find("MainMenu_Button")?.GetComponent<UnityEngine.UI.Button>();
                }
            }
        }

        if (bowInteractable == null)
        {
            bowInteractable = FindAnyObjectByType<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        }

        if (bowInteractable != null)
        {
            bowInteractable.selectEntered.RemoveListener(OnBowSelectEntered);
            bowInteractable.selectEntered.AddListener(OnBowSelectEntered);
        }

        if (startButton != null)
        {
            startButton.onClick.RemoveListener(StartGame);
            startButton.onClick.AddListener(StartGame);
        }

        if (guideButton != null)
        {
            guideButton.onClick.RemoveListener(ShowGuide);
            guideButton.onClick.AddListener(ShowGuide);
        }

        if (guideBackButton != null)
        {
            guideBackButton.onClick.RemoveListener(HideGuide);
            guideBackButton.onClick.AddListener(HideGuide);
        }

        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(RestartGame);
            restartButton.onClick.AddListener(RestartGame);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveListener(ReturnToMainMenu);
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        }

        UpdatePanelVisibility();
    }

    private void OnDestroy()
    {
        if (bowInteractable != null)
        {
            bowInteractable.selectEntered.RemoveListener(OnBowSelectEntered);
        }
    }

    public void OnBowGrabbed()
    {
        // Chỉ kích hoạt khi đang ở trạng thái Playing và bộ đếm chưa chạy
        if (state == GameState.Playing && !isTimerRunning)
        {
            isTimerRunning = true;
            lastDisplayedSeconds = Mathf.CeilToInt(timeRemaining);
            UpdateScoreUI();
        }
    }

    public void OnBowSelectEntered(UnityEngine.XR.Interaction.Toolkit.SelectEnterEventArgs args)
    {
        OnBowGrabbed();
    }

    public bool RecordShot()
    {
        if (state != GameState.Playing || !isTimerRunning || ArrowsRemaining <= 0 || timeRemaining <= 0f)
        {
            return false;
        }

        arrowsShot++;
        UpdateScoreUI();

        if (ArrowsRemaining <= 0)
        {
            TriggerGameOver(GameOverReason.ArrowsExhausted);
        }

        return true;
    }

    public void AddScore(int points = -1)
    {
        // Bảo vệ: Nếu ván đã kết thúc, chưa cầm cung hoặc hết thời gian, ngăn tuyệt đối việc cộng điểm
        if (state != GameState.Playing || !isTimerRunning || timeRemaining <= 0f)
            return;

        if (points < 0) points = pointsPerHit;
        currentScore += points;
        UpdateScoreUI();
    }

    public void SetScoreText(TMP_Text text)
    {
        scoreText = text;
        UpdateScoreUI();
    }

    public void StartGame()
    {
        currentScore = 0;
        arrowsShot = 0;
        timeRemaining = roundDuration;
        isTimerRunning = false; // Chuyển sang trạng thái chờ cầm cung, chưa chạy đồng hồ
        hasRecordedResultThisRound = false;
        lastDisplayedSeconds = Mathf.CeilToInt(roundDuration);
        lastGameOverReason = GameOverReason.None;
        state = GameState.Playing;

        UpdatePanelVisibility();
        UpdateScoreUI();
        ResetBowShoot();
    }

    private void TriggerGameOver(GameOverReason reason = GameOverReason.ArrowsExhausted)
    {
        // Chống kết thúc lần thứ hai nếu đã kết thúc hoặc đã ghi nhận
        if (state != GameState.Playing || hasRecordedResultThisRound)
            return;

        hasRecordedResultThisRound = true;
        state = GameState.GameOver;
        isTimerRunning = false;
        lastGameOverReason = reason;

        // Tính thời gian thực tế đã chơi
        float timeUsed = (reason == GameOverReason.TimeExpired) 
            ? roundDuration 
            : Mathf.Clamp(roundDuration - timeRemaining, 0f, roundDuration);

        // Ghi nhận và xếp hạng Top 3
        int newRank = RecordScore(currentScore, timeUsed);

        // Cập nhật nội dung hiển thị kết quả
        if (resultSummaryText != null)
        {
            string reasonStr = (reason == GameOverReason.TimeExpired) ? "HẾT THỜI GIAN" : "HẾT MŨI TÊN";
            string reasonColor = (reason == GameOverReason.TimeExpired) ? "#FF7675" : "#E5C158";
            string timeStr = FormatTime(timeUsed);

            string recordBanner = "";
            if (newRank > 0)
            {
                recordBanner = string.Format("\n<b><size=75%><color=#FFD700>* KỶ LỤC MỚI! BẠN ĐÃ ĐẠT HẠNG {0}! *</color></size></b>", newRank);
            }

            resultSummaryText.text = string.Format(
                "<size=72%><color=#A0AAB8>LÝ DO KẾT THÚC: </color><color={0}><b>{1}</b></color></size>\n" +
                "<size=75%><color=#A0AAB8>TỔNG ĐIỂM</color></size>\n<b><size=135%><color=#FFFFFF>{2}</color></size></b>\n" +
                "<size=70%><color=#A0AAB8>THỜI GIAN: </color><color=#E5C158>{3}</color>  <color=#A0AAB8>•</color>  <color=#A0AAB8>MŨI TÊN: </color><color=#E5C158>{4}/{5}</color></size>{6}",
                reasonColor, reasonStr, currentScore, timeStr, arrowsShot, maxArrows, recordBanner
            );
        }

        if (resultScoreBadgeText != null)
        {
            resultScoreBadgeText.text = currentScore.ToString();
        }

        if (resultDetailText != null)
        {
            string timeStr = FormatTime(timeUsed);
            string recordBanner = "";
            if (newRank > 0)
            {
                recordBanner = string.Format("\n<size=80%><color=#D84315><b>* KỶ LỤC MỚI! BẠN ĐÃ ĐẠT HẠNG {0}! *</b></color></size>", newRank);
            }

            resultDetailText.text = string.Format(
                "<size=120%><color=#3E2723><b>TỔNG ĐIỂM</b></color></size>\n" +
                "<size=78%><color=#5C3A21>MŨI TÊN ĐÃ DÙNG: <b>{0}/{1}</b>  •  THỜI GIAN: <b>{2}</b></color></size>{3}",
                arrowsShot, maxArrows, timeStr, recordBanner
            );
        }

        // Cập nhật bảng kỷ lục trên Result Panel
        if (resultLeaderboardText != null)
        {
            resultLeaderboardText.text = GetFormattedLeaderboard(newRank);
        }

        UpdatePanelVisibility();
        UpdateScoreUI();
        ResetBowShoot();
    }

    public void RestartGame()
    {
        currentScore = 0;
        arrowsShot = 0;
        timeRemaining = roundDuration;
        isTimerRunning = false; // Trở về trạng thái chờ cầm cung ở ván mới
        hasRecordedResultThisRound = false;
        lastDisplayedSeconds = Mathf.CeilToInt(roundDuration);
        lastGameOverReason = GameOverReason.None;
        state = GameState.Playing;

        UpdatePanelVisibility();

        // Dọn sạch các mũi tên cũ đã cắm trong scene
        ClearExistingArrows();

        // Đảm bảo dây cung và mũi tên dở dang được reset hoàn toàn
        ResetBowShoot();

        UpdateScoreUI();
    }

    public void ReturnToMainMenu()
    {
        currentScore = 0;
        arrowsShot = 0;
        timeRemaining = roundDuration;
        isTimerRunning = false;
        hasRecordedResultThisRound = false;
        lastGameOverReason = GameOverReason.None;
        state = GameState.StartMenu;

        ClearExistingArrows();
        ResetBowShoot();

        UpdateLeaderboardUI();
        UpdatePanelVisibility();
        UpdateScoreUI();
    }

    public void ShowGuide()
    {
        if (state != GameState.StartMenu) return;

        if (startPanel != null) startPanel.SetActive(false);
        if (guidePanel != null) guidePanel.SetActive(true);
    }

    public void HideGuide()
    {
        if (state != GameState.StartMenu) return;

        if (guidePanel != null) guidePanel.SetActive(false);
        if (startPanel != null) startPanel.SetActive(true);
        UpdateLeaderboardUI();
    }

    private void ClearExistingArrows()
    {
        var existingArrows = FindObjectsByType<ArrowStick>(FindObjectsInactive.Exclude);
        foreach (var arrow in existingArrows)
        {
            if (arrow != null)
            {
                Destroy(arrow.gameObject);
            }
        }
    }

    private void ResetBowShoot()
    {
        var bowShoots = FindObjectsByType<BowShoot>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var bs in bowShoots)
        {
            if (bs != null)
            {
                bs.ResetBowState();
            }
        }
    }

    public void SetTimeRemaining(float seconds)
    {
        timeRemaining = Mathf.Max(0f, seconds);
        if (timeRemaining <= 0f && state == GameState.Playing)
        {
            TriggerGameOver(GameOverReason.TimeExpired);
        }
        else
        {
            lastDisplayedSeconds = Mathf.CeilToInt(timeRemaining);
            UpdateScoreUI();
        }
    }

    private void UpdatePanelVisibility()
    {
        if (startPanel != null)
        {
            startPanel.SetActive(state == GameState.StartMenu && (guidePanel == null || !guidePanel.activeSelf));
        }

        if (guidePanel != null)
        {
            guidePanel.SetActive(state == GameState.StartMenu && guidePanel.activeSelf);
        }

        if (resultPanel != null)
        {
            resultPanel.SetActive(state == GameState.GameOver);
        }

        if (scorePanel != null)
        {
            // Khôi phục HUD bảng điểm khi vào ván chơi hoặc kết thúc ván
            scorePanel.SetActive(state != GameState.StartMenu);
        }
    }

    private void UpdateScoreUI()
    {
        int seconds = Mathf.CeilToInt(TimeRemaining);
        if (scoreText != null)
        {
            string format = !string.IsNullOrEmpty(scoreFormat)
                ? scoreFormat
                : "<size=60%><color=#E5C158>ĐIỂM SỐ</color></size>\n<b><color=#FFFFFF>{0}</color></b>\n<size=44%><color=#E5C158>MŨI TÊN: </color><color=#FFFFFF>{1}/{2}</color>  <color=#E5C158>THỜI GIAN: </color><color=#FFFFFF>{3}s</color></size>";
            scoreText.text = string.Format(format, currentScore, ArrowsRemaining, maxArrows, seconds);
        }

        if (hudScoreText != null)
        {
            hudScoreText.text = currentScore.ToString();
        }

        if (hudArrowsText != null)
        {
            hudArrowsText.text = string.Format("MŨI TÊN\n<align=\"center\">{0}/{1}</align>", ArrowsRemaining, maxArrows);
        }

        if (hudTimeText != null)
        {
            hudTimeText.text = string.Format("THỜI GIAN\n<align=\"center\">{0}s</align>", seconds);
        }
    }

    // =========================================================
    // HỆ THỐNG KỶ LỤC TOP 3 (LEADERBOARD)
    // =========================================================

    public void LoadLeaderboard()
    {
        if (PlayerPrefs.HasKey(LeaderboardPrefKey))
        {
            string json = PlayerPrefs.GetString(LeaderboardPrefKey, "");
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    leaderboardData = JsonUtility.FromJson<LeaderboardData>(json);
                    if (leaderboardData == null) leaderboardData = new LeaderboardData();
                    if (leaderboardData.records == null) leaderboardData.records = new List<LeaderboardRecord>();
                    SortLeaderboard();
                    return;
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Không thể tải bảng kỷ lục từ PlayerPrefs: " + e.Message);
                }
            }
        }

        leaderboardData = new LeaderboardData();
    }

    public void SaveLeaderboard()
    {
        if (leaderboardData == null) leaderboardData = new LeaderboardData();
        SortLeaderboard();
        string json = JsonUtility.ToJson(leaderboardData);
        PlayerPrefs.SetString(LeaderboardPrefKey, json);
        PlayerPrefs.Save();
    }

    public void ClearLeaderboard()
    {
        leaderboardData = new LeaderboardData();
        PlayerPrefs.DeleteKey(LeaderboardPrefKey);
        PlayerPrefs.Save();
        UpdateLeaderboardUI();
    }

    private void SortLeaderboard()
    {
        if (leaderboardData == null || leaderboardData.records == null) return;

        leaderboardData.records.Sort((a, b) =>
        {
            // 1. Điểm số cao hơn xếp trên
            int scoreCompare = b.score.CompareTo(a.score);
            if (scoreCompare != 0) return scoreCompare;

            // 2. Nếu bằng điểm, thời gian ít hơn xếp trên
            return a.timeUsed.CompareTo(b.timeUsed);
        });

        // Giữ lại tối đa 3 thành tích
        if (leaderboardData.records.Count > 3)
        {
            leaderboardData.records = leaderboardData.records.GetRange(0, 3);
        }
    }

    public int RecordScore(int newScore, float timeUsed)
    {
        if (leaderboardData == null) leaderboardData = new LeaderboardData();
        if (leaderboardData.records == null) leaderboardData.records = new List<LeaderboardRecord>();

        // Kiểm tra thứ hạng tiềm năng
        var candidate = new LeaderboardRecord { score = newScore, timeUsed = timeUsed };
        var testList = new List<LeaderboardRecord>(leaderboardData.records) { candidate };

        testList.Sort((a, b) =>
        {
            int sc = b.score.CompareTo(a.score);
            if (sc != 0) return sc;
            return a.timeUsed.CompareTo(b.timeUsed);
        });

        int rank = testList.IndexOf(candidate) + 1;

        if (rank <= 3)
        {
            leaderboardData.records = testList;
            if (leaderboardData.records.Count > 3)
            {
                leaderboardData.records = leaderboardData.records.GetRange(0, 3);
            }
            SaveLeaderboard();
            UpdateLeaderboardUI();
            return rank;
        }

        return 0; // Không lọt top 3
    }

    public void UpdateLeaderboardUI()
    {
        if (startLeaderboardText != null)
        {
            startLeaderboardText.text = GetFormattedLeaderboard(0);
        }

        if (resultLeaderboardText != null)
        {
            resultLeaderboardText.text = GetFormattedLeaderboard(0);
        }
    }

    public string GetFormattedLeaderboard(int highlightRank = 0)
    {
        if (leaderboardData == null || leaderboardData.records == null || leaderboardData.records.Count == 0)
        {
            return "<align=\"center\"><size=90%><color=#795548>CHƯA CÓ KỶ LỤC</color></size></align>";
        }

        var sb = new System.Text.StringBuilder();
        string[] rankNames = new string[] { "HẠNG 1", "HẠNG 2", "HẠNG 3" };
        string[] rankColors = new string[] { "#B8860B", "#546E7A", "#8D6E63" };

        for (int i = 0; i < 3; i++)
        {
            string rankBadge = string.Format("<color={0}><b>{1}</b></color>", rankColors[i], rankNames[i]);
            if (i < leaderboardData.records.Count)
            {
                var rec = leaderboardData.records[i];
                string timeStr = FormatTime(rec.timeUsed);
                bool isNew = (i + 1 == highlightRank);
                string prefix = isNew ? "<pos=48><color=#D84315><b>></b></color>" : "";
                string scoreColor = isNew ? "#C62828" : "#8B4513";
                string scoreTag = "<b>";
                string scoreEnd = "</b>";

                sb.AppendLine(string.Format("<align=\"left\">{0}<pos=68>{1}<pos=228>{2}<color={3}>{4} ĐIỂM</color>{5}<pos=378><color=#4E342E>{6}</color></align>",
                    prefix, rankBadge, scoreTag, scoreColor, rec.score, scoreEnd, timeStr));
            }
            else
            {
                sb.AppendLine(string.Format("<align=\"left\"><pos=68>{0}<pos=240><color=#8D6E63>---  CHƯA CÓ KỶ LỤC  ---</color></align>", rankBadge));
            }
        }

        return sb.ToString().TrimEnd();
    }

    public static string FormatTime(float seconds)
    {
        int totalSec = Mathf.Max(0, Mathf.RoundToInt(seconds));
        int m = totalSec / 60;
        int s = totalSec % 60;
        return string.Format("{0:00}:{1:00}", m, s);
    }
}
