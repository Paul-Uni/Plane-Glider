using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Kurzer Steuerungshinweis unten am Bildschirm, blendet nach ein paar Sekunden aus.
public class ControlsHint : MonoBehaviour
{
    private const float Visible = 3.5f;
    private const float FadeTime = 0.8f;

    public static void Show(string text)
    {
        GameObject go = new GameObject("Controls Hint", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(CanvasGroup));
        Canvas canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 150;
        CanvasScaler scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject t = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        RectTransform rt = t.GetComponent<RectTransform>();
        rt.SetParent(go.transform, false);
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0, 60);
        rt.sizeDelta = new Vector2(1600, 80);
        TextMeshProUGUI tmp = t.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 42;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.raycastTarget = false;

        go.AddComponent<ControlsHint>();
    }

    IEnumerator Start()
    {
        CanvasGroup group = GetComponent<CanvasGroup>();
        yield return new WaitForSecondsRealtime(Visible);
        for (float t = 0f; t < FadeTime; t += Time.unscaledDeltaTime)
        {
            group.alpha = 1f - t / FadeTime;
            yield return null;
        }
        Destroy(gameObject);
    }
}
