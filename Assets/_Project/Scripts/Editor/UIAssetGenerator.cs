using System;
using System.IO;
using UnityEngine;
using UnityEditor;

public static class UIAssetGenerator
{
    private const string OutDir = "Assets/_Project/UI/Sprites/";

    [MenuItem("Tools/ArcheryVR/Generate Cartoon UI Sprites")]
    public static void GenerateAll()
    {
        if (!Directory.Exists(OutDir))
        {
            Directory.CreateDirectory(OutDir);
        }

        // 1. Wood Board Frame (9-sliced)
        GenerateWoodBoardFrame();

        // 2. Inner Wood Card (9-sliced)
        GenerateInnerWoodCard();

        // 3. Ribbon Banner Red
        GenerateRibbonBanner();

        // 4. 3D Cartoon Buttons (Yellow, Blue, Green, Red)
        Generate3DButton("button_3d_yellow.png", new Color32(255, 175, 40, 255), new Color32(255, 145, 0, 255), new Color32(215, 65, 5, 255), new Color32(110, 30, 0, 255), new Color32(255, 225, 130, 255));
        Generate3DButton("button_3d_blue.png", new Color32(45, 180, 250, 255), new Color32(15, 140, 230, 255), new Color32(10, 85, 180, 255), new Color32(5, 45, 110, 255), new Color32(160, 230, 255, 255));
        Generate3DButton("button_3d_green.png", new Color32(105, 205, 80, 255), new Color32(70, 175, 50, 255), new Color32(35, 115, 25, 255), new Color32(15, 65, 10, 255), new Color32(185, 245, 165, 255));
        Generate3DButton("button_3d_red.png", new Color32(245, 85, 75, 255), new Color32(225, 50, 45, 255), new Color32(180, 25, 25, 255), new Color32(95, 10, 10, 255), new Color32(255, 185, 180, 255));

        // 5. Capsules for HUD (Blue for arrows, Green for time)
        GenerateCapsule("capsule_blue.png", new Color32(40, 165, 240, 255), new Color32(20, 130, 215, 255), new Color32(10, 75, 165, 255), new Color32(5, 40, 100, 255));
        GenerateCapsule("capsule_green.png", new Color32(90, 195, 75, 255), new Color32(65, 165, 50, 255), new Color32(30, 110, 25, 255), new Color32(15, 60, 10, 255));

        // 6. Badges
        GenerateWoodTargetBadge();
        GenerateGoldShieldBadge();

        // 7. Decorative Leaves & Corner Decor
        GenerateCornerLeaves();
        GenerateCornerArrow();

        // 8. Icons
        GenerateTrophyIcon("icon_trophy_gold.png", new Color32(255, 215, 0, 255), new Color32(255, 170, 0, 255), new Color32(180, 110, 0, 255));
        GenerateTrophyIcon("icon_trophy_silver.png", new Color32(230, 235, 245, 255), new Color32(190, 200, 215, 255), new Color32(130, 140, 160, 255));
        GenerateTrophyIcon("icon_trophy_bronze.png", new Color32(235, 155, 100, 255), new Color32(205, 120, 70, 255), new Color32(140, 70, 30, 255));
        GenerateAvatarIcon();
        GenerateActionIcons();

        AssetDatabase.Refresh();
        Debug.Log("Successfully generated all Cartoon UI sprites!");
    }

    private static void SaveAndImport(Texture2D tex, string fileName, Vector4 border)
    {
        tex.Apply();
        string fullPath = OutDir + fileName;
        File.WriteAllBytes(fullPath, tex.EncodeToPNG());
        AssetDatabase.ImportAsset(fullPath, ImportAssetOptions.ForceUpdate);

        var ti = AssetImporter.GetAtPath(fullPath) as TextureImporter;
        if (ti != null)
        {
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.spritePixelsPerUnit = 100;
            ti.filterMode = FilterMode.Bilinear;
            ti.alphaIsTransparency = true;
            ti.spriteBorder = border;
            ti.SaveAndReimport();
        }
    }

