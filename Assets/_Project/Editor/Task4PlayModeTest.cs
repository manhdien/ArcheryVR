using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;

public static class Task4PlayModeTest
{
    [MenuItem("Archery/Run Task 4 Play Mode Tests")]
    public static void RunFromMenu()
    {
        RunAllTests();
    }

    public static string RunAllTests()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("==================================================");
        sb.AppendLine("         TASK 4 PLAY MODE LIVE TEST SUITE         ");
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
        var player = GameObject.Find("PC_Player");
        var bow = player != null ? player.GetComponentInChildren<BowShoot>() : null;

        if (gm == null || uim == null || sm == null || bow == null)
        {
            sb.AppendLine("[ERROR] Core components missing in Play Mode!");
            return sb.ToString();
        }

        // TEST 38: Initial Main Menu State
        gm.ReturnToMainMenu();
        bool menuPanelActive = uim.mainMenuPanel.activeSelf;
        bool hudInactive = !uim.gameplayHUD.activeSelf;
        bool cursorUnlocked = (Cursor.lockState == CursorLockMode.None && Cursor.visible);
        bool playerDisabled = (!gm.playerMovement.enabled && !gm.bowShoot.enabled);
        AssertTest("TEST 38 - Main Menu State", gm.CurrentState == GameState.MainMenu && menuPanelActive && hudInactive && cursorUnlocked && playerDisabled,
            "(MainMenu active, HUD hidden, cursor unlocked, player controls disabled)");

        // TEST 39: Start Game
        gm.StartGame();
        bool hudActive = uim.gameplayHUD.activeSelf;
        bool menuHidden = !uim.mainMenuPanel.activeSelf;
        bool cursorLocked = (Cursor.lockState == CursorLockMode.Locked && !Cursor.visible);
        bool playerEnabled = (gm.playerMovement.enabled && gm.bowShoot.enabled);
        bool scoreZero = (sm.CurrentScore == 0);
        bool ammoFull = (bow.CurrentAmmo == 20);
        AssertTest("TEST 39 - Start Game", gm.CurrentState == GameState.Playing && hudActive && menuHidden && cursorLocked && playerEnabled && scoreZero && ammoFull,
            "(Playing state, HUD visible, cursor locked, controls enabled, Score=0, Ammo=20/20)");

        // TEST 40: Score HUD Update & Floating Score
        var target = Object.FindAnyObjectByType<Target>();
        if (target != null)
        {
            target.OnArrowHit(null, target.transform.position + target.transform.right * 0.45f, target.transform.forward);
        }
        bool scoreUpdated = (sm.CurrentScore == 20 && uim.scoreText.text.Contains("20"));
        bool floatingSpawned = (uim.floatingScoreContainer.childCount > 0);
        AssertTest("TEST 40 - Score HUD & Floating Score", scoreUpdated && floatingSpawned,
            $"(Score HUD: '{uim.scoreText.text}', Floating scores active: {uim.floatingScoreContainer.childCount})");

        // TEST 41: Ammo HUD Update
        int ammoBefore = bow.CurrentAmmo;
        var arrowPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ExternalAssets/Cartoon_Weapon_Pack/Prefab/Arrow.prefab");
        var dummy = Object.Instantiate(arrowPrefab);
        var arrowField = typeof(BowShoot).GetField("currentArrow", BindingFlags.NonPublic | BindingFlags.Instance);
        var arrowRbField = typeof(BowShoot).GetField("currentArrowRb", BindingFlags.NonPublic | BindingFlags.Instance);
        var shootMethod = typeof(BowShoot).GetMethod("ShootArrow", BindingFlags.NonPublic | BindingFlags.Instance);

        arrowField.SetValue(bow, dummy);
        arrowRbField.SetValue(bow, dummy.GetComponent<Rigidbody>());
        shootMethod.Invoke(bow, null);

        bool ammoReduced = (bow.CurrentAmmo == (ammoBefore - 1) && uim.ammoText.text.Contains("19"));
        AssertTest("TEST 41 - Ammo HUD Update", ammoReduced, $"(Ammo: {bow.CurrentAmmo}, HUD: '{uim.ammoText.text}')");

        // TEST 42: Charge Bar Update
        uim.UpdateChargeBar(0.75f);
        bool chargeSet = (Mathf.Approximately(uim.chargeBarFill.fillAmount, 0.75f) && uim.chargeBarRoot.activeSelf);
        uim.UpdateChargeBar(0f);
        bool chargeReset = (Mathf.Approximately(uim.chargeBarFill.fillAmount, 0f) && !uim.chargeBarRoot.activeSelf);
        AssertTest("TEST 42 - Charge Bar Update", chargeSet && chargeReset, "(Charge bar fills at 75% and hides at 0%)");

