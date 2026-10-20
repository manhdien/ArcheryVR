using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

public static class UIDesigner
{
    private const string SpriteDir = "Assets/_Project/UI/Sprites/";
    private const string FontPath = "Assets/_Project/Fonts/SegoeUI-Bold SDF.asset";

    [MenuItem("Tools/ArcheryVR/Apply Cartoon UI Redesign")]
    public static void ApplyRedesign()
    {
        var cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("Main Camera not found in scene!");
            return;
        }

        var canvasObj = cam.transform.Find("Score_HUD_Canvas")?.gameObject;
        if (canvasObj == null)
        {
            Debug.LogError("Score_HUD_Canvas not found under Main Camera!");
            return;
        }

        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        if (font == null)
        {
            Debug.LogError("Font asset not found at " + FontPath);
            return;
        }

        var woodBoardSpr = LoadSprite("wood_board_frame.png");
        var woodCardSpr = LoadSprite("wood_card_inner.png");
        var ribbonBannerSpr = LoadSprite("ribbon_banner_red.png");
        var btnYellowSpr = LoadSprite("button_3d_yellow.png");
        var btnBlueSpr = LoadSprite("button_3d_blue.png");
        var btnGreenSpr = LoadSprite("button_3d_green.png");
        var btnRedSpr = LoadSprite("button_3d_red.png");
        var capsuleBlueSpr = LoadSprite("capsule_blue.png");
        var capsuleGreenSpr = LoadSprite("capsule_green.png");
        var badgeWoodSpr = LoadSprite("badge_target_wood.png");
        var badgeShieldSpr = LoadSprite("badge_shield_gold.png");
        var leafSpr = LoadSprite("icon_leaf_corner.png");
        var arrowCornerSpr = LoadSprite("icon_arrow_corner.png");
        var arrowMiniSpr = LoadSprite("icon_arrow_mini.png");
        var clockSpr = LoadSprite("icon_clock.png");
        var targetMiniSpr = LoadSprite("icon_target_mini.png");
        var restartSpr = LoadSprite("icon_restart.png");
        var homeSpr = LoadSprite("icon_home.png");
        var backSpr = LoadSprite("icon_back.png");

        Undo.RegisterFullObjectHierarchyUndo(canvasObj, "Redesign 4 UI Panels");

        // 1. REDESIGN START_PANEL (Menu Chính)
        var startPanel = canvasObj.transform.Find("Start_Panel");
        if (startPanel != null)
        {
            SetupPanelRoot(startPanel, new Vector2(640, 480), Vector2.zero, woodBoardSpr);
            SetupCornerDecors(startPanel, leafSpr, arrowCornerSpr);
            SetupBanner(startPanel, "ARCHERY VR", ribbonBannerSpr, font, 34);

            // Leaderboard Section
            var lbSec = startPanel.Find("Leaderboard_Section");
            if (lbSec != null)
            {
                var lbRt = lbSec.GetComponent<RectTransform>();
                lbRt.anchorMin = new Vector2(0.5f, 0.5f);
                lbRt.anchorMax = new Vector2(0.5f, 0.5f);
                lbRt.pivot = new Vector2(0.5f, 0.5f);
                lbRt.anchoredPosition = new Vector2(0, 16);
                lbRt.sizeDelta = new Vector2(520, 240);

                var lbImg = lbSec.GetComponent<Image>();
                if (lbImg != null)
                {
                    lbImg.sprite = woodCardSpr;
                    lbImg.type = Image.Type.Sliced;
                    lbImg.color = Color.white;
                }

                var header = lbSec.Find("Header_Text")?.GetComponent<TMP_Text>();
                if (header != null)
                {
                    var hRt = header.GetComponent<RectTransform>();
                    hRt.anchorMin = new Vector2(0.5f, 1f);
                    hRt.anchorMax = new Vector2(0.5f, 1f);
                    hRt.pivot = new Vector2(0.5f, 1f);
                    hRt.anchoredPosition = new Vector2(0, -8);
                    hRt.sizeDelta = new Vector2(360, 36);

                    header.text = "BẢNG KỶ LỤC TOP 3";
                    header.font = font;
                    header.fontSize = 20;
                    header.fontStyle = FontStyles.Bold;
                    header.color = Color.white;
                    header.outlineWidth = 0.22f;
                    header.outlineColor = new Color32(75, 35, 10, 255);
                    header.alignment = TextAlignmentOptions.Center;
                }

                var lbText = lbSec.Find("Leaderboard_Text")?.GetComponent<TMP_Text>();
                if (lbText != null)
                {
                    var tRt = lbText.GetComponent<RectTransform>();
                    tRt.anchorMin = new Vector2(0.5f, 0.5f);
                    tRt.anchorMax = new Vector2(0.5f, 0.5f);
                    tRt.pivot = new Vector2(0.5f, 0.5f);
                    tRt.anchoredPosition = new Vector2(0, -14);
                    tRt.sizeDelta = new Vector2(490, 175);

                    lbText.font = font;
                    lbText.fontSize = 17;
                    lbText.lineSpacing = 35f;
                    lbText.color = new Color32(65, 35, 15, 255);
                    lbText.alignment = TextAlignmentOptions.Center;
                }
            }

            // Buttons
            var startBtn = startPanel.Find("Start_Button");
            if (startBtn != null)
            {
                SetupButton(startBtn, "BẮT ĐẦU", new Vector2(-135, 46), new Vector2(230, 64), btnYellowSpr, null, font, 24, new Color32(110, 35, 0, 255));
            }

            var guideBtn = startPanel.Find("Guide_Button");
            if (guideBtn != null)
            {
                SetupButton(guideBtn, "HƯỚNG DẪN CHƠI", new Vector2(135, 46), new Vector2(230, 64), btnBlueSpr, null, font, 20, new Color32(5, 45, 110, 255));
            }

            // Remove unwanted children like Accent_Line if present
            var oldAccent = startPanel.Find("Accent_Line");
            if (oldAccent != null) UnityEngine.Object.DestroyImmediate(oldAccent.gameObject);
        }

