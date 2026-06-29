using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

[System.Serializable]
public class RequisitoNarrativo
{
    [Tooltip("Dia en que aplica este requisito. 0 = todos los dias")]
    public int dia;

    [Tooltip("ID de la interaccion que debe estar completada para poder cruzar")]
    public string interaccionId;

    [Tooltip("Lo que dice Mateo si intenta cruzar sin haber completado esta interaccion")]
    [TextArea(1, 2)]
    public string textoBloqueo = "Todavia no, necesito hablar con alguien primero.";
}

[System.Serializable]
public class ReflexionPorDia
{
    [Tooltip("Dia en que se muestra esta reflexion. 0 = cualquier dia")]
    public int dia;

    [Tooltip("Datos de la reflexion")]
    public ReflexionData reflexionData;
}

[System.Serializable]
public class EscenaPorDia
{
    [Tooltip("Dia en que aplica esta escena. 0 = cualquier dia")]
    public int dia;

    [Tooltip("Escena de rol a mostrar si las condiciones se cumplen")]
    public EscenaRolData escenaRolData;

    [Tooltip("Si esta marcado, la escena se muestra ANTES de la reflexion del dia. " +
             "Si no, se muestra DESPUES (y si hay cambio de escena, DESPUES de cargar).")]
    public bool antesDeReflexion = true;

    [Header("Condiciones de variables (dejar en Ignorar si no importa)")]
    public CondicionVariable contaminacion;
    public CondicionVariable riesgoInundacion;

    public bool CondicionesCumplidas(int diaParaEvaluar = -1)
    {
        if (escenaRolData == null) return false;

        var gm = GameManager.Instance;
        int diaEval = diaParaEvaluar >= 0 ? diaParaEvaluar : gm.diaActual;

        if (dia != 0 && diaEval != dia) return false;
        if (!contaminacion.Cumplida(gm.contaminacion))       return false;
        if (!riesgoInundacion.Cumplida(gm.riesgoInundacion)) return false;

        return true;
    }
}

public class SceneLoader : MonoBehaviour
{
    [Tooltip("Valores validos: '1. Home' | '2. StreetWest' | '3. Park' | '4. StreetEast' | '5. Work'")]
    public string destinoEscena;

    [Tooltip("Marca true si este trigger esta en el lado DERECHO de la escena.")]
    public bool esTriggerDerecho = false;

    [Header("Control de dia/noche")]
    public bool soloDeNoche    = false;
    public bool activaNoche    = false;
    public bool desactivaNoche = false;

    [Header("Progreso narrativo")]
    public bool diaSiguiente = false;

    [Header("Reflexiones por dia")]
    public ReflexionPorDia[] reflexiones;

    [Header("Escenas de rol por dia")]
    [Tooltip("Se evaluan de arriba a abajo. Se muestra la primera que cumpla dia + condiciones de variable. " +
             "Cada una indica si se dispara antes o despues de la reflexion.")]
    public EscenaPorDia[] escenasPorDia;

    [Header("Final del juego")]
    [Tooltip("Si es mayor a 0, este trigger dispara el final en ese dia especifico en vez de cargar la escena normal.")]
    public int diaFinal = 0;
    public RequisitoNarrativo[] requisitos;

    [Header("Transicion")]
    public bool  usarFade     = false;
    public float duracionFade = 1f;
    public float pausaEnNegro = 0.5f;

    [Header("Indicador de texto")]
    public string  mensajeIndicador = "Presiona \"E\" para avanzar";
    public Vector3 offsetTexto      = new Vector3(0f, 1.5f, 0f);

    [Header("Apariencia")]
    public float anchoCanvas  = 300f;
    public float altoCanvas   = 60f;
    public float escalaCanvas = 0.01f;
    public int   tamanoFuente = 24;
    public Color colorTexto   = Color.white;
    public Color colorFondo   = new Color(0f, 0f, 0f, 0f);

    private bool       jugadorDentro        = false;
    private bool       bloqueandoPorDialogo = false;
    private GameObject canvasObj;

    private void Start()
    {
        CrearIndicador();
        ActualizarVisibilidadPorHora();
    }

    public void ActualizarVisibilidadPorHora()
    {
        bool esNoche    = GameManager.Instance.esNoche;
        bool debeActivo = soloDeNoche ? esNoche : !esNoche;
        gameObject.SetActive(debeActivo);
    }

    private void Update()
    {
        if (!jugadorDentro)       return;
        if (bloqueandoPorDialogo) return;
        if (Input.GetKeyDown(KeyCode.E))
            IntentarCargar();
    }

    // ── API publica ───────────────────────────────────────────────────────────

