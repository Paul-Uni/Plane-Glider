using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

// Tools > Plane Glider > Build Levels 3 + 4
// Baut die Levels aus den Layouts unten (Level3 / Level4). Zum Ändern: Zahlen anpassen und neu bauen.
//
// Koordinaten: Spieler startet bei x = -5, y = 0. Spielfeld y ca. -4.3 (Boden) bis 4.5 (oben).
// Ohne Turbo: 2 Einheiten/s nach vorne, 2 Einheiten/s hoch/runter -> max. 45° Steigung.
public static class LevelBuilder
{
    private const string TemplateScene = "Assets/Scenes/Level_1.unity";
    private const string HomeScene = "Assets/Scenes/HomeScreen.unity";
    private const float PlayerStartX = -5f;
    private const float PlayerSpeed = 2f;
    private const float RockBottom = -6f;   // Felsen von unten starten unter dem Boden
    private const float RockTop = 6.5f;     // Felsen von oben starten über dem Bild
    private const float GroundY = -4.65f;
    private const float GroundScaleX = 1.804f;
    private const float DefaultRockWidth = 2.2f;

    // ------------------------------------------------------------------ Layouts

    private static LevelDef Level3() => new LevelDef
    {
        Scene = "Level_3",
        Title = "LEVEL 3",
        RockUp = "Assets/B2/Images/Basic Obstacles/rockIce.png",
        RockDown = "Assets/B2/Images/Basic Obstacles/rockIceDown.png",
        Ground = "Assets/B2/Images/Ground/groundIce.png",
        GoalX = 57f,
        StartFuel = 100f,
        Layout = b =>
        {
            // A: Aufwärmen - drei Tore im Zickzack
            b.Gate(3f, 1.0f, 3.2f);
            b.Item(6f, 1.0f);
            b.Gate(9f, -1.5f, 3.2f);
            b.Item(12f, 0.3f);
            b.Gate(15f, 2.0f, 3.2f);
            b.Item(16.5f, 3.3f);           // riskant: danach schnell runter unter die Decke

            // B: niedrige Eisdecke, darunter zwei hüpfende Frösche
            b.RockDown(19f, 2.2f, 2.0f);
            b.Frog(21f, -1.0f);
            b.RockDown(23f, 2.2f, 2.0f);
            b.Item(23f, 1.2f);             // knapp unter der Spitze
            b.Frog(25f, -1.0f);
            b.RockDown(27f, 2.2f, 2.0f);
            b.Item(29f, -3.0f);

            // C: Bär fegt diagonal durchs Bild, danach Croc unter einem Zapfen
            b.Item(33f, 3.5f);
            b.Bear(36f, 0f);
            b.RockDown(40f, 1.5f);
            b.Croc(40f, -3.3f);
            b.Item(43f, -1.0f);

            // D: Schlussspurt - zwei enge Tore, Sprit lockt nach oben
            b.Gate(47f, 0f, 2.8f);
            b.Item(50f, 3.0f);
            b.Gate(53f, 0.5f, 3.0f);
        }
    };

