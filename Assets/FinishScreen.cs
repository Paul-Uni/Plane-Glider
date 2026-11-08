using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Wird beim Erreichen des Ziels angezeigt: Sterne, Zeit, Sprit, Bestzeit.
// Danach geht es zurück zum HomeScreen. Erzeugt sich selbst, kein Szenen-Setup nötig.
public class FinishScreen : MonoBehaviour
{
    private const string HomeScene = "HomeScreen";
    private const float AutoReturnSeconds = 5f;

    private static readonly Color Gold = new Color(1f, 0.85f, 0.2f);
    private static readonly Color StarOff = new Color(0.3f, 0.3f, 0.35f, 0.8f);
    private static readonly Color Ok = new Color(0.55f, 1f, 0.55f);
    private static readonly Color Miss = new Color(0.75f, 0.75f, 0.75f);
    private static Sprite starSprite;

    private CanvasGroup group;
    private TextMeshProUGUI countdownText;
    private Image[] stars;
    private int earnedStars;
    private bool leaving;

    public static void Show(float levelTime, float fuelLeft, float timeGoal, float fuelGoal)
    {
        GameObject go = new GameObject("Finish Screen", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup));
        Canvas canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;
        CanvasScaler scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        go.AddComponent<FinishScreen>().Build(levelTime, fuelLeft, timeGoal, fuelGoal);
    }

    private void Build(float levelTime, float fuelLeft, float timeGoal, float fuelGoal)
    {
        group = GetComponent<CanvasGroup>();
        group.alpha = 0f;

        bool fastEnough = levelTime <= timeGoal;
        bool savedFuel = fuelLeft >= fuelGoal;
        earnedStars = 1 + (fastEnough ? 1 : 0) + (savedFuel ? 1 : 0);

        // Bestzeit pro Level speichern
        string key = "BestTime_" + SceneManager.GetActiveScene().name;
        float best = PlayerPrefs.GetFloat(key, float.MaxValue);
        bool newBest = levelTime < best;
        if (newBest)
        {
            PlayerPrefs.SetFloat(key, levelTime);
            PlayerPrefs.Save();
            best = levelTime;
        }

        // Abgedunkelter Hintergrund
        GameObject bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
        RectTransform bgRect = bg.GetComponent<RectTransform>();
        bgRect.SetParent(transform, false);
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = bgRect.offsetMax = Vector2.zero;
        bg.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.7f);

        TextMeshProUGUI title = CreateText("Title", new Vector2(0, 300), 120, FontStyles.Bold);
        title.text = "LEVEL COMPLETE!";
        title.color = Gold;

        // Sterne
        stars = new Image[3];
        for (int i = 0; i < 3; i++)
        {
            GameObject s = new GameObject("Star " + (i + 1), typeof(RectTransform), typeof(Image));
            RectTransform rt = s.GetComponent<RectTransform>();
            rt.SetParent(transform, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            float size = i == 1 ? 170f : 140f; // mittlerer Stern etwas größer
            rt.sizeDelta = new Vector2(size, size);
            rt.anchoredPosition = new Vector2((i - 1) * 190f, i == 1 ? 120f : 100f);
            Image img = s.GetComponent<Image>();
            img.sprite = GetStarSprite();
            img.color = StarOff;
            img.raycastTarget = false;
            stars[i] = img;
        }

        // Was es für die Sterne braucht
        string fuel = Mathf.RoundToInt(fuelLeft) + "%";
        TextMeshProUGUI goals = CreateText("Goals", new Vector2(0, -50), 44, FontStyles.Normal);
        goals.text =
            Line(true, "Ziel erreicht") + "\n" +
            Line(fastEnough, $"Zeit: {levelTime:0.0}s  <size=75%>(unter {timeGoal:0.0}s)</size>") + "\n" +
            Line(savedFuel, $"Sprit übrig: {fuel}  <size=75%>(mind. {fuelGoal:0}%)</size>");
        goals.rectTransform.sizeDelta = new Vector2(1800, 220);

        TextMeshProUGUI bestText = CreateText("Best", new Vector2(0, -200), 40, FontStyles.Bold);
        bestText.text = newBest ? "NEUE BESTZEIT!" : $"Bestzeit: {best:0.0}s";
        bestText.color = newBest ? Gold : Miss;

        countdownText = CreateText("Countdown", new Vector2(0, -300), 34, FontStyles.Italic);
        countdownText.color = Miss;

        StartCoroutine(Run());
    }

    private static string Line(bool done, string text)
    {
        string hex = ColorUtility.ToHtmlStringRGB(done ? Ok : Miss);
        return $"<color=#{hex}>{(done ? "+" : "-")}  {text}</color>";
    }

    private IEnumerator Run()
    {
        // Einblenden (unscaled, weil das Spiel pausiert ist)
        for (float t = 0f; t < 0.3f; t += Time.unscaledDeltaTime)
        {
            group.alpha = t / 0.3f;
            yield return null;
        }
        group.alpha = 1f;

        // Sterne nacheinander "einploppen"
        for (int i = 0; i < earnedStars; i++)
        {
            yield return new WaitForSecondsRealtime(0.25f);
            stars[i].color = Gold;
            RectTransform rt = stars[i].rectTransform;
            for (float t = 0f; t < 0.2f; t += Time.unscaledDeltaTime)
            {
                rt.localScale = Vector3.one * Mathf.Lerp(1.6f, 1f, t / 0.2f);
                yield return null;
            }
            rt.localScale = Vector3.one;
        }

        for (float left = AutoReturnSeconds; left > 0f; left -= Time.unscaledDeltaTime)
        {
            countdownText.text = $"Zurück zum Menü in {Mathf.CeilToInt(left)}...   (Leertaste / Klick = sofort)";
            yield return null;
        }
        GoHome();
    }

    void Update()
    {
        // kurze Sperre, damit ein noch gehaltenes Leertaste-Drücken nicht sofort überspringt
        if (group.alpha < 1f) return;
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)) GoHome();
    }

    private void GoHome()
    {
        if (leaving) return;
        leaving = true;
        Time.timeScale = 1f; // sonst bleibt auch der HomeScreen eingefroren
        SceneManager.LoadScene(HomeScene);
    }

    private TextMeshProUGUI CreateText(string name, Vector2 pos, float size, FontStyles style)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(transform, false);
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(1800, size * 1.5f);
        rt.anchoredPosition = pos;
        TextMeshProUGUI text = go.GetComponent<TextMeshProUGUI>();
        text.fontSize = size;
        text.fontStyle = style;
        text.alignment = TextAlignmentOptions.Center;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.raycastTarget = false;
        return text;
    }

    // 5-zackiger Stern per Code (kein Asset nötig)
    private static Sprite GetStarSprite()
    {
        if (starSprite != null) return starSprite;
        const int n = 128, ss = 3;
        Vector2[] poly = new Vector2[10];
        for (int i = 0; i < 10; i++)
        {
            float angle = Mathf.Deg2Rad * (90f + i * 36f);
            float r = i % 2 == 0 ? 0.48f : 0.2f;
            poly[i] = new Vector2(0.5f + Mathf.Cos(angle) * r, 0.47f + Mathf.Sin(angle) * r);
        }

        Texture2D tex = new Texture2D(n, n, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        Color32[] px = new Color32[n * n];
        for (int y = 0; y < n; y++)
        for (int x = 0; x < n; x++)
        {
            int inside = 0;
            for (int sy = 0; sy < ss; sy++)
            for (int sx = 0; sx < ss; sx++)
            {
                Vector2 p = new Vector2((x + (sx + 0.5f) / ss) / n, (y + (sy + 0.5f) / ss) / n);
                if (InPolygon(p, poly)) inside++;
            }
            px[y * n + x] = new Color32(255, 255, 255, (byte)(255 * inside / (ss * ss)));
        }
        tex.SetPixels32(px);
        tex.Apply();
        starSprite = Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), 100f);
        return starSprite;
    }

    private static bool InPolygon(Vector2 p, Vector2[] poly)
    {
        bool inside = false;
        for (int i = 0, j = poly.Length - 1; i < poly.Length; j = i++)
        {
            if ((poly[i].y > p.y) != (poly[j].y > p.y) &&
                p.x < (poly[j].x - poly[i].x) * (p.y - poly[i].y) / (poly[j].y - poly[i].y) + poly[i].x)
                inside = !inside;
        }
        return inside;
    }
}