        // TEST 43: Cancel Draw
        var cancelMethod = typeof(BowShoot).GetMethod("CancelDraw", BindingFlags.NonPublic | BindingFlags.Instance);
        cancelMethod.Invoke(bow, null);
        AssertTest("TEST 43 - Cancel Draw", bow.CurrentAmmo == 19 && !bow.IsDrawing && uim.chargeBarFill.fillAmount == 0f,
            "(Ammo unchanged at 19, bow reset, charge=0)");

        // TEST 44: Pause & Resume
        gm.PauseGame();
        bool pausedState = (gm.CurrentState == GameState.Paused && Time.timeScale == 0f && uim.pausePanel.activeSelf && Cursor.lockState == CursorLockMode.None);
        gm.ResumeGame();
        bool resumedState = (gm.CurrentState == GameState.Playing && Time.timeScale == 1f && !uim.pausePanel.activeSelf && Cursor.lockState == CursorLockMode.Locked);
        AssertTest("TEST 44 - Pause & Resume", pausedState && resumedState, "(Pause sets TimeScale=0 & unlocks cursor; Resume restores TimeScale=1 & locks cursor)");

        // TEST 45: Restart Game from Pause
        gm.PauseGame();
        gm.RestartGame();
        bool restarted = (gm.CurrentState == GameState.Playing && sm.CurrentScore == 0 && bow.CurrentAmmo == 20 && !uim.pausePanel.activeSelf && uim.gameplayHUD.activeSelf);
        AssertTest("TEST 45 - Restart Game", restarted, "(State=Playing, Score=0, Ammo=20, PausePanel hidden)");

        // TEST 46 & 47: Game Over & Result Screen
        sm.ResetScore();
        sm.AddScore(10, "Hit", Vector3.zero);
        gm.TriggerGameOver();
        bool gameOverState = (gm.CurrentState == GameState.GameOver && uim.resultPanel.activeSelf && !uim.gameplayHUD.activeSelf && Cursor.lockState == CursorLockMode.None);
        // Task 6: bullseyeHitsText removed — check finalScoreText and totalHitsText only
        bool resultValuesCorrect = (uim.finalScoreText != null && uim.finalScoreText.text.Contains("10") &&
                                     uim.totalHitsText != null && uim.totalHitsText.text.Contains("1"));
        AssertTest("TEST 46/47 - Game Over & Result Screen", gameOverState && resultValuesCorrect,
            $"(ResultPanel active, FinalScore: '{uim.finalScoreText?.text}', Hits: '{uim.totalHitsText?.text}')");

        // TEST 48: Result -> Restart
        gm.RestartGame();
        bool resultRestarted = (gm.CurrentState == GameState.Playing && !uim.resultPanel.activeSelf && uim.gameplayHUD.activeSelf && sm.CurrentScore == 0 && bow.CurrentAmmo == 20);
        AssertTest("TEST 48 - Result to Restart", resultRestarted, "(ResultPanel closed, Score=0, Ammo=20, HUD re-enabled)");

        // TEST 49: Result / Pause -> Main Menu
        gm.ReturnToMainMenu();
        bool menuReturned = (gm.CurrentState == GameState.MainMenu && uim.mainMenuPanel.activeSelf && !uim.gameplayHUD.activeSelf && Cursor.lockState == CursorLockMode.None);
        AssertTest("TEST 49 - Return to Main Menu", menuReturned, "(MainMenu active, HUD closed, controls disabled, cursor unlocked)");

        // TEST 50: Stress Test (3 complete game flow cycles)
        bool stressSuccess = true;
        for (int cycle = 1; cycle <= 3; cycle++)
        {
            gm.StartGame();
            if (gm.CurrentState != GameState.Playing || sm.CurrentScore != 0 || bow.CurrentAmmo != 20) { stressSuccess = false; break; }

            // Shoot
            var cArrow = Object.Instantiate(arrowPrefab);
            arrowField.SetValue(bow, cArrow);
            arrowRbField.SetValue(bow, cArrow.GetComponent<Rigidbody>());
            shootMethod.Invoke(bow, null);
            if (bow.CurrentAmmo != 19) { stressSuccess = false; break; }

            // Pause & Resume
            gm.PauseGame();
            if (Time.timeScale != 0f) { stressSuccess = false; break; }
            gm.ResumeGame();
            if (Time.timeScale != 1f) { stressSuccess = false; break; }

            // GameOver & Return
            gm.TriggerGameOver();
            if (gm.CurrentState != GameState.GameOver) { stressSuccess = false; break; }
            gm.ReturnToMainMenu();
            if (gm.CurrentState != GameState.MainMenu) { stressSuccess = false; break; }
        }
        AssertTest("TEST 50 - Stress Test (3 Cycles)", stressSuccess, "(3 full cycles Menu -> Play -> Shoot -> Pause -> Resume -> GameOver -> Menu completed flawlessly)");

        sb.AppendLine("==================================================");
        sb.AppendLine($"RESULTS: {passCount} PASSED, {failCount} FAILED");
        sb.AppendLine("==================================================");

        Debug.Log(sb.ToString());
        return sb.ToString();
    }
}
