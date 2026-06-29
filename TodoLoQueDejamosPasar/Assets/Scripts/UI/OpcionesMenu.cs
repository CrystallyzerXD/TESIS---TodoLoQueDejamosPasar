using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OpcionesMenu : MonoBehaviour
{
    private GameObject       panel       = null;
    private GraphicRaycaster fadeRaycaster = null;
    private Slider           sliderMusica;
    private GameObject[]     botonesMenu;

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

        var fadeCanvas = GameObject.Find("FadeCanvas");
        if (fadeCanvas != null)
            fadeRaycaster = fadeCanvas.GetComponent<GraphicRaycaster>();
    }

    public void Abrir()
    {
        if (botonesMenu != null)
            foreach (var b in botonesMenu) b.SetActive(false);
        if (fadeRaycaster != null) fadeRaycaster.enabled = false;
        panel.SetActive(true);
    }

    public void Cerrar()
    {
        if (botonesMenu != null)
            foreach (var b in botonesMenu) b.SetActive(true);
        if (fadeRaycaster != null) fadeRaycaster.enabled = true;
        panel.SetActive(false);
    }

    private void ConstruirPanel()
    {
        // Canvas
        GameObject cvGo        = new GameObject("OpcionesCanvas");
        Canvas cv              = cvGo.AddComponent<Canvas>();
        cv.renderMode          = RenderMode.ScreenSpaceOverlay;
        cv.sortingOrder        = 200;
        CanvasScaler cs        = cvGo.AddComponent<CanvasScaler>();
        cs.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1920, 1080);
        cs.matchWidthOrHeight  = 0.5f;
        cvGo.AddComponent<GraphicRaycaster>();

        // Fondo oscuro
        panel = Img(cvGo, "OpcionesFondo", new Color(0, 0, 0, 0.72f));
        Stretch(panel);

        // Caja — 500px ancho, altura ajustada al contenido
        float W   = 500f;   // ancho de la caja
        float pad = 30f;    // padding horizontal interno
        float iw  = W - pad * 2f; // ancho util interno = 440

        GameObject box         = Img(panel, "Caja", new Color(0.09f, 0.09f, 0.09f, 0.97f));
        var boxRt              = box.GetComponent<RectTransform>();
        boxRt.anchorMin        = new Vector2(0.5f, 0.5f);
        boxRt.anchorMax        = new Vector2(0.5f, 0.5f);
        boxRt.pivot            = new Vector2(0.5f, 0.5f);
        boxRt.anchoredPosition = Vector2.zero;
        boxRt.sizeDelta        = new Vector2(W, 345f);

        float y = 148f;

        // ── Titulo ────────────────────────────────────────────────────────────
        var titulo = TMP(box, "Titulo", "Opciones", 26f, Color.white);
        Place(titulo.gameObject, new Vector2(0, y), new Vector2(iw, 38));
        titulo.alignment = TextAlignmentOptions.Center;
        titulo.fontStyle = FontStyles.Bold;
        y -= 42f;

        HLine(box, y + 8f, iw); y -= 14f;

        // ── Sonido ────────────────────────────────────────────────────────────
        Label(box, "SONIDO", y, iw);
        y -= 24f;

        sliderMusica = SliderRow(box, "Musica", "VolMusica", 0.7f, "M\u00fasica", y, iw);
        sliderMusica.onValueChanged.AddListener(v => {
            PlayerPrefs.SetFloat("VolMusica", v);
            if (AudioManager.Instance != null) AudioManager.Instance.SetVolumenMusica(v);
        });
        y -= 36f + 10f;

        // ── Controles ─────────────────────────────────────────────────────────
        Label(box, "CONTROLES", y, iw);
        y -= 24f;

        ControlsGrid(box, y, iw);
        y -= 2 * (28f + 8f) + 10f;

        HLine(box, y + 6f, iw); y -= 14f;

        // ── Boton Regresar ────────────────────────────────────────────────────
        Btn(box, "Regresar", y, iw, Color.white, new Color(0.18f, 0.18f, 0.18f))
            .onClick.AddListener(Cerrar);
    }

    // ── Helpers de layout ─────────────────────────────────────────────────────

    /// <summary>Label de seccion alineada a la izquierda del area util.</summary>
    private void Label(GameObject padre, string texto, float y, float iw)
    {
        var lbl = TMP(padre, "Lbl_"+texto, texto, 10f, new Color(0.4f, 0.4f, 0.4f));
        // ancla a izquierda: x = -iw/2, ancho = iw/2 para que quede flush left
        Place(lbl.gameObject, new Vector2(-iw * 0.25f, y), new Vector2(iw * 0.5f, 20));
        lbl.alignment = TextAlignmentOptions.Left;
        lbl.fontStyle = FontStyles.Bold;
    }

    private Slider SliderRow(GameObject padre, string id, string prefKey,
                              float defVal, string etiqueta, float y, float iw)
    {
        float val    = PlayerPrefs.GetFloat(prefKey, defVal);
        float lblW   = 80f;
        float pctW   = 44f;
        float sliderW = iw - lblW - pctW - 16f;
        float sliderX = -iw / 2f + lblW + 8f + sliderW / 2f;

        // Label izquierdo
        var lbl = TMP(padre, "Lbl_"+id, etiqueta, 13f, new Color(0.75f, 0.75f, 0.75f));
        Place(lbl.gameObject, new Vector2(-iw / 2f + lblW / 2f, y), new Vector2(lblW, 28));
        lbl.alignment = TextAlignmentOptions.Left;

        // Porcentaje derecho
        var pct = TMP(padre, "Pct_"+id, Mathf.RoundToInt(val * 100) + "%", 12f,
                      new Color(0.45f, 0.45f, 0.45f));
        Place(pct.gameObject, new Vector2(iw / 2f - pctW / 2f, y), new Vector2(pctW, 28));
        pct.alignment = TextAlignmentOptions.Right;

        // Slider
        var slGo = new GameObject("Slider_"+id);
        slGo.transform.SetParent(padre.transform, false);
        slGo.AddComponent<RectTransform>();
        Place(slGo, new Vector2(sliderX, y), new Vector2(sliderW, 20));

        var bg = new GameObject("Background"); bg.transform.SetParent(slGo.transform, false);
        bg.AddComponent<RectTransform>(); bg.AddComponent<Image>().color = new Color(0.25f, 0.25f, 0.25f);
        var bgRt = bg.GetComponent<RectTransform>();
        bgRt.anchorMin = new Vector2(0, 0.25f); bgRt.anchorMax = new Vector2(1, 0.75f);
        bgRt.offsetMin = Vector2.zero; bgRt.offsetMax = Vector2.zero;

        var fa = new GameObject("Fill Area"); fa.transform.SetParent(slGo.transform, false);
        var faRt = fa.AddComponent<RectTransform>();
        faRt.anchorMin = new Vector2(0, 0.25f); faRt.anchorMax = new Vector2(1, 0.75f);
        faRt.offsetMin = new Vector2(5, 0); faRt.offsetMax = new Vector2(-15, 0);

        var fill = new GameObject("Fill"); fill.transform.SetParent(fa.transform, false);
        fill.AddComponent<RectTransform>(); fill.AddComponent<Image>().color = new Color(0.85f, 0.85f, 0.85f);
        var fillRt = fill.GetComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero; fillRt.anchorMax = new Vector2(val, 1);
        fillRt.offsetMin = Vector2.zero; fillRt.offsetMax = Vector2.zero;

        var ha = new GameObject("Handle Slide Area"); ha.transform.SetParent(slGo.transform, false);
        var haRt = ha.AddComponent<RectTransform>();
        haRt.anchorMin = Vector2.zero; haRt.anchorMax = Vector2.one;
        haRt.offsetMin = new Vector2(10, 0); haRt.offsetMax = new Vector2(-10, 0);

        var handle = new GameObject("Handle"); handle.transform.SetParent(ha.transform, false);
        handle.AddComponent<RectTransform>();
        var hImg = handle.AddComponent<Image>(); hImg.color = Color.white;
        var hRt  = handle.GetComponent<RectTransform>();
        hRt.anchorMin = new Vector2(val, 0.5f); hRt.anchorMax = new Vector2(val, 0.5f);
        hRt.sizeDelta = new Vector2(16, 16);

        var sl = slGo.AddComponent<Slider>();
        sl.fillRect = fill.GetComponent<RectTransform>();
        sl.handleRect = handle.GetComponent<RectTransform>();
        sl.targetGraphic = hImg;
        sl.direction = Slider.Direction.LeftToRight;
        sl.minValue = 0f; sl.maxValue = 1f; sl.value = val;
        sl.onValueChanged.AddListener(v => pct.text = Mathf.RoundToInt(v * 100) + "%");

        return sl;
    }

    private void ControlsGrid(GameObject padre, float y, float iw)
    {
        (string tecla, string desc)[] c = {
            ("A D  /  < >", "Moverse"),
            ("E",           "Interactuar"),
            ("Esc",         "Pausar"),
            ("1  2  3",     "Elegir opci\u00f3n")
        };

        float cw  = iw / 2f - 4f;  // dos columnas con gap de 8px
        float ch  = 28f;
        float gap = 8f;

        for (int i = 0; i < 4; i++)
        {
            int   col = i % 2;
            int   row = i / 2;
            float cx  = -iw / 2f + col * (cw + gap) + cw / 2f;
            float cy  = y - row * (ch + gap);

            var cell = Img(padre, "Ctrl"+i, new Color(1, 1, 1, 0.05f));
            Place(cell, new Vector2(cx, cy), new Vector2(cw, ch));

            // Tecla — lado izquierdo con padding
            float keyW = 86f;
            var key    = TMP(cell, "Key", c[i].tecla, 10f, Color.white);
            var keyRt  = key.gameObject.GetComponent<RectTransform>();
            keyRt.anchorMin        = new Vector2(0, 0);
            keyRt.anchorMax        = new Vector2(0, 1);
            keyRt.pivot            = new Vector2(0, 0.5f);
            keyRt.anchoredPosition = new Vector2(8f, 0);
            keyRt.sizeDelta        = new Vector2(keyW, 0);
            key.alignment          = TextAlignmentOptions.Left;
            key.fontStyle          = FontStyles.Bold;

            // Separador
            var sep   = Img(cell, "Sep", new Color(1, 1, 1, 0.1f));
            var sepRt = sep.GetComponent<RectTransform>();
            sepRt.anchorMin        = new Vector2(0, 0.15f);
            sepRt.anchorMax        = new Vector2(0, 0.85f);
            sepRt.pivot            = new Vector2(0, 0.5f);
            sepRt.anchoredPosition = new Vector2(keyW + 12f, 0);
            sepRt.sizeDelta        = new Vector2(1f, 0);

            // Descripcion — resto del ancho
            var desc   = TMP(cell, "Desc", c[i].desc, 11f, new Color(0.65f, 0.65f, 0.65f));
            var descRt = desc.gameObject.GetComponent<RectTransform>();
            descRt.anchorMin        = new Vector2(0, 0);
            descRt.anchorMax        = new Vector2(1, 1);
            descRt.pivot            = new Vector2(0, 0.5f);
            descRt.anchoredPosition = new Vector2(keyW + 20f, 0);
            descRt.sizeDelta        = new Vector2(-(keyW + 28f), 0);
            desc.alignment          = TextAlignmentOptions.Left;
        }
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
        cb.highlightedColor = new Color(Mathf.Min(cBg.r+0.1f,1), Mathf.Min(cBg.g+0.1f,1), Mathf.Min(cBg.b+0.1f,1), 1);
        cb.pressedColor     = new Color(Mathf.Max(cBg.r-0.06f,0), Mathf.Max(cBg.g-0.06f,0), Mathf.Max(cBg.b-0.06f,0), 1);
        cb.selectedColor    = cb.normalColor;
        btn.colors          = cb;

        var txt = TMP(go, "Label", label, 14f, cTxt);
        Stretch(txt.gameObject);
        txt.alignment = TextAlignmentOptions.Center;
        return btn;
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
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos; rt.sizeDelta = size;
    }

    private void HLine(GameObject padre, float y, float iw)
    {
        Place(Img(padre, "Line", new Color(1,1,1,0.07f)), new Vector2(0, y), new Vector2(iw, 1));
    }
}