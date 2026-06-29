using UnityEngine;
using System;
using System.Collections;

public class DecisionUI : MonoBehaviour
{
    public static DecisionUI Instance { get; private set; }

    [Header("Transicion al elegir")]
    public float duracionFade = 0.5f;

    private DecisionData decisionActual;
    private Transform    anclaActual;
    private Action<int>  callbackOpcion;
    private bool         activo = false;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Update()
    {
        if (!activo) return;

        for (int i = 0; i < decisionActual.opciones.Length; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                activo = false;
                StartCoroutine(ElegirConFade(i));
                return;
            }
        }
    }

    // ── API publica ───────────────────────────────────────────────────────────

    public void MostrarDecision(DecisionData datos, Transform ancla, Action<int> onOpcionElegida)
    {
        if (datos == null || datos.opciones.Length == 0)
        {
            Debug.LogWarning("DecisionUI: datos de decision vacios.");
            onOpcionElegida?.Invoke(0);
            return;
        }

        decisionActual = datos;
        anclaActual    = ancla;
        callbackOpcion = onOpcionElegida;
        activo         = true;

        DialogueManager.Instance.InputBloqueado = true;
        PlayerMovement.Bloqueado                = true;
        MostrarOpciones();
    }

    public void CerrarDecision()
    {
        activo                                  = false;
        DialogueManager.Instance.InputBloqueado = false;
        DialogueManager.Instance.CerrarDialogo();
        PlayerMovement.Bloqueado                = false;
    }

    // ── Logica interna ────────────────────────────────────────────────────────

    private void MostrarOpciones()
    {
        string texto = decisionActual.textoPregunta + "\n\n";

        for (int i = 0; i < decisionActual.opciones.Length; i++)
            texto += decisionActual.opciones[i].textoOpcion + "\n";

        DialogueManager.Instance.MostrarLineaDirecta(
            decisionActual.nombrePersonaje,
            texto.TrimEnd(),
            anclaActual
        );
    }

    private IEnumerator ElegirConFade(int indice)
    {
        OpcionDecision opcion = decisionActual.opciones[indice];

        // Cerrar el dialogo visual pero mantener PlayerMovement.Bloqueado = true
        // durante todo el fade para que el jugador no pueda moverse
        DialogueManager.Instance.InputBloqueado = false;
        DialogueManager.Instance.CerrarDialogoSinLiberarMovimiento();

        bool hayConsecuencias = opcion.deltaCont      != 0 ||
                                opcion.deltaBienestar != 0 ||
                                opcion.deltaRiesgo    != 0;

        if (hayConsecuencias && FadeManager.Instance != null && duracionFade > 0f)
        {
            bool fadeOutCompleto = false;
            FadeManager.Instance.FadeOut(duracionFade, () => fadeOutCompleto = true);
            yield return new WaitUntil(() => fadeOutCompleto);

            if (opcion.deltaCont      != 0) GameManager.Instance.ModificarContaminacion(opcion.deltaCont);
            if (opcion.deltaBienestar != 0) GameManager.Instance.ModificarBienestar(opcion.deltaBienestar);
            if (opcion.deltaRiesgo    != 0) GameManager.Instance.ModificarRiesgoInundacion(opcion.deltaRiesgo);

            if (opcion.deltaArboles != 0)
                GameManager.Instance.ProgramarCambioArboles(opcion.deltaArboles);

            SceneController sc = FindAnyObjectByType<SceneController>();
            if (sc != null) sc.RefrescarVisual();

            yield return new WaitForSeconds(0.5f);

            bool fadeInCompleto = false;
            FadeManager.Instance.FadeIn(duracionFade, () => fadeInCompleto = true);
            yield return new WaitUntil(() => fadeInCompleto);
        }
        else
        {
            if (opcion.deltaCont      != 0) GameManager.Instance.ModificarContaminacion(opcion.deltaCont);
            if (opcion.deltaBienestar != 0) GameManager.Instance.ModificarBienestar(opcion.deltaBienestar);
            if (opcion.deltaRiesgo    != 0) GameManager.Instance.ModificarRiesgoInundacion(opcion.deltaRiesgo);

            if (opcion.deltaArboles != 0)
                GameManager.Instance.ProgramarCambioArboles(opcion.deltaArboles);
        }

        // Solo liberar el movimiento si NO hay dialogo de reaccion pendiente
        // Si hay reaccion, NPCInteraction se encarga de mantener el bloqueo
        // y DialogueManager lo libera al terminar
        PlayerMovement.Bloqueado = false;
        callbackOpcion?.Invoke(indice);
    }
}