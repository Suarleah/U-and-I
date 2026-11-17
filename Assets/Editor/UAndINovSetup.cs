#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// One-time setup for the November update (ads, Spam patient, lobotomy blood, observe meter, stack counts).
// Tools > U and I > Run Nov Update Setup. Only adds what's missing, so running it twice is safe.
// Tools > U and I > Apply Preliminary Difficulty Values is separate so it never overwrites your tuning by accident.
// Delete this file once you're happy with the result.
public static class UAndINovSetup
{
    const string AdsLayer = "Ads";
    const string UILayer = "UI";

    const string GameScene = "Assets/Scenes/Playtest/PT-Game.unity";
    const string MinigamePrefab = "Assets/Prefabs/Minigames/Minigame Canvas.prefab";
    const string WirePrefab = "Assets/Scripts/Game/Day/Minigames/Wires/Wire.prefab";
    const string PlayerPrefab = "Assets/Prefabs/Player/Player.prefab";
    const string PatientManagerPrefab = "Assets/Prefabs/Patients/PatientManager.prefab";
    const string ZeroPrefab = "Assets/Prefabs/Patients/PatientZero/PatientZero.prefab";
    const string ZeroSO = "Assets/Prefabs/Patients/PatientZero/PatientZeroSO.asset";
    const string BooPrefab = "Assets/Prefabs/Patients/PatientBoo/PatientBoo.prefab";
    const string ShyPrefab = "Assets/Prefabs/Patients/PatientShy/PatientShy.prefab";
    const string SpamFolder = "Assets/Prefabs/Patients/PatientSpam";
    const string SpamPrefab = SpamFolder + "/PatientSpam.prefab";
    const string SpamSO = SpamFolder + "/PatientSpamSO.asset";
    const string SpamScript = "Assets/Scripts/Patients/PatientTypes/PatientSpam/PatientSpam.cs";
    const string UIFolder = "Assets/Prefabs/UI";
    const string AdPopupPrefab = UIFolder + "/AdPopup.prefab";
    const string CountOutlineMat = UIFolder + "/StackCount Outline.mat";
    const string BloodFolder = "Assets/Visual/To Be Replaced/Lbotomy";
    const string BloodTexture = BloodFolder + "/BloodDrop.png";
    const string BloodMaterial = BloodFolder + "/BloodDrop.mat";

    static StringBuilder report;

    // ───────────────────────────── main ─────────────────────────────

    [MenuItem("Tools/U and I/Run Nov Update Setup")]
    public static void Run()
    {
        if (PrefabStageUtility.GetCurrentPrefabStage() != null)
        {
            EditorUtility.DisplayDialog("U and I setup", "Save and exit Prefab Mode first, then run this again.", "OK");
            return;
        }
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        report = new StringBuilder();

        EnsureSortingLayers(AdsLayer, UILayer);
        if (!LayerExists(AdsLayer) || !LayerExists(UILayer))
        {
            EditorUtility.DisplayDialog("U and I setup", "Added the Ads and UI sorting layers. Unity needs a moment to register them: run Tools > U and I > Run Nov Update Setup once more.", "OK");
            return;
        }

        EnsureFolder(UIFolder);
        EnsureFolder(SpamFolder);

        CreateAdPopupPrefab();
        SetupMinigameCanvas();
        EditPrefab(WirePrefab, root => SetRendererLayers(root, "Wire.prefab"));
        SetupPlayer();
        SetupSpamPatient();
        SetupGameScene();

        AssetDatabase.SaveAssets();
        Debug.Log("[U and I setup]\n" + report);
        EditorUtility.DisplayDialog("U and I setup", "Done. Full list of what changed is in the Console.\n\n" + report, "OK");
    }

    // ───────────────────────────── sorting layers ─────────────────────────────

    static void EnsureSortingLayers(params string[] names)
    {
        var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layers = tagManager.FindProperty("m_SortingLayers");
        var rng = new System.Random();

        foreach (string layerName in names)
        {
            bool exists = false;
            for (int i = 0; i < layers.arraySize; i++)
            {
                if (layers.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue == layerName) exists = true;
            }
            if (exists) continue;

            layers.InsertArrayElementAtIndex(layers.arraySize); // appended = drawn above everything before it
            SerializedProperty e = layers.GetArrayElementAtIndex(layers.arraySize - 1);
            e.FindPropertyRelative("name").stringValue = layerName;
            e.FindPropertyRelative("uniqueID").longValue = rng.Next(1, int.MaxValue);
            SerializedProperty locked = e.FindPropertyRelative("locked");
            if (locked != null)
            {
                if (locked.propertyType == SerializedPropertyType.Boolean) locked.boolValue = false;
                else locked.intValue = 0;
            }
            report.AppendLine($"Added sorting layer '{layerName}'");
        }
        tagManager.ApplyModifiedPropertiesWithoutUndo();
    }

