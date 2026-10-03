#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Play Mode Tests cho Task 6 — 20 test cases kiểm tra gameplay mới:
/// Moving Targets, Flat Scoring (+10), Ammo (20), Boundary, Accuracy, Result Screen.
/// </summary>
public class Task6PlayModeTest : Editor
{
    private static int testsPassed = 0;
    private static int testsFailed = 0;

    private static System.Text.StringBuilder currentSb;

    [MenuItem("Archery/Run Task 6 Play Mode Tests")]
    public static string RunTask6Tests()
    {
        currentSb = new System.Text.StringBuilder();
        testsPassed = 0;
        testsFailed = 0;

        currentSb.AppendLine("=== TASK 6 PLAY MODE TESTS (20 Tests) ===");

        // --- System References ---
        GameManager gm = GameObject.FindAnyObjectByType<GameManager>();
        ScoreManager sm = GameObject.FindAnyObjectByType<ScoreManager>();
        UIManager uim = GameObject.FindAnyObjectByType<UIManager>();
        PlayerMovement pm = GameObject.FindAnyObjectByType<PlayerMovement>();
        BowShoot bow = GameObject.FindAnyObjectByType<BowShoot>();
        AudioManager am = GameObject.FindAnyObjectByType<AudioManager>();

        if (gm != null && uim != null)
        {
            // Đảm bảo events được subscribe trong môi trường test
            gm.OnGameStateChanged -= uim.HandleGameStateChanged;
            gm.OnGameStateChanged += uim.HandleGameStateChanged;
        }

        // ── SECTION 1: Moving Targets ─────────────────────────────────────────
        var movingTargets = GameObject.FindObjectsByType<MovingTarget>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        AssertTest("TEST T601 - 7 MovingTargets present", movingTargets.Length == 7,
            $"Found {movingTargets.Length} (expected 7)");

        bool allHaveDifferentPhase = true;
        for (int i = 0; i < movingTargets.Length - 1; i++)
            for (int j = i + 1; j < movingTargets.Length; j++)
                if (Mathf.Approximately(movingTargets[i].phaseOffset, movingTargets[j].phaseOffset))
                    allHaveDifferentPhase = false;
        AssertTest("TEST T602 - All targets have different phase offsets", allHaveDifferentPhase, "");

        bool allTargetsActive = true;
        foreach (var mt in movingTargets)
            if (!mt.gameObject.activeInHierarchy) allTargetsActive = false;
        AssertTest("TEST T603 - All MovingTargets active in hierarchy", allTargetsActive, "");

        // ── SECTION 2: NPC Disabled ───────────────────────────────────────────
        var npcs = GameObject.FindObjectsByType<NPCTarget>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        AssertTest("TEST T604 - No active NPCTargets in scene", npcs.Length == 0,
            $"Found {npcs.Length} active NPCTargets (expected 0)");

        // ── SECTION 3: Target Components ──────────────────────────────────────
        var targets = GameObject.FindObjectsByType<Target>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        AssertTest("TEST T605 - 7 Target components active", targets.Length == 7,
            $"Found {targets.Length} (expected 7)");

        bool allFlat10 = true;
        foreach (var t in targets)
            if (t.hitScore != 10) allFlat10 = false;
        AssertTest("TEST T606 - All targets have hitScore=10", allFlat10, "");

        // ── SECTION 4: Scoring Logic ──────────────────────────────────────────
        if (sm != null)
        {
            sm.ResetScore();
            AssertTest("TEST T607 - ScoreManager resets to 0", sm.CurrentScore == 0 && sm.TotalHits == 0 && sm.TotalArrowsUsed == 0,
                $"Score={sm.CurrentScore} Hits={sm.TotalHits} Arrows={sm.TotalArrowsUsed}");

            sm.RecordArrowFired();
            sm.AddScore(10, "Hit", Vector3.zero);
            AssertTest("TEST T608 - AddScore(10) gives correct result", sm.CurrentScore == 10 && sm.TotalHits == 1,
                $"Score={sm.CurrentScore} Hits={sm.TotalHits}");

            AssertTest("TEST T609 - Accuracy calculated correctly (1 hit / 1 shot = 100%)", Mathf.Approximately(sm.Accuracy, 100f),
                $"Accuracy={sm.Accuracy}% (expected 100%)");

            sm.RecordArrowFired(); // miss
            sm.RecordArrowFired();
            sm.AddScore(10, "Hit", Vector3.zero); // 2 hits / 3 shots = 66.7%
            AssertTest("TEST T610 - Accuracy with miss = 66.7%", Mathf.Abs(sm.Accuracy - 66.667f) < 0.1f,
                $"Accuracy={sm.Accuracy}% (expected ~66.7%)");

            // Max score 20 arrows * 10 = 200
            sm.ResetScore();
            for (int i = 0; i < 20; i++) sm.AddScore(10, "Hit", Vector3.zero);
            AssertTest("TEST T611 - Perfect score = 200 (20 hits x 10)", sm.CurrentScore == 200,
                $"Score={sm.CurrentScore} (expected 200)");

            sm.ResetScore();
        }
        else
        {
            AssertTest("TEST T607-T611 - ScoreManager", false, "ScoreManager not found!");
        }

        // ── SECTION 5: Ammo System ───────────────────────────────────────────
        if (bow != null)
        {
            AssertTest("TEST T612 - maxAmmo = 20", bow.MaxAmmo == 20, $"maxAmmo={bow.MaxAmmo}");
            bow.ResetAmmo();
            AssertTest("TEST T613 - ResetAmmo restores to 20", bow.CurrentAmmo == 20, $"currentAmmo={bow.CurrentAmmo}");
        }
        else
        {
            AssertTest("TEST T612-T613 - BowShoot ammo", false, "BowShoot not found!");
        }

        // ── SECTION 6: Player Shooting Area Boundary ─────────────────────────
        if (pm != null)
        {
            AssertTest("TEST T614 - Shooting area boundary enabled", pm.enableShootingAreaBoundary,
                "enableShootingAreaBoundary=false");
            AssertTest("TEST T615 - maxZ boundary set (< 30)", pm.maxZ < 30f && pm.maxZ > 0f,
                $"maxZ={pm.maxZ}");
        }
        else
        {
            AssertTest("TEST T614-T615 - PlayerMovement boundary", false, "PlayerMovement not found!");
        }

        // ── SECTION 7: UIManager Result Screen ───────────────────────────────
        if (uim != null)
        {
            AssertTest("TEST T616 - arrowsUsedText wired", uim.arrowsUsedText != null, "arrowsUsedText is null");
            AssertTest("TEST T617 - accuracyText wired", uim.accuracyText != null, "accuracyText is null");

            // Simulate game over and check result screen
            if (sm != null && gm != null)
            {
                sm.ResetScore();
                if (bow != null) bow.ResetAmmo();
                gm.StartGame();
                sm.RecordArrowFired();
                sm.AddScore(10, "Hit", Vector3.zero);
                gm.TriggerGameOver();

                bool resultShowing = uim.resultPanel != null && uim.resultPanel.activeSelf;
                bool scoreCorrect = uim.finalScoreText != null && uim.finalScoreText.text.Contains("10");
                bool hitsCorrect = uim.totalHitsText != null && uim.totalHitsText.text.Contains("1");
                bool arrowsCorrect = uim.arrowsUsedText != null && uim.arrowsUsedText.text.Contains("1");
                bool accCorrect = uim.accuracyText != null && uim.accuracyText.text.Contains("100");

                AssertTest("TEST T618 - Result screen shows correct data",
                    resultShowing && scoreCorrect && hitsCorrect && arrowsCorrect && accCorrect,
                    $"Result={resultShowing} Score={uim.finalScoreText?.text} Hits={uim.totalHitsText?.text} Arrows={uim.arrowsUsedText?.text} Acc={uim.accuracyText?.text}");

                // Restart test
                gm.RestartGame();
                bool resetOK = sm.CurrentScore == 0 && sm.TotalHits == 0 && sm.TotalArrowsUsed == 0;
                AssertTest("TEST T619 - Restart resets all stats", resetOK,
                    $"Score={sm.CurrentScore} Hits={sm.TotalHits} Arrows={sm.TotalArrowsUsed}");

                // Return to main menu
                gm.ReturnToMainMenu();
            }
        }
        else
        {
            AssertTest("TEST T616-T619 - UIManager", false, "UIManager not found!");
        }

        // ── SECTION 8: Final Systems Check ───────────────────────────────────
        AssertTest("TEST T620 - GameManager singleton present", gm != null, "GameManager not found");

        // Summary
        string summary = $"=== TASK 6 TEST RESULTS: {testsPassed}/{testsPassed + testsFailed} PASSED ===";
        currentSb.AppendLine(summary);
        Debug.Log(summary);
        if (testsFailed == 0)
        {
            string passMsg = "<color=green>[TASK 6] ALL 20 TESTS PASSED!</color>";
            currentSb.AppendLine(passMsg);
            Debug.Log(passMsg);
        }
        else
        {
            string failMsg = $"<color=red>[TASK 6] {testsFailed} TESTS FAILED!</color>";
            currentSb.AppendLine(failMsg);
            Debug.LogWarning(failMsg);
        }
        return currentSb.ToString();
    }

    private static void AssertTest(string testName, bool condition, string detail = "")
    {
        if (condition)
        {
            testsPassed++;
            currentSb?.AppendLine($"[PASS] {testName}");
            Debug.Log($"<color=green>[PASS]</color> {testName}");
        }
        else
        {
            testsFailed++;
            currentSb?.AppendLine($"[FAIL] {testName} | {detail}");
            Debug.LogError($"[FAIL] {testName} | {detail}");
        }
    }
}
#endif
