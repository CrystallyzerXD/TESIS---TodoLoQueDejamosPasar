using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CreditosMenu : MonoBehaviour
{
    private GameObject   panel       = null;
    private GameObject[] botonesMenu;

    private void Start()
    {
        var canvas = GameObject.Find("Canvas");
        if (canvas != null)
        {
            var buttons = canvas.GetComponentsInChildren<Button>(true);
            botonesMenu = new GameObject[buttons.Length];
            for (int i = 0; i < buttons.Length; i++)
                botonesMenu[i] = buttons[i].gameObject;
        }

        ConstruirPanel();
        panel.SetActive(false);
    }

    public void AbrirCreditos()
    {
        if (botonesMenu != null)
            foreach (var b in botonesMenu) b.SetActive(false);
        panel.SetActive(true);
    }

    public void CerrarCreditos()
    {
        if (botonesMenu != null)
            foreach (var b in botonesMenu) b.SetActive(true);
        panel.SetActive(false);
    }

    private void ConstruirPanel()
    {
        GameObject cvGo        = new GameObject("CreditosCanvas");
        Canvas cv              = cvGo.AddComponent<Canvas>();
        cv.renderMode          = RenderMode.ScreenSpaceOverlay;
        cv.sortingOrder        = 200;
        CanvasScaler cs        = cvGo.AddComponent<CanvasScaler>();
        cs.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1920, 1080);
        cs.matchWidthOrHeight  = 0.5f;
        cvGo.AddComponent<GraphicRaycaster>();

        panel = Img(cvGo, "CreditosFondo", new Color(0, 0, 0, 0.75f));
        Stretch(panel);

        float W  = 560f;
        float iw = 500f;   // ancho util interno

        GameObject box         = Img(panel, "Caja", new Color(0.09f, 0.09f, 0.09f, 0.97f));
        var boxRt              = box.GetComponent<RectTransform>();
        boxRt.anchorMin        = new Vector2(0.5f, 0.5f);
        boxRt.anchorMax        = new Vector2(0.5f, 0.5f);
        boxRt.pivot            = new Vector2(0.5f, 0.5f);
        boxRt.anchoredPosition = Vector2.zero;
        boxRt.sizeDelta        = new Vector2(W, 430f);

        float y = 188f;

        // ── Titulo ────────────────────────────────────────────────────────────
        var titulo = TMP(box, "Titulo",
            "Todo Lo Que Dejamos Pasar", 22f, Color.white);
        Place(titulo.gameObject, new Vector2(0, y), new Vector2(iw, 34));
        titulo.alignment = TextAlignmentOptions.Center;
        titulo.fontStyle = FontStyles.Bold;
        y -= 30f;

        var sub = TMP(box, "Sub", "Videojuego narrativo 2D", 11f,
                      new Color(0.5f, 0.5f, 0.5f));
        Place(sub.gameObject, new Vector2(0, y), new Vector2(iw, 20));
        sub.alignment = TextAlignmentOptions.Center;
        y -= 26f;

        HLine(box, y + 8f, iw); y -= 16f;

        // ── Contexto ──────────────────────────────────────────────────────────
        string contexto =
            "\n\nUn juego sobre las decisiones cotidianas que moldean el entorno urbano. " +
            "A trav\u00e9s de los ojos de Mateo, el jugador navega siete d\u00edas en un " +
            "barrio que responde a cada acci\u00f3n: la contaminaci\u00f3n, el riesgo de " +
            "inundaci\u00f3n y el bienestar de la comunidad cambian seg\u00fan lo que se " +
            "decide hacer \u2014 o dejar pasar.";

        var ctx = TMP(box, "Contexto", contexto, 11f,
                      new Color(0.68f, 0.68f, 0.68f));
        Place(ctx.gameObject, new Vector2(0, y - 4f), new Vector2(iw, 62));
        ctx.verticalAlignment = VerticalAlignmentOptions.Middle;
        ctx.alignment          = TextAlignmentOptions.Center;
        ctx.textWrappingMode = TextWrappingModes.Normal;
        y -= 66f;

        HLine(box, y + 8f, iw); y -= 20f;

        // ── Ficha tecnica — cada fila es un bloque centrado ───────────────────
        // Formato: etiqueta pequeña arriba, valor grande abajo, separados por linea
        (string etiqueta, string valor)[] ficha = {
            ("Desarrollado por",         "Jean Pierre Benites Ruiz"),
            ("Asesor",                   "Luis Robles"),
            ("Tesis de pregrado",        "Ingenier\u00eda Inform\u00e1tica"),
            ("Instituci\u00f3n",         "Pontificia Universidad Cat\u00f3lica del Per\u00fa"),
            ("A\u00f1o",                 "2026"),
        };

        // Dos columnas: izquierda (etiqueta) | derecha (valor)
        float colEtq = iw * 0.42f;
        float colVal = iw * 0.55f;
        float xEtq   = -iw * 0.5f + colEtq * 0.5f;
        float xVal   = -iw * 0.5f + colEtq + iw * 0.03f + colVal * 0.5f;
        float rowH   = 22f;
        float rowGap = 10f;

        foreach (var (etiqueta, valor) in ficha)
        {
            var lbl = TMP(box, "E_" + etiqueta, etiqueta, 10f,
                          new Color(0.42f, 0.42f, 0.42f));
            Place(lbl.gameObject, new Vector2(xEtq, y), new Vector2(colEtq, rowH));
            lbl.alignment = TextAlignmentOptions.Right;
            lbl.fontStyle = FontStyles.Bold;

            var val = TMP(box, "V_" + etiqueta, valor, 11f, Color.white);
            Place(val.gameObject, new Vector2(xVal, y), new Vector2(colVal, rowH));
            val.alignment          = TextAlignmentOptions.Left;
            val.textWrappingMode = TextWrappingModes.NoWrap;
            val.overflowMode       = TextOverflowModes.Overflow;

            y -= rowH + rowGap;
        }

        y -= 6f;
        HLine(box, y + 6f, iw); y -= 14f;

        // ── Boton Regresar ────────────────────────────────────────────────────
        Btn(box, "Regresar", y, iw, Color.white, new Color(0.18f, 0.18f, 0.18f))
            .onClick.AddListener(CerrarCreditos);
    }

    // ── Primitivas ────────────────────────────────────────────────────────────

    private GameObject Img(GameObject padre, string nombre, Color color)
    {
        var go = new GameObject(nombre);
        go.transform.SetParent(padre.transform, false);
        go.AddComponent<RectTransform>();
        go.AddComponent<Image>().color = color;
        return go;
    }

    private TextMeshProUGUI TMP(GameObject padre, string nombre, string texto,
                                float size, Color color)
    {
        var go = new GameObject(nombre);
        go.transform.SetParent(padre.transform, false);
        go.AddComponent<RectTransform>();
        var t = go.AddComponent<TextMeshProUGUI>();
        t.text = texto; t.fontSize = size; t.color = color;
        return t;
    }

    private void Stretch(GameObject go)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }

    private void Place(GameObject go, Vector2 pos, Vector2 size)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos; rt.sizeDelta = size;
    }

    private void HLine(GameObject padre, float y, float iw)
    {
        Place(Img(padre, "Line", new Color(1,1,1,0.07f)),
              new Vector2(0, y), new Vector2(iw, 1));
    }

    private Button Btn(GameObject padre, string label, float y, float iw,
                       Color cTxt, Color cBg)
    {
        var go = Img(padre, "Btn_"+label, cBg);
        Place(go, new Vector2(0, y), new Vector2(iw, 38));

        var btn = go.AddComponent<Button>();
        btn.targetGraphic = go.GetComponent<Image>();
        var cb = btn.colors;
        cb.normalColor      = cBg;
        cb.highlightedColor = new Color(Mathf.Min(cBg.r+0.1f,1),
                                        Mathf.Min(cBg.g+0.1f,1),
                                        Mathf.Min(cBg.b+0.1f,1), 1);
        cb.pressedColor     = new Color(Mathf.Max(cBg.r-0.06f,0),
                                        Mathf.Max(cBg.g-0.06f,0),
                                        Mathf.Max(cBg.b-0.06f,0), 1);
        cb.selectedColor    = cb.normalColor;
        btn.colors          = cb;

        var txt = TMP(go, "Label", label, 14f, cTxt);
        Stretch(txt.gameObject);
        txt.alignment = TextAlignmentOptions.Center;
        return btn;
    }
}