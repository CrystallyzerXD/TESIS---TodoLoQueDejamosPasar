using UnityEngine;
using System.Collections;

public class RainManager : MonoBehaviour
{
    public static RainManager Instance { get; private set; }

    [Header("Configuracion de lluvia")]
    public float gotasPorSegundo = 800f;
    public float velocidadGotas  = 15f;

    [Tooltip("Angulo de inclinacion en grados. Positivo = izquierda")]
    public float anguloLluvia    = 10f;

    public float largoGota       = 0.4f;
    public float anchoGota       = 0.05f;
    public Color colorGota       = new Color(0.7f, 0.85f, 1f, 0.8f);
    public float duracionFade    = 2f;
    public int   ordenCapa       = 20;

    [Header("Sonido")]
    public AudioClip sonidoLluvia;
    [Range(0f, 1f)]
    public float volumenMax = 0.4f;

    private bool           lluviaActiva = false;
    private float          intensidad   = 0f;
    private Coroutine      coroutineFade;
    private ParticleSystem ps;
    private AudioSource    audioSource;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        CrearSistemaParticulas();

        audioSource             = gameObject.AddComponent<AudioSource>();
        audioSource.loop        = true;
        audioSource.playOnAwake = false;
        audioSource.volume      = 0f;
        if (sonidoLluvia != null)
            audioSource.clip = sonidoLluvia;
    }

    private void Update()
    {
        if (!lluviaActiva && intensidad <= 0f) return;

        var emission = ps.emission;
        emission.rateOverTime = gotasPorSegundo * intensidad;

        Camera cam = Camera.main;
        if (cam != null)
        {
            float camAltura = cam.orthographicSize;
            float camAncho  = camAltura * cam.aspect;

            Vector3 pos = cam.transform.position;
            pos.z       = 0f;
            pos.y      += camAltura + 0.5f;
            transform.position = pos;

            var shape   = ps.shape;
            shape.scale = new Vector3(camAncho * 2.5f, 0.1f, 1f);

            var main = ps.main;
            main.startLifetime = (camAltura * 2f + 2f) / velocidadGotas;
        }

        if (audioSource.clip != null)
        {
            audioSource.volume = intensidad * volumenMax;
            if (intensidad > 0f && !audioSource.isPlaying)
                audioSource.Play();
            else if (intensidad <= 0f && audioSource.isPlaying)
                audioSource.Stop();
        }
    }

    // ── API publica ───────────────────────────────────────────────────────────

    public void ActivarLluvia()
    {
        if (lluviaActiva) return;
        lluviaActiva = true;
        ps.Play();
        if (coroutineFade != null) StopCoroutine(coroutineFade);
        coroutineFade = StartCoroutine(FadeIntensidad(1f));
    }

    public void DesactivarLluvia()
    {
        if (!lluviaActiva) return;
        lluviaActiva = false;
        if (coroutineFade != null) StopCoroutine(coroutineFade);
        coroutineFade = StartCoroutine(FadeIntensidadYDetener(0f));
    }

    public bool EstaActiva() => lluviaActiva;

    // ── Construccion ──────────────────────────────────────────────────────────

    private void CrearSistemaParticulas()
    {
        GameObject psObj = new GameObject("LluviaParticulas");
        psObj.transform.SetParent(transform);
        psObj.transform.localPosition = Vector3.zero;

        ps = psObj.AddComponent<ParticleSystem>();

        var renderer           = psObj.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode    = ParticleSystemRenderMode.Stretch;
        renderer.lengthScale   = largoGota;
        renderer.velocityScale = 0f;
        renderer.sortingOrder  = ordenCapa;
        renderer.material      = new Material(Shader.Find("Sprites/Default"));

        var main             = ps.main;
        main.loop            = true;
        main.playOnAwake     = false;
        main.startLifetime   = 1f;
        main.startSpeed      = 0f;
        main.startSize       = anchoGota;
        main.startColor      = colorGota;
        main.gravityModifier = 0f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles    = 15000;

        var emission          = ps.emission;
        emission.rateOverTime = 0f;

        var shape        = ps.shape;
        shape.enabled    = true;
        shape.shapeType  = ParticleSystemShapeType.Box;
        shape.scale      = new Vector3(30f, 0.1f, 1f);

        var velocity     = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space   = ParticleSystemSimulationSpace.World;
        float vx = -Mathf.Sin(anguloLluvia * Mathf.Deg2Rad) * velocidadGotas;
        float vy = -velocidadGotas;
        velocity.x = new ParticleSystem.MinMaxCurve(vx);
        velocity.y = new ParticleSystem.MinMaxCurve(vy);
        velocity.z = new ParticleSystem.MinMaxCurve(0f);

        psObj.SetActive(true);
    }

    // ── Fade ──────────────────────────────────────────────────────────────────

    private IEnumerator FadeIntensidad(float objetivo)
    {
        float inicio = intensidad;
        float tiempo = 0f;
        while (tiempo < duracionFade)
        {
            tiempo    += Time.deltaTime;
            intensidad = Mathf.Lerp(inicio, objetivo, Mathf.Clamp01(tiempo / duracionFade));
            ActualizarColor();
            yield return null;
        }
        intensidad = objetivo;
        ActualizarColor();
    }

    private IEnumerator FadeIntensidadYDetener(float objetivo)
    {
        float inicio = intensidad;
        float tiempo = 0f;
        while (tiempo < duracionFade)
        {
            tiempo    += Time.deltaTime;
            intensidad = Mathf.Lerp(inicio, objetivo, Mathf.Clamp01(tiempo / duracionFade));
            ActualizarColor();
            yield return null;
        }
        intensidad = objetivo;
        ActualizarColor();
        ps.Stop();
    }

    private void ActualizarColor()
    {
        var main    = ps.main;
        Color c     = colorGota;
        c.a         = colorGota.a * intensidad;
        main.startColor = c;
    }
}