using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }

    private bool             pausado       = false;
    private GameObject       pausePanel    = null;
    private GraphicRaycaster fadeRaycaster = null;

    private TextMeshProUGUI textoDia;
    private Slider          sliderMusica;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        ConstruirPanel();
        pausePanel.SetActive(false);

        var fadeCanvas = GameObject.Find("FadeCanvas");
        if (fadeCanvas != null)
            fadeRaycaster = fadeCanvas.GetComponent<GraphicRaycaster>();
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape)) return;

        bool enCutscene = (DialogueManager.Instance != null &&
                          (DialogueManager.Instance.EstaActivo ||
                           DialogueManager.Instance.InputBloqueado))
                       || (EscenaRolManager.Instance != null && EscenaRolManager.Instance.EstaActivo)
                       || (ReflexionManager.Instance != null && ReflexionManager.Instance.EstaActivo)
                       || PlayerMovement.Bloqueado;

        if (pausado)          Reanudar();
        else if (!enCutscene) Pausar();
    }

    public void Pausar()
    {
        pausado                  = true;
        Time.timeScale           = 0f;
        PlayerMovement.Bloqueado = true;
        if (fadeRaycaster != null) fadeRaycaster.enabled = false;
        ActualizarTextoDia();
        pausePanel.SetActive(true);
    }

    public void Reanudar()
    {
        pausado                  = false;
        Time.timeScale           = 1f;
        PlayerMovement.Bloqueado = false;
        if (fadeRaycaster != null) fadeRaycaster.enabled = true;
        pausePanel.SetActive(false);
    }

    private void ConstruirPanel()
    {
        GameObject cvGo        = new GameObject("PauseCanvas");
        Canvas cv              = cvGo.AddComponent<Canvas>();
        cv.renderMode          = RenderMode.ScreenSpaceOverlay;
        cv.sortingOrder        = 200;
        CanvasScaler cs        = cvGo.AddComponent<CanvasScaler>();
        cs.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1920, 1080);
        cs.matchWidthOrHeight  = 0.5f;
        cvGo.AddComponent<GraphicRaycaster>();

        pausePanel = Img(cvGo, "PauseFondo", new Color(0, 0, 0, 0.72f));
        Stretch(pausePanel);

        // Caja ajustada: menos alta ahora que no hay slider de efectos
        GameObject box         = Img(pausePanel, "Caja", new Color(0.09f, 0.09f, 0.09f, 0.97f));
        var boxRt              = box.GetComponent<RectTransform>();
        boxRt.anchorMin        = new Vector2(0.5f, 0.5f);
        boxRt.anchorMax        = new Vector2(0.5f, 0.5f);
        boxRt.pivot            = new Vector2(0.5f, 0.5f);
        boxRt.anchoredPosition = Vector2.zero;
        boxRt.sizeDelta        = new Vector2(430, 468f);

        float y = 200f;

        // Dia
        textoDia = TMP(box, "TextoDia", "", 13f, new Color(0.55f, 0.55f, 0.55f));
        Place(textoDia.gameObject, new Vector2(0, y), new Vector2(360, 26));
        textoDia.alignment = TextAlignmentOptions.Center;
        y -= 34f;

        // Titulo
        var titulo = TMP(box, "Titulo", "Pausado", 28f, Color.white);
        Place(titulo.gameObject, new Vector2(0, y), new Vector2(360, 42));
        titulo.alignment = TextAlignmentOptions.Center;
        titulo.fontStyle = FontStyles.Bold;
        y -= 48f;

        HLine(box, y + 10f); y -= 16f;

        // Sonido — solo musica
        SectionLabel(box, "SONIDO", ref y);
        sliderMusica = SliderRow(box, "Musica", "VolMusica", 0.7f, "M\u00fasica", ref y);
        sliderMusica.onValueChanged.AddListener(v => {
            PlayerPrefs.SetFloat("VolMusica", v);
            if (AudioManager.Instance != null) AudioManager.Instance.SetVolumenMusica(v);
        });
        y -= 8f;

        // Controles
        SectionLabel(box, "CONTROLES", ref y);
        ControlsGrid(box, ref y);
        y -= 10f;

        HLine(box, y + 6f); y -= 14f;

        // Botones
        Btn(box, "Reanudar", y,
            Color.white, new Color(0.18f, 0.18f, 0.18f))
            .onClick.AddListener(Reanudar);
        y -= 46f;
        Btn(box, "Men\u00fa principal", y,
            new Color(0.7f, 0.7f, 0.7f), new Color(0.09f, 0.09f, 0.09f))
            .onClick.AddListener(IrAPantallaPrincipal);
        y -= 46f;
        Btn(box, "Salir del juego", y,
            new Color(0.9f, 0.35f, 0.35f), new Color(0.09f, 0.09f, 0.09f))
            .onClick.AddListener(SalirDelJuego);
    }

    private void IrAPantallaPrincipal()
    {
        Reanudar();

        // Resetear todo para que Nueva Partida empiece desde cero
        if (NarrativeManager.Instance != null)
            NarrativeManager.Instance.Resetear();

        GameManager.Instance.esInicioJuego    = true;
        GameManager.Instance.diaActual        = 1;
        GameManager.Instance.escenaActual     = Escenas.Home;
        GameManager.Instance.contaminacion    = 5;
        GameManager.Instance.bienestarUrbano  = 5;
        GameManager.Instance.riesgoInundacion = 5;
        GameManager.Instance.estadoArboles    = 1;
        GameManager.Instance.esNoche          = false;
        GameManager.Instance.finalPendiente   = false;
        GameManager.Instance.fase2Final       = false;
        GameManager.Instance.introPendiente   = false;
        GameManager.Instance.escenaRolPendiente = null;

        SceneManager.LoadScene("MainMenu");
    }

    private void SalirDelJuego()
    {
        PlayerPrefs.Save(); Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void ActualizarTextoDia()
    {
        if (textoDia == null || GameManager.Instance == null) return;
        string hora = GameManager.Instance.esNoche ? "Noche" : "Ma\u00f1ana";
        textoDia.text = $"D\u00eda {GameManager.Instance.diaActual}  -  {hora}";
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private GameObject Img(GameObject padre, string nombre, Color color)
    {
        var go = new GameObject(nombre);
        go.transform.SetParent(padre.transform, false);
        go.AddComponent<RectTransform>();
        go.AddComponent<Image>().color = color;
        return go;
    }

    private TextMeshProUGUI TMP(GameObject padre, string nombre, string texto, float size, Color color)
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

    private void HLine(GameObject padre, float y)
    {
        Place(Img(padre, "Line", new Color(1,1,1,0.07f)), new Vector2(0,y), new Vector2(360,1));
    }

    private void SectionLabel(GameObject padre, string texto, ref float y)
    {
        var lbl = TMP(padre, "Lbl_"+texto, texto, 10f, new Color(0.4f,0.4f,0.4f));
        Place(lbl.gameObject, new Vector2(-90f,y), new Vector2(180,20));
        lbl.alignment = TextAlignmentOptions.Left;
        lbl.fontStyle = FontStyles.Bold;
        y -= 26f;
    }

    private Slider SliderRow(GameObject padre, string id, string prefKey,
                              float defVal, string etiqueta, ref float y)
    {
        float val = PlayerPrefs.GetFloat(prefKey, defVal);

        var lbl = TMP(padre, "Lbl_"+id, etiqueta, 13f, new Color(0.75f,0.75f,0.75f));
        Place(lbl.gameObject, new Vector2(-118f,y), new Vector2(90,28));
        lbl.alignment = TextAlignmentOptions.Left;

        var pct = TMP(padre, "Pct_"+id, Mathf.RoundToInt(val*100)+"%", 12f, new Color(0.45f,0.45f,0.45f));
        Place(pct.gameObject, new Vector2(162f,y), new Vector2(40,28));
        pct.alignment = TextAlignmentOptions.Right;

        var slGo = new GameObject("Slider_"+id);
        slGo.transform.SetParent(padre.transform, false);
        slGo.AddComponent<RectTransform>();
        Place(slGo, new Vector2(20f,y), new Vector2(190,20));

        var bg = new GameObject("Background"); bg.transform.SetParent(slGo.transform, false);
        bg.AddComponent<RectTransform>(); bg.AddComponent<Image>().color = new Color(0.25f,0.25f,0.25f);
        var bgRt = bg.GetComponent<RectTransform>();
        bgRt.anchorMin = new Vector2(0,0.25f); bgRt.anchorMax = new Vector2(1,0.75f);
        bgRt.offsetMin = Vector2.zero; bgRt.offsetMax = Vector2.zero;

        var fa = new GameObject("Fill Area"); fa.transform.SetParent(slGo.transform, false);
        var faRt = fa.AddComponent<RectTransform>();
        faRt.anchorMin = new Vector2(0,0.25f); faRt.anchorMax = new Vector2(1,0.75f);
        faRt.offsetMin = new Vector2(5,0); faRt.offsetMax = new Vector2(-15,0);

        var fill = new GameObject("Fill"); fill.transform.SetParent(fa.transform, false);
        fill.AddComponent<RectTransform>(); fill.AddComponent<Image>().color = new Color(0.85f,0.85f,0.85f);
        var fillRt = fill.GetComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero; fillRt.anchorMax = new Vector2(val,1);
        fillRt.offsetMin = Vector2.zero; fillRt.offsetMax = Vector2.zero;

        var ha = new GameObject("Handle Slide Area"); ha.transform.SetParent(slGo.transform, false);
        var haRt = ha.AddComponent<RectTransform>();
        haRt.anchorMin = Vector2.zero; haRt.anchorMax = Vector2.one;
        haRt.offsetMin = new Vector2(10,0); haRt.offsetMax = new Vector2(-10,0);

        var handle = new GameObject("Handle"); handle.transform.SetParent(ha.transform, false);
        handle.AddComponent<RectTransform>();
        var hImg = handle.AddComponent<Image>(); hImg.color = Color.white;
        var hRt = handle.GetComponent<RectTransform>();
        hRt.anchorMin = new Vector2(val,0.5f); hRt.anchorMax = new Vector2(val,0.5f);
        hRt.sizeDelta = new Vector2(16,16);

        var sl = slGo.AddComponent<Slider>();
        sl.fillRect = fill.GetComponent<RectTransform>();
        sl.handleRect = handle.GetComponent<RectTransform>();
        sl.targetGraphic = hImg;
        sl.direction = Slider.Direction.LeftToRight;
        sl.minValue = 0f; sl.maxValue = 1f; sl.value = val;
        sl.onValueChanged.AddListener(v => pct.text = Mathf.RoundToInt(v*100)+"%");

        y -= 36f;
        return sl;
    }

    private void ControlsGrid(GameObject padre, ref float y)
    {
        // Tecla | descripcion — con padding interno generoso para que no se peguen
        (string tecla, string desc)[] c = {
            ("A D  /  < >", "Moverse"),
            ("E",           "Interactuar"),
            ("Esc",         "Pausar"),
            ("1  2  3",     "Elegir opci\u00f3n")
        };

        float cw  = 190f;
        float ch  = 28f;
        float gap = 8f;

        for (int i = 0; i < 4; i++)
        {
            float cx = (i % 2 == 0) ? -100f : 100f;
            float cy = y - (i / 2) * (ch + gap);

            // Fondo celda
            var cell = Img(padre, "Ctrl"+i, new Color(1,1,1,0.05f));
            Place(cell, new Vector2(cx, cy), new Vector2(cw, ch));

            // Tecla — anclada al lado izquierdo con padding de 10px
            var key = TMP(cell, "Key", c[i].tecla, 11f, Color.white);
            var keyRt = key.gameObject.GetComponent<RectTransform>();
            keyRt.anchorMin        = new Vector2(0f, 0f);
            keyRt.anchorMax        = new Vector2(0f, 1f);
            keyRt.pivot            = new Vector2(0f, 0.5f);
            keyRt.anchoredPosition = new Vector2(10f, 0f);
            keyRt.sizeDelta        = new Vector2(82f, 0f);
            key.alignment          = TextAlignmentOptions.Left;
            key.fontStyle          = FontStyles.Bold;

            // Separador vertical
            var sep = Img(cell, "Sep", new Color(1,1,1,0.1f));
            var sepRt = sep.GetComponent<RectTransform>();
            sepRt.anchorMin        = new Vector2(0f, 0.15f);
            sepRt.anchorMax        = new Vector2(0f, 0.85f);
            sepRt.pivot            = new Vector2(0f, 0.5f);
            sepRt.anchoredPosition = new Vector2(98f, 0f);
            sepRt.sizeDelta        = new Vector2(1f, 0f);

            // Descripcion
            var desc = TMP(cell, "Desc", c[i].desc, 11f, new Color(0.65f,0.65f,0.65f));
            var descRt = desc.gameObject.GetComponent<RectTransform>();
            descRt.anchorMin        = new Vector2(0f, 0f);
            descRt.anchorMax        = new Vector2(1f, 1f);
            descRt.pivot            = new Vector2(0f, 0.5f);
            descRt.anchoredPosition = new Vector2(108f, 0f);
            descRt.sizeDelta        = new Vector2(-114f, 0f);
            desc.alignment          = TextAlignmentOptions.Left;
        }

        y -= 2 * (ch + gap) + 2f;
    }

    private Button Btn(GameObject padre, string label, float y, Color cTxt, Color cBg)
    {
        var go = Img(padre, "Btn_"+label, cBg);
        Place(go, new Vector2(0,y), new Vector2(360,38));

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
}