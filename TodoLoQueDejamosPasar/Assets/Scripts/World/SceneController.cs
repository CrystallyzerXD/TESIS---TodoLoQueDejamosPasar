using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

[System.Serializable]
public class TriggerConfig
{
    [Tooltip("En que escena este trigger esta activo, ej: '3. Park'")]
    public string     escenaQueLoUsa;
    [Tooltip("El GameObject que tiene el componente SceneLoader")]
    public GameObject triggerObj;
}

public class SceneController : MonoBehaviour
{
    [Header("Arrastrar los 5 ScriptableObjects de escena")]
    public SceneVisualConfig[] configuraciones;

    [Header("Referencias en GameScene")]
    public SpriteRenderer fondoRenderer;
    public Transform      playerSpawnPoint;
    public GameObject     player;

    [Header("Todos los triggers de navegacion de la escena")]
    public TriggerConfig[] triggers;

    [Header("Lluvia")]
    [Tooltip("Configuracion de cuando llueve. Dejar vacio para no usar lluvia.")]
    public RainConfig rainConfig;

    private SceneVisualConfig configActual;

    private void Start()
    {
        configActual = EncontrarConfig(GameManager.Instance.escenaActual);

        if (configActual == null)
        {
            Debug.LogWarning($"SceneController: sin config para '{GameManager.Instance.escenaActual}'");
            return;
        }

        AplicarFondo();
        AplicarTinteCombinado();
        AplicarSpawn();
        AplicarTriggers();
        AplicarMusica();
        AplicarLluvia();

        // ── Transiciones post-carga ───────────────────────────────────────────

        bool hayEscenaPendiente = GameManager.Instance.escenaRolPendiente != null;
        bool esIntro            = GameManager.Instance.introPendiente;
        bool esFinal            = GameManager.Instance.finalPendiente;
        bool esFase2Final       = GameManager.Instance.fase2Final;
        bool esGaleria          = GameManager.Instance.galeriaActiva;

        if (esGaleria && hayEscenaPendiente)
        {
            GameManager.Instance.galeriaActiva      = false;
            GameManager.Instance.reabrirGaleria     = true;
            GameManager.Instance.vieneDeReflexion   = false;
            EscenaRolData pendiente = GameManager.Instance.escenaRolPendiente;
            GameManager.Instance.escenaRolPendiente = null;
            StartCoroutine(MostrarEscenaRolPendiente(pendiente, () =>
            {
                // Resetear estado del juego antes de volver al menu
                GameManager.Instance.esInicioJuego    = true;
                GameManager.Instance.diaActual        = 1;
                GameManager.Instance.escenaActual     = Escenas.Home;
                GameManager.Instance.esNoche          = false;
                GameManager.Instance.llegoPorDerecha  = false;
                GameManager.Instance.vieneDeReflexion = false;
                GameManager.Instance.reabrirGaleria   = true;
                SceneManager.LoadScene("MainMenu");
            }));
        }
        else if (esFinal)
        {
            GameManager.Instance.finalPendiente = false;
            StartCoroutine(FadeInYLuego(() =>
            {
                if (FinalManager.Instance != null)
                    FinalManager.Instance.IniciarFinales();
            }));
        }
        else if (esFase2Final)
        {
            GameManager.Instance.fase2Final = false;
            Debug.Log($"[SceneController] Fase 2 — FinalManager.Instance: {FinalManager.Instance}");
            EscenaRolData escenaContaminacion = FinalManager.Instance != null
                ? FinalManager.Instance.ObtenerContaminacionPublico()
                : null;
            StartCoroutine(MostrarEscenaRolPendiente(escenaContaminacion, () =>
            {
                Debug.Log($"[SceneController] Escena contaminacion terminada — llamando MostrarFin");
                if (FinalManager.Instance != null)
                    FinalManager.Instance.MostrarFin();
            }));
        }
        else if (GameManager.Instance.vieneDeReflexion)
        {
            GameManager.Instance.vieneDeReflexion = false;

            if (hayEscenaPendiente && esIntro)
            {
                // INTRO: mostrar escena sobre negro, luego FadeIn al escenario
                GameManager.Instance.introPendiente = false;
                EscenaRolData pendiente = GameManager.Instance.escenaRolPendiente;
                GameManager.Instance.escenaRolPendiente = null;
                StartCoroutine(MostrarIntro(pendiente));
            }
            else if (hayEscenaPendiente)
            {
                // Escena post-reflexion normal: FadeIn primero, luego escena
                EscenaRolData pendiente = GameManager.Instance.escenaRolPendiente;
                GameManager.Instance.escenaRolPendiente = null;
                StartCoroutine(MostrarEscenaRolPendiente(pendiente));
            }
            else
            {
                StartCoroutine(FadeInSiguienteFrame());
            }
        }
        else if (hayEscenaPendiente)
        {
            EscenaRolData pendiente = GameManager.Instance.escenaRolPendiente;
            GameManager.Instance.escenaRolPendiente = null;
            StartCoroutine(MostrarEscenaRolPendiente(pendiente));
        }
    }