    private static LevelDef Level4() => new LevelDef
    {
        Scene = "Level_4",
        Title = "LEVEL 4",
        RockUp = "Assets/B2/Images/Basic Obstacles/rockGrass.png",
        RockDown = "Assets/B2/Images/Basic Obstacles/rockGrassDown.png",
        Ground = "Assets/B2/Images/Ground/groundGrass.png",
        GoalX = 66f,
        StartFuel = 100f,
        Layout = b =>
        {
            // A: enge Treppe
            b.Gate(2f, 2.0f, 2.6f);
            b.Item(4.2f, 0.5f);
            b.Gate(6.5f, -1.0f, 2.6f);
            b.Item(8.7f, 0.3f);
            b.Gate(11f, 1.5f, 2.6f);

            // B: Narwal-Schwarm - drei Wellen kommen dir entgegen (Lücken suchen!)
            //    Narwhal(x, y): x = wo du ihn OHNE Turbo triffst
            b.Narwhal(17f, -3.2f); b.Narwhal(17f, -1.6f); b.Narwhal(17f, 2.6f); b.Narwhal(17f, 4.1f);
            b.Item(19f, 2.0f);
            b.Narwhal(21f, -3.8f); b.Narwhal(21f, -0.2f); b.Narwhal(21f, 1.2f); b.Narwhal(21f, 4.4f);
            b.Item(23f, 2.2f);
            b.Narwhal(25f, -2.6f); b.Narwhal(25f, -1.1f); b.Narwhal(25f, 0.4f); b.Narwhal(25f, 3.9f);
            b.Item(27f, 0.5f);

            // C: Croc-Graben mit Bär in der Mitte
            b.RockDown(30f, 2.6f);
            b.Croc(30f, -3.4f);
            b.Bear(34f, 0f);
            b.Item(34f, 3.8f);
            b.RockDown(38f, 2.6f);
            b.Croc(38f, -3.4f);
            b.Item(41f, -1.0f);

            // D: Tunnel mit drei Fröschen (alle im Gleichtakt)
            b.Gate(43f, -0.2f, 4.8f, 2.0f);
            b.Frog(45f, -0.2f);
            b.Gate(47f, -0.2f, 4.8f, 2.0f);
            b.Item(47f, 1.6f);             // knapp unter der Spitze
            b.Frog(49f, -0.2f);
            b.Gate(51f, -0.2f, 4.8f, 2.0f);
            b.Item(51f, -2.0f);            // knapp über der Spitze
            b.Frog(53f, -0.2f);
            b.Gate(55f, -0.2f, 4.8f, 2.0f);

            // E: Nadelöhr kurz vorm Ziel
            b.Item(56f, -2.4f);
            b.Gate(60f, 0.5f, 2.2f);
            b.Croc(62.5f, 2.6f, 0.6f);
        }
    };

    // ------------------------------------------------------------------ Menü

    [MenuItem("Tools/Plane Glider/Build Levels 3 + 4")]
    private static void BuildAll()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        if (!EditorUtility.DisplayDialog("Level Builder",
                "Level_3 und Level_4 werden (neu) gebaut, in die Build Settings eingetragen und im HomeScreen verlinkt.\n\nBestehende Level_3/Level_4-Szenen werden überschrieben.",
                "Bauen", "Abbrechen")) return;

        LevelDef[] levels = { Level3(), Level4() };
        foreach (LevelDef def in levels) Build(def);
        UpdateHomeScreen(levels);

