using UnityEngine;

// Una linea de dialogo individual
[System.Serializable]
public class DialogueLine
{
    [Tooltip("Nombre del personaje que habla. Ej: 'Mateo', 'Don Eladio', 'Narrador'")]
    public string nombrePersonaje;

    [Tooltip("Texto completo de la linea")]
    [TextArea(2, 5)]
    public string texto;

    [Tooltip("Transform del personaje sobre quien aparece la burbuja. " +
             "Si es null, la burbuja aparece sobre Mateo por defecto.")]
    public Transform anclaPersonaje;
}

// ScriptableObject que contiene una secuencia de lineas
[CreateAssetMenu(fileName = "DialogueData", menuName = "Juego/Dialogo")]
public class DialogueData : ScriptableObject
{
    [Tooltip("Secuencia de lineas de este dialogo")]
    public DialogueLine[] lineas;
}