    static bool LayerExists(string layerName)
    {
        foreach (SortingLayer l in SortingLayer.layers)
        {
            if (l.name == layerName) return true;
        }
        return false;
    }

    // Every screen space canvas, line and particle renderer under root goes on the UI layer.
    static void SetRendererLayers(GameObject root, string where)
    {
        int count = 0;
        foreach (Canvas c in root.GetComponentsInChildren<Canvas>(true))
        {
            if (c.renderMode == RenderMode.WorldSpace || c.sortingLayerName == AdsLayer) continue;
            if (c.sortingLayerName != UILayer) { c.sortingLayerName = UILayer; count++; }
        }
        foreach (LineRenderer r in root.GetComponentsInChildren<LineRenderer>(true))
        {
            if (r.sortingLayerName != UILayer) { r.sortingLayerName = UILayer; count++; }
        }
        foreach (ParticleSystemRenderer r in root.GetComponentsInChildren<ParticleSystemRenderer>(true))
        {
            if (IsUnderScreenCanvas(r.transform) && r.sortingLayerName != UILayer) { r.sortingLayerName = UILayer; count++; }
        }
        if (count > 0) report.AppendLine($"{where}: moved {count} canvases/renderers to the UI sorting layer");
    }

    static bool IsUnderScreenCanvas(Transform t)
    {
        Canvas c = t.GetComponentInParent<Canvas>(true);
        return c != null && c.renderMode != RenderMode.WorldSpace;
    }

    // ───────────────────────────── ad popup prefab ─────────────────────────────

