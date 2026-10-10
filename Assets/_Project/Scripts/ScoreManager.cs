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

    [Header("UI Reference")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private GameObject scorePanel;
    [SerializeField] private GameObject startPanel;
    [SerializeField] private UnityEngine.UI.Button startButton;
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TMP_Text resultSummaryText;
    [SerializeField] private UnityEngine.UI.Button restartButton;

    [Header("Display Format")]
    [SerializeField] private string scoreFormat = "<size=60%><color=#E5C158>SCORE</color></size>\n<b><color=#FFFFFF>{0}</color></b>\n<size=44%><color=#E5C158>ARROWS: </color><color=#FFFFFF>{1}/{2}</color>  <color=#E5C158>TIME: </color><color=#FFFFFF>{3}s</color></size>";

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

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        InitializeUIReferences();
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
        // 2. Hỗ trợ phím tắt Space/Enter để bắt đầu game nhanh khi ở StartMenu
        else if (state == GameState.StartMenu)
        {
            if (UnityEngine.InputSystem.Keyboard.current != null &&
                (UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame ||
                 UnityEngine.InputSystem.Keyboard.current.enterKey.wasPressedThisFrame))
            {
                StartGame();
            }
        }
        // 3. Hỗ trợ phím tắt R hoặc Space để chơi lại nhanh khi GameOver (thuận tiện cho PC/Editor test)
        else if (state == GameState.GameOver)
        {
            if (UnityEngine.InputSystem.Keyboard.current != null &&
                (UnityEngine.InputSystem.Keyboard.current.rKey.wasPressedThisFrame ||
                 UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame))
            {
                RestartGame();
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
            // Tự động tìm Start_Panel nếu chưa được gán trên Inspector
            if (startPanel == null)
            {
                Transform sp = hudCanvas.Find("Start_Panel");
                if (sp != null)
                {
                    startPanel = sp.gameObject;
                    if (startButton == null)
                    {
                        startButton = sp.Find("Start_Button")?.GetComponent<UnityEngine.UI.Button>();
                    }
                }
            }

            // Tự động tìm Score_Panel nếu chưa được gán trên Inspector
            if (scorePanel == null)
            {
                Transform scp = hudCanvas.Find("Score_Panel");
                if (scp != null)
                {
                    scorePanel = scp.gameObject;
                }
            }

            // Tự động tìm Result_Panel nếu chưa được gán trên Inspector
            if (resultPanel == null)
            {
                Transform res = hudCanvas.Find("Result_Panel");
                if (res != null)
                {
                    resultPanel = res.gameObject;
                    if (resultSummaryText == null)
                    {
                        resultSummaryText = res.Find("Summary_Text")?.GetComponent<TMP_Text>();
                    }
                    if (restartButton == null)
                    {
                        restartButton = res.Find("Restart_Button")?.GetComponent<UnityEngine.UI.Button>();
                    }
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

        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(RestartGame);
            restartButton.onClick.AddListener(RestartGame);
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
        // Bảo vệ: Nếu ván đã kết thúc, chưa cầm cung hoặc hết thời gian, ngăn tuyệt đối việc cộng điểm (kể cả va chạm đến muộn)
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
        lastDisplayedSeconds = Mathf.CeilToInt(roundDuration);
        lastGameOverReason = GameOverReason.None;
        state = GameState.Playing;

        UpdatePanelVisibility();
        UpdateScoreUI();
    }

    private void TriggerGameOver(GameOverReason reason = GameOverReason.ArrowsExhausted)
    {
        // Chống kết thúc lần thứ hai nếu đã kết thúc
        if (state != GameState.Playing)
            return;

        state = GameState.GameOver;
        isTimerRunning = false;
        lastGameOverReason = reason;

        UpdatePanelVisibility();

        if (resultSummaryText != null)
        {
            string reasonStr = (reason == GameOverReason.TimeExpired) ? "HẾT THỜI GIAN" : "HẾT MŨI TÊN";
            string reasonColor = (reason == GameOverReason.TimeExpired) ? "#FF7675" : "#E5C158";

            resultSummaryText.text = string.Format(
                "<size=75%><color=#A0AAB8>TỔNG ĐIỂM</color></size>\n<b><size=130%><color=#FFFFFF>{0}</color></size></b>\n<size=70%><color=#A0AAB8>MŨI TÊN: </color><color=#E5C158>{1}/{2}</color>  <color=#A0AAB8>•</color>  <color={3}>{4}</color></size>",
                currentScore, arrowsShot, maxArrows, reasonColor, reasonStr
            );
        }

        UpdateScoreUI();
    }

    public void RestartGame()
    {
        currentScore = 0;
        arrowsShot = 0;
        timeRemaining = roundDuration;
        isTimerRunning = false; // Trở về trạng thái chờ cầm cung ở ván mới
        lastDisplayedSeconds = Mathf.CeilToInt(roundDuration);
        lastGameOverReason = GameOverReason.None;
        state = GameState.Playing;

        UpdatePanelVisibility();

        // Dọn sạch các mũi tên cũ đã cắm trong scene để không ảnh hưởng ván mới
        var existingArrows = FindObjectsByType<ArrowStick>(FindObjectsInactive.Exclude);
        foreach (var arrow in existingArrows)
        {
            if (arrow != null)
            {
                Destroy(arrow.gameObject);
            }
        }

        UpdateScoreUI();
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
            startPanel.SetActive(state == GameState.StartMenu);
        }

        if (resultPanel != null)
        {
            resultPanel.SetActive(state == GameState.GameOver);
        }

        if (scorePanel != null)
        {
            // Khôi phục HUD bảng điểm cũ khi vào ván chơi hoặc kết thúc ván
            scorePanel.SetActive(state != GameState.StartMenu);
        }
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            int seconds = Mathf.CeilToInt(TimeRemaining);
            string format = !string.IsNullOrEmpty(scoreFormat)
                ? scoreFormat
                : "<size=60%><color=#E5C158>SCORE</color></size>\n<b><color=#FFFFFF>{0}</color></b>\n<size=44%><color=#E5C158>ARROWS: </color><color=#FFFFFF>{1}/{2}</color>  <color=#E5C158>TIME: </color><color=#FFFFFF>{3}s</color></size>";
            scoreText.text = string.Format(format, currentScore, ArrowsRemaining, maxArrows, seconds);
        }
    }
}