    public void EjecutarDesdeNPC()
    {
        bloqueandoPorDialogo = true;
        EjecutarFlujo();
    }

    // ── Logica de carga ───────────────────────────────────────────────────────

    private void IntentarCargar()
    {
        RequisitoNarrativo requisitoPendiente = ObtenerRequisitoPendiente();
        if (requisitoPendiente != null)
        {
            bloqueandoPorDialogo = true;
            MostrarBloqueo(requisitoPendiente.textoBloqueo);
            return;
        }

        bloqueandoPorDialogo = true;
        EjecutarFlujo();
    }

    /// <summary>
    /// Flujo completo.
    ///
    /// A) escena(antes) → reflexion → ProcederConCarga
    ///
    /// B) sin cambio de escena:
    ///    reflexion → [negro] AplicarCambiosDeDia + RefrescarFondo
    ///             → FadeIn → escena(despues) → ProcederConCarga
    ///
    /// C) con cambio de escena:
    ///    reflexion → [negro] AplicarCambios → CargarEscena
    ///             → (SceneController detecta escenaRolPendiente)
    ///             → FadeIn → escena(despues) → ProcederConCarga en nueva escena
    ///
    /// D) sin escena → reflexion → ProcederConCarga
    /// </summary>
    private void EjecutarFlujo()
    {
        PlayerMovement.Bloqueado = true;

        int          diaDeEsteDisparo = GameManager.Instance.diaActual;
        EscenaPorDia escenaEncontrada = ObtenerEscenaDelDia(diaDeEsteDisparo);
        bool         hayDestino       = !string.IsNullOrEmpty(destinoEscena);

        if (escenaEncontrada != null && escenaEncontrada.antesDeReflexion)
        {
            // A) escena ANTES → reflexion → carga
            EscenaRolManager.Instance.MostrarEscena(escenaEncontrada.escenaRolData, () =>
            {
                MostrarReflexionY(diaDeEsteDisparo, ProcederConCarga);
            });
        }
        else if (escenaEncontrada != null && hayDestino)
        {
            // C) hay cambio de escena → la escena de rol se muestra en la nueva escena
            //    Guardamos la data en GameManager para que SceneController la recupere.
            MostrarReflexionY(diaDeEsteDisparo, () =>
            {
                // Durante el negro aplicamos cambios y guardamos la escena pendiente.
                AplicarCambiosDeDia();

                GameManager.Instance.escenaRolPendiente = escenaEncontrada.escenaRolData;
                GameManager.Instance.vieneDeReflexion   = true;

                // ProcederConCarga hara el FadeOutIn normal; SceneController
                // detectara escenaRolPendiente en su Start() y la disparara.
                ProcederConCarga();
            });
        }
        else if (escenaEncontrada != null)
        {
            // B) sin cambio de escena → reflexion → FadeIn → escena DESPUES
            MostrarReflexionY(diaDeEsteDisparo, () =>
            {
                AplicarCambiosDeDia();

                var sc = FindAnyObjectByType<SceneController>();
                if (sc != null) sc.RefrescarTodo();

                FadeManager.Instance.FadeIn(duracionFade, () =>
                {
                    EscenaRolManager.Instance.MostrarEscena(
                        escenaEncontrada.escenaRolData,
                        ProcederConCarga);
                });
            });
        }
        else
        {
            // D) sin escena → reflexion → carga
            MostrarReflexionY(diaDeEsteDisparo, ProcederConCarga);
        }
    }

    private void MostrarReflexionY(int diaParaEvaluar, System.Action onCompleto)
    {
        ReflexionData reflexionDelDia = ObtenerReflexionDelDia(diaParaEvaluar);

        if (reflexionDelDia != null && ReflexionManager.Instance != null)
            ReflexionManager.Instance.MostrarReflexion(reflexionDelDia, onCompleto);
        else
            onCompleto?.Invoke();
    }

    // ── Obtener datos ─────────────────────────────────────────────────────────

    private EscenaPorDia ObtenerEscenaDelDia(int diaParaEvaluar)
    {
        if (escenasPorDia == null || escenasPorDia.Length == 0) return null;

        foreach (var e in escenasPorDia)
            if (e != null && e.CondicionesCumplidas(diaParaEvaluar)) return e;

        return null;
    }

    private ReflexionData ObtenerReflexionDelDia(int diaParaEvaluar)
    {
        if (reflexiones == null || reflexiones.Length == 0) return null;

        foreach (var r in reflexiones)
        {
            if (r.reflexionData == null) continue;
            if (r.dia == 0 || r.dia == diaParaEvaluar) return r.reflexionData;
        }

        return null;
    }

