using UnityEngine;

[System.Serializable]
public class ConfigDia
{
    [Tooltip("Dia en que aparece este NPC")]
    public int dia = 1;

    [Tooltip("Cuando puede aparecer en ese dia")]
    public CondicionHora hora = CondicionHora.Cualquiera;
}

public class NPCSchedule : MonoBehaviour
{
    [Header("Ubicacion")]
    public string locationID = "3. Park";

    [Header("Tipo")]
    [Tooltip("FALSE = Flujo principal: aparece segun dias.\n" +
             "TRUE  = NPC extra: aparece segun bienestar.")]
    public bool esExtra = false;

    // ── Flujo principal ───────────────────────────────────────────────────────
    [Tooltip("Cada elemento define un dia y la hora en que aparece el NPC ese dia")]
    public ConfigDia[] diasActivos = { new ConfigDia { dia = 1 } };

    // ── NPC extra ─────────────────────────────────────────────────────────────
    [Tooltip("Bienestar minimo requerido. -1 = sin limite")]
    public int bienestarMin = -1;

    [Tooltip("Bienestar maximo requerido. -1 = sin limite")]
    public int bienestarMax = -1;

    [Tooltip("Cuando puede aparecer este NPC extra")]
    public CondicionHora hora = CondicionHora.Cualquiera;

    private void Start()
    {
        Evaluar();
    }

    public void Evaluar()
    {
        var gm = GameManager.Instance;

        bool enEscenaCorrecta = gm.escenaActual == locationID;
        bool cumpleCondicion  = false;

        if (esExtra)
        {
            bool cumpleMin = bienestarMin < 0 || gm.bienestarUrbano >= bienestarMin;
            bool cumpleMax = bienestarMax < 0 || gm.bienestarUrbano <= bienestarMax;
            bool cumpleHora = CumpleHora(hora, gm.esNoche);
            cumpleCondicion = cumpleMin && cumpleMax && cumpleHora;
        }
        else
        {
            // Busca si hay algun ConfigDia que coincida con el dia y hora actual
            foreach (var config in diasActivos)
            {
                if (config.dia == gm.diaActual && CumpleHora(config.hora, gm.esNoche))
                {
                    cumpleCondicion = true;
                    break;
                }
            }
        }

        gameObject.SetActive(enEscenaCorrecta && cumpleCondicion);
    }

    private bool CumpleHora(CondicionHora condicion, bool esNoche)
    {
        return condicion == CondicionHora.Cualquiera
            || (condicion == CondicionHora.SoloDeDia   && !esNoche)
            || (condicion == CondicionHora.SoloDeNoche &&  esNoche);
    }
}