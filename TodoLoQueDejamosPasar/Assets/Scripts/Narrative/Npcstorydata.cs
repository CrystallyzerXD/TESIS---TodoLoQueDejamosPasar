using UnityEngine;

[CreateAssetMenu(fileName = "NPCStoryData", menuName = "Juego/NPC Story Data")]
public class NPCStoryData : ScriptableObject
{
    [Header("Identificacion")]
    public string nombreNPC;

    [Tooltip("FALSE = Flujo principal: interacciones por dia con decisiones.\n" +
             "TRUE  = NPC extra: interacciones por bienestar, solo dialogo.")]
    public bool esExtra = false;

    public InteraccionPrincipal[] interaccionesPrincipales;
    public InteraccionExtra[]     interaccionesExtra;

    [Tooltip("Dialogo interno de Mateo si no hay interaccion disponible")]
    [TextArea(2, 3)]
    public string textoSinInteraccion = "No tengo nada que decirle ahora.";
}