    private RequisitoNarrativo ObtenerRequisitoPendiente()
    {
        if (requisitos == null || requisitos.Length == 0) return null;

        int diaActual = GameManager.Instance.diaActual;

        foreach (var req in requisitos)
        {
            bool aplicaHoy = req.dia == 0 || req.dia == diaActual;
            if (!aplicaHoy) continue;
            if (!NarrativeManager.Instance.EstaCompletada(req.interaccionId))
                return req;
        }

        return null;
    }

    // ── Carga de escena ───────────────────────────────────────────────────────

    private void ProcederConCarga()
    {
        bloqueandoPorDialogo     = false;
        if (GameManager.Instance.escenaRolPendiente == null)
            PlayerMovement.Bloqueado = false;

        // Si es el dia del final, cargar la escena y marcar finalPendiente
        if (diaFinal > 0 && GameManager.Instance.diaActual == diaFinal)
            GameManager.Instance.finalPendiente = true;

        if ((reflexiones   != null && reflexiones.Length   > 0) ||
            (escenasPorDia != null && escenasPorDia.Length > 0))
            GameManager.Instance.vieneDeReflexion = true;

        if (usarFade && FadeManager.Instance != null)
        {
            FadeManager.Instance.FadeOutIn(
                duracionFade,
                pausaEnNegro,
                onNegro: () => {
                    AplicarCambios();
                    CargarEscena();
                },
                onCompleto: null
            );
        }
        else
        {
            AplicarCambios();
            CargarEscena();
        }
    }

    private void AplicarCambiosDeDia()
    {
        if (diaSiguiente)
        {
            GameManager.Instance.AplicarCambiosPendientes();
            GameManager.Instance.diaActual++;
            diaSiguiente = false;
        }

        if (activaNoche)         GameManager.Instance.esNoche = true;
        else if (desactivaNoche) GameManager.Instance.esNoche = false;
    }

    private void AplicarCambios()
    {
        if (diaSiguiente)
        {
            GameManager.Instance.AplicarCambiosPendientes();
            GameManager.Instance.diaActual++;
        }

        if (activaNoche)         GameManager.Instance.esNoche = true;
        else if (desactivaNoche) GameManager.Instance.esNoche = false;
    }

    private void CargarEscena()
    {
        GameManager.Instance.escenaActual    = destinoEscena;
        GameManager.Instance.llegoPorDerecha = esTriggerDerecho;
        SceneManager.LoadScene("GameScene");
    }

    // ── Bloqueo por requisito ─────────────────────────────────────────────────

    private void MostrarBloqueo(string texto)
    {
        var datos = ScriptableObject.CreateInstance<DialogueData>();
        datos.lineas = new DialogueLine[]
        {
            new DialogueLine { nombrePersonaje = "Mateo", texto = texto }
        };

        DialogueManager.Instance.IniciarDialogo(datos, null, () =>
        {
            StartCoroutine(LiberarBloqueoConDelay());
        });
    }

    private IEnumerator LiberarBloqueoConDelay()
    {
        yield return new WaitForSeconds(0.2f);
        bloqueandoPorDialogo = false;
    }

    // ── Indicador ─────────────────────────────────────────────────────────────

    private void CrearIndicador()
    {
        canvasObj = new GameObject("IndicadorCanvas");
        canvasObj.transform.SetParent(transform);
        canvasObj.transform.localPosition = offsetTexto;
        canvasObj.transform.localRotation = Quaternion.identity;
        canvasObj.transform.localScale    = Vector3.one * escalaCanvas;

        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.WorldSpace;
        canvas.sortingOrder = 25;

        RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(anchoCanvas, altoCanvas);

        GameObject panelObj = new GameObject("Panel");
        panelObj.transform.SetParent(canvasObj.transform, false);
        Image img = panelObj.AddComponent<Image>();
        img.color = colorFondo;
        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        GameObject textoObj = new GameObject("Texto");
        textoObj.transform.SetParent(canvasObj.transform, false);
        TextMeshProUGUI tmp = textoObj.AddComponent<TextMeshProUGUI>();
        tmp.text      = mensajeIndicador;
        tmp.fontSize  = tamanoFuente;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = colorTexto;
        RectTransform textoRect = textoObj.GetComponent<RectTransform>();
        textoRect.anchorMin = Vector2.zero;
        textoRect.anchorMax = Vector2.one;
        textoRect.offsetMin = new Vector2(8f, 4f);
        textoRect.offsetMax = new Vector2(-8f, -4f);

        canvasObj.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        jugadorDentro = true;
        if (canvasObj != null) canvasObj.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        jugadorDentro        = false;
        bloqueandoPorDialogo = false;
        if (canvasObj != null) canvasObj.SetActive(false);
    }
}