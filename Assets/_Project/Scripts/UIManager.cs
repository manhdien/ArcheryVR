using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Quản lý toàn bộ giao diện Canvas UI: Main Menu, Gameplay HUD, Pause Menu và Result Screen.
/// Task 6: Result screen hiển thị Final Score, Targets Hit, Arrows Used, Accuracy.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("4 Panel Giao diện Chính")]
    public GameObject mainMenuPanel;
    public GameObject gameplayHUD;
    public GameObject pausePanel;
    public GameObject resultPanel;

    [Header("Thành phần Gameplay HUD")]
    public TMP_Text scoreText;
    public TMP_Text ammoText;
    public GameObject crosshair;
    public Image chargeBarFill;
    public GameObject chargeBarRoot;
    public GameObject hitMarker;
    public RectTransform floatingScoreContainer;
    public GameObject floatingScorePrefab;

    [Header("Thành phần Result Screen")]
    public TMP_Text finalScoreText;
    public TMP_Text totalHitsText;
    public TMP_Text arrowsUsedText;
    public TMP_Text accuracyText;

    [Header("Buttons")]
    public Button startButton;
    public Button quitButton;
    public Button resumeButton;
    public Button pauseRestartButton;
    public Button pauseMainMenuButton;
    public Button resultRestartButton;
    public Button resultMainMenuButton;

    [Header("Tham chiếu Hệ thống")]
    public BowShoot bowShoot;

    private Coroutine hitMarkerCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        FindBowShoot();
        BindButtons();
    }

    private void Start()
    {
        // Khởi tạo hiển thị ban đầu
        if (ScoreManager.Instance != null)
        {
            UpdateScoreText(ScoreManager.Instance.CurrentScore);
        }
        else
        {
            UpdateScoreText(0);
        }

        if (bowShoot != null)
        {
            UpdateAmmoText(bowShoot.CurrentAmmo, bowShoot.MaxAmmo);
        }
        else
        {
            UpdateAmmoText(20, 20);
        }

        UpdateChargeBar(0f);
        if (hitMarker != null) hitMarker.SetActive(false);
    }

    private void OnEnable()
    {
        SubscribeEvents();
    }

    private void OnDisable()
    {
        UnsubscribeEvents();
    }

    private void FindBowShoot()
    {
        if (bowShoot == null)
        {
            GameObject player = GameObject.Find("PC_Player");
            if (player != null)
            {
                bowShoot = player.GetComponentInChildren<BowShoot>();
            }
        }
    }

    private void BindButtons()
    {
        if (startButton != null)
            startButton.onClick.AddListener(() => { AudioManager.Instance?.PlayButtonClick(); GameManager.Instance?.StartGame(); });

        if (quitButton != null)
            quitButton.onClick.AddListener(() => { AudioManager.Instance?.PlayButtonClick(); GameManager.Instance?.QuitGame(); });

        if (resumeButton != null)
            resumeButton.onClick.AddListener(() => { AudioManager.Instance?.PlayButtonClick(); GameManager.Instance?.ResumeGame(); });

        if (pauseRestartButton != null)
            pauseRestartButton.onClick.AddListener(() => { AudioManager.Instance?.PlayButtonClick(); GameManager.Instance?.RestartGame(); });

        if (pauseMainMenuButton != null)
            pauseMainMenuButton.onClick.AddListener(() => { AudioManager.Instance?.PlayButtonClick(); GameManager.Instance?.ReturnToMainMenu(); });

        if (resultRestartButton != null)
            resultRestartButton.onClick.AddListener(() => { AudioManager.Instance?.PlayButtonClick(); GameManager.Instance?.RestartGame(); });

        if (resultMainMenuButton != null)
            resultMainMenuButton.onClick.AddListener(() => { AudioManager.Instance?.PlayButtonClick(); GameManager.Instance?.ReturnToMainMenu(); });
    }

    private void SubscribeEvents()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged += UpdateScoreText;
            ScoreManager.Instance.OnTargetHit += HandleTargetHit;
        }

        FindBowShoot();
        if (bowShoot != null)
        {
            bowShoot.OnAmmoChanged += UpdateAmmoText;
            bowShoot.OnChargeChanged += UpdateChargeBar;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
        }
    }

    private void UnsubscribeEvents()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged -= UpdateScoreText;
            ScoreManager.Instance.OnTargetHit -= HandleTargetHit;
        }

        if (bowShoot != null)
        {
            bowShoot.OnAmmoChanged -= UpdateAmmoText;
            bowShoot.OnChargeChanged -= UpdateChargeBar;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
        }
    }

    public void UpdateScoreText(int newScore)
    {
        if (scoreText != null)
        {
            scoreText.text = $"SCORE: {newScore}";
        }
    }

    public void UpdateAmmoText(int current, int max)
    {
        if (ammoText != null)
        {
            ammoText.text = $"ARROWS: {current} / {max}";
        }
    }

    public void UpdateChargeBar(float chargeNormalized)
    {
        if (chargeBarFill != null)
        {
            chargeBarFill.fillAmount = chargeNormalized;
        }

        if (chargeBarRoot != null)
        {
            chargeBarRoot.SetActive(chargeNormalized > 0.01f);
        }
    }

    public void HandleTargetHit(int score, string hitType, Vector3 worldHitPoint)
    {
        // 1. Hiển thị Hit Marker
        ShowHitMarker();

        // 2. Hiển thị Floating Score Text
        SpawnFloatingScore(score, hitType, worldHitPoint);
    }

    public void ShowHitMarker()
    {
        if (hitMarker == null) return;

        if (hitMarkerCoroutine != null)
        {
            StopCoroutine(hitMarkerCoroutine);
        }
        hitMarkerCoroutine = StartCoroutine(HitMarkerRoutine());
    }

    private IEnumerator HitMarkerRoutine()
    {
        hitMarker.SetActive(true);
        yield return new WaitForSecondsRealtime(0.2f);
        hitMarker.SetActive(false);
    }

    private void SpawnFloatingScore(int score, string hitType, Vector3 worldHitPoint)
    {
        if (floatingScorePrefab == null || floatingScoreContainer == null) return;

        GameObject popupObj = Instantiate(floatingScorePrefab, floatingScoreContainer);
        FloatingScore fs = popupObj.GetComponent<FloatingScore>();
        if (fs != null)
        {
            fs.Init(score, hitType, worldHitPoint);
        }
    }

    public void HandleGameStateChanged(GameState newState)
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(newState == GameState.MainMenu);
        if (gameplayHUD != null) gameplayHUD.SetActive(newState == GameState.Playing);
        if (pausePanel != null) pausePanel.SetActive(newState == GameState.Paused);
        if (resultPanel != null) resultPanel.SetActive(newState == GameState.GameOver);

        if (newState == GameState.GameOver)
        {
            UpdateResultScreen();
        }
    }

    public void UpdateResultScreen()
    {
        if (ScoreManager.Instance != null)
        {
            if (finalScoreText != null)
                finalScoreText.text = $"FINAL SCORE: {ScoreManager.Instance.CurrentScore}";

            if (totalHitsText != null)
                totalHitsText.text = $"TARGETS HIT: {ScoreManager.Instance.TotalHits}";

            if (arrowsUsedText != null)
                arrowsUsedText.text = $"ARROWS USED: {ScoreManager.Instance.TotalArrowsUsed}";

            if (accuracyText != null)
                accuracyText.text = $"ACCURACY: {ScoreManager.Instance.Accuracy:F1}%";
        }
    }
}
