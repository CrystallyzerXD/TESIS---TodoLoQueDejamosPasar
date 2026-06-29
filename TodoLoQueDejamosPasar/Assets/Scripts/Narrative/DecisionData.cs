using UnityEngine;

[System.Serializable]
public class OpcionDecision
{
    [Tooltip("Texto que aparece como opcion. Ej: '1. Si, te ayudo'")]
    public string textoOpcion;

    [Header("Consecuencias al elegir esta opcion")]
    public int deltaCont      = 0;
    public int deltaBienestar = 0;
    public int deltaRiesgo    = 0;

    [Tooltip("Cambia el estado de los arboles. 0 = sin cambio. 1 = conservados. 2 o 3 = talados.\n" +
             "Este cambio se aplica al DIA SIGUIENTE, no de inmediato.")]
    public int deltaArboles = 0;

    [Header("Dialogo que aparece despues de elegir (opcional)")]
    public DialogueData dialogoReaccion;
}

[CreateAssetMenu(fileName = "DecisionData", menuName = "Juego/Decision")]
public class DecisionData : ScriptableObject
{
    [TextArea(2, 4)]
    public string textoPregunta;

    public string nombrePersonaje;

    public OpcionDecision[] opciones;
}