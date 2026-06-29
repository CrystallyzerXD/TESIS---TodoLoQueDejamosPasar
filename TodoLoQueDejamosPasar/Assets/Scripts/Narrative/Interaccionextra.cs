using UnityEngine;

[System.Serializable]
public class InteraccionExtra
{
    [Tooltip("Bienestar minimo requerido. -1 = sin limite")]
    public int bienestarMin = -1;

    [Tooltip("Bienestar maximo requerido. -1 = sin limite")]
    public int bienestarMax = -1;

    [Tooltip("Cuando puede aparecer esta interaccion")]
    public CondicionHora hora = CondicionHora.Cualquiera;

    [Tooltip("Dialogo que aparece al interactuar")]
    public DialogueData dialogoInicial;

    public bool CondicionesCumplidas()
    {
        var gm = GameManager.Instance;

        if (bienestarMin >= 0 && gm.bienestarUrbano < bienestarMin) return false;
        if (bienestarMax >= 0 && gm.bienestarUrbano > bienestarMax) return false;
        if (hora == CondicionHora.SoloDeDia   &&  gm.esNoche) return false;
        if (hora == CondicionHora.SoloDeNoche && !gm.esNoche) return false;

        return true;
    }
}