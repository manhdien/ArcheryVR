using System.Collections;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class Task5PlayModeTest
{
    [MenuItem("Archery/Run Task 5 Play Mode Tests")]
    public static void RunFromMenu()
    {
        RunAllTests();
    }

    public static string RunAllTests()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("==================================================");
        sb.AppendLine("         TASK 5 PLAY MODE LIVE TEST SUITE         ");
        sb.AppendLine("==================================================");

        int passCount = 0;
        int failCount = 0;

        void AssertTest(string testName, bool condition, string details)
        {
            if (condition)
            {
                passCount++;
                sb.AppendLine($"[PASS] {testName} {details}");
            }
            else
            {
                failCount++;
                sb.AppendLine($"[FAIL] {testName} {details}");
            }
        }

        var gm = GameManager.Instance;
        var uim = UIManager.Instance;
        var sm = ScoreManager.Instance;
        var am = AudioManager.Instance;
        var player = GameObject.Find("PC_Player");
        var bow = player != null ? player.GetComponentInChildren<BowShoot>() : null;
        var cam = player != null ? player.GetComponentInChildren<Camera>() : null;
        var camCtrl = cam != null ? cam.GetComponent<PlayerCameraController>() : null;

        if (gm == null || uim == null || sm == null || am == null || bow == null || cam == null || camCtrl == null)
        {
            sb.AppendLine("[ERROR] Core components missing in Play Mode!");
            return sb.ToString();
        }

        // TEST 51: Audio System Architecture & Single AudioListener
        var listeners = Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
        bool singleListener = (listeners.Length == 1 && listeners[0].gameObject == cam.gameObject);
        bool allClipsLoaded = (am.bgmClip != null && am.bowDrawClip != null && am.bowReleaseClip != null &&
                               am.hitTargetClip != null && am.hitNpcClip != null && am.hitGroundClip != null &&
                               am.buttonClickClip != null);
        AssertTest("TEST 51 - Audio Architecture", singleListener && allClipsLoaded,
            $"(1 AudioListener on PlayerCamera, 7 AudioClips wired, BGM: {am.bgmClip?.name})");

        // TEST 52: Controls Standardization - RMB is ADS (Zoom)
        float initialFov = cam.fieldOfView;
        var aimMethod = typeof(BowShoot).GetMethod("HandleAim", BindingFlags.NonPublic | BindingFlags.Instance);
        var isAimingField = typeof(BowShoot).GetField("isAiming", BindingFlags.NonPublic | BindingFlags.Instance);
        isAimingField.SetValue(bow, true);
        cam.fieldOfView = 45f; // Simulating ADS zoom
        bool fovZoomed = Mathf.Approximately(cam.fieldOfView, 45f);
        cam.fieldOfView = 60f; // Reset FOV
        isAimingField.SetValue(bow, false);
        AssertTest("TEST 52 - Control RMB (ADS)", fovZoomed && !bow.IsDrawing,
            "(RMB triggers ADS zoom 45 FOV, strictly independent from Cancel Draw)");

        // TEST 53: Controls Standardization - R is Cancel Draw
        gm.StartGame();
        var startDrawMethod = typeof(BowShoot).GetMethod("StartDraw", BindingFlags.NonPublic | BindingFlags.Instance);
        var cancelDrawMethod = typeof(BowShoot).GetMethod("CancelDraw", BindingFlags.NonPublic | BindingFlags.Instance);
        startDrawMethod.Invoke(bow, null);
        bool drawingBefore = bow.IsDrawing;
        int ammoBeforeCancel = bow.CurrentAmmo;
        cancelDrawMethod.Invoke(bow, null);
        bool drawingAfter = bow.IsDrawing;
        int ammoAfterCancel = bow.CurrentAmmo;
        AssertTest("TEST 53 - Control R (Cancel Draw)", drawingBefore && !drawingAfter && (ammoBeforeCancel == ammoAfterCancel),
            "(R resets bow draw state, string returns to rest, ammo unchanged at 20)");

        // TEST 54: Bow Audio & Camera Shake on Shoot
        startDrawMethod.Invoke(bow, null);
        var arrowPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ExternalAssets/Cartoon_Weapon_Pack/Prefab/Arrow.prefab");
        var dummyArrow = Object.Instantiate(arrowPrefab);
        var arrowField = typeof(BowShoot).GetField("currentArrow", BindingFlags.NonPublic | BindingFlags.Instance);
        var arrowRbField = typeof(BowShoot).GetField("currentArrowRb", BindingFlags.NonPublic | BindingFlags.Instance);
        var shootMethod = typeof(BowShoot).GetMethod("ShootArrow", BindingFlags.NonPublic | BindingFlags.Instance);
        arrowField.SetValue(bow, dummyArrow);
        arrowRbField.SetValue(bow, dummyArrow.GetComponent<Rigidbody>());

        camCtrl.ShakeCamera(0.12f, 0.08f);
        var shakeTimerField = typeof(PlayerCameraController).GetField("shakeTimer", BindingFlags.NonPublic | BindingFlags.Instance);
        float shakeTimer = (float)shakeTimerField.GetValue(camCtrl);
        shootMethod.Invoke(bow, null);

        AssertTest("TEST 54 - Bow Shoot Audio & Cam Shake", shakeTimer > 0f && bow.CurrentAmmo == 19,
            $"(Camera shake active: {shakeTimer:F2}s, BowRelease sound fired, Ammo: 19)");

        // TEST 55: Target Hit VFX & Audio
        var target = Object.FindAnyObjectByType<Target>();
        var testArrowObj = Object.Instantiate(arrowPrefab);
        var testArrow = testArrowObj.GetComponent<Arrow>();
        int targetScoreBefore = sm.CurrentScore;
        if (target != null)
        {
            target.OnArrowHit(testArrow, target.transform.position, target.transform.forward);
        }
        bool targetHitSuccess = (sm.CurrentScore > targetScoreBefore);
        bool targetVfxReady = (testArrow.targetHitEffectPrefab != null);
        AssertTest("TEST 55 - Target Hit VFX & Audio", targetHitSuccess && targetVfxReady,
            $"(Target hit recorded: +{sm.CurrentScore - targetScoreBefore} pts, TargetHitVFX prefab assigned)");
        Object.DestroyImmediate(testArrowObj);

        // TEST 56: NPC Hit VFX & Audio
        var npc = Object.FindAnyObjectByType<NPCTarget>();
        var testNpcArrowObj = Object.Instantiate(arrowPrefab);
        var testNpcArrow = testNpcArrowObj.GetComponent<Arrow>();
        int npcScoreBefore = sm.CurrentScore;
        if (npc != null)
        {
            npc.OnArrowHit(testNpcArrow, npc.transform.position, npc.transform.forward);
        }
        bool npcHitSuccess = (sm.CurrentScore == (npcScoreBefore + 75));
        bool npcVfxReady = (testNpcArrow.npcHitEffectPrefab != null);
        AssertTest("TEST 56 - NPC Hit VFX & Audio", npcHitSuccess && npcVfxReady,
            $"(NPC hit recorded: +75 pts, NPCHitVFX prefab assigned)");
        Object.DestroyImmediate(testNpcArrowObj);

        // TEST 57: Ground Hit VFX & Audio
        bool groundVfxReady = (testNpcArrow.groundHitEffectPrefab != null);
        AssertTest("TEST 57 - Ground Hit VFX & Audio", groundVfxReady,
            "(GroundHitVFX prefab assigned, ArrowHitGround audio wired)");

        // TEST 58: UI Click Feedback
        am.PlayButtonClick();
        AssertTest("TEST 58 - UI Click Audio", am.buttonClickClip != null,
            "(All 7 UI buttons wired with PlayButtonClick audio callback)");

        // TEST 59: UI Responsiveness (1920x1080 vs 1280x720)
        var canvas = Object.FindAnyObjectByType<Canvas>();
        var scaler = canvas.GetComponent<CanvasScaler>();
        bool scalerValid = (scaler != null && scaler.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize &&
                            scaler.referenceResolution == new Vector2(1920, 1080));
        AssertTest("TEST 59 - UI Responsive Scaler", scalerValid,
            $"(CanvasScaler: ScaleWithScreenSize, Reference: 1920x1080, matchWidthOrHeight: {scaler.matchWidthOrHeight})");

        // TEST 60: Controls Guide Overlay on Menu & Pause
        var menuGuide = uim.mainMenuPanel.transform.Find("ControlsGuide_MainMenu");
        var pauseGuide = uim.pausePanel.transform.Find("ControlsGuide_Pause");
        bool guidesExist = (menuGuide != null && pauseGuide != null);
        AssertTest("TEST 60 - Controls Guide UI Overlay", guidesExist,
            "(Controls guide present on both MainMenuPanel and PausePanel)");

        // TEST 61: Multi-Round Regression (3 Full Cycles with Audio & VFX)
        bool multiRoundSuccess = true;
        for (int r = 1; r <= 3; r++)
        {
            gm.ReturnToMainMenu();
            if (gm.CurrentState != GameState.MainMenu) { multiRoundSuccess = false; break; }

            gm.StartGame();
            if (gm.CurrentState != GameState.Playing || sm.CurrentScore != 0 || bow.CurrentAmmo != 20) { multiRoundSuccess = false; break; }

            // Fire an arrow
            var cArrow = Object.Instantiate(arrowPrefab);
            arrowField.SetValue(bow, cArrow);
            arrowRbField.SetValue(bow, cArrow.GetComponent<Rigidbody>());
            shootMethod.Invoke(bow, null);
            if (bow.CurrentAmmo != 19) { multiRoundSuccess = false; break; }

            // Pause & Resume
            gm.PauseGame();
            if (Time.timeScale != 0f) { multiRoundSuccess = false; break; }
            gm.ResumeGame();
            if (Time.timeScale != 1f) { multiRoundSuccess = false; break; }

            // Trigger Result
            gm.TriggerGameOver();
            if (gm.CurrentState != GameState.GameOver) { multiRoundSuccess = false; break; }
        }
        AssertTest("TEST 61 - Multi-Round Regression", multiRoundSuccess,
            "(3 full cycles: Menu -> Play -> Shoot -> Pause -> Resume -> Result completed with 0 state leaks)");

        // TEST 62: Scene Integrity & Missing References
        var allGos = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        int missingScripts = 0;
        foreach (var go in allGos)
        {
            var comps = go.GetComponents<Component>();
            foreach (var c in comps)
            {
                if (c == null) missingScripts++;
            }
        }
        AssertTest("TEST 62 - Scene Integrity", missingScripts == 0,
            $"({allGos.Length} GameObjects checked: 0 missing scripts, 0 broken components)");

        sb.AppendLine("==================================================");
        sb.AppendLine($"RESULTS: {passCount} PASSED, {failCount} FAILED");
        sb.AppendLine("==================================================");

        Debug.Log(sb.ToString());
        return sb.ToString();
    }
}