    // -------------------------------------------------------------
    // 1. WOOD BOARD FRAME (256x256, 9-slice border: 48, 48, 48, 48)
    // -------------------------------------------------------------
    private static void GenerateWoodBoardFrame()
    {
        int size = 256;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color32 clear = new Color32(0, 0, 0, 0);

        Color32 outerBorder = new Color32(65, 30, 10, 255);    // Dark chocolate wood outline
        Color32 woodRim = new Color32(120, 60, 22, 255);       // Rich warm wood
        Color32 woodHighlight = new Color32(165, 90, 35, 255); // Top wood bevel
        Color32 woodShadow = new Color32(85, 40, 15, 255);     // Bottom wood shadow
        Color32 innerLine = new Color32(75, 35, 12, 255);      // Separation line
        Color32 creamBg = new Color32(255, 246, 222, 255);     // Cream parchment face
        Color32 creamShade = new Color32(245, 232, 200, 255);  // Subtle parchment gradient

        float rOuter = 46f;
        float rInner = 26f;
        float rimThickness = 20f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dOuter = SdfRoundedRect(x, y, 0, 0, size, size, rOuter);
                if (dOuter > 0)
                {
                    tex.SetPixel(x, y, clear);
                    continue;
                }

                // Outer dark outline (3px)
                if (dOuter > -3.5f)
                {
                    float a = Mathf.Clamp01(-dOuter);
                    tex.SetPixel(x, y, Color.Lerp(clear, outerBorder, a));
                    continue;
                }

                // Check inner cream area
                float dInner = SdfRoundedRect(x, y, rimThickness, rimThickness, size - rimThickness * 2, size - rimThickness * 2, rInner);
                if (dInner <= 0)
                {
                    // Inner cream parchment
                    if (dInner > -2f)
                    {
                        // Inner dark groove
                        tex.SetPixel(x, y, innerLine);
                    }
                    else
                    {
                        // Subtle vertical parchment gradient
                        float vGrad = (float)y / size;
                        Color c = Color.Lerp(creamShade, creamBg, vGrad * 0.7f + 0.3f);
                        tex.SetPixel(x, y, c);
                    }
                }
                else
                {
                    // Wood rim with 3D bevel
                    float vNorm = (float)y / size;
                    Color wood = woodRim;
                    if (y > size - 32)
                        wood = Color.Lerp(woodRim, woodHighlight, (y - (size - 32)) / 32f);
                    else if (y < 32)
                        wood = Color.Lerp(woodShadow, woodRim, y / 32f);

                    tex.SetPixel(x, y, wood);
                }
            }
        }

        SaveAndImport(tex, "wood_board_frame.png", new Vector4(48, 48, 48, 48));
    }

    // -------------------------------------------------------------
    // 2. INNER WOOD CARD (128x128, 9-slice: 24, 24, 24, 24)
    // -------------------------------------------------------------
    private static void GenerateInnerWoodCard()
    {
        int size = 128;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color32 clear = new Color32(0, 0, 0, 0);

        Color32 darkLine = new Color32(110, 60, 20, 255);
        Color32 woodBevel = new Color32(170, 110, 45, 255);
        Color32 tanBgTop = new Color32(245, 222, 175, 255);
        Color32 tanBgBtm = new Color32(230, 205, 155, 255);

        float r = 18f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float d = SdfRoundedRect(x, y, 0, 0, size, size, r);
                if (d > 0)
                {
                    tex.SetPixel(x, y, clear);
                    continue;
                }

                if (d > -2.5f)
                {
                    tex.SetPixel(x, y, darkLine);
                }
                else if (d > -6f)
                {
                    tex.SetPixel(x, y, woodBevel);
                }
                else
                {
                    float t = (float)y / size;
                    tex.SetPixel(x, y, Color.Lerp(tanBgBtm, tanBgTop, t));
                }
            }
        }

        SaveAndImport(tex, "wood_card_inner.png", new Vector4(24, 24, 24, 24));
    }

    // -------------------------------------------------------------
    // 3. RIBBON BANNER RED (384x96)
    // -------------------------------------------------------------
    private static void GenerateRibbonBanner()
    {
        int w = 384;
        int h = 96;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        Color32 clear = new Color32(0, 0, 0, 0);

        Color32 darkOutline = new Color32(115, 20, 10, 255);
        Color32 shadowWing = new Color32(150, 25, 15, 255);
        Color32 redTop = new Color32(245, 75, 35, 255);
        Color32 redBtm = new Color32(200, 35, 20, 255);
        Color32 goldTrim = new Color32(255, 215, 70, 255);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                tex.SetPixel(x, y, clear);

                // Left wing
                if (x < 64)
                {
                    float wingYCenter = h * 0.42f;
                    float dy = Mathf.Abs(y - wingYCenter);
                    if (dy < 26f)
                    {
                        // Triangular cutout on the left
                        if (x < 24 && Mathf.Abs(y - wingYCenter) < (24 - x) * 1.1f)
                        {
                            continue;
                        }
                        tex.SetPixel(x, y, shadowWing);
                    }
                }
                // Right wing
                else if (x > w - 64)
                {
                    float wingYCenter = h * 0.42f;
                    float dy = Mathf.Abs(y - wingYCenter);
                    if (dy < 26f)
                    {
                        // Triangular cutout on the right
                        int rx = w - 1 - x;
                        if (rx < 24 && Mathf.Abs(y - wingYCenter) < (24 - rx) * 1.1f)
                        {
                            continue;
                        }
                        tex.SetPixel(x, y, shadowWing);
                    }
                }

                // Main central banner body (curved / arched top)
                if (x >= 44 && x <= w - 44)
                {
                    float normX = (x - 44f) / (w - 88f);
                    float arch = Mathf.Sin(normX * Mathf.PI) * 10f; // Arched top
                    float topY = h - 14f + arch * 0.5f;
                    float btmY = 12f + arch * 0.5f;

                    if (y >= btmY && y <= topY)
                    {
                        float dEdge = Mathf.Min(y - btmY, topY - y);
                        dEdge = Mathf.Min(dEdge, Mathf.Min(x - 44f, (w - 44f) - x));

                        if (dEdge < 3.5f)
                        {
                            tex.SetPixel(x, y, darkOutline);
                        }
                        else if (topY - y < 8f || y - btmY < 8f)
                        {
                            // Gold trim highlight
                            tex.SetPixel(x, y, goldTrim);
                        }
                        else
                        {
                            float t = (y - btmY) / (topY - btmY);
                            tex.SetPixel(x, y, Color.Lerp(redBtm, redTop, t));
                        }
                    }
                }
            }
        }

        SaveAndImport(tex, "ribbon_banner_red.png", new Vector4(70, 20, 70, 24));
    }

    // -------------------------------------------------------------
    // 4. 3D CARTOON BUTTONS (128x128, 9-slice: 28, 38, 28, 24)
    // -------------------------------------------------------------
    private static void Generate3DButton(string fileName, Color32 topColor, Color32 btmColor, Color32 bevelColor, Color32 outlineColor, Color32 highlightColor)
    {
        int size = 128;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color32 clear = new Color32(0, 0, 0, 0);

        float r = 24f;
        float lipHeight = 16f; // 3D bevel height at the bottom

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                // Full button silhouette
                float dTotal = SdfRoundedRect(x, y, 0, 0, size, size, r);
                if (dTotal > 0)
                {
                    tex.SetPixel(x, y, clear);
                    continue;
                }

                // Dark cartoon outline (3.5px)
                if (dTotal > -3.5f)
                {
                    tex.SetPixel(x, y, outlineColor);
                    continue;
                }

                // Check if in bottom 3D bevel zone
                if (y < lipHeight + 6f)
                {
                    tex.SetPixel(x, y, bevelColor);
                    continue;
                }

                // Main button face
                float dFace = SdfRoundedRect(x, y - (int)lipHeight, 0, 0, size, size - (int)lipHeight, r - 3f);
                if (dFace <= 0)
                {
                    float t = (float)(y - lipHeight) / (size - lipHeight);
                    if (y > size - 16)
                    {
                        // Specular top highlight
                        tex.SetPixel(x, y, Color.Lerp(topColor, highlightColor, 0.7f));
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.Lerp(btmColor, topColor, t));
                    }
                }
                else
                {
                    tex.SetPixel(x, y, bevelColor);
                }
            }
        }

        SaveAndImport(tex, fileName, new Vector4(28, 36, 28, 24));
    }

    // -------------------------------------------------------------
    // 5. CAPSULES FOR HUD (128x64, 9-slice: 30, 20, 30, 20)
    // -------------------------------------------------------------
    private static void GenerateCapsule(string fileName, Color32 topColor, Color32 btmColor, Color32 bevelColor, Color32 outlineColor)
    {
        int w = 128;
        int h = 64;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        Color32 clear = new Color32(0, 0, 0, 0);

        float r = 24f;
        float lip = 8f;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float d = SdfRoundedRect(x, y, 0, 0, w, h, r);
                if (d > 0)
                {
                    tex.SetPixel(x, y, clear);
                    continue;
                }

                if (d > -3.5f)
                {
                    tex.SetPixel(x, y, outlineColor);
                }
                else if (y < lip + 4f)
                {
                    tex.SetPixel(x, y, bevelColor);
                }
                else
                {
                    float t = (float)y / h;
                    tex.SetPixel(x, y, Color.Lerp(btmColor, topColor, t));
                }
            }
        }

        SaveAndImport(tex, fileName, new Vector4(30, 20, 30, 20));
    }

    // -------------------------------------------------------------
    // 6. HUD WOOD TARGET BADGE (256x256, circular with notches)
    // -------------------------------------------------------------
    private static void GenerateWoodTargetBadge()
    {
        int size = 256;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color32 clear = new Color32(0, 0, 0, 0);

        Color32 darkWood = new Color32(65, 30, 10, 255);
        Color32 midWood = new Color32(115, 55, 20, 255);
        Color32 goldRing = new Color32(235, 145, 15, 255);
        Color32 goldCenter = new Color32(255, 195, 45, 255);
        Color32 innerYellow = new Color32(255, 220, 95, 255);

        float cx = 128f;
        float cy = 128f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - cx;
                float dy = y - cy;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                float angle = Mathf.Atan2(dy, dx);

                // 4 wooden decorative notches at 0, 90, 180, 270 deg
                float notchR = 120f;
                float notchMod = Mathf.Cos(angle * 4f);
                if (notchMod > 0.7f) notchR += (notchMod - 0.7f) * 16f;

                if (dist > notchR)
                {
                    tex.SetPixel(x, y, clear);
                }
                else if (dist > notchR - 4f)
                {
                    tex.SetPixel(x, y, darkWood);
                }
                else if (dist > 92f)
                {
                    tex.SetPixel(x, y, midWood);
                }
                else if (dist > 86f)
                {
                    tex.SetPixel(x, y, darkWood);
                }
                else if (dist > 74f)
                {
                    tex.SetPixel(x, y, goldRing);
                }
                else
                {
                    float t = dist / 74f;
                    tex.SetPixel(x, y, Color.Lerp(innerYellow, goldCenter, t));
                }
            }
        }

        SaveAndImport(tex, "badge_target_wood.png", Vector4.zero);
    }

    // -------------------------------------------------------------
    // 7. GOLD SHIELD BADGE (128x128)
    // -------------------------------------------------------------
    private static void GenerateGoldShieldBadge()
    {
        int size = 128;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color32 clear = new Color32(0, 0, 0, 0);

        Color32 darkOutline = new Color32(110, 50, 0, 255);
        Color32 goldBorder = new Color32(220, 130, 10, 255);
        Color32 goldFillTop = new Color32(255, 220, 75, 255);
        Color32 goldFillBtm = new Color32(245, 170, 20, 255);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = (x - 64f) / 54f;
                float ny = (y - 18f) / 96f; // 0 at bottom tip, 1 at top

                if (ny < 0 || ny > 1 || Mathf.Abs(nx) > 1)
                {
                    tex.SetPixel(x, y, clear);
                    continue;
                }

                // Shield profile curve
                float maxW = ny > 0.5f ? 1f : Mathf.Sqrt(ny / 0.5f);
                float curW = Mathf.Abs(nx);

                if (curW > maxW)
                {
                    tex.SetPixel(x, y, clear);
                }
                else if (maxW - curW < 0.08f)
                {
                    tex.SetPixel(x, y, darkOutline);
                }
                else if (maxW - curW < 0.22f)
                {
                    tex.SetPixel(x, y, goldBorder);
                }
                else
                {
                    tex.SetPixel(x, y, Color.Lerp(goldFillBtm, goldFillTop, ny));
                }
            }
        }

        SaveAndImport(tex, "badge_shield_gold.png", Vector4.zero);
    }

    // -------------------------------------------------------------
    // 8. DECORATIVE CORNER LEAVES (128x128)
    // -------------------------------------------------------------
    private static void GenerateCornerLeaves()
    {
        int size = 128;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color32 clear = new Color32(0, 0, 0, 0);

        Color32 darkGreen = new Color32(20, 80, 20, 255);
        Color32 leafGreen = new Color32(95, 195, 65, 255);
        Color32 leafLight = new Color32(145, 230, 95, 255);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                tex.SetPixel(x, y, clear);

                // Leaf 1 (Pointing up-right)
                float d1 = SdfEllipseRotated(x - 45, y - 45, 38, 18, 45f);
                // Leaf 2 (Pointing up)
                float d2 = SdfEllipseRotated(x - 30, y - 65, 36, 16, 75f);
                // Leaf 3 (Pointing right)
                float d3 = SdfEllipseRotated(x - 65, y - 30, 36, 16, 15f);

                float d = Mathf.Min(d1, Mathf.Min(d2, d3));
                if (d <= 0)
                {
                    if (d > -3f)
                        tex.SetPixel(x, y, darkGreen);
                    else
                        tex.SetPixel(x, y, (x + y > 100) ? leafLight : leafGreen);
                }
            }
        }

        SaveAndImport(tex, "icon_leaf_corner.png", Vector4.zero);
    }

    // -------------------------------------------------------------
    // 9. CORNER ARROW (128x128)
    // -------------------------------------------------------------
    private static void GenerateCornerArrow()
    {
        int size = 128;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color32 clear = new Color32(0, 0, 0, 0);

        Color32 outline = new Color32(50, 30, 15, 255);
        Color32 woodShaft = new Color32(165, 110, 60, 255);
        Color32 redFletching = new Color32(235, 60, 45, 255);
        Color32 ironTip = new Color32(175, 190, 205, 255);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                tex.SetPixel(x, y, clear);

                // Arrow oriented diagonally from (15, 15) to (110, 110)
                float shaftDist = SdfCapsule(x, y, 20, 20, 105, 105, 4.5f);
                if (shaftDist <= 0)
                {
                    if (shaftDist > -1.5f) tex.SetPixel(x, y, outline);
                    else tex.SetPixel(x, y, woodShaft);
                }

                // Arrow head at (105, 105)
                float headDist = SdfEquilateralTriangle(x - 105, y - 105, 16f, 45f);
                if (headDist <= 0)
                {
                    if (headDist > -1.5f) tex.SetPixel(x, y, outline);
                    else tex.SetPixel(x, y, ironTip);
                }

                // Fletchings at (30, 30)
                float f1 = SdfCapsule(x, y, 25, 35, 40, 40, 5f);
                float f2 = SdfCapsule(x, y, 35, 25, 40, 40, 5f);
                if (f1 <= 0 || f2 <= 0)
                {
                    tex.SetPixel(x, y, redFletching);
                }
            }
        }

        SaveAndImport(tex, "icon_arrow_corner.png", Vector4.zero);
    }

    // -------------------------------------------------------------
    // 10. TROPHY ICON (64x64)
    // -------------------------------------------------------------
    private static void GenerateTrophyIcon(string fileName, Color32 bright, Color32 mid, Color32 dark)
    {
        int size = 64;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color32 clear = new Color32(0, 0, 0, 0);
        Color32 outline = new Color32(60, 40, 10, 255);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                tex.SetPixel(x, y, clear);

                // Base at y = 8..16
                if (y >= 8 && y <= 16 && Mathf.Abs(x - 32) <= 16)
                {
                    tex.SetPixel(x, y, (Mathf.Abs(x - 32) >= 14 || y <= 10 || y >= 15) ? outline : dark);
                    continue;
                }

                // Stem at y = 16..26
                if (y >= 16 && y <= 26 && Mathf.Abs(x - 32) <= 5)
                {
                    tex.SetPixel(x, y, (Mathf.Abs(x - 32) >= 4) ? outline : mid);
                    continue;
                }

                // Cup body at y = 26..54
                if (y >= 26 && y <= 54)
                {
                    float cupW = 10f + (y - 26f) * 0.45f;
                    if (Mathf.Abs(x - 32) <= cupW)
                    {
                        if (Mathf.Abs(x - 32) >= cupW - 2f || y <= 28 || y >= 52)
                            tex.SetPixel(x, y, outline);
                        else
                            tex.SetPixel(x, y, (x < 32) ? bright : mid);
                        continue;
                    }

                    // Handles
                    if (y >= 34 && y <= 48 && Mathf.Abs(x - 32) <= cupW + 8f && Mathf.Abs(x - 32) >= cupW + 2f)
                    {
                        tex.SetPixel(x, y, dark);
                    }
                }
            }
        }

        SaveAndImport(tex, fileName, Vector4.zero);
    }

    // -------------------------------------------------------------
    // 11. AVATAR ICON (64x64)
    // -------------------------------------------------------------
    private static void GenerateAvatarIcon()
    {
        int size = 64;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color32 clear = new Color32(0, 0, 0, 0);
        Color32 bgBlue = new Color32(65, 145, 225, 255);
        Color32 white = new Color32(255, 255, 255, 255);
        Color32 darkLine = new Color32(25, 75, 140, 255);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dBox = SdfRoundedRect(x, y, 4, 4, 56, 56, 14f);
                if (dBox > 0)
                {
                    tex.SetPixel(x, y, clear);
                    continue;
                }

                if (dBox > -2.5f)
                {
                    tex.SetPixel(x, y, darkLine);
                    continue;
                }

                // Head circle (cx=32, cy=38, r=10)
                float dHead = Mathf.Sqrt(Mathf.Pow(x - 32, 2) + Mathf.Pow(y - 38, 2)) - 10f;
                // Body shoulder (cx=32, cy=12, r=18)
                float dBody = Mathf.Sqrt(Mathf.Pow(x - 32, 2) + Mathf.Pow(y - 12, 2)) - 18f;

                if (dHead <= 0 || (dBody <= 0 && y >= 10 && y <= 26))
                {
                    tex.SetPixel(x, y, white);
                }
                else
                {
                    tex.SetPixel(x, y, bgBlue);
                }
            }
        }

        SaveAndImport(tex, "icon_avatar.png", Vector4.zero);
    }

    // -------------------------------------------------------------
    // 12. ACTION ICONS (Home, Restart, Back, Question, Clock, Target)
    // -------------------------------------------------------------
    private static void GenerateActionIcons()
    {
        // Icon Restart (Circular arrow)
        GenerateIcon("icon_restart.png", 64, (x, y) =>
        {
            float dRing = Mathf.Abs(Mathf.Sqrt(Mathf.Pow(x - 32, 2) + Mathf.Pow(y - 32, 2)) - 18f);
            float angle = Mathf.Atan2(y - 32, x - 32) * Mathf.Rad2Deg;
            if (dRing < 5f && (angle < 45 || angle > 110)) return new Color32(255, 255, 255, 255);
            // Arrow tip at top
            if (x >= 28 && x <= 46 && y >= 40 && y <= 56 && Mathf.Abs(x - 38) + Mathf.Abs(y - 48) < 10) return new Color32(255, 255, 255, 255);
            return new Color32(0, 0, 0, 0);
        });

        // Icon Home (Little house)
        GenerateIcon("icon_home.png", 64, (x, y) =>
        {
            // Roof triangle
            if (y >= 30 && y <= 54 && Mathf.Abs(x - 32) <= (54 - y) * 1.1f) return new Color32(255, 255, 255, 255);
            // House body
            if (y >= 12 && y <= 32 && Mathf.Abs(x - 32) <= 18)
            {
                // Door cutout
                if (y <= 24 && Mathf.Abs(x - 32) <= 6) return new Color32(40, 140, 40, 255);
                return new Color32(255, 255, 255, 255);
            }
            return new Color32(0, 0, 0, 0);
        });

        // Icon Back (Left Arrow)
        GenerateIcon("icon_back.png", 64, (x, y) =>
        {
            if (y >= 26 && y <= 38 && x >= 24 && x <= 50) return new Color32(255, 255, 255, 255);
            if (x >= 12 && x <= 30 && Mathf.Abs(y - 32) <= (x - 12)) return new Color32(255, 255, 255, 255);
            return new Color32(0, 0, 0, 0);
        });

        // Icon Clock (Stopwatch)
        GenerateIcon("icon_clock.png", 64, (x, y) =>
        {
            float r = Mathf.Sqrt(Mathf.Pow(x - 32, 2) + Mathf.Pow(y - 28, 2));
            if (r >= 18f && r <= 24f) return new Color32(255, 255, 255, 255);
            if (r < 18f)
            {
                // Clock hands
                if ((x == 32 && y >= 28 && y <= 42) || (y == 28 && x >= 32 && x <= 40)) return new Color32(20, 60, 20, 255);
                return new Color32(245, 255, 240, 255);
            }
            // Top button
            if (y >= 50 && y <= 56 && Mathf.Abs(x - 32) <= 6) return new Color32(255, 255, 255, 255);
            return new Color32(0, 0, 0, 0);
        });

        // Icon Arrow Mini
        GenerateIcon("icon_arrow_mini.png", 64, (x, y) =>
        {
            // Diagonal arrow
            float shaft = SdfCapsule(x, y, 16, 16, 46, 46, 3f);
            if (shaft <= 0) return new Color32(255, 255, 255, 255);
            float tip = SdfEquilateralTriangle(x - 46, y - 46, 10f, 45f);
            if (tip <= 0) return new Color32(255, 255, 255, 255);
            return new Color32(0, 0, 0, 0);
        });

        // Icon Target Mini
        GenerateIcon("icon_target_mini.png", 64, (x, y) =>
        {
            float r = Mathf.Sqrt(Mathf.Pow(x - 32, 2) + Mathf.Pow(y - 34, 2));
            if (r > 24)
            {
                // Tripod legs
                if (y < 20 && (Mathf.Abs(x - 20) < 3 || Mathf.Abs(x - 44) < 3 || Mathf.Abs(x - 32) < 3)) return new Color32(110, 60, 20, 255);
                return new Color32(0, 0, 0, 0);
            }
            if (r <= 24 && r > 18) return new Color32(235, 60, 45, 255); // Outer red ring
            if (r <= 18 && r > 12) return new Color32(255, 255, 255, 255); // White ring
            if (r <= 12 && r > 6) return new Color32(40, 140, 230, 255);  // Blue ring
            return new Color32(255, 205, 40, 255);                       // Yellow bullseye
        });
    }

    private static void GenerateIcon(string name, int size, Func<int, int, Color32> pixelFunc)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                tex.SetPixel(x, y, pixelFunc(x, y));
            }
        }
        SaveAndImport(tex, name, Vector4.zero);
    }

    // -------------------------------------------------------------
    // SDF MATH HELPERS
    // -------------------------------------------------------------
    private static float SdfRoundedRect(float px, float py, float x, float y, float w, float h, float r)
    {
        float qx = Mathf.Abs(px - (x + w * 0.5f)) - (w * 0.5f - r);
        float qy = Mathf.Abs(py - (y + h * 0.5f)) - (h * 0.5f - r);
        return Mathf.Min(Mathf.Max(qx, qy), 0f) + Mathf.Sqrt(Mathf.Max(qx, 0f) * Mathf.Max(qx, 0f) + Mathf.Max(qy, 0f) * Mathf.Max(qy, 0f)) - r;
    }

    private static float SdfEllipseRotated(float x, float y, float a, float b, float deg)
    {
        float rad = deg * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        float rx = cos * x - sin * y;
        float ry = sin * x + cos * y;
        return (rx * rx) / (a * a) + (ry * ry) / (b * b) - 1f;
    }

    private static float SdfCapsule(float px, float py, float ax, float ay, float bx, float by, float r)
    {
        float pax = px - ax, pay = py - ay;
        float bax = bx - ax, bay = by - ay;
        float h = Mathf.Clamp01((pax * bax + pay * bay) / (bax * bax + bay * bay));
        float dx = pax - bax * h;
        float dy = pay - bay * h;
        return Mathf.Sqrt(dx * dx + dy * dy) - r;
    }

    private static float SdfEquilateralTriangle(float x, float y, float r, float deg)
    {
        float rad = deg * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        float rx = cos * x - sin * y;
        float ry = sin * x + cos * y;
        float k = Mathf.Sqrt(3f);
        rx = Mathf.Abs(rx) - r;
        ry = ry + r / k;
        if (rx + k * ry > 0f)
        {
            float newRx = (rx - k * ry) / 2f;
            float newRy = (-k * rx - ry) / 2f;
            rx = newRx; ry = newRy;
        }
        rx -= Mathf.Clamp(rx, -2f * r, 0f);
        return -Mathf.Sqrt(rx * rx + ry * ry) * Mathf.Sign(ry);
    }
}