        // 2. REDESIGN GUIDE_PANEL (Hướng Dẫn & Quy Tắc)
        var guidePanel = canvasObj.transform.Find("Guide_Panel");
        if (guidePanel != null)
        {
            SetupPanelRoot(guidePanel, new Vector2(640, 480), Vector2.zero, woodBoardSpr);
            SetupCornerDecors(guidePanel, leafSpr, arrowCornerSpr);
            SetupBanner(guidePanel, "HƯỚNG DẪN CHƠI & QUY TẮC", ribbonBannerSpr, font, 25);

            var contentSec = guidePanel.Find("Content_Section");
            if (contentSec != null)
            {
                var cRt = contentSec.GetComponent<RectTransform>();
                cRt.anchorMin = new Vector2(0.5f, 0.5f);
                cRt.anchorMax = new Vector2(0.5f, 0.5f);
                cRt.pivot = new Vector2(0.5f, 0.5f);
                cRt.anchoredPosition = new Vector2(0, 16);
                cRt.sizeDelta = new Vector2(530, 250);

                var cImg = contentSec.GetComponent<Image>();
                if (cImg != null)
                {
                    cImg.sprite = woodCardSpr;
                    cImg.type = Image.Type.Sliced;
                    cImg.color = Color.white;
                }

                var guideText = contentSec.Find("Guide_Text")?.GetComponent<TMP_Text>();
                if (guideText != null)
                {
                    var gRt = guideText.GetComponent<RectTransform>();
                    gRt.anchorMin = Vector2.zero;
                    gRt.anchorMax = Vector2.one;
                    gRt.offsetMin = new Vector2(22, 16);
                    gRt.offsetMax = new Vector2(-22, -16);

                    guideText.font = font;
                    guideText.fontSize = 17;
                    guideText.lineSpacing = 28f;
                    guideText.color = new Color32(55, 30, 15, 255);
                    guideText.alignment = TextAlignmentOptions.TopLeft;
                    guideText.text =
                        "<size=108%><color=#6B3510><b>1. CÁCH ĐIỀU KHIỂN BẮN CUNG:</b></color></size>\n" +
                        "  • <color=#B22222><b>Cầm cung:</b></color> Đưa tay trái cầm cung và bấm nút <b>Grip</b>\n" +
                        "  • <color=#B22222><b>Kéo dây:</b></color> Đưa tay phải lại gần dây, giữ nút <b>Grip</b> và kéo về sau\n" +
                        "  • <color=#B22222><b>Bắn tên:</b></color> Căn góc ngắm và <b>thả nút Grip</b> tay phải\n\n" +
                        "<size=108%><color=#6B3510><b>2. QUY TẮC TÍNH ĐIỂM & KỶ LỤC:</b></color></size>\n" +
                        "  • <color=#B22222><b>+10 điểm</b></color> cho mỗi phát bắn trúng bia\n" +
                        "  • Mỗi ván có tối đa <color=#B22222><b>10 mũi tên</b></color> & thời gian tối đa <color=#B22222><b>60 giây</b></color>\n" +
                        "  • Bảng kỷ lục giữ lại <b>Top 3</b> thành tích xuất sắc nhất!";
                }
            }

            var backBtn = guidePanel.Find("Back_Button");
            if (backBtn != null)
            {
                SetupButton(backBtn, "QUAY LẠI", new Vector2(0, 44), new Vector2(220, 60), btnRedSpr, backSpr, font, 22, new Color32(90, 15, 15, 255));
            }

            var oldAccent = guidePanel.Find("Accent_Line");
            if (oldAccent != null) UnityEngine.Object.DestroyImmediate(oldAccent.gameObject);
        }