    // ── Coroutines de transicion ──────────────────────────────────────────────

    private IEnumerator FadeInSiguienteFrame()
    {
        yield return null;
        yield return null;
        if (FadeManager.Instance != null)
            FadeManager.Instance.FadeIn(1f);
    }

    private IEnumerator FadeInYLuego(System.Action onCompleto)
    {
        yield return null;
        yield return null;
        if (FadeManager.Instance != null)
        {
            bool done = false;
            FadeManager.Instance.FadeIn(1f, () => done = true);
            yield return new WaitUntil(() => done);
        }
        onCompleto?.Invoke();
    }

    /// <summary>
    /// Para la intro: muestra la escena sobre negro, y al terminar hace FadeIn
    /// para revelar el escenario. El jugador nunca ve el escenario antes de la intro.
    /// </summary>
    private IEnumerator MostrarIntro(EscenaRolData datos)
    {
        // Esperar a que EscenaRolManager esté listo
        yield return null;
        yield return null;

        // Asegurarse que la pantalla está en negro
        if (FadeManager.Instance != null)
            FadeManager.Instance.PantallaEnNegro();

        yield return null;

        // Mostrar controles primero, luego la intro
        if (ControlsScreen.Instance != null)
        {
            ControlsScreen.Instance.Mostrar(() =>
            {
                if (EscenaRolManager.Instance != null)
                {
                    EscenaRolManager.Instance.MostrarEscena(datos, () =>
                    {
                        if (FadeManager.Instance != null)
                            FadeManager.Instance.FadeIn(1f);
                        PlayerMovement.Bloqueado = false;
                    });
                }
                else
                {
                    if (FadeManager.Instance != null)
                        FadeManager.Instance.FadeIn(1f);
                    PlayerMovement.Bloqueado = false;
                }
            });
        }
        else if (EscenaRolManager.Instance != null)
        {
            EscenaRolManager.Instance.MostrarEscena(datos, () =>
            {
                if (FadeManager.Instance != null)
                    FadeManager.Instance.FadeIn(1f);
                PlayerMovement.Bloqueado = false;
            });
        }
        else
        {
            if (FadeManager.Instance != null)
                FadeManager.Instance.FadeIn(1f);
            PlayerMovement.Bloqueado = false;
        }
    }

    /// <summary>
    /// Espera a que el FadeManager termine de quitar el negro (si estaba activo),
    /// luego muestra la escena de rol pendiente y al terminar libera el movimiento.
    /// </summary>
    private IEnumerator MostrarEscenaRolPendiente(EscenaRolData datos,
                                                   System.Action onCompleto = null)
    {
        yield return null;
        yield return null;

        if (FadeManager.Instance != null)
        {
            bool fadeTerminado = false;
            FadeManager.Instance.FadeIn(1f, () => fadeTerminado = true);
            yield return new WaitUntil(() => fadeTerminado);
        }

        if (EscenaRolManager.Instance != null)
        {
            EscenaRolManager.Instance.MostrarEscena(datos, () =>
            {
                PlayerMovement.Bloqueado = false;
                onCompleto?.Invoke();
            });
        }
        else
        {
            PlayerMovement.Bloqueado = false;
            onCompleto?.Invoke();
        }
    }

