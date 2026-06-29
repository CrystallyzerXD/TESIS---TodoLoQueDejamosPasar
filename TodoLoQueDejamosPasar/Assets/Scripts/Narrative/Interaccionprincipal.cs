using UnityEngine;

[System.Serializable]
public class InteraccionPrincipal
{
    [Tooltip("ID unico para registrar esta interaccion.\nEj: 'Lucia_Dia1', 'DonEladio_Dia3'")]
    public string id;

    [Tooltip("Dia requerido. 0 = cualquier dia")]
    public int diaRequerido = 0;

    [Tooltip("Cuando puede aparecer esta interaccion")]
    public CondicionHora hora = CondicionHora.Cualquiera;

    [Header("Flujo — solo uno de los dos")]
    [Tooltip("Dialogo normal con burbuja (opcional si hay escenaRol)")]
    public DialogueData dialogoInicial;

    [Tooltip("Escena de rol fullscreen (si esta asignada, ignora dialogoInicial)")]
    public EscenaRolData escenaRol;

    [Tooltip("Decision con consecuencias — solo aplica si NO hay escenaRol")]
    public DecisionData decision;

    [Tooltip("Si false, esta interaccion solo ocurre una vez")]
    public bool repetible = false;

    public bool CondicionesCumplidas()
    {
        var gm = GameManager.Instance;

        if (diaRequerido > 0 && gm.diaActual != diaRequerido) return false;
        if (hora == CondicionHora.SoloDeDia   &&  gm.esNoche) return false;
        if (hora == CondicionHora.SoloDeNoche && !gm.esNoche) return false;

        return true;
    }
}