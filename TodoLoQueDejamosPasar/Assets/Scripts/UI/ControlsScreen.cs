using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

/// <summary>
/// Muestra una pantalla de controles sobre fondo negro antes de liberar el movimiento.
/// Se activa desde SceneController despues de la intro, antes del FadeIn.
/// </summary>
public class ControlsScreen : MonoBehaviour
{
    public static ControlsScreen Instance { get; private set; }

    private bool          activo   = false;
    private Action        callback = null;
    private Canvas        canvas;
    private CanvasGroup   cg;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        CrearUI();
    }

    private void Update()
    {
        if (!activo) return;
        if (Input.anyKeyDown)
            Cerrar();
    }

    // ── API publica ───────────────────────────────────────────────────────────

    public void Mostrar(Action onCerrado)
    {
        callback = onCerrado;
        activo   = true;
        canvas.gameObject.SetActive(true);
        StartCoroutine(FadeIn());
    }

    // ── Logica interna ────────────────────────────────────────────────────────

    private void Cerrar()
    {
        activo = false;
        StartCoroutine(FadeOutYCerrar());
    }

    private IEnumerator FadeIn()
    {
        float t = 0f;
        while (t < 0.4f)
        {
            t     += Time.deltaTime;
            cg.alpha = Mathf.Clamp01(t / 0.4f);
            yield return null;
        }
        cg.alpha = 1f;
    }

    private IEnumerator FadeOutYCerrar()
    {
        float t = 0f;
        while (t < 0.3f)
        {
            t        += Time.deltaTime;
            cg.alpha  = 1f - Mathf.Clamp01(t / 0.3f);
            yield return null;
        }
        canvas.gameObject.SetActive(false);
        callback?.Invoke();
    }

    // ── Construccion de UI ────────────────────────────────────────────────────

    private void CrearUI()
    {
        GameObject cvGo   = new GameObject("ControlsCanvas");
        cvGo.transform.SetParent(transform);
        canvas            = cvGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1001; // encima de todo

        var cs            = cvGo.AddComponent<CanvasScaler>();
        cs.uiScaleMode    = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1920, 1080);
        cs.matchWidthOrHeight  = 0.5f;
        cvGo.AddComponent<GraphicRaycaster>();

        cg        = cvGo.AddComponent<CanvasGroup>();
        cg.alpha  = 0f;

        // Fondo negro total
        GameObject fondo = new GameObject("Fondo");
        fondo.transform.SetParent(cvGo.transform, false);
        fondo.AddComponent<RectTransform>();
        var fondoImg   = fondo.AddComponent<Image>();
        fondoImg.color = Color.black;
        var fondoRt    = fondo.GetComponent<RectTransform>();
        fondoRt.anchorMin = Vector2.zero;
        fondoRt.anchorMax = Vector2.one;
        fondoRt.offsetMin = Vector2.zero;
        fondoRt.offsetMax = Vector2.zero;

        // Caja central
        GameObject box   = new GameObject("Caja");
        box.transform.SetParent(cvGo.transform, false);
        box.AddComponent<RectTransform>();
        var boxImg   = box.AddComponent<Image>();
        boxImg.color = new Color(0.08f, 0.08f, 0.08f, 0.97f);
        var boxRt    = box.GetComponent<RectTransform>();
        boxRt.anchorMin        = new Vector2(0.5f, 0.5f);
        boxRt.anchorMax        = new Vector2(0.5f, 0.5f);
        boxRt.pivot            = new Vector2(0.5f, 0.5f);
        boxRt.anchoredPosition = Vector2.zero;
        boxRt.sizeDelta        = new Vector2(580, 400);

        float y = 164f;

        // Titulo
        var titulo = Txt(box, "Titulo", "C\u00f3mo jugar", 26f, Color.white, true);
        Place(titulo.gameObject, new Vector2(0, y), new Vector2(540, 42));
        titulo.alignment = TextAlignmentOptions.Center;
        y -= 44f;

        // Linea
        HLine(box, y + 8f); y -= 14f;

        // Controles
        (string tecla, string desc)[] controles = {
            ("A  D   /   \u2190 \u2192",  "Moverse"),
            ("E",                          "Interactuar / avanzar di\u00e1logo"),
            ("1   2   3",                  "Elegir opci\u00f3n"),
            ("Esc",                        "Pausar"),
        };

        float cw = 510f, ch = 34f, gap = 10f;

        foreach (var (tecla, desc) in controles)
        {
            // Celda
            var cell = new GameObject("Ctrl");
            cell.transform.SetParent(box.transform, false);
            cell.AddComponent<RectTransform>();
            var cellImg   = cell.AddComponent<Image>();
            cellImg.color = new Color(1f, 1f, 1f, 0.05f);
            Place(cell, new Vector2(0, y), new Vector2(cw, ch));

            // Tecla izquierda
            var keyTxt = Txt(cell, "Key", tecla, 11f, Color.white, true);
            var keyRt  = keyTxt.gameObject.GetComponent<RectTransform>();
            keyRt.anchorMin        = new Vector2(0, 0);
            keyRt.anchorMax        = new Vector2(0, 1);
            keyRt.pivot            = new Vector2(0, 0.5f);
            keyRt.anchoredPosition = new Vector2(12f, 0);
            keyRt.sizeDelta        = new Vector2(130f, 0);
            keyTxt.alignment       = TextAlignmentOptions.Left;

            // Separador
            var sep   = new GameObject("Sep");
            sep.transform.SetParent(cell.transform, false);
            sep.AddComponent<RectTransform>();
            var sepImg   = sep.AddComponent<Image>();
            sepImg.color = new Color(1f, 1f, 1f, 0.1f);
            var sepRt    = sep.GetComponent<RectTransform>();
            sepRt.anchorMin        = new Vector2(0, 0.15f);
            sepRt.anchorMax        = new Vector2(0, 0.85f);
            sepRt.pivot            = new Vector2(0, 0.5f);
            sepRt.anchoredPosition = new Vector2(148f, 0);
            sepRt.sizeDelta        = new Vector2(1f, 0);

            // Descripcion derecha
            var descTxt = Txt(cell, "Desc", desc, 11f, new Color(0.75f, 0.75f, 0.75f), false);
            var descRt  = descTxt.gameObject.GetComponent<RectTransform>();
            descRt.anchorMin        = new Vector2(0, 0);
            descRt.anchorMax        = new Vector2(1, 1);
            descRt.pivot            = new Vector2(0, 0.5f);
            descRt.anchoredPosition = new Vector2(158f, 0);
            descRt.sizeDelta        = new Vector2(-164f, 0);
            descTxt.alignment       = TextAlignmentOptions.Left;

            y -= ch + gap;
        }

        y -= 4f;
        HLine(box, y + 6f); y -= 16f;

        // Instruccion para continuar
        var continuar = Txt(box, "Continuar",
            "Presiona cualquier tecla para continuar",
            11f, new Color(0.45f, 0.45f, 0.45f), false);
        Place(continuar.gameObject, new Vector2(0, y), new Vector2(540, 24));
        continuar.alignment = TextAlignmentOptions.Center;

        canvas.gameObject.SetActive(false);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private TextMeshProUGUI Txt(GameObject padre, string nombre, string texto,
                                float size, Color color, bool bold)
    {
        var go = new GameObject(nombre);
        go.transform.SetParent(padre.transform, false);
        go.AddComponent<RectTransform>();
        var t = go.AddComponent<TextMeshProUGUI>();
        t.text      = texto;
        t.fontSize  = size;
        t.color     = color;
        if (bold) t.fontStyle = FontStyles.Bold;
        return t;
    }

    private void Place(GameObject go, Vector2 pos, Vector2 size)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0.5f, 0.5f);
        rt.anchorMax        = new Vector2(0.5f, 0.5f);
        rt.pivot            = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta        = size;
    }

    private void HLine(GameObject padre, float y)
    {
        var go = new GameObject("Line");
        go.transform.SetParent(padre.transform, false);
        go.AddComponent<RectTransform>();
        go.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.07f);
        Place(go, new Vector2(0, y), new Vector2(540, 1));
    }
}