        // 3. REDESIGN SCORE_PANEL (HUD In-Game)
        var scorePanel = canvasObj.transform.Find("Score_Panel");
        if (scorePanel != null)
        {
            SetupPanelRoot(scorePanel, new Vector2(340, 240), new Vector2(-270, 175), woodBoardSpr);
            SetupCornerDecors(scorePanel, leafSpr, arrowCornerSpr, 55f);
            SetupBanner(scorePanel, "ĐIỂM SỐ", ribbonBannerSpr, font, 22, new Vector2(210, 50), new Vector2(0, 8));

            // Central Score Badge
            Transform badge = scorePanel.Find("Badge_Score");
            if (badge == null)
            {
                var go = new GameObject("Badge_Score", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(scorePanel, false);
                badge = go.transform;
            }
            var bRt = badge.GetComponent<RectTransform>();
            bRt.anchorMin = new Vector2(0.5f, 0.5f);
            bRt.anchorMax = new Vector2(0.5f, 0.5f);
            bRt.pivot = new Vector2(0.5f, 0.5f);
            bRt.anchoredPosition = new Vector2(0, 10);
            bRt.sizeDelta = new Vector2(115, 115);

            var bImg = badge.GetComponent<Image>();
            bImg.sprite = badgeWoodSpr;
            bImg.type = Image.Type.Simple;
            bImg.color = Color.white;

            // Score Value Text
            Transform scoreVal = badge.Find("Score_Value");
            if (scoreVal == null)
            {
                var go = new GameObject("Score_Value", typeof(RectTransform), typeof(TextMeshProUGUI));
                go.transform.SetParent(badge, false);
                scoreVal = go.transform;
            }
            var svRt = scoreVal.GetComponent<RectTransform>();
            svRt.anchorMin = Vector2.zero;
            svRt.anchorMax = Vector2.one;
            svRt.offsetMin = Vector2.zero;
            svRt.offsetMax = Vector2.zero;

            var svTmp = scoreVal.GetComponent<TMP_Text>();
            svTmp.font = font;
            svTmp.fontSize = 50;
            svTmp.fontStyle = FontStyles.Bold;
            svTmp.color = new Color32(95, 35, 10, 255);
            svTmp.alignment = TextAlignmentOptions.Center;
            svTmp.text = "0";

            // Left mini target decor
            EnsureDecor(scorePanel, "Decor_Target", targetMiniSpr, new Vector2(-75, 10), new Vector2(44, 44));
            // Right mini arrow decor
            EnsureDecor(scorePanel, "Decor_Arrow", arrowCornerSpr, new Vector2(75, 10), new Vector2(40, 40), 90f);

            // Capsule Arrows
            Transform capArrows = scorePanel.Find("Capsule_Arrows");
            if (capArrows == null)
            {
                var go = new GameObject("Capsule_Arrows", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(scorePanel, false);
                capArrows = go.transform;
            }
            var caRt = capArrows.GetComponent<RectTransform>();
            caRt.anchorMin = new Vector2(0.5f, 0f);
            caRt.anchorMax = new Vector2(0.5f, 0f);
            caRt.pivot = new Vector2(0.5f, 0f);
            caRt.anchoredPosition = new Vector2(-75, 22);
            caRt.sizeDelta = new Vector2(142, 38);

            var caImg = capArrows.GetComponent<Image>();
            caImg.sprite = capsuleBlueSpr;
            caImg.type = Image.Type.Sliced;
            caImg.color = Color.white;

            EnsureDecor(capArrows, "Icon", arrowMiniSpr, new Vector2(-46, 0), new Vector2(22, 22));
            var caText = EnsureText(capArrows, "Text", "MŨI TÊN: 10/10", font, 13, Color.white, new Vector2(14, 0), new Vector2(105, 30));
            caText.outlineWidth = 0.22f;
            caText.outlineColor = new Color32(5, 45, 110, 255);

            // Capsule Time
            Transform capTime = scorePanel.Find("Capsule_Time");
            if (capTime == null)
            {
                var go = new GameObject("Capsule_Time", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(scorePanel, false);
                capTime = go.transform;
            }
            var ctRt = capTime.GetComponent<RectTransform>();
            ctRt.anchorMin = new Vector2(0.5f, 0f);
            ctRt.anchorMax = new Vector2(0.5f, 0f);
            ctRt.pivot = new Vector2(0.5f, 0f);
            ctRt.anchoredPosition = new Vector2(75, 22);
            ctRt.sizeDelta = new Vector2(142, 38);

            var ctImg = capTime.GetComponent<Image>();
            ctImg.sprite = capsuleGreenSpr;
            ctImg.type = Image.Type.Sliced;
            ctImg.color = Color.white;

            EnsureDecor(capTime, "Icon", clockSpr, new Vector2(-46, 0), new Vector2(22, 22));
            var ctText = EnsureText(capTime, "Text", "THỜI GIAN: 60s", font, 13, Color.white, new Vector2(14, 0), new Vector2(105, 30));
            ctText.outlineWidth = 0.22f;
            ctText.outlineColor = new Color32(15, 65, 10, 255);

            // Keep Score_Text hidden/minimal so legacy references don't break
            var oldScoreText = scorePanel.Find("Score_Text");
            if (oldScoreText != null)
            {
                oldScoreText.gameObject.SetActive(false);
            }
            var oldAccent = scorePanel.Find("Accent_Line");
            if (oldAccent != null) UnityEngine.Object.DestroyImmediate(oldAccent.gameObject);
        }

        // 4. REDESIGN RESULT_PANEL (Kết Thúc Ván)
        var resultPanel = canvasObj.transform.Find("Result_Panel");
        if (resultPanel != null)
        {
            SetupPanelRoot(resultPanel, new Vector2(640, 480), Vector2.zero, woodBoardSpr);
            SetupCornerDecors(resultPanel, leafSpr, arrowCornerSpr);
            SetupBanner(resultPanel, "KẾT THÚC VÁN CHƠI", ribbonBannerSpr, font, 28);

            // Score Showcase Section
            Transform showcase = resultPanel.Find("Score_Showcase");
            if (showcase == null)
            {
                var go = new GameObject("Score_Showcase", typeof(RectTransform));
                go.transform.SetParent(resultPanel, false);
                showcase = go.transform;
            }
            var scRt = showcase.GetComponent<RectTransform>();
            scRt.anchorMin = new Vector2(0.5f, 1f);
            scRt.anchorMax = new Vector2(0.5f, 1f);
            scRt.pivot = new Vector2(0.5f, 1f);
            scRt.anchoredPosition = new Vector2(0, -90);
            scRt.sizeDelta = new Vector2(460, 85);

            // Shield Badge
            Transform badgeShield = showcase.Find("Badge_Shield");
            if (badgeShield == null)
            {
                var go = new GameObject("Badge_Shield", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(showcase, false);
                badgeShield = go.transform;
            }
            var bsRt = badgeShield.GetComponent<RectTransform>();
            bsRt.anchorMin = new Vector2(0f, 0.5f);
            bsRt.anchorMax = new Vector2(0f, 0.5f);
            bsRt.pivot = new Vector2(0.5f, 0.5f);
            bsRt.anchoredPosition = new Vector2(45, 0);
            bsRt.sizeDelta = new Vector2(85, 85);

            var bsImg = badgeShield.GetComponent<Image>();
            bsImg.sprite = badgeShieldSpr;
            bsImg.type = Image.Type.Simple;
            bsImg.color = Color.white;

            var shieldVal = EnsureText(badgeShield, "Score_Value", "80", font, 38, new Color32(95, 35, 10, 255), Vector2.zero, new Vector2(80, 80));
            shieldVal.fontStyle = FontStyles.Bold;
            shieldVal.alignment = TextAlignmentOptions.Center;

            // Detail Text
            var detailText = EnsureText(showcase, "Detail_Text", "<size=120%><color=#3E2723><b>TỔNG ĐIỂM</b></color></size>\n<size=78%><color=#5C3A21>MŨI TÊN ĐÃ DÙNG: <b>10/10</b>  •  THỜI GIAN: <b>00:35</b></color></size>", font, 18, Color.white, new Vector2(80, 0), new Vector2(320, 80));
            detailText.alignment = TextAlignmentOptions.Left;

            // Leaderboard Section
            var lbSec = resultPanel.Find("Leaderboard_Section");
            if (lbSec != null)
            {
                var lbRt = lbSec.GetComponent<RectTransform>();
                lbRt.anchorMin = new Vector2(0.5f, 0.5f);
                lbRt.anchorMax = new Vector2(0.5f, 0.5f);
                lbRt.pivot = new Vector2(0.5f, 0.5f);
                lbRt.anchoredPosition = new Vector2(0, -56);
                lbRt.sizeDelta = new Vector2(520, 175);

                var lbImg = lbSec.GetComponent<Image>();
                if (lbImg != null)
                {
                    lbImg.sprite = woodCardSpr;
                    lbImg.type = Image.Type.Sliced;
                    lbImg.color = Color.white;
                }

                var header = lbSec.Find("Header_Text")?.GetComponent<TMP_Text>();
                if (header != null)
                {
                    var hRt = header.GetComponent<RectTransform>();
                    hRt.anchorMin = new Vector2(0.5f, 1f);
                    hRt.anchorMax = new Vector2(0.5f, 1f);
                    hRt.pivot = new Vector2(0.5f, 1f);
                    hRt.anchoredPosition = new Vector2(0, -6);
                    hRt.sizeDelta = new Vector2(340, 32);

                    header.text = "BẢNG KỶ LỤC TOP 3";
                    header.font = font;
                    header.fontSize = 18;
                    header.fontStyle = FontStyles.Bold;
                    header.color = Color.white;
                    header.outlineWidth = 0.22f;
                    header.outlineColor = new Color32(75, 35, 10, 255);
                    header.alignment = TextAlignmentOptions.Center;
                }

                var lbText = lbSec.Find("Leaderboard_Text")?.GetComponent<TMP_Text>();
                if (lbText != null)
                {
                    var tRt = lbText.GetComponent<RectTransform>();
                    tRt.anchorMin = new Vector2(0.5f, 0.5f);
                    tRt.anchorMax = new Vector2(0.5f, 0.5f);
                    tRt.pivot = new Vector2(0.5f, 0.5f);
                    tRt.anchoredPosition = new Vector2(0, -12);
                    tRt.sizeDelta = new Vector2(490, 130);

                    lbText.font = font;
                    lbText.fontSize = 15;
                    lbText.lineSpacing = 26f;
                    lbText.color = new Color32(65, 35, 15, 255);
                    lbText.alignment = TextAlignmentOptions.Center;
                }
            }

            // Buttons
            var restartBtn = resultPanel.Find("Restart_Button");
            if (restartBtn != null)
            {
                SetupButton(restartBtn, "CHƠI LẠI", new Vector2(-135, 46), new Vector2(230, 64), btnYellowSpr, restartSpr, font, 22, new Color32(110, 35, 0, 255));
            }

            var mainMenuBtn = resultPanel.Find("MainMenu_Button");
            if (mainMenuBtn != null)
            {
                SetupButton(mainMenuBtn, "VỀ MÀN HÌNH CHÍNH", new Vector2(135, 46), new Vector2(230, 64), btnGreenSpr, homeSpr, font, 19, new Color32(15, 65, 10, 255));
            }

            // Remove unwanted old elements
            var oldSummary = resultPanel.Find("Summary_Text");
            if (oldSummary != null) oldSummary.gameObject.SetActive(false);
            var oldAccent = resultPanel.Find("Accent_Line");
            if (oldAccent != null) UnityEngine.Object.DestroyImmediate(oldAccent.gameObject);
        }

        // Link all references in ScoreManager
        var gameSystem = GameObject.Find("GAME SYSTEM");
        if (gameSystem != null)
        {
            var sm = gameSystem.GetComponent<ScoreManager>();
            if (sm != null)
            {
                var so = new SerializedObject(sm);

                // HUD Elements
                so.FindProperty("hudScoreText").objectReferenceValue = scorePanel?.Find("Badge_Score/Score_Value")?.GetComponent<TMP_Text>();
                so.FindProperty("hudArrowsText").objectReferenceValue = scorePanel?.Find("Capsule_Arrows/Text")?.GetComponent<TMP_Text>();
                so.FindProperty("hudTimeText").objectReferenceValue = scorePanel?.Find("Capsule_Time/Text")?.GetComponent<TMP_Text>();

                // Result Elements
                so.FindProperty("resultScoreBadgeText").objectReferenceValue = resultPanel?.Find("Score_Showcase/Badge_Shield/Score_Value")?.GetComponent<TMP_Text>();
                so.FindProperty("resultDetailText").objectReferenceValue = resultPanel?.Find("Score_Showcase/Detail_Text")?.GetComponent<TMP_Text>();

                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(sm);
            }
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Debug.Log("Successfully applied Cartoon UI Redesign and saved scene!");
    }

    private static Sprite LoadSprite(string name)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>(SpriteDir + name);
    }

    private static void SetupPanelRoot(Transform panel, Vector2 size, Vector2 pos, Sprite bgSprite)
    {
        var rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;

        var img = panel.GetComponent<Image>();
        if (img == null) img = panel.gameObject.AddComponent<Image>();
        img.sprite = bgSprite;
        img.type = Image.Type.Sliced;
        img.color = Color.white;
    }

    private static void SetupBanner(Transform panel, string title, Sprite bannerSprite, TMP_FontAsset font, float fontSize, Vector2? customSize = null, Vector2? customPos = null)
    {
        Transform banner = panel.Find("Banner");
        if (banner == null)
        {
            var go = new GameObject("Banner", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(panel, false);
            banner = go.transform;
        }

        var bRt = banner.GetComponent<RectTransform>();
        bRt.anchorMin = new Vector2(0.5f, 1f);
        bRt.anchorMax = new Vector2(0.5f, 1f);
        bRt.pivot = new Vector2(0.5f, 1f);
        bRt.anchoredPosition = customPos ?? new Vector2(0, 16);
        bRt.sizeDelta = customSize ?? new Vector2(440, 96);

        var bImg = banner.GetComponent<Image>();
        bImg.sprite = bannerSprite;
        bImg.type = Image.Type.Sliced;
        bImg.color = Color.white;

        // Title Text
        Transform titleObj = panel.Find("Title_Text");
        if (titleObj != null)
        {
            titleObj.SetParent(banner, false);
        }
        else
        {
            titleObj = banner.Find("Title_Text");
            if (titleObj == null)
            {
                var go = new GameObject("Title_Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                go.transform.SetParent(banner, false);
                titleObj = go.transform;
            }
        }

        var tRt = titleObj.GetComponent<RectTransform>();
        tRt.anchorMin = Vector2.zero;
        tRt.anchorMax = Vector2.one;
        tRt.offsetMin = new Vector2(30, 10);
        tRt.offsetMax = new Vector2(-30, -10);

        var tmp = titleObj.GetComponent<TMP_Text>();
        tmp.font = font;
        tmp.fontSize = fontSize;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = Color.white;
        tmp.outlineWidth = 0.25f;
        tmp.outlineColor = new Color32(90, 15, 10, 255);
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.text = title;
    }

    private static void SetupCornerDecors(Transform panel, Sprite leafSpr, Sprite arrowSpr, float leafSize = 80f)
    {
        EnsureDecor(panel, "Decor_Arrow_TL", arrowSpr, new Vector2(-panel.GetComponent<RectTransform>().rect.width * 0.5f + 30f, panel.GetComponent<RectTransform>().rect.height * 0.5f - 10f), new Vector2(leafSize * 1.1f, leafSize * 1.1f));
        EnsureDecor(panel, "Decor_Leaf_TL", leafSpr, new Vector2(-panel.GetComponent<RectTransform>().rect.width * 0.5f + 35f, panel.GetComponent<RectTransform>().rect.height * 0.5f - 15f), new Vector2(leafSize, leafSize));
        EnsureDecor(panel, "Decor_Leaf_BR", leafSpr, new Vector2(panel.GetComponent<RectTransform>().rect.width * 0.5f - 25f, -panel.GetComponent<RectTransform>().rect.height * 0.5f + 25f), new Vector2(leafSize, leafSize), 180f);
        EnsureDecor(panel, "Decor_Leaf_BL", leafSpr, new Vector2(-panel.GetComponent<RectTransform>().rect.width * 0.5f + 25f, -panel.GetComponent<RectTransform>().rect.height * 0.5f + 25f), new Vector2(leafSize * 0.85f, leafSize * 0.85f), 90f);
    }

    private static void EnsureDecor(Transform parent, string name, Sprite sprite, Vector2 pos, Vector2 size, float rotZ = 0f)
    {
        Transform decor = parent.Find(name);
        if (decor == null)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            decor = go.transform;
        }

        var rt = decor.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        rt.localEulerAngles = new Vector3(0, 0, rotZ);

        var img = decor.GetComponent<Image>();
        img.sprite = sprite;
        img.type = Image.Type.Simple;
        img.color = Color.white;
        img.raycastTarget = false; // Decorations must not block raycasts
    }

    private static void SetupButton(Transform btnTrans, string label, Vector2 pos, Vector2 size, Sprite btnSprite, Sprite iconSprite, TMP_FontAsset font, float fontSize, Color32 outlineColor)
    {
        var rt = btnTrans.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        var img = btnTrans.GetComponent<Image>();
        if (img == null) img = btnTrans.gameObject.AddComponent<Image>();
        img.sprite = btnSprite;
        img.type = Image.Type.Sliced;
        img.color = Color.white;
        img.raycastTarget = true;

        var btn = btnTrans.GetComponent<Button>();
        if (btn == null) btn = btnTrans.gameObject.AddComponent<Button>();

        // Set rich button color transitions
        var colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.08f, 1.08f, 1.08f, 1f);
        colors.pressedColor = new Color(0.88f, 0.88f, 0.88f, 1f);
        colors.selectedColor = Color.white;
        btn.colors = colors;

        // Optional Icon
        Transform icon = btnTrans.Find("Icon");
        if (iconSprite != null)
        {
            if (icon == null)
            {
                var go = new GameObject("Icon", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(btnTrans, false);
                icon = go.transform;
            }
            var iRt = icon.GetComponent<RectTransform>();
            iRt.anchorMin = new Vector2(0f, 0.5f);
            iRt.anchorMax = new Vector2(0f, 0.5f);
            iRt.pivot = new Vector2(0.5f, 0.5f);
            iRt.anchoredPosition = new Vector2(28, 2);
            iRt.sizeDelta = new Vector2(28, 28);

            var iImg = icon.GetComponent<Image>();
            iImg.sprite = iconSprite;
            iImg.type = Image.Type.Simple;
            iImg.color = Color.white;
            iImg.raycastTarget = false;
        }
        else if (icon != null)
        {
            icon.gameObject.SetActive(false);
        }

        // Label Text
        Transform textTrans = btnTrans.Find("Text");
        if (textTrans == null)
        {
            var go = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(btnTrans, false);
            textTrans = go.transform;
        }

        var tRt = textTrans.GetComponent<RectTransform>();
        tRt.anchorMin = Vector2.zero;
        tRt.anchorMax = Vector2.one;
        tRt.offsetMin = new Vector2(iconSprite != null ? 36 : 10, 4);
        tRt.offsetMax = new Vector2(-10, -2);

        var tmp = textTrans.GetComponent<TMP_Text>();
        tmp.font = font;
        tmp.fontSize = fontSize;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = Color.white;
        tmp.outlineWidth = 0.25f;
        tmp.outlineColor = outlineColor;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.text = label;
        tmp.raycastTarget = false;
    }

    private static TMP_Text EnsureText(Transform parent, string name, string initialText, TMP_FontAsset font, float fontSize, Color color, Vector2 pos, Vector2 size)
    {
        Transform t = parent.Find(name);
        if (t == null)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            t = go.transform;
        }

        var rt = t.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        var tmp = t.GetComponent<TMP_Text>();
        tmp.font = font;
        tmp.fontSize = fontSize;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = color;
        tmp.text = initialText;
        return tmp;
    }
}

