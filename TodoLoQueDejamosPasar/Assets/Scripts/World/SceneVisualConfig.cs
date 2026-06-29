using UnityEngine;

public enum TipoEscena
{
    Fija,                // 5. Work: una imagen, nunca cambia
    NocheSola,           // 1. Home: dia/noche, sin contaminacion
    Variable,            // 4. StreetEast: fondos por contaminacion, sin noche
    VariableConNoche,    // 2. StreetWest: fondos por contaminacion + noche
    VariableConArboles   // 3. Park: fondos por contaminacion x2 (con/sin arboles)
}

[CreateAssetMenu(fileName = "SceneVisualConfig",
                 menuName = "Juego/Configuracion Visual de Escena")]
public class SceneVisualConfig : ScriptableObject
{
    [Header("Identificador — debe coincidir con GameManager.escenaActual")]
    public string     nombreEscena;
    public TipoEscena tipoEscena;

    // ── 5. Work ───────────────────────────────────────────────────────────────
    [Header("Solo Fija (5. Work)")]
    public Sprite fondoFijo;

    // ── 1. Home ───────────────────────────────────────────────────────────────
    [Header("Solo NocheSola (1. Home)")]
    public Sprite fondoDia;
    public Sprite fondoNoche;

    // ── Fondos por contaminacion 1-10 (indice 0 = nivel 1) ───────────────────
    [Header("Fondos dia — contaminacion 1-10")]
    public Sprite[] fondosDia = new Sprite[10];

    [Header("Fondos noche — solo VariableConNoche (2. StreetWest)")]
    public Sprite[] fondosNoche = new Sprite[10];

    [Header("Fondos sin arboles — solo VariableConArboles (3. Park)")]
    public Sprite[] fondosSinArboles = new Sprite[10];

    // ── Tinte de contaminacion ────────────────────────────────────────────────
    // contaminacion 0  → sin tinte (limpio)
    // contaminacion 10 → tinte gris/marron sucio
    [Header("Tinte de contaminacion (contaminacion alta = mas sucio)")]
    public Color colorLimpio    = Color.white;
    public Color colorContaminado = new Color(0.6f, 0.55f, 0.45f, 1f); // marron grisaceo

    // ── Tinte de calidez ──────────────────────────────────────────────────────
    // bienestar 0  → tinte calido/amarillo (isla de calor)
    // bienestar 10 → sin tinte (fresco)
    [Header("Tinte de calidez (bienestar bajo = mas calido)")]
    public Color colorFresco      = Color.white;
    public Color colorCalorMaximo = new Color(1f, 0.88f, 0.72f, 1f); // amarillo calido

    // ── Spawn ─────────────────────────────────────────────────────────────────
    [Header("Spawn del jugador")]
    [Tooltip("Jugador llego por trigger derecho → aparece aqui (lado izquierdo)")]
    public float playerSpawnXIzquierda = -8f;

    [Tooltip("Jugador llego por trigger izquierdo → aparece aqui (lado derecho)")]
    public float playerSpawnXDerecha   =  8f;

    // ── API ───────────────────────────────────────────────────────────────────

    public Sprite ObtenerFondo(int contaminacion, bool esNoche, int estadoArboles)
    {
        switch (tipoEscena)
        {
            case TipoEscena.Fija:
                return fondoFijo;

            case TipoEscena.NocheSola:
                return esNoche ? fondoNoche : fondoDia;

            case TipoEscena.Variable:
            {
                int i = Mathf.Clamp(contaminacion - 1, 0, 9);
                return fondosDia[i];
            }

            case TipoEscena.VariableConNoche:
            {
                int i = Mathf.Clamp(contaminacion - 1, 0, 9);
                if (esNoche && fondosNoche.Length > i && fondosNoche[i] != null)
                    return fondosNoche[i];
                return fondosDia[i];
            }

            case TipoEscena.VariableConArboles:
            {
                int i = Mathf.Clamp(contaminacion - 1, 0, 9);
                bool talados = estadoArboles >= 2;
                if (talados && fondosSinArboles.Length > i && fondosSinArboles[i] != null)
                    return fondosSinArboles[i];
                return fondosDia[i];
            }

            default:
                Debug.LogWarning($"SceneVisualConfig: TipoEscena no reconocido en {nombreEscena}");
                return null;
        }
    }

    /// <summary>
    /// Combina el tinte de contaminacion y el tinte de calidez en un solo color.
    /// Ambos efectos se multiplican — si los dos son blancos (neutros), el resultado es blanco.
    /// </summary>
    public Color ObtenerTinteCombinado(int contaminacion, int bienestarUrbano)
    {
        // Home y Work no tienen efectos de tinte
        if (tipoEscena == TipoEscena.Fija || tipoEscena == TipoEscena.NocheSola)
            return Color.white;

        // Tinte contaminacion: 0 = limpio, 10 = sucio
        float tCont = Mathf.Clamp01(contaminacion / 10f);
        Color tinteCont = Color.Lerp(colorLimpio, colorContaminado, tCont);

        // Tinte calidez: 0 = calor maximo, 10 = fresco
        float tCalor = 1f - Mathf.Clamp01(bienestarUrbano / 10f);
        Color tinteCalor = Color.Lerp(colorFresco, colorCalorMaximo, tCalor);

        // Multiplicamos ambos efectos
        return tinteCont * tinteCalor;
    }
}