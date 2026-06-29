using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

public class EscenaRolManager : MonoBehaviour
{
    public static EscenaRolManager Instance { get; private set; }

    [Header("Configuracion visual")]
    public float velocidadTypewriter = 40f;
    public float duracionFade        = 0.5f;

    public bool EstaActivo => activo || enDecision;

    private Canvas   canvasRol;
    private Image    imagenFondo;
    private Image    degradadoInferior;
    private TMP_Text textoDialogo;
    private TMP_Text textoContinuar;

    private bool          activo           = false;
    private bool          enDecision       = false;
    private bool          typewriterListo  = false;
    private float         tiempoEsperaPost = 0f;
    private int           lineaActual      = 0;
    private EscenaRolData datosActuales;
    private Action        callbackCompleto;
    private Coroutine     coroutineTypewriter;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        CrearUI();
    }

    private void Update()
    {
        if (!activo) return;

        if (tiempoEsperaPost > 0f)
        {
            tiempoEsperaPost -= Time.deltaTime;
            return;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            LineaRol linea = datosActuales.lineas[lineaActual];
            if (linea.decision != null) return;

            if (!typewriterListo)
            {
                if (coroutineTypewriter != null)
                    StopCoroutine(coroutineTypewriter);

                textoDialogo.maxVisibleCharacters = int.MaxValue;
                typewriterListo                   = true;
                tiempoEsperaPost                  = 0.2f;
                MostrarIndicador(true);
            }
            else
            {
                AvanzarLinea();
            }
        }
    }

    // ── API publica ───────────────────────────────────────────────────────────

    public void MostrarEscena(EscenaRolData datos, Action onCompleto)
    {
        if (datos == null || datos.lineas.Length == 0)
        {
            onCompleto?.Invoke();
            return;
        }

        datosActuales    = datos;
        callbackCompleto = onCompleto;
        lineaActual      = 0;
        activo           = true;
        enDecision       = false;

        canvasRol.gameObject.SetActive(true);
        StartCoroutine(AbrirEscena());
    }

    // ── Logica interna ────────────────────────────────────────────────────────

    private IEnumerator AbrirEscena()
    {
        yield return StartCoroutine(FadeCanvas(0f, 1f, duracionFade));
        MostrarLinea(lineaActual);
    }

    private void MostrarLinea(int indice)
    {
        LineaRol linea = datosActuales.lineas[indice];

        if (linea.imagen != null)
            imagenFondo.sprite = linea.imagen;

        textoDialogo.text                 = linea.texto;
        textoDialogo.maxVisibleCharacters = 0;
        typewriterListo                   = false;
        tiempoEsperaPost                  = 0f;
        MostrarIndicador(false);

        if (coroutineTypewriter != null)
            StopCoroutine(coroutineTypewriter);

        coroutineTypewriter = StartCoroutine(EfectoTypewriter(linea.texto));

        if (linea.decision != null)
            StartCoroutine(MostrarDecisionAlTerminar(linea.decision));
    }

    private IEnumerator EfectoTypewriter(string texto)
    {
        int   total  = texto.Length;
        float espera = 1f / velocidadTypewriter;

        for (int i = 0; i <= total; i++)
        {
            textoDialogo.maxVisibleCharacters = i;
            yield return new WaitForSeconds(espera);
        }

        typewriterListo = true;

        LineaRol linea = datosActuales.lineas[lineaActual];
        if (linea.decision == null)
            MostrarIndicador(true);
    }

    private IEnumerator MostrarDecisionAlTerminar(DecisionData decision)
    {
        yield return new WaitUntil(() => typewriterListo);

        yield return StartCoroutine(FadeCanvas(1f, 0f, duracionFade));
        canvasRol.gameObject.SetActive(false);

        activo     = false;
        enDecision = true;

        DecisionUI.Instance.MostrarDecision(
            decision,
            null,
            (indice) =>
            {
                enDecision = false;

                OpcionDecision opcion  = decision.opciones[indice];
                int siguienteLinea     = lineaActual + 1;
                bool hayMasLineas      = siguienteLinea < datosActuales.lineas.Length;

                if (opcion.dialogoReaccion != null)
                {
                    DialogueManager.Instance.IniciarDialogo(
                        opcion.dialogoReaccion,
                        null,
                        () =>
                        {
                            if (hayMasLineas)
                            {
                                activo = true;
                                canvasRol.gameObject.SetActive(true);
                                StartCoroutine(ReanudarEscena());
                            }
                            else
                            {
                                callbackCompleto?.Invoke();
                            }
                        }
                    );
                }
                else
                {
                    if (hayMasLineas)
                    {
                        activo = true;
                        canvasRol.gameObject.SetActive(true);
                        StartCoroutine(ReanudarEscena());
                    }
                    else
                    {
                        callbackCompleto?.Invoke();
                    }
                }
            }
        );
    }

    private IEnumerator ReanudarEscena()
    {
        yield return StartCoroutine(FadeCanvas(0f, 1f, duracionFade));
        AvanzarLinea();
    }

    private void AvanzarLinea()
    {
        lineaActual++;

        if (lineaActual < datosActuales.lineas.Length)
            MostrarLinea(lineaActual);
        else
            CerrarEscena();
    }

    private void CerrarEscena()
    {
        activo = false;
        StartCoroutine(CerrarConFade());
    }

    private IEnumerator CerrarConFade()
    {
        MostrarIndicador(false);
        yield return StartCoroutine(FadeCanvas(1f, 0f, duracionFade));
        canvasRol.gameObject.SetActive(false);
        callbackCompleto?.Invoke();
    }

    private IEnumerator FadeCanvas(float desde, float hasta, float duracion)
    {
        CanvasGroup cg = canvasRol.GetComponent<CanvasGroup>();
        if (cg == null) cg = canvasRol.gameObject.AddComponent<CanvasGroup>();

        float tiempo = 0f;
        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            cg.alpha = Mathf.Lerp(desde, hasta, Mathf.Clamp01(tiempo / duracion));
            yield return null;
        }
        cg.alpha = hasta;
    }

    private void MostrarIndicador(bool mostrar)
    {
        if (textoContinuar != null)
            textoContinuar.gameObject.SetActive(mostrar);
    }

    // ── Construccion de UI ────────────────────────────────────────────────────

    private void CrearUI()
    {
        GameObject canvasObj = new GameObject("EscenaRolCanvas");
        canvasObj.transform.SetParent(transform);

        canvasRol = canvasObj.AddComponent<Canvas>();
        canvasRol.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvasRol.sortingOrder = 1000; // Por encima del FadeCanvas (999)

        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        CanvasGroup cg = canvasObj.AddComponent<CanvasGroup>();
        cg.alpha = 0f;

        GameObject imgObj = new GameObject("ImagenFondo");
        imgObj.transform.SetParent(canvasObj.transform, false);
        imagenFondo                = imgObj.AddComponent<Image>();
        imagenFondo.color          = Color.white;
        imagenFondo.preserveAspect = false;
        imagenFondo.raycastTarget  = false;
        EstirarAlCanvas(imgObj);

        GameObject degradadoObj = new GameObject("Degradado");
        degradadoObj.transform.SetParent(canvasObj.transform, false);
        degradadoInferior = degradadoObj.AddComponent<Image>();

        Texture2D texGrad = new Texture2D(1, 2);
        texGrad.SetPixel(0, 0, new Color(0f, 0f, 0f, 1f));
        texGrad.SetPixel(0, 1, new Color(0f, 0f, 0f, 0f));
        texGrad.Apply();
        degradadoInferior.sprite = Sprite.Create(
            texGrad,
            new Rect(0, 0, 1, 2),
            new Vector2(0.5f, 0.5f)
        );
        degradadoInferior.color         = Color.white;
        degradadoInferior.raycastTarget = false;

        RectTransform rtDeg = degradadoObj.GetComponent<RectTransform>();
        rtDeg.anchorMin = new Vector2(0f, 0f);
        rtDeg.anchorMax = new Vector2(1f, 0.4f);
        rtDeg.offsetMin = Vector2.zero;
        rtDeg.offsetMax = Vector2.zero;

        GameObject textoObj = new GameObject("TextoDialogo");
        textoObj.transform.SetParent(canvasObj.transform, false);
        textoDialogo                  = textoObj.AddComponent<TextMeshProUGUI>();
        textoDialogo.fontSize         = 26;
        textoDialogo.color            = Color.white;
        textoDialogo.alignment        = TextAlignmentOptions.BottomLeft;
        textoDialogo.textWrappingMode = TextWrappingModes.Normal;

        RectTransform rtTexto = textoObj.GetComponent<RectTransform>();
        rtTexto.anchorMin = new Vector2(0.05f, 0.06f);
        rtTexto.anchorMax = new Vector2(0.95f, 0.32f);
        rtTexto.offsetMin = Vector2.zero;
        rtTexto.offsetMax = Vector2.zero;

        GameObject continObj = new GameObject("Continuar");
        continObj.transform.SetParent(canvasObj.transform, false);
        textoContinuar           = continObj.AddComponent<TextMeshProUGUI>();
        textoContinuar.text      = "[ E ] continuar";
        textoContinuar.fontSize  = 14;
        textoContinuar.color     = new Color(0.7f, 0.7f, 0.7f, 1f);
        textoContinuar.alignment = TextAlignmentOptions.Right;

        RectTransform rtCont = continObj.GetComponent<RectTransform>();
        rtCont.anchorMin = new Vector2(0.6f, 0.03f);
        rtCont.anchorMax = new Vector2(0.95f, 0.08f);
        rtCont.offsetMin = Vector2.zero;
        rtCont.offsetMax = Vector2.zero;
        continObj.SetActive(false);

        canvasRol.gameObject.SetActive(false);
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