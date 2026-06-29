using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class FinalManager : MonoBehaviour
{
    public static FinalManager Instance { get; private set; }

    [Header("Escenas de inundacion")]
    [Tooltip("riesgoInundacion <= 2")]
    public EscenaRolData finalInundacionBueno;

    [Tooltip("riesgoInundacion >= 4 y <= 5")]
    public EscenaRolData finalInundacionMedio;

    [Tooltip("riesgoInundacion >= 6")]
    public EscenaRolData finalInundacionMalo;

    [Header("Escenas de contaminacion")]
    [Tooltip("contaminacion <= 2")]
    public EscenaRolData finalContaminacionBueno;

    [Tooltip("contaminacion >= 3")]
    public EscenaRolData finalContaminacionMalo;

    [Header("Pantalla de fin")]
    public float duracionFin = 4f;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    // ── API publica ───────────────────────────────────────────────────────────

    /// <summary>
    /// Fase 1: muestra escena de inundacion, luego teletransporta a StreetWest
    /// de noche y guarda la escena de contaminacion como pendiente.
    /// </summary>
    public void IniciarFinales()
    {
        PlayerMovement.Bloqueado = true;
        DontDestroyOnLoad(gameObject);
        StartCoroutine(Fase1_Inundacion());
    }

    /// <summary>
    /// Fase 2: llamado por SceneController despues de mostrar la escena
    /// de contaminacion. Muestra la pantalla FIN y vuelve al menu.
    /// </summary>
    public void MostrarFin()
    {
        StartCoroutine(Fase2_Fin());
    }

    // ── Secuencias ────────────────────────────────────────────────────────────

    private IEnumerator Fase1_Inundacion()
    {
        // Mostrar escena de inundacion
        EscenaRolData escenaInundacion = ObtenerEscenaInundacion();

        // Desbloquear en galeria
        int riesgo = GameManager.Instance.riesgoInundacion;
        if (riesgo <= 2) GaleriaManager.Desbloquear(GaleriaManager.KEY_DBUENO);
        else if (riesgo <= 7) GaleriaManager.Desbloquear(GaleriaManager.KEY_DMEDIO);
        else GaleriaManager.Desbloquear(GaleriaManager.KEY_DMALO);

        if (escenaInundacion != null)
        {
            bool terminado = false;
            EscenaRolManager.Instance.MostrarEscena(escenaInundacion, () => terminado = true);
            yield return new WaitUntil(() => terminado);
        }

        // FadeOut → cambiar a StreetWest de noche → guardar contaminacion pendiente
        if (FadeManager.Instance != null)
        {
            bool fadeOut = false;
            FadeManager.Instance.FadeOut(0.8f, () => fadeOut = true);
            yield return new WaitUntil(() => fadeOut);
        }

        GameManager.Instance.esNoche          = true;
        GameManager.Instance.escenaActual     = Escenas.StreetWest;
        GameManager.Instance.llegoPorDerecha  = false; // spawn derecho
        GameManager.Instance.vieneDeReflexion = true;

        // Guardar escena de contaminacion — SceneController la mostrara al cargar
        GameManager.Instance.escenaRolPendiente = ObtenerEscenaContaminacion();
        GameManager.Instance.fase2Final         = true;  // fase 2, no fase 1
        GameManager.Instance.finalPendiente     = false; // evitar que SceneController lo detecte como fase 1

        SceneManager.LoadScene("GameScene");
    }

    private IEnumerator Fase2_Fin()
    {
        PlayerMovement.Bloqueado = true;

        // Desbloquear final de contaminacion en galeria
        int cont = GameManager.Instance.contaminacion;
        if (cont <= 2) GaleriaManager.Desbloquear(GaleriaManager.KEY_RBUENA);
        else GaleriaManager.Desbloquear(GaleriaManager.KEY_RMALA);
        // FadeOut
        if (FadeManager.Instance != null)
        {
            bool fadeOut = false;
            FadeManager.Instance.FadeOut(1f, () => fadeOut = true);
            yield return new WaitUntil(() => fadeOut);
        }

        // Texto FIN
        GameObject canvasObj = new GameObject("FinCanvas");
        Canvas cv            = canvasObj.AddComponent<Canvas>();
        cv.renderMode        = RenderMode.ScreenSpaceOverlay;
        cv.sortingOrder      = 1001;

        GameObject textoObj = new GameObject("TextoFin");
        textoObj.transform.SetParent(canvasObj.transform, false);
        var tmp       = textoObj.AddComponent<TextMeshProUGUI>();
        tmp.text      = "FIN";
        tmp.fontSize  = 72;
        tmp.color     = new Color(1f, 1f, 1f, 0f);
        tmp.alignment = TextAlignmentOptions.Center;
        var rt        = textoObj.GetComponent<RectTransform>();
        rt.anchorMin  = Vector2.zero;
        rt.anchorMax  = Vector2.one;
        rt.offsetMin  = Vector2.zero;
        rt.offsetMax  = Vector2.zero;

        // FadeIn del texto
        float t = 0f, dur = 1.5f;
        while (t < dur) { t += Time.deltaTime; tmp.color = new Color(1,1,1, t/dur); yield return null; }
        tmp.color = Color.white;

        yield return new WaitForSeconds(duracionFin);

        // FadeOut del texto
        t = 0f;
        while (t < dur) { t += Time.deltaTime; tmp.color = new Color(1,1,1, 1f - t/dur); yield return null; }

        Destroy(canvasObj);

        // Reset y volver al menu
        GameManager.Instance.esInicioJuego    = true;
        GameManager.Instance.diaActual        = 1;
        GameManager.Instance.escenaActual     = Escenas.Home;
        GameManager.Instance.contaminacion    = 5;
        GameManager.Instance.bienestarUrbano  = 5;
        GameManager.Instance.riesgoInundacion = 5;
        GameManager.Instance.estadoArboles    = 1;
        GameManager.Instance.esNoche          = false;
        PlayerMovement.Bloqueado              = false;

        if (NarrativeManager.Instance != null)
            NarrativeManager.Instance.Resetear();

        Destroy(gameObject);
        SceneManager.LoadScene("MainMenu");
    }

    // ── Logica de seleccion ───────────────────────────────────────────────────

    public EscenaRolData ObtenerContaminacionPublico() => ObtenerEscenaContaminacion();

    private EscenaRolData ObtenerEscenaInundacion()
    {
        int riesgo = GameManager.Instance.riesgoInundacion;
        if (riesgo <= 2) return finalInundacionBueno;
        if (riesgo <= 7) return finalInundacionMedio;
        return finalInundacionMalo; // >= 8
    }

    private EscenaRolData ObtenerEscenaContaminacion()
    {
        int cont = GameManager.Instance.contaminacion;
        if (cont <= 2) return finalContaminacionBueno;
        return finalContaminacionMalo;
    }
}