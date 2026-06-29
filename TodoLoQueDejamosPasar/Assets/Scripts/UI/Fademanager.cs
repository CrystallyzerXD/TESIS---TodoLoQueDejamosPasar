using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance { get; private set; }

    [Header("Duracion por defecto en segundos")]
    public float duracionDefault = 1f;

    public bool EnTransicion => enTransicion;

    private Image panelFade;
    private bool  enTransicion = false;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        CrearPanel();

        // Si hay intro o reflexion pendiente, arrancar en negro para que
        // no se vea el escenario antes de que la escena/intro aparezca
        bool arrancarEnNegro = GameManager.Instance != null &&
                               (GameManager.Instance.vieneDeReflexion     ||
                                GameManager.Instance.introPendiente        ||
                                GameManager.Instance.escenaRolPendiente != null);

        SetAlpha(arrancarEnNegro ? 1f : 0f);
    }

    // ── API publica ───────────────────────────────────────────────────────────

    public void FadeOut(float duracion, Action onCompleto = null)
    {
        if (enTransicion) return;
        StartCoroutine(EfectoFade(0f, 1f, duracion, onCompleto));
    }

    public void FadeIn(float duracion, Action onCompleto = null)
    {
        if (enTransicion) return;
        StartCoroutine(EfectoFade(1f, 0f, duracion, onCompleto));
    }

    public void FadeOutIn(float duracion, float pausa, Action onNegro = null, Action onCompleto = null)
    {
        if (enTransicion) return;
        StartCoroutine(SecuenciaFadeOutIn(duracion, pausa, onNegro, onCompleto));
    }

    public void PantallaEnNegro()      => SetAlpha(1f);
    public void PantallaTransparente() => SetAlpha(0f);

    // ── Coroutines ────────────────────────────────────────────────────────────

    private IEnumerator EfectoFade(float desde, float hasta, float duracion, Action onCompleto)
    {
        enTransicion = true;
        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            SetAlpha(Mathf.Lerp(desde, hasta, Mathf.Clamp01(tiempo / duracion)));
            yield return null;
        }

        SetAlpha(hasta);
        enTransicion = false;
        onCompleto?.Invoke();
    }

    private IEnumerator SecuenciaFadeOutIn(float duracion, float pausa, Action onNegro, Action onCompleto)
    {
        enTransicion = true;

        yield return StartCoroutine(EfectoFadeLibre(0f, 1f, duracion));
        onNegro?.Invoke();

        if (pausa > 0f)
            yield return new WaitForSeconds(pausa);

        yield return StartCoroutine(EfectoFadeLibre(1f, 0f, duracion));

        enTransicion = false;
        onCompleto?.Invoke();
    }

    private IEnumerator EfectoFadeLibre(float desde, float hasta, float duracion)
    {
        float tiempo = 0f;
        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            SetAlpha(Mathf.Lerp(desde, hasta, Mathf.Clamp01(tiempo / duracion)));
            yield return null;
        }
        SetAlpha(hasta);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void SetAlpha(float alpha)
    {
        if (panelFade == null) return;
        Color c = panelFade.color;
        c.a = alpha;
        panelFade.color = c;
    }

    private void CrearPanel()
    {
        GameObject canvasObj = new GameObject("FadeCanvas");
        canvasObj.transform.SetParent(transform);

        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject panelObj = new GameObject("PanelFade");
        panelObj.transform.SetParent(canvasObj.transform, false);

        panelFade       = panelObj.AddComponent<Image>();
        panelFade.color = new Color(0f, 0f, 0f, 0f);

        RectTransform rt = panelObj.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}