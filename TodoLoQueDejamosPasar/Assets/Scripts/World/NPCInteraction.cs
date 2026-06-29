using UnityEngine;

public class NPCInteraction : MonoBehaviour
{
    [Header("Narrativa del NPC")]
    public NPCStoryData storyData;

    [Header("Trigger automatico al terminar")]
    [Tooltip("Si esta asignado, se ejecuta automaticamente al terminar la interaccion principal.\n" +
             "Util para transiciones narrativas (dormir, salir, etc).")]
    public SceneLoader triggerAlTerminar;

    [Header("Indicador de texto")]
    public string  mensajeIndicador = "Presiona \"E\" para hablar";
    public Vector3 offsetIndicador  = new Vector3(0f, 2f, 0f);

    private bool                 jugadorCerca         = false;
    private bool                 interactuando        = false;
    private bool                 esperandoSoltarE     = false;
    private float                tiempoEsperaPost     = 0f;
    private InteraccionPrincipal interaccionPrincipal = null;
    private InteraccionExtra     interaccionExtra     = null;
    private GameObject           indicadorObj;

    private void Start()
    {
        CrearIndicador();

        if (storyData == null)
            Debug.LogWarning($"NPCInteraction en '{gameObject.name}': storyData no asignado.");
    }

    private void Update()
    {
        if (!jugadorCerca || interactuando) return;
        if (storyData == null) return;

        if (tiempoEsperaPost > 0f)
        {
            tiempoEsperaPost -= Time.deltaTime;
            return;
        }

        if (esperandoSoltarE)
        {
            if (!Input.GetKey(KeyCode.E))
                esperandoSoltarE = false;
            return;
        }

        if (Input.GetKeyDown(KeyCode.E))
            IniciarInteraccion();
    }

    // ── Flujo ─────────────────────────────────────────────────────────────────

    private void IniciarInteraccion()
    {
        if (storyData == null) return;

        if (storyData.esExtra)
            IniciarExtra();
        else
            IniciarPrincipal();
    }

    private void IniciarPrincipal()
    {
        interaccionPrincipal = ObtenerPrincipalDisponible();

        if (interaccionPrincipal == null)
        {
            MostrarTextoInterno(storyData.textoSinInteraccion);
            return;
        }

        interactuando = true;
        OcultarIndicador();

        if (interaccionPrincipal.escenaRol != null)
        {
            EscenaRolManager.Instance.MostrarEscena(
                interaccionPrincipal.escenaRol,
                TerminarPrincipal
            );
            return;
        }

        if (interaccionPrincipal.dialogoInicial != null)
        {
            DialogueManager.Instance.IniciarDialogo(
                interaccionPrincipal.dialogoInicial,
                transform,
                DespuesDelDialogoPrincipal
            );
        }
        else
        {
            DespuesDelDialogoPrincipal();
        }
    }

    private void DespuesDelDialogoPrincipal()
    {
        if (interaccionPrincipal.decision != null)
        {
            // Bloquear movimiento ANTES de mostrar la decision para que
            // no haya ventana libre entre decision y dialogo de reaccion
            PlayerMovement.Bloqueado = true;
            DecisionUI.Instance.MostrarDecision(
                interaccionPrincipal.decision,
                transform,
                OnOpcionElegida
            );
        }
        else
        {
            TerminarPrincipal();
        }
    }

    private void OnOpcionElegida(int indice)
    {
        // Mantener bloqueo explicitamente — no hay gap entre decision y reaccion
        PlayerMovement.Bloqueado = true;

        OpcionDecision opcion = interaccionPrincipal.decision.opciones[indice];

        if (opcion.dialogoReaccion != null)
        {
            DialogueManager.Instance.IniciarDialogo(
                opcion.dialogoReaccion,
                transform,
                TerminarPrincipal
            );
        }
        else
        {
            PlayerMovement.Bloqueado = false;
            TerminarPrincipal();
        }
    }

    private void TerminarPrincipal()
    {
        if (interaccionPrincipal != null)
            NarrativeManager.Instance.MarcarCompletada(interaccionPrincipal.id);

        PlayerMovement.Bloqueado = false;
        interactuando            = false;
        interaccionPrincipal     = null;
        esperandoSoltarE         = true;

        if (triggerAlTerminar != null)
        {
            triggerAlTerminar.EjecutarDesdeNPC();
            return;
        }

        if (jugadorCerca && ObtenerPrincipalDisponible() != null)
            MostrarIndicador();
        else
            OcultarIndicador();
    }

