using UnityEngine;

[System.Serializable]
public class LineaRol
{
    [Tooltip("Imagen que se muestra en esta linea")]
    public Sprite imagen;

    [Tooltip("Texto que aparece abajo. Ej: 'Padre: ¿Qué hacemos con la basura?'")]
    [TextArea(2, 4)]
    public string texto;

    [Tooltip("Decision opcional al llegar a esta linea (reemplaza el avance normal con E)")]
    public DecisionData decision;
}

[CreateAssetMenu(fileName = "EscenaRolData", menuName = "Juego/Escena de Rol")]
public class EscenaRolData : ScriptableObject
{
    [Tooltip("Lineas de la escena en orden")]
    public LineaRol[] lineas;
}