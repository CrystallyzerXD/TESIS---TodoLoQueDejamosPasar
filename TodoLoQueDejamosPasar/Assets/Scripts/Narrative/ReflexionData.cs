using UnityEngine;

[System.Serializable]
public class Reflexion
{
    [TextArea(2, 4)]
    public string texto;

    [Header("Condiciones")]
    [Tooltip("Dia requerido. 0 = cualquier dia")]
    public int diaRequerido = 0;

    public CondicionVariable contaminacion;
    public CondicionVariable bienestar;
    public CondicionVariable riesgo;

    [Tooltip("Condicion sobre el estado actual de los arboles.\n" +
             "Ej: MayorOIgual + 2 = ya talados. Ignorar = no importa.")]
    public CondicionVariable arboles;

    [Tooltip("Condicion sobre el cambio de arboles pendiente para el dia siguiente.\n" +
             "Usar 'Igual' con el valor exacto. Ej: Igual + 2 = decidio talar.\n" +
             "Usar 'Ignorar' si no importa.")]
    public CondicionVariable arbolesPendiente;

    public bool CondicionesCumplidas()
    {
        var gm = GameManager.Instance;

        if (diaRequerido > 0 && gm.diaActual != diaRequerido)             return false;
        if (!contaminacion.Cumplida(gm.contaminacion))                     return false;
        if (!bienestar.Cumplida(gm.bienestarUrbano))                       return false;
        if (!riesgo.Cumplida(gm.riesgoInundacion))                         return false;
        if (!arboles.Cumplida(gm.estadoArboles))                           return false;
        if (!arbolesPendiente.Cumplida(gm.estadoArbolesPendiente))         return false;

        return true;
    }
}

[CreateAssetMenu(fileName = "ReflexionData", menuName = "Juego/Reflexion Data")]
public class ReflexionData : ScriptableObject
{
    [Tooltip("Se evaluan de arriba a abajo. Se muestra la primera que cumpla condiciones.\n" +
             "La ultima deberia tener todo en Ignorar (fallback).")]
    public Reflexion[] reflexiones;
}