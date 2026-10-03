using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class Task5Setup
{
    [MenuItem("Archery/Setup Task 5 Audio and VFX")]
    public static void RunSetup()
    {
        Debug.Log("[Task5Setup] Starting Task 5 Audio, VFX, and Polish Setup...");

        // 1. Ensure Prefabs directory exists
        if (!AssetDatabase.IsValidFolder("Assets/_Project/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets/_Project", "Prefabs");
        }

        // 2. Create Particle Prefabs
        var mat = AssetDatabase.LoadAssetAtPath<Material>("Packages/com.unity.render-pipelines.universal/Runtime/Materials/ParticlesUnlit.mat");

        GameObject targetVfx = CreateParticlePrefab("TargetHitVFX", new Color(0.75f, 0.52f, 0.28f, 1f), 15, 3.5f, 0.05f, 1.2f, 0.4f, mat);
        GameObject npcVfx = CreateParticlePrefab("NPCHitVFX", new Color(1.0f, 0.72f, 0.15f, 1f), 18, 4.5f, 0.04f, 0.8f, 0.35f, mat);
        GameObject groundVfx = CreateParticlePrefab("GroundHitVFX", new Color(0.55f, 0.50f, 0.45f, 0.7f), 12, 2.0f, 0.12f, 0.4f, 0.45f, mat);

        // 3. Update Arrow Prefab
        string arrowPrefabPath = "Assets/ExternalAssets/Cartoon_Weapon_Pack/Prefab/Arrow.prefab";
        GameObject arrowPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(arrowPrefabPath);
        if (arrowPrefab != null)
        {
            var arrowComp = arrowPrefab.GetComponent<Arrow>();
            if (arrowComp != null)
            {
                arrowComp.targetHitEffectPrefab = targetVfx;
                arrowComp.npcHitEffectPrefab = npcVfx;
                arrowComp.groundHitEffectPrefab = groundVfx;
                EditorUtility.SetDirty(arrowPrefab);
                PrefabUtility.SavePrefabAsset(arrowPrefab);
                Debug.Log("[Task5Setup] Arrow.prefab updated with 3 VFX prefabs.");
            }
        }

        // 4. Update Scene: AudioManager
        var scene = EditorSceneManager.GetActiveScene();
        GameObject gameSystem = GameObject.Find("GAME SYSTEM");
        if (gameSystem == null)
        {
            gameSystem = new GameObject("GAME SYSTEM");
            Undo.RegisterCreatedObjectUndo(gameSystem, "Create GAME SYSTEM");
        }

        var audioMgr = gameSystem.GetComponent<AudioManager>();
        if (audioMgr == null)
        {
            audioMgr = Undo.AddComponent<AudioManager>(gameSystem);
        }

        audioMgr.bowDrawClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/BowDraw.wav");
        audioMgr.bowReleaseClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/BowRelease.wav");
        audioMgr.hitTargetClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/ArrowHitTarget.wav");
        audioMgr.hitNpcClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/ArrowHitNPC.wav");
        audioMgr.hitGroundClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/ArrowHitGround.wav");
        audioMgr.buttonClickClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/ButtonClick.wav");
        audioMgr.bgmClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/MedievalAmbience.wav");

        EditorUtility.SetDirty(gameSystem);

        // 5. Add Controls Guide to MainMenuPanel and PausePanel
        AddControlsGuideToPanel("MainMenuPanel", "ControlsGuide_MainMenu", new Vector2(0f, -380f));
        AddControlsGuideToPanel("PausePanel", "ControlsGuide_Pause", new Vector2(0f, -280f));

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();

        Debug.Log("[Task5Setup] Task 5 Audio and VFX Setup completed successfully!");
    }

    private static GameObject CreateParticlePrefab(string name, Color color, int burstCount, float speed, float size, float gravity, float duration, Material mat)
    {
        string localPath = $"Assets/_Project/Prefabs/{name}.prefab";

        GameObject go = new GameObject(name);
        var ps = go.AddComponent<ParticleSystem>();
        var psr = go.GetComponent<ParticleSystemRenderer>();
        if (mat != null) psr.sharedMaterial = mat;

        // Main module
        var main = ps.main;
        main.duration = duration;
        main.loop = false;
        main.startLifetime = duration;
        main.startSpeed = speed;
        main.startSize = size;
        main.startColor = color;
        main.gravityModifier = gravity;
        main.playOnAwake = true;
        main.stopAction = ParticleSystemStopAction.Destroy;

        // Emission module
        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, burstCount) });

        // Shape module
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Hemisphere;
        shape.radius = 0.05f;

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, localPath);
        Object.DestroyImmediate(go);
        Debug.Log($"[Task5Setup] Created Particle Prefab: {localPath}");
        return prefab;
    }

    private static void AddControlsGuideToPanel(string panelName, string guideName, Vector2 anchoredPos)
    {
        var uim = Object.FindAnyObjectByType<UIManager>();
        GameObject panel = null;
        if (uim != null)
        {
            if (panelName == "MainMenuPanel") panel = uim.mainMenuPanel;
            else if (panelName == "PausePanel") panel = uim.pausePanel;
        }

        if (panel == null)
        {
            var canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas != null)
            {
                var tr = canvas.transform.Find(panelName);
                if (tr != null) panel = tr.gameObject;
            }
        }

        if (panel == null)
        {
            Debug.LogWarning($"[Task5Setup] Could not find panel: {panelName}");
            return;
        }

        Transform existing = panel.transform.Find(guideName);
        if (existing != null)
        {
            Object.DestroyImmediate(existing.gameObject);
        }

        GameObject guideObj = new GameObject(guideName, typeof(RectTransform));
        guideObj.transform.SetParent(panel.transform, false);

        var rt = guideObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = new Vector2(800f, 60f);

        // Background
        var bg = guideObj.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.65f);

        // Text
        GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(guideObj.transform, false);

        var trt = textObj.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = new Vector2(10f, 5f);
        trt.offsetMax = new Vector2(-10f, -5f);

        var tmp = textObj.GetComponent<TextMeshProUGUI>();
        tmp.text = "<b>CONTROLS:</b> WASD: Move | Shift: Sprint | Mouse: Look | LMB: Draw & Shoot | RMB: ADS (Zoom) | R: Cancel Draw | ESC: Pause";
        tmp.fontSize = 17;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(0.92f, 0.92f, 0.92f, 1f);
    }
}
