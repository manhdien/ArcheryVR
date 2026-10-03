using System;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public enum GameState
{
    MainMenu,
    Playing,
    Paused,
    GameOver
}

/// <summary>
/// Quản lý vòng đời và trạng thái trò chơi (MainMenu, Playing, Paused, GameOver).
/// </summary>
public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<GameManager>();
            }
            return _instance;
        }
        private set => _instance = value;
    }

    [Header("Trạng thái Game")]
    [SerializeField] private GameState currentState = GameState.MainMenu;
    public GameState CurrentState => currentState;

    /// <summary>
    /// Event phát ra khi trạng thái game chuyển đổi: (GameState newState)
    /// </summary>
    public event Action<GameState> OnGameStateChanged;

    [Header("Tham chiếu Nhân vật & Điều khiển")]
    public GameObject playerObject;
    public Transform playerSpawnPoint;
    public BowShoot bowShoot;
    public PlayerMovement playerMovement;
    public PlayerCameraController playerCameraController;

    [Header("Tọa độ hồi sinh mặc định")]
    [SerializeField] private Vector3 defaultSpawnPosition = new Vector3(15.9f, 1.0f, 17.9f);
    [SerializeField] private Quaternion defaultSpawnRotation = Quaternion.identity;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;

        FindPlayerReferences();
    }

    private void Start()
    {
        // Khởi động game ở màn hình Main Menu
        ReturnToMainMenu();
    }

    private void Update()
    {
        HandlePauseInput();
    }

    private void FindPlayerReferences()
    {
        if (playerObject == null)
        {
            playerObject = GameObject.Find("PC_Player");
        }

        if (playerObject != null)
        {
            if (bowShoot == null) bowShoot = playerObject.GetComponentInChildren<BowShoot>();
            if (playerMovement == null) playerMovement = playerObject.GetComponent<PlayerMovement>();
            if (playerCameraController == null) playerCameraController = playerObject.GetComponentInChildren<PlayerCameraController>();
        }
    }

    private void HandlePauseInput()
    {
        bool escapePressed = false;
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            escapePressed = true;
        }
#endif
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            escapePressed = true;
        }

        if (escapePressed)
        {
            if (currentState == GameState.Playing)
            {
                PauseGame();
            }
            else if (currentState == GameState.Paused)
            {
                ResumeGame();
            }
        }
    }

    /// <summary>
    /// Bắt đầu một ván chơi mới.
    /// </summary>
    public void StartGame()
    {
        Time.timeScale = 1f;
        currentState = GameState.Playing;

        // 1. Reset vị trí người chơi
        ResetPlayerPosition();

        // 2. Kích hoạt điều khiển nhân vật & khóa chuột
        SetPlayerControl(true);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // 3. Reset điểm và số lượng mũi tên
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetScore();
        }

        if (bowShoot != null)
        {
            bowShoot.ResetAmmo();
        }

        // 4. Dọn sạch các mũi tên cũ cắm trên scene
        CleanUpArrows();

        // 5. Reset vị trí các bia bay (MovingTarget)
        ResetMovingTargets();

        Debug.Log("[GameManager] StartGame: GameState -> Playing, Player controls enabled, Cursor locked.");
        OnGameStateChanged?.Invoke(currentState);
    }

    /// <summary>
    /// Tạm dừng ván chơi (Pause).
    /// </summary>
    public void PauseGame()
    {
        if (currentState != GameState.Playing) return;

        currentState = GameState.Paused;
        Time.timeScale = 0f;

        SetPlayerControl(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("[GameManager] PauseGame: GameState -> Paused, TimeScale=0, Cursor unlocked.");
        OnGameStateChanged?.Invoke(currentState);
    }

    /// <summary>
    /// Tiếp tục ván chơi từ trạng thái Pause.
    /// </summary>
    public void ResumeGame()
    {
        if (currentState != GameState.Paused) return;

        Time.timeScale = 1f;
        currentState = GameState.Playing;

        SetPlayerControl(true);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log("[GameManager] ResumeGame: GameState -> Playing, TimeScale=1, Cursor locked.");
        OnGameStateChanged?.Invoke(currentState);
    }

    /// <summary>
    /// Khởi động lại ván chơi (Restart).
    /// </summary>
    public void RestartGame()
    {
        Debug.Log("[GameManager] RestartGame requested.");
        StartGame();
    }

    /// <summary>
    /// Kết thúc ván chơi khi hết tên.
    /// </summary>
    public void TriggerGameOver()
    {
        if (currentState == GameState.GameOver) return;

        currentState = GameState.GameOver;
        Time.timeScale = 1f;

        SetPlayerControl(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log($"[GameManager] GameOver triggered! Final Score: {ScoreManager.Instance?.CurrentScore}");
        OnGameStateChanged?.Invoke(currentState);
    }

    /// <summary>
    /// Trở về giao diện Main Menu chính.
    /// </summary>
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        currentState = GameState.MainMenu;

        SetPlayerControl(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("[GameManager] ReturnToMainMenu: GameState -> MainMenu, Cursor unlocked.");
        OnGameStateChanged?.Invoke(currentState);
    }

    /// <summary>
    /// Thoát ứng dụng game.
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("[GameManager] QuitGame requested.");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /// <summary>
    /// Bật/tắt các component điều khiển nhân vật.
    /// </summary>
    public void SetPlayerControl(bool enabled)
    {
        if (playerMovement != null) playerMovement.enabled = enabled;
        if (playerCameraController != null) playerCameraController.enabled = enabled;
        if (bowShoot != null) bowShoot.enabled = enabled;
    }

    /// <summary>
    /// Đưa nhân vật về vị trí spawn ban đầu.
    /// </summary>
    public void ResetPlayerPosition()
    {
        if (playerObject == null) return;

        CharacterController cc = playerObject.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        if (playerSpawnPoint != null)
        {
            playerObject.transform.position = playerSpawnPoint.position;
            playerObject.transform.rotation = playerSpawnPoint.rotation;
        }
        else
        {
            playerObject.transform.position = defaultSpawnPosition;
            playerObject.transform.rotation = defaultSpawnRotation;
        }

        if (cc != null) cc.enabled = true;
    }

    /// <summary>
    /// Dọn sạch các mũi tên đã bắn và cắm vào scene.
    /// </summary>
    public void CleanUpArrows()
    {
        Arrow[] allArrows = FindObjectsByType<Arrow>(FindObjectsSortMode.None);
        int cleaned = 0;
        foreach (Arrow a in allArrows)
        {
            // Chỉ xóa mũi tên đã cắm hoặc đang bay độc lập trong thế giới, không xóa mũi tên đang nạp trên cung
            if (a != null && a.State != ArrowState.Nocked)
            {
                Destroy(a.gameObject);
                cleaned++;
            }
        }
        if (cleaned > 0)
        {
            Debug.Log($"[GameManager] Cleaned up {cleaned} arrows from previous round.");
        }
    }

    /// <summary>
    /// Reset tất cả bia bay về vị trí gốc (gọi khi bắt đầu ván mới).
    /// </summary>
    public void ResetMovingTargets()
    {
        MovingTarget[] movingTargets = FindObjectsByType<MovingTarget>(FindObjectsSortMode.None);
        foreach (MovingTarget mt in movingTargets)
        {
            if (mt != null) mt.ResetToOrigin();
        }
        if (movingTargets.Length > 0)
        {
            Debug.Log($"[GameManager] Reset {movingTargets.Length} moving targets.");
        }
    }
}
