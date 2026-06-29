using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

// Este componente vive en el prefab de la burbuja de dialogo.
// El prefab necesita:
//   - Canvas (modo World Space)
//       - Panel (imagen de fondo de la burbuja)
//           - TMP_Text (nombre del personaje)
//           - TMP_Text (texto del dialogo)

public class DialogueBubble : MonoBehaviour
{
    [Header("Referencias UI")]
    public TMP_Text textoNombre;
    public TMP_Text textoDialogo;
    public Image    panelFondo;     // arrastra el Panel aqui para cambiar el color de fondo

    [Header("Typewriter")]
    [Tooltip("Letras por segundo")]
    public float velocidadTypewriter = 40f;

    [Header("Colores por personaje")]
    [Tooltip("Define el color del nombre segun el personaje. " +
             "El nombre debe coincidir exactamente con el que usas en los dialogos.")]
    public ColorPersonaje[] coloresPersonajes;

    [Header("Color por defecto (personajes sin color asignado)")]
    public Color colorNombreDefault  = new Color(0.9f, 0.75f, 0.4f);  // dorado calido

    [Header("Fondo de la burbuja")]
    [Tooltip("Color base del fondo. Recomendado: oscuro con alpha 0.88")]
    public Color colorFondoDefault   = new Color(0.06f, 0.06f, 0.10f, 0.88f);

    private Coroutine coroutineTypewriter;
    private bool      typewriterTerminado = false;

    // ── API publica ───────────────────────────────────────────────────────────

    public void MostrarLinea(string nombre, string texto)
    {
        gameObject.SetActive(true);

        textoNombre.text  = nombre;
        textoDialogo.text = "";
        typewriterTerminado = false;

        AplicarColorPersonaje(nombre);

        if (coroutineTypewriter != null)
            StopCoroutine(coroutineTypewriter);

        coroutineTypewriter = StartCoroutine(EfectoTypewriter(texto));
    }

    public bool CompletarOAvanzar(string textoCompleto)
    {
        if (!typewriterTerminado)
        {
            if (coroutineTypewriter != null)
                StopCoroutine(coroutineTypewriter);

            textoDialogo.text   = textoCompleto;
            typewriterTerminado = true;
            return false;
        }
        return true;
    }

    public void Ocultar()
    {
        if (coroutineTypewriter != null)
            StopCoroutine(coroutineTypewriter);

        gameObject.SetActive(false);
    }

    public void SeguirPersonaje(Transform ancla, Vector3 offset)
    {
        if (ancla == null) return;
        transform.position = ancla.position + offset;
    }

    // ── Color por personaje ───────────────────────────────────────────────────

    private void AplicarColorPersonaje(string nombre)
    {
        if (panelFondo != null)
            panelFondo.color = colorFondoDefault;

        Color colorNombre = colorNombreDefault;

        if (coloresPersonajes != null)
        {
            foreach (var cp in coloresPersonajes)
            {
                if (string.Equals(cp.nombre, nombre,
                    System.StringComparison.OrdinalIgnoreCase))
                {
                    colorNombre = cp.colorNombre;

                    if (panelFondo != null && cp.usarColorFondo)
                        panelFondo.color = cp.colorFondo;

                    break;
                }
            }
        }

        if (textoNombre != null)
            textoNombre.color = colorNombre;
    }

    // ── Typewriter ────────────────────────────────────────────────────────────

    private IEnumerator EfectoTypewriter(string textoCompleto)
    {
        textoDialogo.text   = "";
        typewriterTerminado = false;

        foreach (char letra in textoCompleto)
        {
            textoDialogo.text += letra;
            yield return new WaitForSeconds(1f / velocidadTypewriter);
        }

        typewriterTerminado = true;
    }
}

// ── Datos de color por personaje ──────────────────────────────────────────────

[System.Serializable]
public class ColorPersonaje
{
    [Tooltip("Nombre exacto del personaje tal como aparece en los dialogos")]
    public string nombre;

    [Tooltip("Color del texto del nombre")]
    public Color colorNombre = new Color(0.9f, 0.75f, 0.4f);

    [Tooltip("Si true, usa un color de fondo especifico para este personaje")]
    public bool  usarColorFondo = false;

    [Tooltip("Color del fondo de la burbuja para este personaje")]
    public Color colorFondo     = new Color(0.06f, 0.06f, 0.10f, 0.88f);
}