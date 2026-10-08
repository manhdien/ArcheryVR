using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Score Settings")]
    [SerializeField] private int currentScore = 0;
    [SerializeField] private int pointsPerHit = 10;

    [Header("UI Reference")]
    [SerializeField] private TMP_Text scoreText;

    [Header("Display Format")]
    [SerializeField] private string scoreFormat = "<size=65%><color=#E5C158>SCORE</color></size>\n<b><color=#FFFFFF>{0}</color></b>";

    public int CurrentScore => currentScore;

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
        if (scoreText == null)
        {
            // Tận dụng UI đã tồn tại trong scene (Score_HUD_Canvas / Score_Text)
            Camera cam = Camera.main;
            if (cam != null)
            {
                Transform existingHud = cam.transform.Find("Score_HUD_Canvas");
                if (existingHud != null)
                {
                    scoreText = existingHud.GetComponentInChildren<TMP_Text>(true);
                }
            }

            if (scoreText == null)
            {
                scoreText = FindAnyObjectByType<TMP_Text>();
            }
        }

        if (scoreText != null)
        {
            Canvas canvas = scoreText.GetComponentInParent<Canvas>();
            if (canvas != null && canvas.worldCamera == null)
            {
                canvas.worldCamera = Camera.main;
            }
        }

        UpdateScoreUI();
    }

    public void AddScore(int points = -1)
    {
        if (points < 0) points = pointsPerHit;
        currentScore += points;
        UpdateScoreUI();
    }

    public void SetScoreText(TMP_Text text)
    {
        scoreText = text;
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            string format = !string.IsNullOrEmpty(scoreFormat)
                ? scoreFormat
                : "<size=65%><color=#E5C158>SCORE</color></size>\n<b><color=#FFFFFF>{0}</color></b>";
            scoreText.text = string.Format(format, currentScore);
        }
    }
}
