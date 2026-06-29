using UnityEngine;

[System.Serializable]
public class ConfigLluvia
{
    [Tooltip("Dia en que llueve. 0 = todos los dias")]
    public int dia;

    [Tooltip("Escenas donde aparece la lluvia ese dia.\n" +
             "Ej: '2. StreetWest', '3. Park'")]
    public string[] escenas;

    [Tooltip("Solo de noche, solo de dia, o cualquier hora")]
    public CondicionHora hora = CondicionHora.Cualquiera;
}

[CreateAssetMenu(fileName = "RainConfig", menuName = "Juego/Configuracion de Lluvia")]
public class RainConfig : ScriptableObject
{
    [Tooltip("Configuraciones de lluvia por dia y escena")]
    public ConfigLluvia[] configuraciones;

    public bool DeberiaLlover(int dia, string escena, bool esNoche)
    {
        if (configuraciones == null) return false;

        foreach (var config in configuraciones)
        {
            bool aplicaDia = config.dia == 0 || config.dia == dia;
            if (!aplicaDia) continue;

            bool aplicaHora = config.hora == CondicionHora.Cualquiera
                || (config.hora == CondicionHora.SoloDeDia   && !esNoche)
                || (config.hora == CondicionHora.SoloDeNoche &&  esNoche);
            if (!aplicaHora) continue;

            if (config.escenas == null) continue;

            foreach (var e in config.escenas)
                if (e == escena) return true;
        }

        return false;
    }
}