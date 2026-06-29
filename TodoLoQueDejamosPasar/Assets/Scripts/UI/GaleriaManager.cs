using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GaleriaManager : MonoBehaviour
{
    [Header("Sprites de los finales")]
    public Sprite spriteDesenlaceBueno;
    public Sprite spriteDesenlaceMedio;
    public Sprite spriteDesenlaceMalo;
    public Sprite spriteReflexionBuena;
    public Sprite spriteReflexionMala;

    [Header("EscenaRolData de los finales")]
    public EscenaRolData escenaDesenlaceBueno;
    public EscenaRolData escenaDesenlaceMedio;
    public EscenaRolData escenaDesenlaceMalo;
    public EscenaRolData escenaReflexionBuena;
    public EscenaRolData escenaReflexionMala;

    [Header("Icono de candado (opcional — mismo para todos)")]
    public Sprite spriteCandado;

    // Claves PlayerPrefs
    public const string KEY_DBUENO = "galeria_dbueno";
    public const string KEY_DMEDIO = "galeria_dmedio";
    public const string KEY_DMALO  = "galeria_dmalo";
    public const string KEY_RBUENA = "galeria_rbuena";
    public const string KEY_RMALA  = "galeria_rmala";

    private GameObject   panel;
    private GameObject[] botonesMenu;

    // Datos internos de cada entrada
    private struct DatosFinal
    {
        public string        key;
        public Sprite        sprite;
        public EscenaRolData escena;
        public string        titulo;
        public bool          esEpilogo;
    }

    private void Start()
    {
        // Guardar botones del menu para ocultarlos
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

        // Si venimos de reproducir una escena desde la galeria, reabrirla
        if (GameManager.Instance != null && GameManager.Instance.reabrirGaleria)
        {
            GameManager.Instance.reabrirGaleria = false;
            AbrirGaleria();
        }
    }

    // ── API publica ───────────────────────────────────────────────────────────

    public void AbrirGaleria()
    {
        if (botonesMenu != null)
            foreach (var b in botonesMenu) b.SetActive(false);

        ActualizarEstados();
        panel.SetActive(true);
    }

    public void CerrarGaleria()
    {
        if (botonesMenu != null)
            foreach (var b in botonesMenu) b.SetActive(true);

        panel.SetActive(false);
    }

    public static void Desbloquear(string key)
    {
        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save();
    }

    // ── Construccion del panel ────────────────────────────────────────────────

    private GameObject[] entradas = new GameObject[5];
    private Image[]      imagenesEntradas = new Image[5];

    private void ConstruirPanel()
    {
        // Canvas
        GameObject cvGo        = new GameObject("GaleriaCanvas");
        Canvas cv              = cvGo.AddComponent<Canvas>();
        cv.renderMode          = RenderMode.ScreenSpaceOverlay;
        cv.sortingOrder        = 200;
        CanvasScaler cs        = cvGo.AddComponent<CanvasScaler>();
        cs.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1920, 1080);
        cs.matchWidthOrHeight  = 0.5f;
        cvGo.AddComponent<GraphicRaycaster>();

        // Fondo oscuro = panel
        panel = Img(cvGo, "GaleriaFondo", new Color(0, 0, 0, 0.82f));
        Stretch(panel);

        // Caja central
        float W  = 780f;
        GameObject box = Img(panel, "Caja", new Color(0.08f, 0.08f, 0.08f, 0.97f));
        var boxRt              = box.GetComponent<RectTransform>();
        boxRt.anchorMin        = new Vector2(0.5f, 0.5f);
        boxRt.anchorMax        = new Vector2(0.5f, 0.5f);
        boxRt.pivot            = new Vector2(0.5f, 0.5f);
        boxRt.anchoredPosition = Vector2.zero;
        boxRt.sizeDelta        = new Vector2(W, 540f);

        // Titulo
        var titulo = TMP(box, "Titulo", "Galer\u00eda", 24f, Color.white);
        Place(titulo.gameObject, new Vector2(0, 222f), new Vector2(W - 40f, 36f));
        titulo.alignment = TextAlignmentOptions.Center;
        titulo.fontStyle = FontStyles.Bold;

        HLine(box, 194f, W - 40f);

        // Subtitulos de fila
        var lblDesenlaces = TMP(box, "LblD", "FINALES AMBIENTALES", 9f, new Color(0.4f, 0.4f, 0.4f));
        Place(lblDesenlaces.gameObject, new Vector2(0f, 168f), new Vector2(W - 40f, 18f));
        lblDesenlaces.alignment = TextAlignmentOptions.Center;
        lblDesenlaces.fontStyle = FontStyles.Bold;

        var lblReflexiones = TMP(box, "LblR", "EPÍLOGOS", 9f, new Color(0.4f, 0.4f, 0.4f));
        Place(lblReflexiones.gameObject, new Vector2(0f, -22f), new Vector2(W - 40f, 18f));
        lblReflexiones.alignment = TextAlignmentOptions.Center;
        lblReflexiones.fontStyle = FontStyles.Bold;

        // Datos de las 5 entradas
        DatosFinal[] datos = new DatosFinal[]
        {
            new DatosFinal { key = KEY_DBUENO, sprite = spriteDesenlaceBueno, escena = escenaDesenlaceBueno, titulo = "Desenlace\nBueno"  },
            new DatosFinal { key = KEY_DMEDIO, sprite = spriteDesenlaceMedio, escena = escenaDesenlaceMedio, titulo = "Desenlace\nMedio"  },
            new DatosFinal { key = KEY_DMALO,  sprite = spriteDesenlaceMalo,  escena = escenaDesenlaceMalo,  titulo = "Desenlace\nMalo"   },
            new DatosFinal { key = KEY_RBUENA, sprite = spriteReflexionBuena, escena = escenaReflexionBuena, titulo = "Reflexi\u00f3n\nBuena" },
            new DatosFinal { key = KEY_RMALA,  sprite = spriteReflexionMala,  escena = escenaReflexionMala,  titulo = "Reflexi\u00f3n\nMala"  },
        };

        // Posiciones: 3 arriba, 2 abajo centradas
        Vector2[] posiciones = new Vector2[]
        {
            new Vector2(-240f,  100f),  // desenlace bueno
            new Vector2(   0f,  100f),  // desenlace medio
            new Vector2( 240f,  100f),  // desenlace malo
            new Vector2(-120f,  -90f),  // reflexion buena
            new Vector2( 120f,  -90f),  // reflexion mala
        };

        float cardW = 180f;
        float cardH = 120f;

        for (int i = 0; i < 5; i++)
        {
            int idx = i; // captura para lambda
            DatosFinal d = datos[i];

            // Contenedor
            GameObject card = Img(box, "Card_" + i, new Color(0.05f, 0.05f, 0.05f, 1f));
            Place(card, posiciones[i], new Vector2(cardW, cardH));

            // Imagen del final
            GameObject imgGo = new GameObject("Img");
            imgGo.transform.SetParent(card.transform, false);
            imgGo.AddComponent<RectTransform>();
            Image imgComp = imgGo.AddComponent<Image>();
            imgComp.sprite = d.sprite;
            imgComp.preserveAspect = false;
            var imgRt = imgGo.GetComponent<RectTransform>();
            imgRt.anchorMin = Vector2.zero;
            imgRt.anchorMax = Vector2.one;
            imgRt.offsetMin = Vector2.zero;
            imgRt.offsetMax = Vector2.zero;
            imagenesEntradas[i] = imgComp;

            // Degradado inferior para legibilidad del titulo
            GameObject deg = new GameObject("Deg");
            deg.transform.SetParent(card.transform, false);
            deg.AddComponent<RectTransform>();
            Image degImg = deg.AddComponent<Image>();
            degImg.color = new Color(0, 0, 0, 0.65f);
            var degRt = deg.GetComponent<RectTransform>();
            degRt.anchorMin = new Vector2(0, 0);
            degRt.anchorMax = new Vector2(1, 0.35f);
            degRt.offsetMin = Vector2.zero;
            degRt.offsetMax = Vector2.zero;

            // Titulo debajo de la card
            var tit = TMP(box, "Tit_" + i, d.titulo, 9f, new Color(0.8f, 0.8f, 0.8f));
            var titRt = tit.gameObject.GetComponent<RectTransform>();
            titRt.anchorMin        = new Vector2(0.5f, 0.5f);
            titRt.anchorMax        = new Vector2(0.5f, 0.5f);
            titRt.pivot            = new Vector2(0.5f, 1f);
            titRt.anchoredPosition = posiciones[idx] + new Vector2(0, -cardH / 2f - 2f);
            titRt.sizeDelta        = new Vector2(cardW, 28f);
            tit.alignment          = TextAlignmentOptions.Center;

            // Candado (si hay sprite)
            if (spriteCandado != null)
            {
                GameObject candGo = new GameObject("Candado");
                candGo.transform.SetParent(card.transform, false);
                candGo.AddComponent<RectTransform>();
                Image candImg = candGo.AddComponent<Image>();
                candImg.sprite = spriteCandado;
                candImg.preserveAspect = true;
                var cRt = candGo.GetComponent<RectTransform>();
                cRt.anchorMin = new Vector2(0.5f, 0.5f);
                cRt.anchorMax = new Vector2(0.5f, 0.5f);
                cRt.pivot     = new Vector2(0.5f, 0.5f);
                cRt.anchoredPosition = new Vector2(0, 15f);
                cRt.sizeDelta = new Vector2(36f, 36f);
                entradas[i] = candGo; // guardamos el candado para mostrarlo/ocultarlo
            }

            // Boton transparente encima de todo
            Button btn = card.AddComponent<Button>();
            btn.targetGraphic = card.GetComponent<Image>();
            var cb = btn.colors;
            cb.normalColor      = new Color(0.05f, 0.05f, 0.05f, 1f);
            cb.highlightedColor = new Color(0.15f, 0.15f, 0.15f, 1f);
            cb.pressedColor     = new Color(0.02f, 0.02f, 0.02f, 1f);
            cb.selectedColor    = cb.normalColor;
            btn.colors = cb;

            EscenaRolData escena = d.escena;
            string key           = d.key;
            btn.onClick.AddListener(() =>
            {
                if (PlayerPrefs.GetInt(key, 0) == 0) return;
                if (escena == null) return;
                bool esEpilogoFinal = (key == KEY_RBUENA || key == KEY_RMALA);
                GameManager.Instance.escenaRolPendiente = escena;
                GameManager.Instance.vieneDeReflexion   = true;
                GameManager.Instance.escenaActual       = Escenas.Home;
                GameManager.Instance.llegoPorDerecha    = true;
                GameManager.Instance.esNoche            = esEpilogoFinal;
                GameManager.Instance.galeriaActiva      = true;
                GameManager.Instance.esInicioJuego      = false;
                PlayerMovement.Bloqueado                = true;
                SceneManager.LoadScene("GameScene");
            });
        }

        HLine(box, -210f, W - 40f);

        // Boton cerrar
        var btnCerrar = BtnTexto(box, "Cerrar", -240f, W - 40f);
        btnCerrar.onClick.AddListener(CerrarGaleria);
    }

    private void ActualizarEstados()
    {
        string[] keys = { KEY_DBUENO, KEY_DMEDIO, KEY_DMALO, KEY_RBUENA, KEY_RMALA };
        for (int i = 0; i < 5; i++)
        {
            bool desbloqueado = PlayerPrefs.GetInt(keys[i], 0) == 1;

            if (imagenesEntradas[i] != null)
                imagenesEntradas[i].color = desbloqueado
                    ? Color.white
                    : new Color(0.35f, 0.35f, 0.35f, 0.45f);

            if (entradas[i] != null) // candado
                entradas[i].SetActive(!desbloqueado);
        }
    }

    // ── Helpers de UI ─────────────────────────────────────────────────────────

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

    private Button BtnTexto(GameObject padre, string label, float y, float iw)
    {
        var go = Img(padre, "Btn_"+label, new Color(0.18f, 0.18f, 0.18f));
        Place(go, new Vector2(0, y), new Vector2(iw, 36f));
        var btn           = go.AddComponent<Button>();
        btn.targetGraphic = go.GetComponent<Image>();
        var cb = btn.colors;
        cb.normalColor      = new Color(0.18f, 0.18f, 0.18f);
        cb.highlightedColor = new Color(0.28f, 0.28f, 0.28f);
        cb.pressedColor     = new Color(0.10f, 0.10f, 0.10f);
        cb.selectedColor    = cb.normalColor;
        btn.colors          = cb;
        var txt = TMP(go, "Label", label, 14f, Color.white);
        Stretch(txt.gameObject);
        txt.alignment = TextAlignmentOptions.Center;
        return btn;
    }
}