    static void CreateAdPopupPrefab()
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(AdPopupPrefab)) return;

        var go = new GameObject("AdPopup", typeof(RectTransform));
        var rt = (RectTransform)go.transform;
        rt.sizeDelta = new Vector2(350f, 260f);

        Image art = go.AddComponent<Image>();
        art.raycastTarget = true; // blocks what's behind the ad

        RectTransform labelRt = NewUI("Label", rt);
        Stretch(labelRt, 16f);
        TextMeshProUGUI label = AddText(labelRt, "AD", 32f);
        label.enableAutoSizing = true;
        label.fontSizeMin = 14f;
        label.fontSizeMax = 48f;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.black;
        label.fontStyle = FontStyles.Bold;

        RectTransform closeRt = NewUI("Close", rt);
        closeRt.anchorMin = closeRt.anchorMax = closeRt.pivot = new Vector2(1f, 1f);
        closeRt.anchoredPosition = new Vector2(-6f, -6f);
        closeRt.sizeDelta = new Vector2(36f, 36f);
        Image closeImg = closeRt.gameObject.AddComponent<Image>();
        closeImg.sprite = UISprite();
        closeImg.color = new Color(0.85f, 0.1f, 0.1f);
        Button closeButton = closeRt.gameObject.AddComponent<Button>();
        closeButton.targetGraphic = closeImg;

        RectTransform xRt = NewUI("X", closeRt);
        Stretch(xRt, 0f);
        TextMeshProUGUI x = AddText(xRt, "X", 24f);
        x.alignment = TextAlignmentOptions.Center;
        x.color = Color.white;
        x.fontStyle = FontStyles.Bold;

        AdPopup popup = go.AddComponent<AdPopup>();
        popup.art = art;
        popup.placeholderLabel = label;
        popup.closeButton = closeButton;

        PrefabUtility.SaveAsPrefabAsset(go, AdPopupPrefab);
        Object.DestroyImmediate(go);
        report.AppendLine("Created " + AdPopupPrefab);
    }

    // ───────────────────────────── minigame canvas ─────────────────────────────

    static void SetupMinigameCanvas()
    {
        EditPrefab(MinigamePrefab, root =>
        {
            SetRendererLayers(root, "Minigame Canvas");
            SetupLobotomy(root);
            SetupObserveMeter(root);
            SetupWireColors(root);
        });
    }

    static void SetupLobotomy(GameObject root)
    {
        LobotomyMinigame lob = root.GetComponentInChildren<LobotomyMinigame>(true);
        if (!lob) { report.AppendLine("WARNING: no LobotomyMinigame found in Minigame Canvas"); return; }

        var eye = FindDeep(lob.transform, "Eye") as RectTransform;
        if (eye && lob.pointsParent != eye)
        {
            lob.pointsParent = eye;
            report.AppendLine("Lobotomy: Points Parent set to Eye (the arch is measured across Eye's rect)");
        }
        lob.useArch = true;

        if (!lob.bloodParticles)
        {
            lob.bloodParticles = CreateBloodParticles(lob.transform);
            report.AppendLine("Lobotomy: created BloodParticles");
        }
    }

    static ParticleSystem CreateBloodParticles(Transform parent)
    {
        var go = new GameObject("BloodParticles");
        go.transform.SetParent(parent, false);
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;

        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        // Sizes/speeds are world units, picked for an orthographic camera size of 10 (your game camera):
        // 1080 px = 20 units, so drops are ~5-15 px and fly ~50-150 px/s. Scale them if your minigame camera differs.
        var main = ps.main;
        main.duration = 1f;
        main.loop = false;
        main.playOnAwake = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.scalingMode = ParticleSystemScalingMode.Local;
        main.maxParticles = 300;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.9f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.9f, 2.8f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.09f, 0.28f);
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.45f, 0f, 0f), new Color(0.85f, 0.05f, 0.05f));
        main.gravityModifier = 0.55f;

        var emission = ps.emission;
        emission.rateOverTime = 0f; // LobotomyMinigame calls Emit itself

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.04f;
        shape.arc = 360f;
        shape.rotation = Vector3.zero;

        var size = ps.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0.3f));

        var color = ps.colorOverLifetime;
        color.enabled = true;
        var fade = new Gradient();
        fade.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.6f), new GradientAlphaKey(0f, 1f) });
        color.color = fade;

        var r = go.GetComponent<ParticleSystemRenderer>();
        r.sortingLayerName = UILayer;
        r.sortingOrder = 103; // above DrawnLine (102)
        Material mat = GetBloodMaterial();
        if (mat) r.sharedMaterial = mat;
        else report.AppendLine("WARNING: couldn't find a material for BloodParticles; assign one by hand (Sprite-Unlit-Default works)");

        return ps;
    }

    static Material GetBloodMaterial()
    {
        var existing = AssetDatabase.LoadAssetAtPath<Material>(BloodMaterial);
        if (existing) return existing;

        Material baseMat =
            AssetDatabase.LoadAssetAtPath<Material>("Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat") ??
            AssetDatabase.LoadAssetAtPath<Material>("Assets/Scripts/Game/Day/Minigames/Lobotomy/LineMaterial.mat");
        if (!baseMat) return null;

        // soft round drop so particles aren't squares
        EnsureFolder(BloodFolder);
        if (!File.Exists(BloodTexture))
        {
            const int n = 32;
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false);
            for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(n / 2f, n / 2f)) / (n / 2f);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01((1f - d) * 3f)));
            }
            File.WriteAllBytes(BloodTexture, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(BloodTexture);
            if (AssetImporter.GetAtPath(BloodTexture) is TextureImporter ti)
            {
                ti.alphaIsTransparency = true;
                ti.mipmapEnabled = false;
                ti.SaveAndReimport();
            }
        }

        var mat = new Material(baseMat) { name = "BloodDrop" };
        var drop = AssetDatabase.LoadAssetAtPath<Texture2D>(BloodTexture);
        if (drop) mat.mainTexture = drop;
        AssetDatabase.CreateAsset(mat, BloodMaterial);
        report.AppendLine("Created " + BloodMaterial);
        return mat;
    }

    static void SetupObserveMeter(GameObject root)
    {
        ObserveMinigame obs = root.GetComponentInChildren<ObserveMinigame>(true);
        if (!obs) { report.AppendLine("WARNING: no ObserveMinigame found in Minigame Canvas"); return; }
        if (obs.observationMeter) return;

        RectTransform bg = NewUI("MeterBG", obs.transform);
        bg.anchorMin = bg.anchorMax = bg.pivot = new Vector2(0.5f, 1f);
        bg.anchoredPosition = new Vector2(0f, -40f);
        bg.sizeDelta = new Vector2(600f, 28f);
        bg.SetAsLastSibling(); // draws above HidingLayer
        Image bgImg = bg.gameObject.AddComponent<Image>();
        bgImg.color = new Color(0f, 0f, 0f, 0.6f);
        bgImg.raycastTarget = false;

        RectTransform fill = NewUI("MeterFill", bg);
        Stretch(fill, 4f);
        Image fillImg = fill.gameObject.AddComponent<Image>();
        fillImg.sprite = UISprite(); // Filled needs a sprite
        fillImg.type = Image.Type.Filled;
        fillImg.fillMethod = Image.FillMethod.Horizontal;
        fillImg.fillOrigin = (int)Image.OriginHorizontal.Left;
        fillImg.fillAmount = 0f;
        fillImg.raycastTarget = false;

        obs.observationMeter = fillImg;
        report.AppendLine("Observe: created MeterBG/MeterFill and assigned Observation Meter");
    }

    static void SetupWireColors(GameObject root)
    {
        ConnectWires wires = root.GetComponentInChildren<ConnectWires>(true);
        if (!wires) return;

        var colors = new List<Color>(wires.wireColors ?? new Color[0]);
        Color[] extra = { Color.red, Color.blue, Color.yellow, Color.green, Color.magenta, new Color(1f, 0.5f, 0f), Color.cyan, Color.white };
        int added = 0;
        foreach (Color c in extra)
        {
            if (colors.Count >= 6) break;
            if (!colors.Contains(c)) { colors.Add(c); added++; }
        }
        if (added > 0)
        {
            wires.wireColors = colors.ToArray();
            report.AppendLine($"Wires: added {added} wire colors (Spam uses 6 wires)");
        }
    }

    // ───────────────────────────── player ─────────────────────────────

    static void SetupPlayer()
    {
        EditPrefab(PlayerPrefab, root =>
        {
            SetRendererLayers(root, "Player.prefab");

            InventoryManager inv = root.GetComponentInChildren<InventoryManager>(true);
            if (!inv) { report.AppendLine("WARNING: no InventoryManager on Player.prefab"); return; }

            bool hasAny = false;
            if (inv.stackCountTexts != null)
                foreach (var t in inv.stackCountTexts) if (t) hasAny = true;
            if (hasAny) return;

            int slots = inv.itemIcons != null && inv.itemIcons.Length > 0 ? inv.itemIcons.Length : 4;
            var texts = new TextMeshProUGUI[slots];
            Material outline = GetCountOutlineMaterial();

            for (int i = 0; i < slots; i++)
            {
                Transform slot = FindDeep(root.transform, "Slot" + i);
                if (!slot) { report.AppendLine($"WARNING: couldn't find Slot{i} for its stack count"); continue; }

                RectTransform rt = NewUI("Count" + i, slot);
                rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(1f, 0f);
                rt.anchoredPosition = new Vector2(-4f, 4f);
                rt.sizeDelta = new Vector2(40f, 30f);
                rt.SetAsLastSibling(); // above the icon and selection frame

                TextMeshProUGUI t = AddText(rt, "2", 22f);
                t.alignment = TextAlignmentOptions.BottomRight;
                t.color = Color.white;
                if (outline) t.fontSharedMaterial = outline;
                texts[i] = t;
            }

            inv.stackCountTexts = texts;
            report.AppendLine($"Player: created Count0-Count{slots - 1} and assigned Stack Count Texts");
        });
    }

    static Material GetCountOutlineMaterial()
    {
        var existing = AssetDatabase.LoadAssetAtPath<Material>(CountOutlineMat);
        if (existing) return existing;

        TMP_FontAsset font = TMP_Settings.defaultFontAsset;
        if (!font || !font.material) return null;

        // a copy of the font material, so only these numbers get an outline
        var mat = new Material(font.material) { name = "StackCount Outline" };
        mat.EnableKeyword(ShaderUtilities.Keyword_Outline);
        mat.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.25f);
        mat.SetColor(ShaderUtilities.ID_OutlineColor, Color.black);
        AssetDatabase.CreateAsset(mat, CountOutlineMat);
        return mat;
    }

    // ───────────────────────────── spam patient ─────────────────────────────

    static void SetupSpamPatient()
    {
        if (!AssetDatabase.LoadAssetAtPath<GameObject>(SpamPrefab))
        {
            if (!AssetDatabase.CopyAsset(ZeroPrefab, SpamPrefab))
            {
                report.AppendLine("WARNING: couldn't copy PatientZero.prefab for Spam");
                return;
            }

            // same as the Debug-inspector script swap: keeps every reference on the prefab
            var spamScript = AssetDatabase.LoadAssetAtPath<MonoScript>(SpamScript);
            EditPrefab(SpamPrefab, root =>
            {
                PatientZero zero = root.GetComponentInChildren<PatientZero>(true);
                if (!zero || !spamScript) { report.AppendLine("WARNING: couldn't swap PatientZero for PatientSpam"); return; }
                var so = new SerializedObject(zero);
                so.FindProperty("m_Script").objectReferenceValue = spamScript;
                so.ApplyModifiedPropertiesWithoutUndo();
            });

            GameObject check = AssetDatabase.LoadAssetAtPath<GameObject>(SpamPrefab);
            report.AppendLine(check && check.GetComponentInChildren<PatientSpam>(true)
                ? "Created PatientSpam.prefab (copy of PatientZero with the script swapped)"
                : "WARNING: PatientSpam.prefab was created but doesn't have a PatientSpam component; swap it by hand");
        }

        GameObject spamGo = AssetDatabase.LoadAssetAtPath<GameObject>(SpamPrefab);

        var spamSO = AssetDatabase.LoadAssetAtPath<PatientSO>(SpamSO);
        if (!spamSO)
        {
            AssetDatabase.CopyAsset(ZeroSO, SpamSO);
            spamSO = AssetDatabase.LoadAssetAtPath<PatientSO>(SpamSO);
            if (spamSO)
            {
                int maxId = 0;
                foreach (string guid in AssetDatabase.FindAssets("t:PatientSO"))
                {
                    var other = AssetDatabase.LoadAssetAtPath<PatientSO>(AssetDatabase.GUIDToAssetPath(guid));
                    if (other && other != spamSO) maxId = Mathf.Max(maxId, other.id);
                }
                spamSO.id = maxId + 1;
                spamSO.namee = "PatientSpam";
                spamSO.desc = "Won't stop showing you ads. Gets worse the less patient it is.";
                spamSO.prefab = spamGo;
                EditorUtility.SetDirty(spamSO);
                report.AppendLine($"Created PatientSpamSO (id {spamSO.id}); photo and logs are still Zero's, replace them");
            }
        }

        if (spamSO)
        {
            EditPrefab(PatientManagerPrefab, root =>
            {
                PatientManager pm = root.GetComponentInChildren<PatientManager>(true);
                if (pm && !pm.allPatients.Contains(spamSO))
                {
                    pm.allPatients.Add(spamSO);
                    report.AppendLine("PatientManager: added PatientSpamSO to All Patients");
                }
            });
        }
    }

    // ───────────────────────────── game scene ─────────────────────────────

    static void SetupGameScene()
    {
        string previous = SceneManager.GetActiveScene().path;
        Scene scene = EditorSceneManager.OpenScene(GameScene, OpenSceneMode.Single);

        Camera uiCam = null;
        AdOverlay overlay = null;
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (!overlay) overlay = root.GetComponentInChildren<AdOverlay>(true);
            foreach (Canvas c in root.GetComponentsInChildren<Canvas>(true))
            {
                if (c.renderMode != RenderMode.WorldSpace && c.worldCamera && (!uiCam || c.name == "Stats")) uiCam = c.worldCamera;
            }
            SetRendererLayers(root, "PT-Game/" + root.name);
        }

        if (!overlay)
        {
            var go = new GameObject("AdOverlayCanvas", typeof(RectTransform));
            Canvas canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = uiCam ? uiCam : Camera.main;
            canvas.planeDistance = 100f;
            canvas.sortingLayerName = AdsLayer;
            canvas.sortingOrder = 0;

            CanvasScaler scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            go.AddComponent<GraphicRaycaster>();
            overlay = go.AddComponent<AdOverlay>();
            report.AppendLine("PT-Game: created AdOverlayCanvas" + (canvas.worldCamera ? $" (camera: {canvas.worldCamera.name})" : " (WARNING: no camera found, assign Render Camera)"));
        }
        if (!overlay.adPrefab)
        {
            overlay.adPrefab = AssetDatabase.LoadAssetAtPath<AdPopup>(AdPopupPrefab);
            EditorUtility.SetDirty(overlay);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        if (!string.IsNullOrEmpty(previous) && previous != GameScene)
        {
            EditorSceneManager.OpenScene(previous, OpenSceneMode.Single);
        }
    }

    // ───────────────────────────── difficulty values ─────────────────────────────

    [MenuItem("Tools/U and I/Apply Preliminary Difficulty Values")]
    public static void ApplyDifficulty()
    {
        if (!EditorUtility.DisplayDialog("U and I setup",
            "Overwrite the Operation Difficulty values on Zero, Boo, Shy and Spam with the preliminary table?", "Overwrite", "Cancel")) return;

        report = new StringBuilder();
        //                    speed  wander            par  perRoll  time  ampTol  freqTol  irrit  offCurve  blink  maxSpd  wires
        SetDifficulty(ZeroPrefab, 200f, new Vector2(1f, 3f),     13f, 3f,  10f, 0.1f,  0.1f,  0.15f, 30f, 0.75f, 400f, 4);
        SetDifficulty(BooPrefab,  120f, new Vector2(2f, 4f),     12f, 4f,  12f, 0.12f, 0.12f, 0.10f, 35f, 0.75f, 450f, 3);
        SetDifficulty(ShyPrefab,  320f, new Vector2(0.5f, 1.5f), 16f, 2f,  8f,  0.08f, 0.08f, 0.20f, 25f, 0.6f,  350f, 5);
        SetDifficulty(SpamPrefab, 250f, new Vector2(1f, 2f),     15f, 3f,  10f, 0.1f,  0.1f,  0.15f, 30f, 0.9f,  400f, 6);
        AssetDatabase.SaveAssets();
        Debug.Log("[U and I difficulty]\n" + report);
    }

    static void SetDifficulty(string prefab, float speed, Vector2 wander, float par, float perRoll,
        float time, float ampTol, float freqTol, float irritation, float offCurve, float blink, float maxSpeed, int wires)
    {
        if (!AssetDatabase.LoadAssetAtPath<GameObject>(prefab)) { report.AppendLine("Skipped (missing) " + prefab); return; }

        EditPrefab(prefab, root =>
        {
            Patient p = root.GetComponentInChildren<Patient>(true);
            if (!p) { report.AppendLine("WARNING: no Patient on " + prefab); return; }

            p.observeSettings.patientSpeed = speed;
            p.observeSettings.wanderInterval = wander;
            p.observeSettings.parTime = par;
            p.observeSettings.secondsPerRollLost = perRoll;

            p.therapySettings.timeGiven = time;
            p.therapySettings.amplitudeTolerance = ampTol;
            p.therapySettings.frequencyTolerance = freqTol;

            p.lobotomySettings.irritationSpeed = irritation;
            p.lobotomySettings.maxOffCurve = offCurve;
            p.lobotomySettings.blinkDuration = blink;
            p.lobotomySettings.maxSpeed = maxSpeed;

            p.wiresSettings.wireCount = wires;
            report.AppendLine("Set difficulty on " + Path.GetFileNameWithoutExtension(prefab));
        });
    }

    // ───────────────────────────── helpers ─────────────────────────────

    static void EditPrefab(string path, System.Action<GameObject> edit)
    {
        if (!AssetDatabase.LoadAssetAtPath<GameObject>(path))
        {
            report.AppendLine("WARNING: missing " + path);
            return;
        }
        GameObject root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            edit(root);
            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = Path.GetDirectoryName(path).Replace('\\', '/');
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
    }

    // breadth-first search by name, ignoring case and spaces ("Eye Shake" == "Eyeshake")
    static Transform FindDeep(Transform root, string name)
    {
        string target = Norm(name);
        var queue = new Queue<Transform>();
        queue.Enqueue(root);
        while (queue.Count > 0)
        {
            Transform t = queue.Dequeue();
            if (t != root && Norm(t.name) == target) return t;
            foreach (Transform child in t) queue.Enqueue(child);
        }
        return null;
    }

    static string Norm(string s) => s.Replace(" ", "").ToLowerInvariant();

    static RectTransform NewUI(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    static void Stretch(RectTransform rt, float padding)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(padding, padding);
        rt.offsetMax = new Vector2(-padding, -padding);
    }

    static TextMeshProUGUI AddText(RectTransform rt, string text, float size)
    {
        TextMeshProUGUI t = rt.gameObject.AddComponent<TextMeshProUGUI>();
        if (!t.font) t.font = TMP_Settings.defaultFontAsset;
        t.text = text;
        t.fontSize = size;
        t.raycastTarget = false;
        return t;
    }

    static Sprite UISprite() => AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
}
#endif
