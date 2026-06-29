using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

public class ReflexionManager : MonoBehaviour
{
    public static ReflexionManager Instance { get; private set; }
    public bool EstaActivo => activo;

    [Header("Configuracion visual")]
    public float velocidadTypewriter = 30f;
    public float duracionFadeIn      = 1f;

    private Canvas   canvasReflexion;
    private Image    fondoNegro;
    private TMP_Text textoNombre;
    private Image    lineaSeparadora;
    private TMP_Text textoReflexion;
    private TMP_Text textoContinuar;

    private bool      activo                    = false;
    private bool      typewriterListo           = false;
    private float     tiempoEsperaPostTypewriter = 0f;
    private float     tiempoEsperaInicial        = 0f; // delay al abrir para ignorar el E del trigger
    private Action    callbackCompleto;
    private Coroutine coroutineTypewriter;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        CrearUI();
    }

    private void Update()
    {
        if (!activo) return;

        // Delay inicial — ignora el E que disparó el trigger
        if (tiempoEsperaInicial > 0f)
        {
            tiempoEsperaInicial -= Time.deltaTime;
            return;
        }

        // Delay post typewriter
        if (tiempoEsperaPostTypewriter > 0f)
        {
            tiempoEsperaPostTypewriter -= Time.deltaTime;
            return;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!typewriterListo)
            {
                // Primer E: completa el typewriter de golpe
                if (coroutineTypewriter != null)
                    StopCoroutine(coroutineTypewriter);

                textoReflexion.maxVisibleCharacters = int.MaxValue;
                typewriterListo                     = true;
                tiempoEsperaPostTypewriter          = 0.3f;
                MostrarIndicador(true);
            }
            else
            {
                // Segundo E: cierra
                activo = false;
                StartCoroutine(CerrarYCargar());
            }
        }
    }

    // ── API publica ───────────────────────────────────────────────────────────

    public void MostrarReflexion(ReflexionData datos, Action onCompleto)
    {
        if (datos == null || datos.reflexiones.Length == 0)
        {
            onCompleto?.Invoke();
            return;
        }

        Reflexion reflexion = ObtenerReflexion(datos);

        if (reflexion == null)
        {
            onCompleto?.Invoke();
            return;
        }

        callbackCompleto           = onCompleto;
        activo                     = true;
        typewriterListo            = false;
        tiempoEsperaPostTypewriter = 0f;
        tiempoEsperaInicial        = 0.3f; // ignora el E del trigger al abrir
        coroutineTypewriter        = null;

        MostrarIndicador(false);
        canvasReflexion.gameObject.SetActive(true);

        StartCoroutine(SecuenciaReflexion(reflexion.texto));
    }

    // ── Logica interna ────────────────────────────────────────────────────────

    private Reflexion ObtenerReflexion(ReflexionData datos)
    {
        foreach (var r in datos.reflexiones)
            if (r.CondicionesCumplidas()) return r;
        return null;
    }

    private IEnumerator SecuenciaReflexion(string texto)
    {
        yield return StartCoroutine(FadePanel(0f, 1f, duracionFadeIn));

        textoReflexion.text                 = texto;
        textoReflexion.maxVisibleCharacters = 0;
        textoReflexion.gameObject.SetActive(true);
        textoNombre.gameObject.SetActive(true);
        lineaSeparadora.gameObject.SetActive(true);

        coroutineTypewriter = StartCoroutine(EfectoTypewriter(texto));
    }

    private IEnumerator EfectoTypewriter(string texto)
    {
        int   total  = texto.Length;
        float espera = 1f / velocidadTypewriter;

        for (int i = 0; i <= total; i++)
        {
            textoReflexion.maxVisibleCharacters = i;
            yield return new WaitForSeconds(espera);
        }

        typewriterListo = true;
        MostrarIndicador(true);
    }

    private IEnumerator CerrarYCargar()
    {
        textoNombre.gameObject.SetActive(false);
        lineaSeparadora.gameObject.SetActive(false);
        textoReflexion.gameObject.SetActive(false);
        MostrarIndicador(false);
        canvasReflexion.gameObject.SetActive(false);

        if (FadeManager.Instance != null)
            FadeManager.Instance.PantallaEnNegro();

        yield return null;

        callbackCompleto?.Invoke();
    }

    private IEnumerator FadePanel(float desde, float hasta, float duracion)
    {
        float tiempo = 0f;
        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            SetAlphaFondo(Mathf.Lerp(desde, hasta, Mathf.Clamp01(tiempo / duracion)));
            yield return null;
        }
        SetAlphaFondo(hasta);
    }

    private void SetAlphaFondo(float alpha)
    {
        Color c  = fondoNegro.color;
        c.a      = alpha;
        fondoNegro.color = c;
    }

    private void MostrarIndicador(bool mostrar)
    {
        if (textoContinuar != null)
            textoContinuar.gameObject.SetActive(mostrar);
    }

    // ── Construccion de UI ────────────────────────────────────────────────────

    private void CrearUI()
    {
        GameObject canvasObj = new GameObject("ReflexionCanvas");
        canvasObj.transform.SetParent(transform);

        canvasReflexion = canvasObj.AddComponent<Canvas>();
        canvasReflexion.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvasReflexion.sortingOrder = 100;

        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject fondoObj = new GameObject("Fondo");
        fondoObj.transform.SetParent(canvasObj.transform, false);
        fondoNegro       = fondoObj.AddComponent<Image>();
        fondoNegro.color = new Color(0f, 0f, 0f, 0f);
        EstirarAlCanvas(fondoObj);

        GameObject contenedorObj = new GameObject("Contenedor");
        contenedorObj.transform.SetParent(canvasObj.transform, false);
        RectTransform rt = contenedorObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.2f, 0.3f);
        rt.anchorMax = new Vector2(0.8f, 0.7f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        GameObject nombreObj = new GameObject("Nombre");
        nombreObj.transform.SetParent(contenedorObj.transform, false);
        textoNombre           = nombreObj.AddComponent<TextMeshProUGUI>();
        textoNombre.text      = "Mateo";
        textoNombre.fontSize  = 18;
        textoNombre.color     = new Color(0.6f, 0.6f, 0.6f, 1f);
        textoNombre.alignment = TextAlignmentOptions.Left;
        RectTransform rtNombre = nombreObj.GetComponent<RectTransform>();
        rtNombre.anchorMin = new Vector2(0f, 0.85f);
        rtNombre.anchorMax = new Vector2(1f, 1f);
        rtNombre.offsetMin = Vector2.zero;
        rtNombre.offsetMax = Vector2.zero;
        nombreObj.SetActive(false);

        GameObject lineaObj = new GameObject("Linea");
        lineaObj.transform.SetParent(contenedorObj.transform, false);
        lineaSeparadora       = lineaObj.AddComponent<Image>();
        lineaSeparadora.color = new Color(0.4f, 0.4f, 0.4f, 1f);
        RectTransform rtLinea = lineaObj.GetComponent<RectTransform>();
        rtLinea.anchorMin = new Vector2(0f, 0.82f);
        rtLinea.anchorMax = new Vector2(1f, 0.83f);
        rtLinea.offsetMin = Vector2.zero;
        rtLinea.offsetMax = Vector2.zero;
        lineaObj.SetActive(false);

        GameObject textoObj = new GameObject("Reflexion");
        textoObj.transform.SetParent(contenedorObj.transform, false);
        textoReflexion                  = textoObj.AddComponent<TextMeshProUGUI>();
        textoReflexion.fontSize         = 22;
        textoReflexion.color            = Color.white;
        textoReflexion.alignment        = TextAlignmentOptions.TopLeft;
        textoReflexion.fontStyle        = FontStyles.Italic;
        textoReflexion.textWrappingMode = TextWrappingModes.Normal;
        RectTransform rtTexto = textoObj.GetComponent<RectTransform>();
        rtTexto.anchorMin = new Vector2(0f, 0f);
        rtTexto.anchorMax = new Vector2(1f, 0.8f);
        rtTexto.offsetMin = Vector2.zero;
        rtTexto.offsetMax = Vector2.zero;
        textoObj.SetActive(false);

        GameObject continObj = new GameObject("Continuar");
        continObj.transform.SetParent(canvasObj.transform, false);
        textoContinuar           = continObj.AddComponent<TextMeshProUGUI>();
        textoContinuar.text      = "[ E ] continuar";
        textoContinuar.fontSize  = 14;
        textoContinuar.color     = new Color(0.5f, 0.5f, 0.5f, 1f);
        textoContinuar.alignment = TextAlignmentOptions.Center;
        RectTransform rtContinuar = continObj.GetComponent<RectTransform>();
        rtContinuar.anchorMin = new Vector2(0.3f, 0.05f);
        rtContinuar.anchorMax = new Vector2(0.7f, 0.12f);
        rtContinuar.offsetMin = Vector2.zero;
        rtContinuar.offsetMax = Vector2.zero;
        continObj.SetActive(false);

        canvasReflexion.gameObject.SetActive(false);
    }

    private void EstirarAlCanvas(GameObject obj)
    {
        RectTransform rt = obj.GetComponent<RectTransform>();
        if (rt == null) rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}