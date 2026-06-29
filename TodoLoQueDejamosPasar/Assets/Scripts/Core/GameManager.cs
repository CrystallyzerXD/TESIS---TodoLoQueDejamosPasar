using UnityEngine;
using UnityEngine.SceneManagement;

public static class Escenas
{
    public const string Home       = "1. Home";
    public const string StreetWest = "2. StreetWest";
    public const string Park       = "3. Park";
    public const string StreetEast = "4. StreetEast";
    public const string Work       = "5. Work";
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Variables del sistema")]
    public int contaminacion    = 5;
    public int bienestarUrbano  = 5;
    public int riesgoInundacion = 5;
    public int estadoArboles    = 1;

    [Header("Cambios pendientes para el dia siguiente")]
    [Tooltip("Si es > 0, se aplica a estadoArboles al avanzar de dia. 0 = sin cambio pendiente.")]
    public int estadoArbolesPendiente = 0;

    [Header("Progreso narrativo")]
    public int    diaActual    = 1;
    public string escenaActual = Escenas.Home;
    public bool   esNoche      = false;

    [Header("Navegacion")]
    public bool llegoPorDerecha = false;

    [Header("Spawn inicial")]
    public float spawnInicialX = -8f;
    public bool  esInicioJuego = true;

    [Header("Transicion")]
    public bool vieneDeReflexion = false;

    [Header("Escena de rol pendiente post-carga")]
    [Tooltip("Si no es null, SceneController la disparara al iniciar la escena.")]
    public EscenaRolData escenaRolPendiente = null;

    [Tooltip("Si true, la escena pendiente es la intro — se muestra ANTES del FadeIn.")]
    public bool introPendiente = false;

    [Tooltip("Si true, SceneController disparara el FinalManager al cargar la escena.")]
    public bool finalPendiente = false;

    [Tooltip("Si true, es la fase 2 del final (escena contaminacion + FIN).")]
    public bool fase2Final = false;

    [Tooltip("Si true, la GameScene fue cargada desde la Galeria — volver al MainMenu al terminar.")]
    public bool galeriaActiva = false;

    [Tooltip("Si true, GaleriaManager se abre automaticamente al cargar MainMenu.")]
    public bool reabrirGaleria = false;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.LoadScene("MainMenu");
    }

    // ── Modificadores ─────────────────────────────────────────────────────────

    public void ModificarContaminacion(int delta)
    {
        contaminacion = Mathf.Clamp(contaminacion + delta, 0, 10);
    }

    public void ModificarBienestar(int delta)
    {
        bienestarUrbano = Mathf.Clamp(bienestarUrbano + delta, 0, 10);
    }

    public void ModificarRiesgoInundacion(int delta)
    {
        riesgoInundacion = Mathf.Clamp(riesgoInundacion + delta, 0, 10);
    }

    public void ModificarEstadoArboles(int valor)
    {
        estadoArboles = Mathf.Clamp(valor, 1, 3);
    }

    /// <summary>
    /// Registra un cambio de arboles para aplicar al dia siguiente.
    /// </summary>
    public void ProgramarCambioArboles(int nuevoEstado)
    {
        estadoArbolesPendiente = Mathf.Clamp(nuevoEstado, 1, 3);
    }

    /// <summary>
    /// Aplica los cambios pendientes del dia anterior.
    /// Llamado por SceneLoader cuando diaSiguiente = true.
    /// </summary>
    public void AplicarCambiosPendientes()
    {
        if (estadoArbolesPendiente > 0)
        {
            estadoArboles          = estadoArbolesPendiente;
            estadoArbolesPendiente = 0;
        }
    }
}