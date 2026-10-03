#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Editor setup script cho Task 6:
/// 1. Gắn MovingTarget lên 7 ArcheryTarget trong ARECHERY group
/// 2. Disable NPC group (tất cả 11 NPC warriors)
/// 3. Thêm arrowsUsedText và accuracyText vào Result Panel nếu chưa có
/// 4. Wire các text mới vào UIManager
/// </summary>
public class Task6Setup : Editor
{
    [MenuItem("Archery/Setup Task 6 - Moving Targets & Gameplay")]
    public static void SetupTask6()
    {
        int changes = 0;
        Undo.SetCurrentGroupName("Task6Setup");

        // ── 1. GẮN MovingTarget lên ARECHERY children ──────────────────────────
        GameObject arechery = GameObject.Find("ARECHERY");
        if (arechery == null)
        {
            Debug.LogError("[Task6Setup] ARECHERY not found in scene!");
        }
        else
        {
            // 7 Vị trí xuất phát riêng biệt, phân bổ đều trong khu vực trường bắn (Target Movement Area)
            Vector3[] distinctPositions = new Vector3[]
            {
                new Vector3(17.5f, 1.8f, 28.0f), // Bia 0: Bên trái, tầm trung
                new Vector3(34.5f, 2.2f, 25.0f), // Bia 1: Bên phải, tầm gần
                new Vector3(26.0f, 1.6f, 31.5f), // Bia 2: Trung tâm, tầm trung
                new Vector3(37.5f, 3.0f, 35.0f), // Bia 3: Bên phải, tầm xa, bay cao
                new Vector3(16.5f, 2.0f, 33.0f), // Bia 4: Bên trái, tầm xa
                new Vector3(29.0f, 2.5f, 24.0f), // Bia 5: Trung tâm, tầm gần
                new Vector3(31.0f, 3.2f, 39.0f)  // Bia 6: Trung tâm-phải, tầm rất xa
            };

            float[] horizontalSpeeds = { 1.2f, 1.8f, 1.5f, 2.0f, 1.0f, 1.6f, 1.3f };
            float[] verticalSpeeds   = { 0.7f, 0.9f, 0.8f, 1.1f, 0.6f, 0.85f, 0.75f };
            float[] horizontalRanges = { 3.0f, 3.5f, 2.5f, 3.0f, 2.8f, 3.2f, 2.5f };
            float[] verticalRanges   = { 0.8f, 1.0f, 0.7f, 0.9f, 0.8f, 1.0f, 0.9f };

            int idx = 0;
            foreach (Transform child in arechery.transform)
            {
                MovingTarget mt = child.GetComponent<MovingTarget>();
                if (mt == null)
                {
                    mt = Undo.AddComponent<MovingTarget>(child.gameObject);
                    Debug.Log($"[Task6Setup] Added MovingTarget to {child.name}");
                    changes++;
                }

                // Thiết lập vị trí xuất phát độc lập cho từng bia
                Vector3 spawnPos = distinctPositions[Mathf.Min(idx, distinctPositions.Length - 1)];
                Undo.RecordObject(child, "Set Target Position");
                child.position = spawnPos;

                Undo.RecordObject(mt, "Configure MovingTarget");
                mt.SetOriginPosition(spawnPos);

                // Gán thông số chuyển động
                mt.horizontalSpeed  = horizontalSpeeds[Mathf.Min(idx, horizontalSpeeds.Length - 1)];
                mt.verticalSpeed    = verticalSpeeds[Mathf.Min(idx, verticalSpeeds.Length - 1)];
                mt.horizontalRange  = horizontalRanges[Mathf.Min(idx, horizontalRanges.Length - 1)];
                mt.verticalRange    = verticalRanges[Mathf.Min(idx, verticalRanges.Length - 1)];
                mt.phaseOffset      = idx * 0.9f; // phase offset khác nhau cho mỗi bia
                mt.minZ = 20f;
                mt.maxZ = 50f;
                mt.minX = 8f;
                mt.maxX = 45f;
                mt.minY = 0.5f;
                mt.maxY = 6.0f;
                mt.minPlayerDistance = 8f;

                EditorUtility.SetDirty(child.gameObject);
                EditorUtility.SetDirty(mt);
                Debug.Log($"[Task6Setup] Configured Target [{idx}] '{child.name}' at {spawnPos}");
                changes++;
                idx++;
            }
            Debug.Log($"[Task6Setup] Configured {idx} distinct moving targets in ARECHERY.");
        }

        // ── 2. DISABLE NPC GROUP ────────────────────────────────────────────────
        GameObject npcGroup = GameObject.Find("NPC");
        if (npcGroup == null)
        {
            Debug.LogWarning("[Task6Setup] NPC group not found — skipping.");
        }
        else
        {
            if (npcGroup.activeSelf)
            {
                Undo.RecordObject(npcGroup, "Disable NPC Group");
                npcGroup.SetActive(false);
                EditorUtility.SetDirty(npcGroup);
                Debug.Log("[Task6Setup] Disabled NPC group (all 11 warriors).");
                changes++;
            }
            else
            {
                Debug.Log("[Task6Setup] NPC group already disabled.");
            }
        }

        // ── 3. DISABLE PatrolPoints (không cần khi NPC tắt) ───────────────────
        string[] patrolPoints = { "PatrolPoint1", "PatrolPoint2", "PatrolPoint3", "PatrolPoint4", "PatrolPoint5", "PatrolPoint6" };
        foreach (string ppName in patrolPoints)
        {
            GameObject pp = GameObject.Find(ppName);
            if (pp != null && pp.activeSelf)
            {
                Undo.RecordObject(pp, "Disable PatrolPoint");
                pp.SetActive(false);
                EditorUtility.SetDirty(pp);
                changes++;
            }
        }
        Debug.Log("[Task6Setup] Patrol points disabled.");

        // ── 4. CẬP NHẬT RESULT PANEL UI ─────────────────────────────────────────
        UIManager uiManager = Object.FindAnyObjectByType<UIManager>();
        if (uiManager == null)
        {
            Debug.LogWarning("[Task6Setup] UIManager not found — skipping Result Panel update.");
        }
        else
        {
            // Tìm Result Panel
            GameObject resultPanel = uiManager.resultPanel;
            if (resultPanel == null)
            {
                Debug.LogWarning("[Task6Setup] resultPanel not assigned in UIManager.");
            }
            else
            {
                // Cố gắng wire arrowsUsedText và accuracyText nếu có text object với tên phù hợp
                TMP_Text[] allTexts = resultPanel.GetComponentsInChildren<TMP_Text>(true);
                foreach (var txt in allTexts)
                {
                    string n = txt.gameObject.name.ToLower();
                    if (n.Contains("arrow") && uiManager.arrowsUsedText == null)
                    {
                        Undo.RecordObject(uiManager, "Wire arrowsUsedText");
                        uiManager.arrowsUsedText = txt;
                        EditorUtility.SetDirty(uiManager);
                        Debug.Log($"[Task6Setup] Wired arrowsUsedText -> {txt.gameObject.name}");
                        changes++;
                    }
                    else if (n.Contains("accuracy") && uiManager.accuracyText == null)
                    {
                        Undo.RecordObject(uiManager, "Wire accuracyText");
                        uiManager.accuracyText = txt;
                        EditorUtility.SetDirty(uiManager);
                        Debug.Log($"[Task6Setup] Wired accuracyText -> {txt.gameObject.name}");
                        changes++;
                    }
                    // Cũng thử wire lại totalHitsText nếu nó tham chiếu bullseye cũ
                    else if ((n.Contains("hit") || n.Contains("total")) && uiManager.totalHitsText == null)
                    {
                        Undo.RecordObject(uiManager, "Wire totalHitsText");
                        uiManager.totalHitsText = txt;
                        EditorUtility.SetDirty(uiManager);
                        Debug.Log($"[Task6Setup] Wired totalHitsText -> {txt.gameObject.name}");
                        changes++;
                    }
                }

                Debug.Log($"[Task6Setup] Result Panel UIManager fields status: " +
                          $"finalScore={uiManager.finalScoreText != null} " +
                          $"totalHits={uiManager.totalHitsText != null} " +
                          $"arrowsUsed={uiManager.arrowsUsedText != null} " +
                          $"accuracy={uiManager.accuracyText != null}");
            }
        }

        // ── 5. SAVE SCENE ────────────────────────────────────────────────────────
        if (changes > 0)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            Debug.Log($"[Task6Setup] Done! {changes} changes made. Scene marked dirty — save with Ctrl+S.");
        }
        else
        {
            Debug.Log("[Task6Setup] No changes needed — everything already configured.");
        }
    }

    [MenuItem("Archery/Task6 - Verify Moving Targets")]
    public static void VerifyTask6()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("=== TASK 6 VERIFICATION ===");

        // Check MovingTargets
        var movingTargets = Object.FindObjectsByType<MovingTarget>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        sb.AppendLine($"MovingTargets: {movingTargets.Length}");
        foreach (var mt in movingTargets)
            sb.AppendLine($"  {mt.name} speed=({mt.horizontalSpeed},{mt.verticalSpeed}) range=({mt.horizontalRange},{mt.verticalRange})");

        // Check NPC
        var npcGroup = GameObject.Find("NPC");
        sb.AppendLine($"NPC group active: {(npcGroup != null ? npcGroup.activeSelf.ToString() : "NOT FOUND")}");

        // Check Target.cs on ArcheryTarget children
        var targets = Object.FindObjectsByType<Target>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        sb.AppendLine($"Target components: {targets.Length}");
        foreach (var t in targets)
            sb.AppendLine($"  {t.name} hitScore={t.hitScore} parent={t.transform.parent?.name}");

        // Check ScoreManager
        var sm = Object.FindAnyObjectByType<ScoreManager>();
        sb.AppendLine($"ScoreManager found: {sm != null}");

        // Check UIManager
        var ui = Object.FindAnyObjectByType<UIManager>();
        if (ui != null)
        {
            sb.AppendLine($"UIManager: arrowsUsedText={ui.arrowsUsedText != null} accuracyText={ui.accuracyText != null}");
        }

        Debug.Log(sb.ToString());
    }
}
#endif