    // ── Busqueda ──────────────────────────────────────────────────────────────

    private SceneVisualConfig EncontrarConfig(string nombre)
    {
        foreach (var c in configuraciones)
            if (c != null && c.nombreEscena == nombre) return c;
        return null;
    }

    // ── Aplicadores ───────────────────────────────────────────────────────────

    private void AplicarFondo()
    {
        if (fondoRenderer == null) return;

        Sprite fondo = configActual.ObtenerFondo(
            GameManager.Instance.contaminacion,
            GameManager.Instance.esNoche,
            GameManager.Instance.estadoArboles
        );

        if (fondo != null)
            fondoRenderer.sprite = fondo;
        else
            Debug.LogWarning($"SceneController: fondo nulo — " +
                             $"escena={configActual.nombreEscena} " +
                             $"cont={GameManager.Instance.contaminacion} " +
                             $"noche={GameManager.Instance.esNoche} " +
                             $"arboles={GameManager.Instance.estadoArboles}");
    }

    private void AplicarTinteCombinado()
    {
        if (fondoRenderer == null) return;

        fondoRenderer.color = configActual.ObtenerTinteCombinado(
            GameManager.Instance.contaminacion,
            GameManager.Instance.bienestarUrbano
        );
    }

    private void AplicarSpawn()
    {
        if (player == null || playerSpawnPoint == null) return;

        float spawnX;

        if (GameManager.Instance.esInicioJuego)
        {
            spawnX = GameManager.Instance.spawnInicialX;
            GameManager.Instance.esInicioJuego = false;
        }
        else
        {
            spawnX = GameManager.Instance.llegoPorDerecha
                ? configActual.playerSpawnXIzquierda
                : configActual.playerSpawnXDerecha;
        }

        player.transform.position = new Vector3(
            spawnX,
            playerSpawnPoint.position.y,
            0f
        );
    }

    private void AplicarTriggers()
    {
        string escenaActual = GameManager.Instance.escenaActual;

        foreach (var t in triggers)
        {
            if (t.triggerObj == null) continue;

            // Si viene de galeria, desactivar todos los triggers
            if (GameManager.Instance.galeriaActiva)
            {
                t.triggerObj.SetActive(false);
                continue;
            }

            bool esDeEstaEscena = t.escenaQueLoUsa == escenaActual;
            t.triggerObj.SetActive(esDeEstaEscena);

            if (esDeEstaEscena)
            {
                SceneLoader loader = t.triggerObj.GetComponent<SceneLoader>();
                if (loader != null)
                    loader.ActualizarVisibilidadPorHora();
            }
        }
    }

    private void AplicarMusica()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.ReproducirSegunContexto();
    }

    private void AplicarLluvia()
    {
        if (RainManager.Instance == null) return;

        if (rainConfig != null && rainConfig.DeberiaLlover(
            GameManager.Instance.diaActual,
            GameManager.Instance.escenaActual,
            GameManager.Instance.esNoche))
        {
            RainManager.Instance.ActivarLluvia();
        }
        else
        {
            RainManager.Instance.DesactivarLluvia();
        }
    }

    private void EvaluarNPCs()
    {
        NPCSchedule[] npcs = FindObjectsByType<NPCSchedule>(FindObjectsInactive.Include);
        foreach (var npc in npcs)
            npc.Evaluar();
    }

    // ── API publica ───────────────────────────────────────────────────────────

    public void RefrescarVisual()
    {
        if (configActual == null) return;
        AplicarFondo();
        AplicarTinteCombinado();
    }

    public void RefrescarTodo()
    {
        if (configActual == null) return;
        AplicarFondo();
        AplicarTinteCombinado();
        AplicarTriggers();
        AplicarMusica();
        AplicarLluvia();
        EvaluarNPCs();
    }
}