    private void IniciarExtra()
    {
        interaccionExtra = ObtenerExtraDisponible();

        if (interaccionExtra == null)
        {
            MostrarTextoInterno(storyData.textoSinInteraccion);
            return;
        }

        interactuando = true;
        OcultarIndicador();

        if (interaccionExtra.dialogoInicial != null)
        {
            DialogueManager.Instance.IniciarDialogo(
                interaccionExtra.dialogoInicial,
                transform,
                TerminarExtra
            );
        }
        else
        {
            TerminarExtra();
        }
    }

    private void TerminarExtra()
    {
        interactuando    = false;
        interaccionExtra = null;
        esperandoSoltarE = true;

        if (jugadorCerca && ObtenerExtraDisponible() != null)
            MostrarIndicador();
        else
            OcultarIndicador();
    }

    // ── Evaluacion ────────────────────────────────────────────────────────────

    private InteraccionPrincipal ObtenerPrincipalDisponible()
    {
        if (storyData?.interaccionesPrincipales == null) return null;

        foreach (var i in storyData.interaccionesPrincipales)
        {
            if (i == null) continue;
            if (!i.repetible && NarrativeManager.Instance.EstaCompletada(i.id)) continue;
            if (i.CondicionesCumplidas()) return i;
        }

        return null;
    }

    private InteraccionExtra ObtenerExtraDisponible()
    {
        if (storyData?.interaccionesExtra == null) return null;

        foreach (var i in storyData.interaccionesExtra)
        {
            if (i == null) continue;
            if (i.CondicionesCumplidas()) return i;
        }

        return null;
    }

    private bool HayInteraccionDisponible()
    {
        if (storyData == null) return false;

        return storyData.esExtra
            ? ObtenerExtraDisponible() != null
            : ObtenerPrincipalDisponible() != null;
    }

    private void MostrarTextoInterno(string texto)
    {
        interactuando = true;
        OcultarIndicador();

        var datos = ScriptableObject.CreateInstance<DialogueData>();
        datos.lineas = new DialogueLine[]
        {
            new DialogueLine { nombrePersonaje = "Mateo", texto = texto }
        };

        DialogueManager.Instance.IniciarDialogo(datos, null, () =>
        {
            interactuando    = false;
            tiempoEsperaPost = 0.2f;
            esperandoSoltarE = true;
        });
    }

    // ── Trigger ───────────────────────────────────────────────────────────────

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        jugadorCerca = true;
        if (HayInteraccionDisponible()) MostrarIndicador();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        jugadorCerca     = false;
        esperandoSoltarE = false;
        tiempoEsperaPost = 0f;
        OcultarIndicador();
    }

    // ── Indicador ─────────────────────────────────────────────────────────────

    private void CrearIndicador()
    {
        indicadorObj = new GameObject("IndicadorNPC");
        indicadorObj.transform.SetParent(transform);
        indicadorObj.transform.localPosition = offsetIndicador;

        var canvas = indicadorObj.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.WorldSpace;
        canvas.sortingOrder = 25;

        var rt = indicadorObj.GetComponent<UnityEngine.RectTransform>();
        rt.sizeDelta = new Vector2(300f, 60f);

        float signoX = Mathf.Sign(transform.localScale.x);
        indicadorObj.transform.localScale = new Vector3(0.01f * signoX, 0.01f, 0.01f);

        var textoObj = new GameObject("Texto");
        textoObj.transform.SetParent(indicadorObj.transform, false);

        var tmp = textoObj.AddComponent<TMPro.TextMeshProUGUI>();
        tmp.text      = mensajeIndicador;
        tmp.fontSize  = 24;
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.color     = Color.white;

        var trt = textoObj.GetComponent<UnityEngine.RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = new Vector2(8f, 4f);
        trt.offsetMax = new Vector2(-8f, -4f);

        indicadorObj.SetActive(false);
    }

    private void MostrarIndicador()
    {
        if (indicadorObj != null) indicadorObj.SetActive(true);
    }

    private void OcultarIndicador()
    {
        if (indicadorObj != null) indicadorObj.SetActive(false);
    }
}