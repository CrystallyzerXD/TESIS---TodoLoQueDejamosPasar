using UnityEngine;
using System;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("Prefab de la burbuja de dialogo")]
    public DialogueBubble bubblePrefab;

    [Header("Referencia al jugador (ancla por defecto)")]
    public Transform transformJugador;

    [Header("Offset de la burbuja sobre el personaje")]
    public Vector3 offsetBurbuja = new Vector3(0f, 2.5f, 0f);

    private DialogueBubble bubbleActiva;
    private DialogueData   dialogoActual;
    private int            lineaActual;
    private Transform      anclaActual;
    private Action         callbackAlTerminar;
    private bool           dialogoActivo = false;

    public bool EstaActivo     => dialogoActivo;
    public bool InputBloqueado { get; set; } = false;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Update()
    {
        if (!dialogoActivo || InputBloqueado) return;
        if (Input.GetKeyDown(KeyCode.E))
            ManejarPresionE();
    }

    private void LateUpdate()
    {
        if ((dialogoActivo || InputBloqueado) && bubbleActiva != null)
            bubbleActiva.SeguirPersonaje(anclaActual, offsetBurbuja);
    }

    // ── API publica ───────────────────────────────────────────────────────────

    public void IniciarDialogo(DialogueData datos, Transform anclaPersonaje, Action onTerminado)
    {
        if (datos == null || datos.lineas.Length == 0)
        {
            Debug.LogWarning("DialogueManager: datos de dialogo vacios.");
            onTerminado?.Invoke();
            return;
        }

        dialogoActual      = datos;
        lineaActual        = 0;
        callbackAlTerminar = onTerminado;
        dialogoActivo      = true;
        InputBloqueado     = false;

        // Bloquear movimiento mientras hay dialogo activo
        PlayerMovement.Bloqueado = true;

        AsegurarBurbuja();
        MostrarLineaActual(anclaPersonaje);
    }

    public void MostrarLineaDirecta(string nombre, string texto, Transform ancla)
    {
        anclaActual    = ancla != null ? ancla : transformJugador;
        dialogoActivo  = false;
        InputBloqueado = true;

        AsegurarBurbuja();
        bubbleActiva.MostrarLinea(nombre, texto);
    }

    public void CerrarDialogo()
    {
        dialogoActivo            = false;
        InputBloqueado           = false;
        PlayerMovement.Bloqueado = false;
        bubbleActiva?.Ocultar();
    }

    /// <summary>
    /// Cierra el dialogo visual (oculta la burbuja) sin liberar PlayerMovement.Bloqueado.
    /// Usado por DecisionUI para mantener el bloqueo durante el fade de consecuencias.
    /// </summary>
    public void CerrarDialogoSinLiberarMovimiento()
    {
        dialogoActivo  = false;
        InputBloqueado = false;
        bubbleActiva?.Ocultar();
    }

    // ── Logica interna ────────────────────────────────────────────────────────

    private void AsegurarBurbuja()
    {
        if (bubbleActiva != null) return;

        if (bubblePrefab == null)
        {
            Debug.LogError("DialogueManager: bubblePrefab no asignado.");
            return;
        }
        bubbleActiva = Instantiate(bubblePrefab);
    }

    private void MostrarLineaActual(Transform anclaPersonaje)
    {
        DialogueLine linea = dialogoActual.lineas[lineaActual];

        anclaActual = linea.anclaPersonaje != null ? linea.anclaPersonaje
                    : anclaPersonaje       != null ? anclaPersonaje
                    : transformJugador;

        bubbleActiva.MostrarLinea(linea.nombrePersonaje, linea.texto);
    }

    private void ManejarPresionE()
    {
        DialogueLine lineaActualData = dialogoActual.lineas[lineaActual];

        bool listo = bubbleActiva.CompletarOAvanzar(lineaActualData.texto);
        if (!listo) return;

        lineaActual++;

        if (lineaActual < dialogoActual.lineas.Length)
            MostrarLineaActual(anclaActual);
        else
            TerminarDialogo();
    }

    private void TerminarDialogo()
    {
        dialogoActivo            = false;
        InputBloqueado           = false;
        PlayerMovement.Bloqueado = false;
        bubbleActiva?.Ocultar();
        callbackAlTerminar?.Invoke();
    }
}