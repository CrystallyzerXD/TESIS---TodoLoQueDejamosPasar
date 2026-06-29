using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Source")]
    public AudioSource musicaSource;

    [Header("Tracks")]
    [Tooltip("MainMenu — Sidewalk Canopy")]
    public AudioClip sidewalkCanopy;

    [Tooltip("In-game de dia — Corner Store Dawn")]
    public AudioClip cornerStoreDawn;

    [Tooltip("Noches excepto dia 5 — Sidewalk After Midnight")]
    public AudioClip sidewalkAfterMidnight;

    [Tooltip("Solo noche del dia 5 — Broken Arcade Glass")]
    public AudioClip brokenArcadeGlass;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ── API publica ───────────────────────────────────────────────────────────

    public void ReproducirMenuPrincipal()
    {
        Reproducir(sidewalkCanopy);
    }

    /// <summary>
    /// Decide automaticamente que musica poner segun dia y hora.
    /// Llamado por SceneController al cargar GameScene.
    /// </summary>
    public void ReproducirSegunContexto()
    {
        int  dia     = GameManager.Instance.diaActual;
        bool esNoche = GameManager.Instance.esNoche;

        AudioClip clip;

        if (!esNoche)
        {
            clip = cornerStoreDawn;
        }
        else if (dia == 5)
        {
            clip = brokenArcadeGlass;
        }
        else
        {
            clip = sidewalkAfterMidnight;
        }

        Reproducir(clip);
    }

    public void Detener()
    {
        musicaSource.Stop();
    }

    public void CambiarVolumen(float volumen)
    {
        musicaSource.volume = Mathf.Clamp01(volumen);
    }

    /// <summary>
    /// Llamado por PauseManager al mover el slider de musica.
    /// Guarda el volumen y lo aplica al AudioSource activo.
    /// </summary>
    public void SetVolumenMusica(float valor)
    {
        musicaSource.volume = Mathf.Clamp01(valor);
    }

    /// <summary>
    /// Placeholder para efectos de sonido cuando los agregues.
    /// Por ahora no hace nada, pero evita errores en PauseManager.
    /// </summary>
    public void SetVolumenEfectos(float valor)
    {
        // Cuando tengas un AudioSource separado para SFX:
        // efectosSource.volume = Mathf.Clamp01(valor);
    }

    // ── Interno ───────────────────────────────────────────────────────────────

    private void Reproducir(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("AudioManager: clip no asignado.");
            return;
        }

        if (musicaSource.clip == clip && musicaSource.isPlaying) return;

        musicaSource.clip   = clip;
        musicaSource.loop   = true;
        musicaSource.volume = PlayerPrefs.GetFloat("VolMusica", 0.7f);
        musicaSource.Play();
    }
}