        EditorSceneManager.OpenScene("Assets/Scenes/" + levels[0].Scene + ".unity");
        EditorUtility.DisplayDialog("Level Builder", "Fertig! Level_3 ist geöffnet. Starte das Spiel am besten über den HomeScreen.", "OK");
    }

    // ------------------------------------------------------------------ Bauen

    private static void Build(LevelDef def)
    {
        string path = "Assets/Scenes/" + def.Scene + ".unity";
        AssetDatabase.DeleteAsset(path);
        if (!AssetDatabase.CopyAsset(TemplateScene, path))
            throw new Exception("Konnte " + TemplateScene + " nicht kopieren.");

        Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        ClearLevel(scene);

        GameObject root = new GameObject("Generated Level");
        LevelContext ctx = new LevelContext(root.transform, def);
        ctx.GroundStrip(-14f, def.GoalX + 14f);
        def.Layout(ctx);

        foreach (Transform t in AllTransforms(scene))
        {
            if (t.CompareTag("Goal"))
                t.position = new Vector3(def.GoalX, t.position.y, t.position.z);

            PlayerController player = t.GetComponent<PlayerController>();
            if (player != null)
            {
                SerializedObject so = new SerializedObject(player);
                so.FindProperty("fuel").floatValue = def.StartFuel;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AddToBuildSettings(path);
        Debug.Log($"[LevelBuilder] {def.Scene} gebaut ({ctx.Count} Objekte).");
    }

    // Entfernt alle Hindernisse, Items und Boden aus der kopierten Vorlage
    private static void ClearLevel(Scene scene)
    {
        HashSet<GameObject> toDelete = new HashSet<GameObject>();
        foreach (Transform t in AllTransforms(scene))
        {
            GameObject go = t.gameObject;
            if (!go.CompareTag("Obstacle") && !go.CompareTag("FuelItem") && !go.CompareTag("Ground")) continue;
            // Teile von Prefab-Instanzen kann man nicht einzeln löschen -> ganze Instanz
            toDelete.Add(PrefabUtility.IsPartOfPrefabInstance(go) ? PrefabUtility.GetOutermostPrefabInstanceRoot(go) : go);
        }
        foreach (GameObject go in toDelete)
            if (go != null) Object.DestroyImmediate(go);
    }

    private static IEnumerable<Transform> AllTransforms(Scene scene)
    {
        return scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)).ToList();
    }

    private static void AddToBuildSettings(string path)
    {
        List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes.ToList();
        if (scenes.Any(s => s.path == path)) return;
        scenes.Add(new EditorBuildSettingsScene(path, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }

    // ------------------------------------------------------------------ HomeScreen

    private static void UpdateHomeScreen(LevelDef[] newLevels)
    {
        Scene scene = EditorSceneManager.OpenScene(HomeScene, OpenSceneMode.Single);
        List<Button> buttons = AllTransforms(scene).Select(t => t.GetComponent<Button>()).Where(b => b != null).ToList();

        Button level1 = buttons.FirstOrDefault(b => HasMethod(b, "LoadLevel1"));
        Button level2 = buttons.FirstOrDefault(b => HasMethod(b, "LoadLevel2"));
        if (level1 == null || level2 == null)
        {
            Debug.LogError("[LevelBuilder] Level-1/2-Buttons im HomeScreen nicht gefunden - Buttons bitte von Hand anlegen.");
            return;
        }

        // 2x2-Raster: oben Level 1+2, unten die neuen
        SetAnchors(level1, 0.10f, 0.52f, 0.45f, 0.88f);
        SetAnchors(level2, 0.55f, 0.52f, 0.90f, 0.88f);
        SetLabel(level1, "LEVEL 1");
        SetLabel(level2, "LEVEL 2");

        Vector4[] slots = { new Vector4(0.10f, 0.08f, 0.45f, 0.44f), new Vector4(0.55f, 0.08f, 0.90f, 0.44f) };
        for (int i = 0; i < newLevels.Length && i < slots.Length; i++)
        {
            LevelDef def = newLevels[i];
            string name = "Button " + def.Scene;
            Transform existing = level2.transform.parent.Find(name);
            GameObject go = existing != null ? existing.gameObject : Object.Instantiate(level2.gameObject, level2.transform.parent);
            go.name = name;

            // Klick -> LevelSelect.Load("Level_X")
            SelectLvL2 oldSelect = go.GetComponent<SelectLvL2>();
            if (oldSelect != null) Object.DestroyImmediate(oldSelect);
            Button button = go.GetComponent<Button>();
            while (button.onClick.GetPersistentEventCount() > 0)
                UnityEventTools.RemovePersistentListener(button.onClick, 0);
            LevelSelect select = go.GetComponent<LevelSelect>();
            if (select == null) select = go.AddComponent<LevelSelect>();
            UnityEventTools.AddStringPersistentListener(button.onClick, new UnityAction<string>(select.Load), def.Scene);

            // Vorschaubild ist ein Platzhalter (Screenshot von Level 2) -> leicht getönt
            go.GetComponent<Image>().color = i == 0 ? new Color(0.75f, 0.9f, 1f) : new Color(0.8f, 1f, 0.75f);

            Vector4 s = slots[i];
            SetAnchors(button, s.x, s.y, s.z, s.w);
            SetLabel(button, def.Title);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static bool HasMethod(Button b, string method)
    {
        for (int i = 0; i < b.onClick.GetPersistentEventCount(); i++)
            if (b.onClick.GetPersistentMethodName(i) == method) return true;
        return false;
    }

    private static void SetAnchors(Button b, float minX, float minY, float maxX, float maxY)
    {
        RectTransform rt = (RectTransform)b.transform;
        rt.anchorMin = new Vector2(minX, minY);
        rt.anchorMax = new Vector2(maxX, maxY);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    // Dunkler Streifen unten im Button mit Text
    private static void SetLabel(Button b, string text)
    {
        Transform bar = b.transform.Find("Label Bar");
        if (bar == null)
        {
            GameObject barGo = new GameObject("Label Bar", typeof(RectTransform), typeof(Image));
            bar = barGo.transform;
            bar.SetParent(b.transform, false);
            RectTransform br = (RectTransform)bar;
            br.anchorMin = new Vector2(0f, 0f);
            br.anchorMax = new Vector2(1f, 0.22f);
            br.offsetMin = br.offsetMax = Vector2.zero;
            Image img = barGo.GetComponent<Image>();
            img.color = new Color(0f, 0f, 0f, 0.55f);
            img.raycastTarget = false;

            GameObject textGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            RectTransform tr = (RectTransform)textGo.transform;
            tr.SetParent(bar, false);
            tr.anchorMin = Vector2.zero;
            tr.anchorMax = Vector2.one;
            tr.offsetMin = tr.offsetMax = Vector2.zero;
            TextMeshProUGUI tmp = textGo.GetComponent<TextMeshProUGUI>();
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 10;
            tmp.fontSizeMax = 60;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
        }
        bar.GetComponentInChildren<TextMeshProUGUI>().text = text;
    }

    // ------------------------------------------------------------------ Datentypen

    private class LevelDef
    {
        public string Scene, Title, RockUp, RockDown, Ground;
        public float GoalX, StartFuel;
        public Action<LevelContext> Layout;
    }

    private class LevelContext
    {
        private readonly Transform parent;
        private readonly Sprite rockUp, rockDown, ground;
        private readonly GameObject frog, bear, croc, narwhal, item;
        private readonly float narwhalSpeed;
        public int Count;

        public LevelContext(Transform parent, LevelDef def)
        {
            this.parent = parent;
            rockUp = LoadSprite(def.RockUp);
            rockDown = LoadSprite(def.RockDown);
            ground = LoadSprite(def.Ground);
            frog = LoadPrefab<VerticalMovingObstacle>("Assets/Frog.prefab");
            bear = LoadPrefab<MovingObstacle>("Assets/B\u00e4r.prefab");
            croc = LoadPrefab<ExpandingObstacle>("Assets/Croc.prefab");
            narwhal = LoadPrefab<ShootingObstacle>("Assets/Narwahl.prefab");
            item = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/B2/Item.prefab");
            if (item == null) throw new Exception("Prefab nicht gefunden: Assets/B2/Item.prefab");
            ShootingObstacle shooter = narwhal.GetComponent<ShootingObstacle>();
            narwhalSpeed = shooter != null ? shooter.speed : 3f;
        }

        // Tor: Felsen von oben und unten, Lücke um centerY
        public void Gate(float x, float centerY, float gap, float width = DefaultRockWidth)
        {
            RockDown(x, centerY + gap / 2f, width);
            RockUp(x, centerY - gap / 2f, width);
        }

        public void RockUp(float x, float tipY, float width = DefaultRockWidth) => Rock("Rock Up", rockUp, x, RockBottom, tipY, width);
        public void RockDown(float x, float tipY, float width = DefaultRockWidth) => Rock("Rock Down", rockDown, x, RockTop, tipY, width);

        public void Item(float x, float y) => Spawn(item, x, y, "Fuel");
        public void Frog(float x, float y) => Spawn(frog, x, y, "Frog");
        public void Bear(float x, float y) => Spawn(bear, x, y, "Bear");

        public void Croc(float x, float y, float maxScale = -1f)
        {
            GameObject go = Spawn(croc, x, y, "Croc");
            if (maxScale <= 0f) return;
            ExpandingObstacle e = go.GetComponent<ExpandingObstacle>();
            e.maxScale = maxScale;
            PrefabUtility.RecordPrefabInstancePropertyModifications(e);
        }

        // Narwale schwimmen ab Levelstart nach links. meetX = wo der Spieler (ohne Turbo) auf sie trifft.
        public void Narwhal(float meetX, float y)
        {
            float startX = PlayerStartX + (meetX - PlayerStartX) * (PlayerSpeed + narwhalSpeed) / PlayerSpeed;
            Spawn(narwhal, startX, y, "Narwhal");
        }

        public void GroundStrip(float fromX, float toX)
        {
            float width = ground.bounds.size.x * GroundScaleX * 0.98f; // leichte Überlappung
            for (float x = fromX; x < toX; x += width)
            {
                GameObject go = new GameObject("Ground");
                go.tag = "Ground";
                go.transform.SetParent(parent, false);
                go.transform.position = new Vector3(x, GroundY, 0f);
                go.transform.localScale = new Vector3(GroundScaleX, 1f, 1f);
                SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = ground;
                sr.sortingOrder = 10;
                AddStaticCollider(go, ground);
                Count++;
            }
        }

        private void Rock(string name, Sprite sprite, float x, float fromY, float tipY, float width)
        {
            float height = Mathf.Abs(fromY - tipY);
            Vector3 scale = new Vector3(width / sprite.bounds.size.x, height / sprite.bounds.size.y, 1f);
            float midY = (fromY + tipY) / 2f;

            GameObject go = new GameObject($"{name} x{x:0.#}");
            go.tag = "Obstacle";
            go.transform.SetParent(parent, false);
            // Pivot des Sprites berücksichtigen, damit die Spitze genau bei tipY liegt
            go.transform.position = new Vector3(x - sprite.bounds.center.x * scale.x, midY - sprite.bounds.center.y * scale.y, 0f);
            go.transform.localScale = scale;
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            AddStaticCollider(go, sprite);
            Count++;
        }

        private GameObject Spawn(GameObject prefab, float x, float y, string name)
        {
            GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            go.transform.position = new Vector3(x, y, 0f);
            go.name = $"{name} x{x:0.#}";
            Count++;
            return go;
        }

        // Collider aus der Physics Shape des Sprites (wie bei den handgebauten Levels)
        private static void AddStaticCollider(GameObject go, Sprite sprite)
        {
            Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Static;
            PolygonCollider2D col = go.AddComponent<PolygonCollider2D>();

            int shapes = sprite.GetPhysicsShapeCount();
            if (shapes > 0)
            {
                col.pathCount = shapes;
                List<Vector2> points = new List<Vector2>();
                for (int i = 0; i < shapes; i++)
                {
                    points.Clear();
                    sprite.GetPhysicsShape(i, points);
                    col.SetPath(i, points);
                }
            }
        }

        private static Sprite LoadSprite(string path)
        {
            Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (s == null) throw new Exception("Sprite nicht gefunden: " + path);
            return s;
        }

        // Erst über den Pfad laden, sonst irgendein Prefab mit dem passenden Script suchen
        private static GameObject LoadPrefab<T>(string path) where T : Component
        {
            GameObject p = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (p != null) return p;
            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" }))
            {
                GameObject candidate = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
                if (candidate != null && candidate.GetComponent<T>() != null) return candidate;
            }
            throw new Exception("Prefab nicht gefunden: " + path);
        }
    }
}
